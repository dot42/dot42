using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dot42.DebuggerLib.Events.Jdwp;
using Dot42.JvmClassLib;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Handle all exceptions.
    /// </summary>
    public class DalvikExceptionManager : DalvikProcessChild
    {
        private readonly ExceptionBehaviorMap _exceptionBehaviorMap = new ExceptionBehaviorMap();
        private readonly ConcurrentDictionary<string, DalvikClassPrepareCookie> _preparers = new ConcurrentDictionary<string, DalvikClassPrepareCookie>();
        private readonly ConcurrentDictionary<ReferenceTypeId, int> _eventRequests = new ConcurrentDictionary<ReferenceTypeId, int>();
        private int defaultBehaviorCaughtEventId = -1;
        private volatile bool _wasPrepared;

        private FieldId _throwableDetailedMessageFieldId;
        private bool _failedToFindThrowableDetailedMessageField;

        private const string ThrowableClassSignature = "Ljava/lang/Throwable;";
        private const string ThrowableDetailMessageFieldName = "detailMessage";

        // Do not report Dalvik/Java internal exception. These are be used for control flow(!) 
        // by the class libaries. They are of no interest whatsover to the user. They will 
        // all be caught by the framework, and be more than distracting if first chance exception 
        // are enabled.
        private static readonly Regex CaughtExceptionClassExcludePattern = new Regex(@"^libcore\..*", RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);
        protected static readonly Regex CaughtExceptionLocationExcludePattern = new Regex(@"^libcore\..*", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly string[] CaughtExceptionExcludeLocations = new[] { "java.lang.BootClassLoader", "libcore.*" };

        /// <summary>
        /// Default ctor
        /// </summary>
        protected internal DalvikExceptionManager(DalvikProcess process)
            : base(process)
        {
        }

        /// <summary>
        /// Gets the map that controls the behavior for specifc exceptions.
        /// </summary>
        public ExceptionBehaviorMap ExceptionBehaviorMap
        {
            get { return _exceptionBehaviorMap; }
        }

        public Task<string> GetExceptionMessageAsync(TaggedObjectId exceptionObject)
        {
            if (_failedToFindThrowableDetailedMessageField)
                return null;
            // TODO: this logic could be factored out into a "ReferenceTypeFieldValueRetriever" (find
            //       a better name...) class, and could also be used to directly show List<T> as
            //       array type in the watch window and similar fancy things.
            return Task.Factory.StartNew(() =>
            {
                if (_throwableDetailedMessageFieldId == null)
                {
                    Process.ReferenceTypeManager.RefreshClassesWithSignatureAsync(ThrowableClassSignature)
                                                .Await(DalvikProcess.VmTimeout);
                    var refType = Process.ReferenceTypeManager.FindBySignature(ThrowableClassSignature);
                    if (refType == null)
                    {
                        _failedToFindThrowableDetailedMessageField = true;
                        return null;
                    }
                    var fieldInfo = Debugger.ReferenceType.FieldsAsync(refType.Id).Await(DalvikProcess.VmTimeout)
                                                          .FirstOrDefault(f => f.Name == ThrowableDetailMessageFieldName);
                    if (fieldInfo == null)
                    {
                        _failedToFindThrowableDetailedMessageField = true;
                        return null;
                    }
                    _throwableDetailedMessageFieldId = fieldInfo.Id;
                }

                var val = Debugger.ObjectReference.GetValuesAsync(exceptionObject.Object, new[] { _throwableDetailedMessageFieldId })
                                                  .Await(DalvikProcess.VmTimeout).First();

                var objectId = (ObjectId)val.ValueObject;
                if(objectId.IsNull)
                    return null;

                var ret = Debugger.StringReference.ValueAsync(objectId).Await(DalvikProcess.VmTimeout);
                return ret;
            });

        }

        /// <summary>
        /// Process the given exception event.
        /// </summary>
        protected internal virtual void OnExceptionEvent(Events.Jdwp.Exception @event, DalvikThread thread)
        {
            // Log
            DLog.Debug(DContext.VSDebuggerEvent, "OnExceptionEvent location: {0}", @event.Location);

            // Note that Process.OnSuspended() has not been called yet, and needs to be called 
            // if the implementation does not decide to continue in spite of the exception.
            Debugger.Process.OnSuspended(SuspendReason.Exception, thread);

            // Save exception in thread
            if(thread != null)
                thread.CurrentException = @event.ExceptionObject;
        }

        /// <summary>
        /// Initialize the debugger so we're ready to start debugging.
        /// </summary>
        internal Task PrepareForDebuggingAsync()
        {
            return SetupBehaviors(_exceptionBehaviorMap.Behaviors);
        }



        public void SetExceptionBehavior(ExceptionBehaviorMap newMap)
        {
            var changed = _exceptionBehaviorMap.CopyFrom(newMap);
            if (!_wasPrepared || changed.Count == 0) return;
            SetupBehaviors(changed);
        }

        private Task SetupBehaviors(IEnumerable<ExceptionBehavior> behaviors)
        {
            // The idea is to set the default behavior, and any overriden behavior
            // that would not be reported by the default behavior.
            // From what I understand we can not suppress specific exceptions using
            // Jdwp. These can be filtered out when received through ShouldHandle() 
            // in a derived class.

            // see http://docs.oracle.com/javase/7/docs/platform/jpda/jdwp/jdwp-protocol.html
            // and http://kingsfleet.blogspot.de/2013/10/write-auto-debugger-to-catch-exceptions.html

            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var refManager = Debugger.Process.ReferenceTypeManager;
                    var defaultStopOnThrow = _exceptionBehaviorMap.DefaultStopOnThrow;
                    var defaultStopUncaught = _exceptionBehaviorMap.DefaultStopUncaught;

                    SetupDefaultOnThrowBehavior(defaultStopOnThrow);
                    // use normal handling for uncauhgt exceptions.
                    SetupBehavior(new ClassId(Debugger.GetIdSizeInfo()), false, defaultStopUncaught, "(default)");

                    foreach (var b in behaviors)
                    {
                        if (b != null)
                        {
                            var needsOverride = (b.StopOnThrow && !defaultStopOnThrow)
                                             || (b.StopUncaught && !defaultStopUncaught);

                            var signature = Debugger.Process.ClrNameToSignature(b.ExceptionName);
                            
                            var refType = refManager.FindBySignature(signature);

                            if (!needsOverride && refType != null)
                            {
                                // remove event, if any.
                                RemoveBehavior(refType.Id, signature);
                            }
                            else if (needsOverride)
                            {
                                EnsureClassPrepareListener(refManager, signature);

                                refManager.RefreshClassesWithSignatureAsync(signature).Await(DalvikProcess.VmTimeout);
                                refType = refManager.FindBySignature(signature);
                                
                                if (refType != null)
                                {
                                    SetupBehavior(refType.Id, b.StopOnThrow, b.StopUncaught, signature);
                                }
                            }
                        }
                        else 
                        {
                            // default behavior has changed
                            var sizeInfo = Debugger.GetIdSizeInfo();
                            SetupBehavior(new ClassId(sizeInfo), defaultStopOnThrow, defaultStopUncaught, "(default)");
                        }
                    }
                }
                finally
                {
                    _wasPrepared = true;
                }
            });
        }

        private void SetupBehavior(ReferenceTypeId refTypeId, bool stopOnThrow, bool stopUncaught, string signature)
        {
            // set the event
            DLog.Info(DContext.DebuggerLibDebugger, "requesting exception event {0}: stopOnThrow: {1} stopUncaught: {2}", signature, stopOnThrow, stopUncaught);

            var modifier = new ExceptionOnlyModifier(refTypeId, stopOnThrow, stopUncaught);
            var eventId = Debugger.EventRequest.SetAsync(Jdwp.EventKind.Exception, Jdwp.SuspendPolicy.All, modifier)
                                               .Await(DalvikProcess.VmTimeout);
            // remove previous event, if any.
            int? prevEventId = null;
            _eventRequests.AddOrUpdate(refTypeId, eventId, (key, prev) =>
            {
                prevEventId = prev;
                return eventId;
            });

            if (prevEventId.HasValue)
            {
                Debugger.EventRequest.ClearAsync(Jdwp.EventKind.Exception, prevEventId.Value)
                                     .Await(DalvikProcess.VmTimeout);
            }
        }

        /// <summary>
        /// special handling for caught exception, to prevent exception pollution
        /// due to ClassNotFoundExceptions an other internal eceptions used for
        /// control flow.
        /// </summary>
        /// <param name="stopOnThrow"></param>
        private void SetupDefaultOnThrowBehavior(bool stopOnThrow)
        {
            var prevEventId = defaultBehaviorCaughtEventId;

            if (!stopOnThrow && prevEventId != -1)
            {
                if (Interlocked.CompareExchange(ref defaultBehaviorCaughtEventId, -1, prevEventId) == prevEventId)
                {
                    Debugger.EventRequest.ClearAsync(Jdwp.EventKind.Exception, prevEventId)
                                         .Await(DalvikProcess.VmTimeout);
                }
            }
            else if(stopOnThrow && prevEventId == -1)
            {
                // 0 means all types.
                var refTypeId = new ClassId(Debugger.GetIdSizeInfo());

                List<EventModifier> mod = new List<EventModifier>
                {
                    new ExceptionOnlyModifier(refTypeId, true, false)
                };
                mod.AddRange(CaughtExceptionExcludeLocations.Select(pattern => new ClassExcludeModifier(pattern)));

                var eventId = Debugger.EventRequest.SetAsync(Jdwp.EventKind.Exception, Jdwp.SuspendPolicy.All, mod.ToArray())
                                                   .Await(DalvikProcess.VmTimeout);

                prevEventId = Interlocked.Exchange(ref defaultBehaviorCaughtEventId, eventId);
                if (prevEventId != -1)
                {
                    Debugger.EventRequest.ClearAsync(Jdwp.EventKind.Exception, prevEventId)
                                         .Await(DalvikProcess.VmTimeout);
                }
            }
        }

        private void RemoveBehavior(ReferenceTypeId refTypeId, string signature)
        {
            int prevEventId;
            if (_eventRequests.TryRemove(refTypeId, out prevEventId))
            {
                DLog.Info(DContext.DebuggerLibDebugger, "clearing exception event: " + signature);
                Debugger.EventRequest.ClearAsync(Jdwp.EventKind.Exception, prevEventId)
                                     .Await(DalvikProcess.VmTimeout);      
            }
        }

        private void EnsureClassPrepareListener(DalvikReferenceTypeManager refManager, string signature)
        {
            if (_preparers.ContainsKey(signature))
                return;            
            var token = refManager.RegisterClassPrepareHandler(signature, OnExceptionClassPrepared);

            if(!_preparers.TryAdd(signature, token))
                refManager.Remove(token);
        }

        private void OnExceptionClassPrepared(ClassPrepare obj)
        {
            // since a class can be loaded multiple times, keep listening
            // for further prepared events.

            // update the behavior.
            var clrName = Debugger.Process.SignatureToClrName(obj.Signature);
            SetupBehaviors(new[]{_exceptionBehaviorMap[clrName]});
        }

        protected bool ShouldHandle(string exceptionName, bool caught)
        {
            if (caught && CaughtExceptionClassExcludePattern.IsMatch(exceptionName))
                return false;
            var behavior = ExceptionBehaviorMap[exceptionName];
            if ((caught && behavior.StopOnThrow) || (!caught && behavior.StopUncaught))
            {
                return true;
            }
            return false;
        }
    }
}
