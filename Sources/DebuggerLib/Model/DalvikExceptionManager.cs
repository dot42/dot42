using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dot42.DebuggerLib.Events.Jdwp;
using Dot42.JvmClassLib;
using Dot42.Mapping;
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
        private readonly ConcurrentDictionary<string, bool> _overridenException = new ConcurrentDictionary<string, bool>();
        private volatile bool _wasPrepared;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected internal DalvikExceptionManager(DalvikProcess process)
            : base(process)
        {
        }

        /// <summary>
        /// Process the given exception event.
        /// 
        /// Note that Process.OnSuspended() has not been called, and needs to be called if the implementation
        /// does not decide to continue in spite of the exception.
        /// </summary>
        protected internal virtual void OnExceptionEvent(Events.Jdwp.Exception @event, DalvikThread thread)
        {
            // Log
            DLog.Debug(DContext.VSDebuggerEvent, "OnExceptionEvent location: {0}", @event.Location);

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
            if (!_wasPrepared) return;
            SetupBehaviors(changed);
        }

        private Task SetupBehaviors(IEnumerable<ExceptionBehavior> behaviors)
        {
            List<Task> asyncRequests = new List<Task>();

            var sizeInfo = Debugger.GetIdSizeInfo(); // note: synchronous call.

            _wasPrepared = true;
            
            var modifier = new ExceptionOnlyModifier(new ClassId(sizeInfo), _exceptionBehaviorMap.DefaultStopOnThrow, _exceptionBehaviorMap.DefaultStopUncaught);
            asyncRequests.Add(Debugger.EventRequest.SetAsync(Jdwp.EventKind.Exception, Jdwp.SuspendPolicy.All, modifier));
            
            // TODO: this code, even though it looks good, does not work. find out why.
            //       since the VS plugin has a fallback mechanism in place, the user is still able
            //       to disable specific exceptions in constrast to the defaults (at a perfomrance penalty)
            //       but not able to enable defaults-disabled ones.
            //       see also http://kingsfleet.blogspot.de/2013/10/write-auto-debugger-to-catch-exceptions.html
            //var netToDex = Debugger.FrameworkTypeMap
            //                       .Values
            //                       .ToLookup(p => p.FullName, p => p.ClassName);
            //var refManager = Debugger.Process.ReferenceTypeManager;

            //foreach (var b in behaviors)
            //{
            //    bool isDefault = b.StopOnThrow == _exceptionBehaviorMap.DefaultStopOnThrow && b.StopUncaught == _exceptionBehaviorMap.DefaultStopUncaught;
                
            //    if(isDefault && !_overridenException.ContainsKey(b.ExceptionName))
            //        continue;

            //    // override the exception
            //    _overridenException.TryAdd(b.ExceptionName, true);

            //    var dexName = netToDex[b.ExceptionName].FirstOrDefault();
            //    if (dexName != null)
            //    {
            //        var signature = GetSignature(dexName);
            //        var type = refManager.FindBySignature(signature);
            //        if (type != null)
            //        {
            //            asyncRequests.Add(SetBehavior(type.Id, b));
            //        }
            //        else if(!_preparers.ContainsKey(dexName))
            //        {
            //            var token = refManager.RegisterClassPrepareHandler(signature, OnExceptionClassPrepared);
            //            _preparers.TryAdd(dexName, token);
            //        }
            //    }
            //}

            var wait = Task.Factory.StartNew(()=>Task.WaitAll(asyncRequests.ToArray()));
            return wait.ContinueWith(task => {  }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private static string GetSignature(string dexName)
        {
            return "L" + dexName + ";";
        }

        private void OnExceptionClassPrepared(ClassPrepare obj)
        {
            var type = Descriptors.ParseClassType(obj.Signature);
            FrameworkTypeMap.TypeEntry typeEntry;

            string clrName;

            if (Debugger.FrameworkTypeMap.TryGet(type.ClassName, out typeEntry))
                clrName = typeEntry.FullName;
            else
                clrName = type.ClrTypeName;
            
            var task = SetBehavior(obj.TypeId, _exceptionBehaviorMap[clrName]);
            task.Await(DalvikProcess.VmTimeout); 
        }

        protected Task<int> SetBehavior(ReferenceTypeId typeId, ExceptionBehavior value)
        {
            return Task.Factory.StartNew(() => 0);
            // TODO: see above.
            //if (!_wasPrepared) 
            //    return Task<int>.Factory.StartNew(()=>0);

            //var modifier = new ExceptionOnlyModifier(typeId, value.StopOnThrow, value.StopUncaught);
            //return Debugger.EventRequest.SetAsync(Jdwp.EventKind.Exception, Jdwp.SuspendPolicy.All,
            //                                      modifier);
        }
    }
}
