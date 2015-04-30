using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private volatile bool _wasPrepared;
        
        // Do not report Dalvik/Java internal exceptions to the user. These 
        // are be used for control flow(!) by the class libaries. They are 
        // of no interest whatsover to the user. While these will all be 
        // caught by the framework, they will be more than distracting if
        // first chance exception are enabled.
        private static readonly Regex CaughtExceptionClassExcludePattern = new Regex(@"^libcore\..*", RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);
        protected static readonly Regex CaughtExceptionLocationExcludePattern = new Regex(@"^libcore\..*", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// Default ctor
        /// </summary>
        protected internal DalvikExceptionManager(DalvikProcess process)
            : base(process)
        {
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

        /// <summary>
        /// Gets the map that controls the behavior for specifc exceptions.
        /// </summary>
        public ExceptionBehaviorMap ExceptionBehaviorMap
        {
            get { return _exceptionBehaviorMap; }
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

                    if (!_wasPrepared)
                    {
                        // set the default.
                        var sizeInfo = Debugger.GetIdSizeInfo();
                        // new ClassId(sizeInfo) means Null class - all exceptions.
                        SetupBehavior(new ClassId(sizeInfo), defaultStopOnThrow, defaultStopUncaught, "(default)");
                    }

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
