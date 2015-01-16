using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    /// <summary>
    /// This class represents a breakpoint that is ready to bind to a code location.
    /// </summary>
    internal sealed class DebugPendingBreakpoint : IDebugPendingBreakpoint2
    {
        private readonly IDebugBreakpointRequest2 request;
        private readonly DebugEngine engine;
        private readonly List<IDebugBoundBreakpoint> boundBreakpoints = new List<IDebugBoundBreakpoint>();
        private readonly object boundBreakpointsLock = new object();
        private bool deleted;
        private bool enabled = true;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DebugPendingBreakpoint(IDebugBreakpointRequest2 request, DebugEngine engine)
        {
            this.request = request;
            this.engine = engine;
        }

        /// <summary>
        /// Record the given bound breakpoint.
        /// </summary>
        internal void AddBoundBreakpoint(IDebugBoundBreakpoint boundBreakpoint)
        {
            lock (boundBreakpointsLock)
            {
                boundBreakpoints.Add(boundBreakpoint);
            }
        }

        /// <summary>
        /// Remove the given bound breakpoint.
        /// </summary>
        internal void RemoveBoundBreakpoint(IDebugBoundBreakpoint boundBreakpoint)
        {
            lock (boundBreakpointsLock)
            {
                boundBreakpoints.Remove(boundBreakpoint);
            }
        }

        /// <summary>
        /// Determines whether this pending breakpoint can bind to a code location.
        /// </summary>
        public int CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.CanBind");
            ppErrorEnum = null;
            if (IsDeleted)
                return HResults.E_BP_DELETED;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Binds this pending breakpoint to one or more code locations.
        /// </summary>
        public int Bind()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.Bind");
            return SetBreakpoint();
        }

        /// <summary>
        /// Gets the state of this pending breakpoint.
        /// </summary>
        public int GetState(PENDING_BP_STATE_INFO[] pState)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.GetState");
            pState[0].Flags = enum_PENDING_BP_STATE_FLAGS.PBPSF_NONE;
            if (IsDeleted)
            {
                pState[0].state = enum_PENDING_BP_STATE.PBPS_DELETED;
            }
            else
            {
                pState[0].state = enabled ? enum_PENDING_BP_STATE.PBPS_ENABLED : enum_PENDING_BP_STATE.PBPS_DISABLED;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Is this breakpoint deleted?
        /// </summary>
        public bool IsDeleted { get { return deleted; } }

        /// <summary>
        /// Gets the breakpoint request that was used to create this pending breakpoint.
        /// </summary>
        public int GetBreakpointRequest(out IDebugBreakpointRequest2 ppBPRequest)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.GetBreakpointRequest");
            if (IsDeleted)
            {
                ppBPRequest = null;
                return HResults.E_BP_DELETED;
            }
            ppBPRequest = request;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Toggles the virtualized state of this pending breakpoint.
        /// </summary>
        public int Virtualize(int fVirtualize)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.Virtualize");
            if (IsDeleted)
                return HResults.E_BP_DELETED;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Toggles the enabled state of this pending breakpoint.
        /// </summary>
        public int Enable(int fEnable)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.Enable");

            // Check deleted
            if (deleted)
                return HResults.E_BP_DELETED;

            // Set enabled
            enabled = (fEnable != 0);
            List<IDebugBoundBreakpoint> all;
            lock (boundBreakpointsLock)
            {
                all = boundBreakpoints.ToList();
            }
            all.ForEach(x => x.Enable(fEnable));

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Sets or changes the condition associated with this pending breakpoint.
        /// </summary>
        public int SetCondition(BP_CONDITION bpCondition)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.SetCondition");
            if (IsDeleted)
                return HResults.E_BP_DELETED;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets or changes the pass count associated with this pending breakpoint.
        /// </summary>
        public int SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.SetPassCount");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enumerates all breakpoints bound from this pending breakpoint.
        /// </summary>
        public int EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.EnumBoundBreakpoints");
            if (IsDeleted)
            {
                ppEnum = null;
                return HResults.E_BP_DELETED;
            }
            ppEnum = new BoundBreakpointsEnum(boundBreakpoints);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Enumerates all error breakpoints that resulted from this pending breakpoint.
        /// </summary>
        public int EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.EnumErrorBreakpoints");
            if (IsDeleted)
            {
                ppEnum = null;
                return HResults.E_BP_DELETED;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove/delete all bound breakpoints.
        /// </summary>
        public void ClearBreakpoints()
        {
            List<IDebugBoundBreakpoint> all;
            lock (boundBreakpointsLock)
            {
                all = boundBreakpoints.ToList();
            }
            all.ForEach(x => x.Delete());
        }

        /// <summary>
        /// Deletes this pending breakpoint and all breakpoints bound from it.
        /// </summary>
        public int Delete()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPendingBreakpoint.Delete");
            if (deleted)
                return HResults.E_BP_DELETED;
            deleted = true;
            ClearBreakpoints();
            return VSConstants.S_OK;
        }

        private int SetBreakpoint()
        {
            if (IsDeleted)
                return HResults.E_BP_DELETED;

            // Get breakpoint type
            var typeArr = new enum_BP_LOCATION_TYPE[1];
            if (ErrorHandler.Failed(request.GetLocationType(typeArr)))
                return VSConstants.E_INVALIDARG;
            var type = typeArr[0];

            // Get info
            var infoArr = new BP_REQUEST_INFO[1];
            if (ErrorHandler.Failed(request.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_ALLFIELDS, infoArr)))
                return VSConstants.E_INVALIDARG;
            var info = infoArr[0];

            // See http://msdn.microsoft.com/en-us/library/bb162191.aspx

            if (type == enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE)
            {
                // Document position
                var documentPosition = (IDebugDocumentPosition2)Marshal.GetObjectForIUnknown(info.bpLocation.unionmember2);
                string fileName;
                if (ErrorHandler.Failed(documentPosition.GetFileName(out fileName)))
                    return VSConstants.E_INVALIDARG;
                var beginPosition = new TEXT_POSITION[1];
                var endPosition = new TEXT_POSITION[1];
                if (ErrorHandler.Failed(documentPosition.GetRange(beginPosition, endPosition)))
                    return VSConstants.E_INVALIDARG;

                // Set breakpoint
                var program = engine.Program;
                if (program == null)
                    return VSConstants.E_INVALIDARG;

                // Convert positions
                var startLine = beginPosition[0].dwLine + 1;
                var startCol = beginPosition[0].dwColumn + 1;
                var endLine = endPosition[0].dwLine + 1;
                var endCol = endPosition[0].dwColumn + 1;

                program.BreakpointManager.SetAtLocation(fileName, (int)startLine, (int)startCol, (int)endLine, (int)endCol, this);
            }

            return VSConstants.S_OK;
        }
    }
}
