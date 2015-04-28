using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.DexLib.Instructions;
using Dot42.FrameworkDefinitions;
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
            
            var stack = (DebugStackFrame)pStackFrame;
            var ctx = (DebugCodeContext)pCodeContext;

            if (stack == null || ctx == null)
                return VSConstants.E_FAIL;

            if (ctx.Location.Equals(stack.Location))
                return VSConstants.S_OK;

            if (!ctx.Location.IsSameMethod(stack.Location))
                return HResults.E_CANNOT_SETIP_TO_DIFFERENT_FUNCTION;

            // for now, only allow to set the position above the current position.
            if(ctx.Location.Index >= stack.Location.Index)
                return VSConstants.E_FAIL;

            // don't check existence of special code, so that we can produce a warning 
            // below.

            //var loc = stack.GetDocumentLocationAsync().Await(DalvikProcess.VmTimeout);
            //if (loc.Document == null)
            //    return VSConstants.E_FAIL;

            return VSConstants.S_OK;
        }

        public int SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.SetNextStatement");
            
            var stack = (DebugStackFrame)pStackFrame;
            var ctx = (DebugCodeContext)pCodeContext;

            // nothing to do.
            if (ctx.Location.Equals(stack.Location))
                return VSConstants.S_OK;

            if (!ctx.Location.IsSameMethod(stack.Location))
                return HResults.E_CANNOT_SETIP_TO_DIFFERENT_FUNCTION;

            var loc = stack.GetDocumentLocationAsync().Await(DalvikProcess.VmTimeout);
            if (loc.Document == null)
            {
                DLog.Info(DContext.VSDebuggerMessage, "Can not set next instruction: Debug info not available."); 
                return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;
            }  

            var nextInstrVar = loc.MethodEntry.Variables.FirstOrDefault(v => v.Name == DebuggerConstants.SetNextInstructionVariableName);
            
            if (nextInstrVar == null)
            {
                DLog.Info(DContext.VSDebuggerMessage, "Can not set next instruction: missing compiler setting or method optimized.");
                return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;
            }

            // make sure we are at the beginning of an instruction
            var disassembly = Program.DisassemblyProvider.GetFromLocation(loc);
            if (disassembly == null)
                return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;

            var ins = disassembly.Method.Body.Instructions.FirstOrDefault(i => (ulong)i.Offset == loc.Location.Index);
            if(ins == null)
                return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;

            if (ins.OpCode != OpCodes.If_nez || ins.Registers.Count != 1 || ins.Registers[0].Index != nextInstrVar.Register)
            {
                DLog.Info(DContext.VSDebuggerMessage, "Can not set next instruction: not on start of valid expression.");
                return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;
            }

            DLog.Info(DContext.VSStatusBar, "Setting next instruction to beginning of block.");

            // set the special variable.
            var newSlotVal = new SlotValue(nextInstrVar.Register, Jdwp.Tag.Int, 1);
            Debugger.StackFrame.SetValuesAsync(stack.Thread.Id, stack.Id, newSlotVal)
                               .Await(DalvikProcess.VmTimeout);

            //var onSuspended = GetOnSuspendedTask();

            // perform one step.
            Debugger.Process.StepAsync(new StepRequest(stack.Thread, Jdwp.StepDepth.Over))
                            .Await(DalvikProcess.VmTimeout);
            
            // wait until the step is finally done.
            //onSuspended.Await(DalvikProcess.VmTimeout);

            // we must not return until we reached our final destination.
            // be the out-commeted code does not work. just wait for a short time for now.
            Task.Delay(500).Wait();
            
            return VSConstants.S_OK;
        }

        ///// <summary>
        ///// Returns a task object, that will be completed when the process is suspended
        ///// again. Will not be completed if the process is currently suspended.
        ///// </summary>
        ///// <returns></returns>
        //private Task<object> GetOnSuspendedTask()
        //{
        //    // This code looks kind of messy. Any ideas?
        //    TaskCompletionSource<object> task = new TaskCompletionSource<object>();
        //    EventHandler sup = (sender, e) => { if (!Debugger.Process.IsSuspended) task.SetResult(null); };
        //    Debugger.Process.IsSuspendedChanged += sup;
        //    task.Task.ContinueWith(t => Debugger.Process.IsSuspendedChanged -= sup);
        //    return task.Task;
        //}

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

        //private DalvikStackFrame GetUnwindStackFrame(DalvikThread thread, DebugCodeContext context)
        //{
        //    foreach (var stackFrame in thread.GetCallStack())
        //    {
        //        bool isSameMethodEarlierOrSame = stackFrame.Location.IsSameMethod(context.Location)
        //                                      && stackFrame.Location.Index >= context.Location.Index;
        //        if (isSameMethodEarlierOrSame)
        //            return stackFrame;
        //    }
        //    return null;
        //}
    }
}
