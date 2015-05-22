using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        /// <summary>
        /// 
        /// </summary>
        public int SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            // TODO: move this code to DalvikThread, or to a SetNextInstructionManager
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugThread2.SetNextStatement");
            
            var stack = (DebugStackFrame)pStackFrame;
            var ctx = (DebugCodeContext)pCodeContext;

            // nothing to do.
            if (ctx.Location.Equals(stack.Location))
                return VSConstants.S_OK;

            if (!ctx.Location.IsSameMethod(stack.Location))
                return HResults.E_CANNOT_SETIP_TO_DIFFERENT_FUNCTION;

            var loc = stack.GetDocumentLocationAsync().Await(DalvikProcess.VmTimeout);
            if (loc.MethodEntry == null)
            {
                DLog.Info(DContext.VSStatusBar, "Can not set next instruction: Debug info not available."); 
                return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;
            }  

            var nextInstrVar = loc.MethodEntry.Variables.FirstOrDefault(v => v.Name == DebuggerConstants.SetNextInstructionVariableName);
            if (nextInstrVar == null)
            {
                DLog.Info(DContext.VSStatusBar, "Can not set next instruction: missing compiler setting or method optimized.");
                return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;
            }

            // make sure there are no branch instructions 
            // between the current instruction and our branch instruction.
            // note that for convinence, we *do* allow assignments to
            // fields of objects, even though these are visible to the
            // program.
            var disassembly = Program.DisassemblyProvider.GetFromLocation(loc);
            if (disassembly == null)
                return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;

            var body = disassembly.Method.Body;
            int idx = body.Instructions.FindIndex(i => (ulong)i.Offset == loc.Location.Index);
            if(idx == -1)
                return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;

            bool foundSetNextInstruction = false;

            for (;idx < body.Instructions.Count; ++idx)
            {
                var ins = body.Instructions[idx];
                foundSetNextInstruction = ins.OpCode == OpCodes.If_nez && ins.Registers.Count == 1 
                                       && ins.Registers[0].Index == nextInstrVar.Register;

                if (foundSetNextInstruction)
                    break;

                if (ins.OpCode.IsJump())
                    break;
            }

            if (!foundSetNextInstruction)
            {
                DLog.Info(DContext.VSStatusBar, "Can not set next instruction from current position. Try again at a later position if any.");
                return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;
            }

            DLog.Info(DContext.VSStatusBar, "Setting next instruction to beginning of block.");

            // find target instruction.
            var targetIns = (Instruction)body.Instructions[idx].Operand;
            idx = body.Instructions.FindIndex(p => p.Offset == targetIns.Offset);
            idx = FindNextLocationWithSource(disassembly, idx) ?? idx;
            targetIns = body.Instructions[idx];
            var targetLoc = loc.Location.GetAtIndex(targetIns.Offset);

            // set a temporary breakpoint. The reset logic could get into a "DalvikTemporaryBreakpoint" class.
            var bp = new DalvikAwaitableBreakpoint(targetLoc);
            var waitBp = bp.WaitUntilHit();
            var waitBound = Debugger.Process.BreakpointManager.SetBreakpoint(bp);

            try
            {
                if (!waitBound.Await(DalvikProcess.VmTimeout))
                    return HResults.E_CANNOT_SET_NEXT_STATEMENT_GENERAL;

                // set the special variable.
                var newSlotVal = new SlotValue(nextInstrVar.Register, Jdwp.Tag.Int, 1);
                Debugger.StackFrame.SetValuesAsync(stack.Thread.Id, stack.Id, newSlotVal)
                                   .Await(DalvikProcess.VmTimeout);

                // resume the process.
                Debugger.Process.ResumeAsync();

                // wait for breakpoint to be hit.
                try
                {
                    waitBp.Await(1000);
                }
                catch (Exception)
                {
                    // ups. something went wrong. suspend again.
                    if (!Debugger.Process.IsSuspended)
                        Debugger.Process.SuspendAsync();
                    return VSConstants.E_FAIL;
                }

                return VSConstants.S_OK;
            }
            finally 
            {
                // clear the breakpoint again.
                Debugger.Process.BreakpointManager.ResetAsync(bp)
                                                  .Await(DalvikProcess.VmTimeout);
                // reset the special variable, in case this was not performed automatically.
                // (should not happen, but maybe the set value code got optimized away per
                //  accident)
                var newSlotVal = new SlotValue(nextInstrVar.Register, Jdwp.Tag.Int, 0);
                Debugger.StackFrame.SetValuesAsync(stack.Thread.Id, stack.Id, newSlotVal)
                                   .Await(DalvikProcess.VmTimeout);
            }
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

        /// <summary>
        /// Finds the next location with source starting from <paramref name="idx"/>.
        /// Will return null if no source is found. Will follow a single goto. 
        /// Will return null if multiple gotos or if any other jump instruction is 
        /// encountered.
        /// </summary>
        private int? FindNextLocationWithSource(MethodDisassembly disassembly, int idx)
        {
            var instructions = disassembly.Method.Body.Instructions;

            var ins = instructions[idx];

            // find the next instruction with source code.
            var loc = disassembly.FindNextSourceCode(ins.Offset);

            if (loc == null) 
                return null;

            int gotos = 0;

            for (; (ins = instructions[idx]).Offset < loc.Position.MethodOffset; ++idx)
            {
                // While working as expected, the following code as no effect on 
                // foreach statements, since the target of the goto is a "IsSpecial"
                // branch instruction back to the very same goto.

                if (ins.OpCode == OpCodes.Goto || ins.OpCode == OpCodes.Goto_16 || ins.OpCode == OpCodes.Goto_32)
                {
                    // follow a single goto. this is typically encountered 
                    // at the beginning of the loop of foreach statements.
                    if (gotos++ > 0)
                        return null;
                    ins = (Instruction)ins.Operand;
                    loc = disassembly.FindNextSourceCode(ins.Offset);
                    if (loc == null)
                        return null;
                    idx = instructions.IndexOf(ins) - 1;
                    continue;
                }

                if (ins.OpCode.IsJump())
                    return null;
            }

            return idx;
        }

    }
}
