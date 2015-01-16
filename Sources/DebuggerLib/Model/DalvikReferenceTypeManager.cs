using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dot42.DebuggerLib.Events.Jdwp;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Maintain class information
    /// </summary>
    public class DalvikReferenceTypeManager : DalvikProcessChild
    {
        private readonly Dictionary<ReferenceTypeId, DalvikReferenceType> classes = new Dictionary<ReferenceTypeId, DalvikReferenceType>();
        private readonly List<DalvikClassPrepareCookie> classPrepareHandlers = new List<DalvikClassPrepareCookie>();
        private int lastClassPrepareCookie = 1;
        private readonly object dataLock = new object();

        /// <summary>
        /// Default ctor
        /// </summary>
        protected internal DalvikReferenceTypeManager(DalvikProcess process)
            : base(process)
        {
        }

        /// <summary>
        /// Gets the class that belongs to the given id.
        /// Create's it if needed.
        /// </summary>
        public DalvikReferenceType this[ReferenceTypeId id]
        {
            get
            {
                lock (dataLock)
                {
                    DalvikReferenceType result;
                    if (classes.TryGetValue(id, out result))
                        return result;
                    result = CreateReferenceType(id);
                    classes[id] = result;
                    return result;
                }
            }
        }

        /// <summary>
        /// Try to get a reference type by its signature.
        /// </summary>
        public DalvikReferenceType FindBySignature(string signature)
        {
            lock (dataLock)
            {
                return classes.Values.FirstOrDefault(x => signature == x.Signature);
            }            
        }

        /// <summary>
        /// Make sure all loaded classes are known by me.
        /// </summary>
        public Task RefreshClassesAsync()
        {
            return Process.Debugger.VirtualMachine.AllClassesWithGenericAsync().ContinueWith(OnProcessAllClasses);
        }

        /// <summary>
        /// Make sure all classes with given signature are known by me.
        /// </summary>
        public Task RefreshClassesWithSignatureAsync(string signature)
        {
            return Process.Debugger.VirtualMachine.ClassBySignatureAsync(signature).ContinueWith(OnProcessAllClasses);
        }

        /// <summary>
        /// Refresh the class list.
        /// </summary>
        internal void OnProcessSuspended(SuspendReason reason)
        {
            // Refresh classes
            //RefreshClasses().Wait();
        }

        /// <summary>
        /// Called during the initialization of the debugger.
        /// We request the VM for CLASS_PREPARE events
        /// </summary>
        internal Task PrepareForDebuggingAsync()
        {
            return Debugger.EventRequest.SetAsync(Jdwp.EventKind.ClassPrepare, Jdwp.SuspendPolicy.All);
        }

        /// <summary>
        /// Register a handler for class prepare events.
        /// </summary>
        internal DalvikClassPrepareCookie RegisterClassPrepareHandler(string signature, Action<ClassPrepare> handler)
        {
            var cookie = Interlocked.Increment(ref lastClassPrepareCookie);
            var result = new DalvikClassPrepareCookie(cookie, Tuple.Create(signature, handler));
            lock (dataLock)
            {
                classPrepareHandlers.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Remove the given handler from the list of handlers for class prepare events.
        /// </summary>
        internal void Remove(DalvikClassPrepareCookie cookie)
        {
            lock (dataLock)
            {
                classPrepareHandlers.Remove(cookie);
            }
        }

        /// <summary>
        /// A class prepare event is received.
        /// </summary>
        internal void OnClassPrepare(ClassPrepare @event)
        {
            // Log
            var signature = @event.Signature;
            DLog.Debug(DContext.DebuggerLibClassPrepare, "Prepare {0}", signature);

            // Record data
            ProcessClassData(@event.TypeId, @event.Signature, @event.Status);

            // Select class prepare listeners
            var matchingCookies = new List<DalvikClassPrepareCookie>();
            lock (dataLock)
            {
                for (var i = 0; i < classPrepareHandlers.Count; )
                {
                    var entry = classPrepareHandlers[i];
                    if (entry.Signature == signature)
                    {
                        matchingCookies.Add(entry);
                        classPrepareHandlers.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            // Notify class prepare listeners
            matchingCookies.ForEach(x => x.Handler(@event));

            // Resume execution
            Debugger.VirtualMachine.ResumeAsync();
        }

        /// <summary>
        /// Process the task result of the given task.
        /// </summary>
        private void OnProcessAllClasses(Task<List<ClassInfo>> t)
        {
            t.ForwardException();
            foreach (var info in t.Result)
            {
                ProcessClassData(info.TypeId, info.Signature, info.Status);
            }
        }

        /// <summary>
        /// Update the info of a given class.
        /// </summary>
        private void ProcessClassData(ReferenceTypeId typeId, string signature, Jdwp.ClassStatus status)
        {
            lock (dataLock)
            {
                DalvikReferenceType refType;
                if (!classes.TryGetValue(typeId, out refType))
                {
                    // Not found, create new one
                    refType = CreateReferenceType(typeId);
                    classes[typeId] = refType;
                }
                refType.SetSignatureIfNull(signature);
                refType.SetStatusIfNull(status);
            }
        }

        /// <summary>
        /// Create an instance representing the given reference type.
        /// </summary>
        protected virtual DalvikReferenceType CreateReferenceType(ReferenceTypeId id)
        {
            return new DalvikReferenceType(id, this);
        }

        /// <summary>
        /// Handler for class prepare events
        /// </summary>
        private class ClassPrepareEntry
        {
            public readonly string Signature;
            public readonly Action<ClassPrepare> Handler;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ClassPrepareEntry(string signature, Action<ClassPrepare> handler)
            {
                Signature = signature;
                this.Handler = handler;
            }
        }
    }
}
