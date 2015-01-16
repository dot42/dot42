using System;
using System.Collections.Generic;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    public class DebugThread : DalvikThread, IDebugThread2
    {
        private readonly DebugProgram program;
        private readonly EngineEventCallback eventCallback;
        private readonly int tid;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DebugThread(DebugProgram program, EngineEventCallback eventCallback, ThreadManager manager, ThreadId threadId, int tid)
            : base(threadId, manager)
        {
            this.program = program;
            this.eventCallback = eventCallback;
            this.tid = tid;
        }

        /// <summary>
        /// Gets the containing program
        /// </summary>
        internal DebugProgram Program { get { return program; } }

        /// <summary>
        /// Gets the current state
        /// </summary>
        internal enum_THREADSTATE State
        {
            get { return enum_THREADSTATE.THREADSTATE_RUNNING;  /* TODO */}
        }

        /// <summary>
        /// Load call stack
        /// </summary>
        public int EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.EnumFrameInfo");

            // Get frames
            var frameInfos = new List<FRAMEINFO>();
            foreach (var stackFrame in GetCallStack())
            {
                FRAMEINFO info;
                ((DebugStackFrame)stackFrame).SetFrameInfo(dwFieldSpec, out info);
                frameInfos.Add(info);
            }

            ppEnum = new FrameInfoEnum(frameInfos);
            return VSConstants.S_OK;
        }

        public int GetName(out string pbstrName)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.GetName");
            pbstrName = GetNameAsync().Await(DalvikProcess.VmTimeout);
            return VSConstants.S_OK;
        }

        public int SetThreadName(string pszName)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.SetThreadName");
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Gets the containing program
        /// </summary>
        public int GetProgram(out IDebugProgram2 ppProgram)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.GetProgram");
            ppProgram = program;
            return VSConstants.S_OK;
        }

        public int CanSetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.CanSetNextStatement");
            return VSConstants.E_FAIL;
        }

        public int SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.SetNextStatement");
            return VSConstants.E_NOTIMPL;
        }

        public int GetThreadId(out uint pdwThreadId)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.GetThreadId");
            pdwThreadId = (uint) tid;
            return VSConstants.S_OK;
        }

        public int Suspend(out uint pdwSuspendCount)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.Suspend");
            throw new NotImplementedException();
        }

        public int Resume(out uint pdwSuspendCount)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.Resume");
            throw new NotImplementedException();
        }

        public int GetThreadProperties(enum_THREADPROPERTY_FIELDS fields, THREADPROPERTIES[] ptp)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.GetThreadProperties {0}", fields);
            var info = new THREADPROPERTIES();
            if (fields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_ID))
            {
                info.dwThreadId = (uint) tid;
                info.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_ID;
            }
            if (fields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_NAME))
            {
                info.bstrName = GetNameAsync().Await(DalvikProcess.VmTimeout);
                info.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_NAME;
            }
            if (fields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_STATE))
            {
                info.dwThreadState = (uint) State;
                info.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_STATE;
            }
            if (fields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT))
            {
                info.dwSuspendCount = (uint)GetSuspendCountAsync().Await(DalvikProcess.VmTimeout);
                info.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT;
            }
            if (fields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_PRIORITY))
            {
                info.bstrPriority = "Normal";
                info.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_PRIORITY;
            }
            ptp[0] = info;
            return VSConstants.S_OK;
        }

        public int GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.GetLogicalThread");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a stack frame for the given parameters.
        /// </summary>
        protected override DalvikStackFrame CreateStackFrame(FrameId frameId, Location location)
        {
            return new DebugStackFrame(frameId, location, this);
        }
    }
}
