using System;
using Dot42.DebuggerLib.Model;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    /// <summary>
    /// breakpoint.
    /// </summary>
    internal sealed class DebugBoundBreakpoint<T> : IDebugBoundBreakpoint2, IDebugBoundBreakpoint
        where T : DalvikBreakpoint
    {
        private readonly DebugPendingBreakpoint pendingBreakpoint;
        private readonly DebugBreakpointManager breakpointManager;
        private readonly T breakpoint;
        private readonly DebugBreakpointResolution resolution;
        private uint hitCount;
        private bool deleted;
        private bool enabled = true;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugBoundBreakpoint(DebugPendingBreakpoint pendingBreakpoint, DebugBreakpointManager breakpointManager, enum_BP_TYPE type, Func<DebugBoundBreakpoint<T>, T> breakpointBuilder)
        {
            this.pendingBreakpoint = pendingBreakpoint;
            this.breakpointManager = breakpointManager;
            breakpoint = breakpointBuilder(this);
            resolution = new DebugBreakpointResolution(type, breakpointManager.Program, breakpoint as DalvikLocationBreakpoint);
        }

        /// <summary>
        /// The contained breakpoint.
        /// </summary>
        public T Breakpoint
        {
            get { return breakpoint; }
        }

        /// <summary>
        /// Is this breakpoint deleted?
        /// </summary>
        private bool IsDeleted
        {
            get { return deleted; }
        }

        /// <summary>
        /// The contained breakpoint.
        /// </summary>
        DalvikBreakpoint IDebugBoundBreakpoint.Breakpoint
        {
            get { return breakpoint; }
        }

        /// <summary>
        /// The given breakpoint has been bound to the VM.
        /// </summary>
        internal void OnBound(IDebugBreakpoint breakpoint)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.OnBound");

            // Check parameters
            if (this.breakpoint != breakpoint)
                throw new ArgumentException("Unknown breakpoint");

            // Record in pending breakpoint
            pendingBreakpoint.AddBoundBreakpoint(this);

            // Notify VS
            breakpointManager.OnBound(pendingBreakpoint);
        }

        /// <summary>
        /// Called when this breakpoint has been reset.
        /// </summary>
        void IDebugBoundBreakpoint.OnReset()
        {
            // Remove from pending breakpoint
            pendingBreakpoint.RemoveBoundBreakpoint(this);
        }

        /// <summary>
        /// Notify visual studio of a trigger in this breakpoint.
        /// </summary>
        internal void OnTrigger(DebuggerLib.Events.Jdwp.Breakpoint @event)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.OnTrigger");

            // Prepare
            var program = breakpointManager.Program;

            // Increment hitcount
            hitCount++;

            // Find thread
            DalvikThread thread;
            if (!program.ThreadManager.TryGet(@event.ThreadId, out thread))
            {
                thread = program.ThreadManager.MainThread();
            }

            // Set resolution info            
            resolution.Thread = (DebugThread) thread; 

            // Send event
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.OnTrigger.Send");
            breakpointManager.Send((DebugThread)thread, new BreakpointEvent(this));
        }

        public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.GetPendingBreakpoint");
            ppPendingBreakpoint = pendingBreakpoint;
            return VSConstants.S_OK;
        }

        public int GetState(enum_BP_STATE[] pState)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.GetState");
            if (IsDeleted)
            {
                pState[0] = enum_BP_STATE.BPS_DELETED;
            }
            else
            {
                pState[0] = enabled ? enum_BP_STATE.BPS_ENABLED : enum_BP_STATE.BPS_DISABLED;
            }
            return VSConstants.S_OK;
        }

        public int GetHitCount(out uint pdwHitCount)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.GetHitCount");
            pdwHitCount = hitCount;
            return IsDeleted ? HResults.E_BP_DELETED : VSConstants.S_OK;
        }

        public int GetBreakpointResolution(out IDebugBreakpointResolution2 ppBPResolution)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.GetBreakpointResolution");
            ppBPResolution = resolution;
            return IsDeleted ? HResults.E_BP_DELETED : VSConstants.S_OK;
        }

        public int Enable(int fEnable)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.Enable");
            if (IsDeleted)
                return HResults.E_BP_DELETED;
            enabled = (fEnable != 0);
            return VSConstants.S_OK;
        }

        public int SetHitCount(uint dwHitCount)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.SetHitCount");
            if (IsDeleted)
                return HResults.E_BP_DELETED;
            hitCount = dwHitCount;
            return VSConstants.S_OK;
        }

        public int SetCondition(BP_CONDITION bpCondition)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.SetCondition");
            if (IsDeleted)
                return HResults.E_BP_DELETED;
            return pendingBreakpoint.SetCondition(bpCondition);
        }

        public int SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.SetPassCount");
            if (IsDeleted)
                return HResults.E_BP_DELETED;
            return pendingBreakpoint.SetPassCount(bpPassCount);
        }

        public int Delete()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugBoundBreakpoint.Delete");
            if (IsDeleted)
                return HResults.E_BP_DELETED;
            deleted = true;
            breakpointManager.ResetAsync(Breakpoint);
            return VSConstants.S_OK;
        }
    }
}
