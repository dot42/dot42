using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dot42.DebuggerLib.Events.Jdwp;
using Dot42.Mapping;
using Dot42.Utility;
using MethodEntry = Dot42.Mapping.MethodEntry;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Manage all breakpoints.
    /// </summary>
    public class DalvikBreakpointManager
    {
        private readonly DalvikProcess process;
        private readonly List<DalvikBreakpoint> breakpoints = new List<DalvikBreakpoint>();
        private readonly List<Breakpoint> pendingEvents = new List<Breakpoint>();
        private readonly object dataLock = new object();

        /// <summary>
        /// Default ctor
        /// </summary>
        protected internal DalvikBreakpointManager(DalvikProcess process)
        {
            this.process = process;
        }

        /// <summary>
        /// Create and set a breakpoint at the given location.
        /// An exception is thrown if the location cannot be found.
        /// </summary>
        public DalvikLocationBreakpoint SetAtLocation(string document, int startLine, int startCol, int endLine, int endCol, object data)
        {
            // Search matching document
            var doc = MapFile.GetOrCreateDocument(document, false);
            if (doc == null)
                throw new ArgumentException("Unknown document " + document);

            // Search best position
            var pos = doc.Find(startLine, startCol, endLine, endCol);
            if (pos == null)
                throw new ArgumentException(string.Format("No code found at position {0},{1}-{2},{3}", startLine, startCol, endLine, endCol));

            // Lookup class & method
            var type = MapFile.GetTypeById(pos.TypeId);
            if (type == null)
                throw new ArgumentException(string.Format("Inconsistent map file, missing type {0}", pos.TypeId));
            var method = type.GetMethodById(pos.MethodId);
            if (method == null)
                throw new ArgumentException(string.Format("Inconsistent map file, missing method {0}", pos.MethodId));

            // Now create the breakpoint
            var bp = CreateLocationBreakpoint(pos, type, method, data);

            //  Record breakpoint
            Add(bp);

            // Try to bind now
            Task.Factory.StartNew(() => bp.TryBind(process));

            return bp;
        }

        /// <summary>
        /// Clear the given breakpoint and remove from my list.
        /// </summary>
        public Task ResetAsync(DalvikBreakpoint breakpoint)
        {
            return process.Debugger.EventRequest.ClearAsync(breakpoint.EventKind, breakpoint.RequestId).ContinueWith(t => OnReset(breakpoint));
        }

        /// <summary>
        /// The given breakpoint has been clear by the VM.
        /// Now remove from the list.
        /// </summary>
        protected virtual void OnReset(DalvikBreakpoint breakpoint)
        {
            Remove(breakpoint);
        }

        /// <summary>
        /// Process the given breakpoint event.
        /// </summary>
        internal void OnBreakpointEvent(Events.Jdwp.Breakpoint @event)
        {
            DLog.Debug(DContext.VSDebuggerEvent, "OnBreakpointEvent location: {0}", @event.Location);
            DalvikBreakpoint bp;
            lock (dataLock)
            {
                if (!TryGet(@event.RequestId, out bp))
                {
                    pendingEvents.Add(@event);
                    return;
                }
            }
            bp.OnTrigger(@event);
        }

        /// <summary>
        /// Create a new location breakpoint.
        /// </summary>
        protected virtual DalvikLocationBreakpoint CreateLocationBreakpoint(DocumentPosition documentPosition, TypeEntry typeEntry, MethodEntry methodEntry, object data)
        {
            return new DalvikLocationBreakpoint(Jdwp.EventKind.BreakPoint, documentPosition, typeEntry, methodEntry);
        }

        /// <summary>
        /// Add the given breakpoint to the list of maintained breakpoints.
        /// </summary>
        private void Add(DalvikBreakpoint breakpoint)
        {
            lock (dataLock)
            {
                breakpoints.Add(breakpoint);
            }
        }

        /// <summary>
        /// Remove the given breakpoint from the list of maintained breakpoints.
        /// </summary>
        private void Remove(DalvikBreakpoint breakpoint)
        {
            lock (dataLock)
            {
                breakpoints.Remove(breakpoint);
            }
        }
                
        /// <summary>
        /// Gets the first breakpoint that matches the given predicate.
        /// Returns null if the predicate does not match for any of the breakpoints.
        /// </summary>
        protected T FirstOrDefault<T>(Func<T, bool> predicate) 
        	where T : DalvikBreakpoint 
        {
            lock (dataLock)
            {
            	return breakpoints.OfType<T>().FirstOrDefault(predicate);
            }                    	
        }

        /// <summary>
        /// Try to get a breakpoint by it's request id.
        /// </summary>
        private bool TryGet(int requestId, out DalvikBreakpoint breakpoint)
        {
            lock (dataLock)
            {
                breakpoint = breakpoints.FirstOrDefault(x => x.RequestId == requestId);
                return (breakpoint != null);
            }            
        }

        /// <summary>
        /// Process all events with given request id that are pending.
        /// </summary>
        internal void ProcessPendingEvents(DalvikBreakpoint breakpoint)
        {
            List<Breakpoint> events;
            lock (dataLock)
            {
                var requestId = breakpoint.RequestId;
                events = pendingEvents.Where(x => x.RequestId == requestId).ToList();
                events.ForEach(x => pendingEvents.Remove(x));
            }

            // Process pending events now
            events.ForEach(x => Task.Factory.StartNew(() => breakpoint.OnTrigger(x)));            
        }

        /// <summary>
        /// Gets the debugging map file
        /// </summary>
        protected MapFile MapFile { get { return process.MapFile; } }
    }
}
