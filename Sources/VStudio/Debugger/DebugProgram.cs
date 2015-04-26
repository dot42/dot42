using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dot42.ApkLib;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.DexLib;
using Dot42.Mapping;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    internal sealed class DebugProgram : DalvikProcess, IDebugProgram3, IDebugProgramNode2, IDebugEngineProgram2, IDebugProgramNodeAttach2
    {
        private readonly DebugProcess process;
        private readonly string apkPath;
        private readonly EngineEventCallback eventCallback;
        private readonly Guid programGuid;
        private readonly List<DebugModule> modules = new List<DebugModule>();
        private readonly Lazy<Dex> dex;


        /// <summary>
        /// Fired when this program has been terminated.
        /// </summary>
        public event EventHandler Terminated;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DebugProgram(DebugProcess process, DebuggerLib.Debugger debugger, string apkPath, MapFile mapFile, EngineEventCallback eventCallback)
            : base(debugger, mapFile, apkPath)
        {
            this.process = process;
            this.apkPath = apkPath;
            this.eventCallback = eventCallback;
            programGuid = Guid.NewGuid();
            modules.Add(new DebugModule());
            dex = new Lazy<Dex>(LoadDex);
        }

        /// <summary>
        /// Gets the containing process
        /// </summary>
        internal DebugProcess Process { get { return process; } }

        /// <summary>
        /// Gets my thread manager
        /// </summary>
        internal new ThreadManager ThreadManager { get { return (ThreadManager) base.ThreadManager; } }

        /// <summary>
        /// Create our thread manager.
        /// </summary>
        protected override DalvikThreadManager CreateThreadManager()
        {
            return new ThreadManager(this, eventCallback);
        }

        /// <summary>
        /// Maintains information about registered breakpoints
        /// </summary>
        internal new DebugBreakpointManager BreakpointManager { get { return (DebugBreakpointManager)base.BreakpointManager; } }

        /// <summary>
        /// Create our breakpoint manager.
        /// </summary>
        protected override DalvikBreakpointManager CreateBreakpointManager()
        {
            return new DebugBreakpointManager(this, eventCallback);
        }

        /// <summary>
        /// Gets my exception manager
        /// </summary>
        internal new DebugExceptionManager ExceptionManager { get { return (DebugExceptionManager) base.ExceptionManager; } }

        /// <summary>
        /// Create our exception manager.
        /// </summary>
        protected override DalvikExceptionManager CreateExceptionManager()
        {
            return new DebugExceptionManager(this, eventCallback);
        }

        /// <summary>
        /// Debugger has disconnected.
        /// </summary>
        protected override void OnConnectionLost()
        {
            base.OnConnectionLost();
            Terminated.Fire(this);
        }

        /// <summary>
        /// Gets the main module
        /// </summary>
        internal DebugModule MainModule { get { return modules.First(); } }

        /// <summary>
        /// Enumerate all threads.
        /// </summary>
        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.EnumThreads");
            ppEnum = new ThreadEnum(ThreadManager.Threads.Cast<IDebugThread2>().ToArray());
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the name of this program
        /// </summary>
        public int GetName(out string pbstrName)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.GetName");
            pbstrName = apkPath;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the containing process.
        /// </summary>
        public int GetProcess(out IDebugProcess2 ppProcess)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.GetProcess");
            ppProcess = process;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Terminate the program.
        /// </summary>
        public int Terminate()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.Terminate");

            // Stop VM
            const int exitCode = 0;
            ExitAndDisconnect(exitCode);

            // Notify listeners
            Terminated.Fire(this);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Attaches to the program.
        /// </summary>
        public int Attach(IDebugEventCallback2 pCallback)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.Attach");
            return HResults.E_ATTACH_DEBUGGER_ALREADY_ATTACHED;
        }

        /// <summary>
        /// Determines if a debug engine (DE) can detach from the program.
        /// </summary>
        public int CanDetach()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.CanDetach");
            return VSConstants.S_FALSE;
        }

        /// <summary>
        /// Detaches a debug engine from the program.
        /// </summary>
        public int Detach()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.Detach");
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Gets a GUID for this program.
        /// </summary>
        public int GetProgramId(out Guid pguidProgramId)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.GetProgramId");
            pguidProgramId = programGuid;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the program's properties.
        /// </summary>
        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.GetDebugProperty");
            ppProperty = null;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Continues running this program from a stopped state. Any previous execution state (such as a step) is cleared, and the program starts executing again.
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            // This method is deprecated. Use the IDebugProcess3::Execute method instead.
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.Execute");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Continues running this program from a stopped state. Any previous execution state (such as a step) is preserved, and the program starts executing again.
        /// </summary>
        public int Continue(IDebugThread2 pThread)
        {
            // This method is deprecated. Use the IDebugProcess3::Continue method instead.
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.Continue");
            Debugger.VirtualMachine.ResumeAsync();
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Performs a step.
        /// </summary>
        public int Step(IDebugThread2 pThread, enum_STEPKIND stepKind, enum_STEPUNIT stepUnit)
        {
            // This method is deprecated. Use the IDebugProcess3::Step method instead.
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.Step kind={0}, step={1}", stepKind, stepUnit);
            Jdwp.StepDepth stepDepth;
            switch (stepKind)
            {
                case enum_STEPKIND.STEP_INTO:
                    stepDepth = Jdwp.StepDepth.Into;
                    break;
                case enum_STEPKIND.STEP_BACKWARDS:
                    return VSConstants.E_NOTIMPL;
                case enum_STEPKIND.STEP_OVER:
                    stepDepth = Jdwp.StepDepth.Over;
                    break;
                case enum_STEPKIND.STEP_OUT:
                    stepDepth = Jdwp.StepDepth.Out;
                    break;
                default:
                    return VSConstants.E_INVALIDARG;
            }
            StepAsync(new StepRequest((DalvikThread) pThread, stepDepth));
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Stop executing
        /// </summary>
        public int CauseBreak()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.CauseBreak");
            SuspendAsync();
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notify listeners that we're suspended.
        /// </summary>
        protected override bool OnSuspended(SuspendReason reason, DalvikThread thread)
        {
            var rc = base.OnSuspended(reason, thread);
            if (rc)
            {
                if (reason == SuspendReason.ProcessSuspend)
                {
                    foreach (var threadx in ThreadManager.Threads)
                    {
                        eventCallback.Send(threadx, new ASyncBreakCompleteEvent());
                    }
                }
                if (reason == SuspendReason.SingleStep)
                {
                    eventCallback.Send((DebugThread) thread, new StepCompleteEvent());
                }
            }
            return rc;
        }

        /// <summary>
        /// Gets the name of the program.
        /// </summary>
        int IDebugProgramNode2.GetProgramName(out string pbstrProgramName)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgramNode2.GetProgramName");
            pbstrProgramName = Path.GetFileName(apkPath);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the name of the process hosting the program.
        /// </summary>
        public int GetHostName(enum_GETHOSTNAME_TYPE dwHostNameType, out string pbstrHostName)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgramNode2.GetHostName");
            switch (dwHostNameType)
            {
                case enum_GETHOSTNAME_TYPE.GHN_FILE_NAME:
                    pbstrHostName = apkPath;
                    break;
                case enum_GETHOSTNAME_TYPE.GHN_FRIENDLY_NAME:
                    pbstrHostName = Path.GetFileName(apkPath);
                    break;
                default:
                    pbstrHostName = null;
                    return VSConstants.E_INVALIDARG;
            }
            return VSConstants.S_OK;
        }

        public int GetHostId(AD_PROCESS_ID[] pProcessId)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugProgram.GetHostId");
            return process.GetPhysicalProcessId(pProcessId);
        }

        public int GetHostMachineName(out string pbstrHostMachineName)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugProgram.GetHostMachineName");
            pbstrHostMachineName = "localhost";
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the system process identifier for the process hosting the program.
        /// </summary>
        int IDebugProgramNode2.GetHostPid(AD_PROCESS_ID[] pHostProcessId)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgramNode2.GetHostPid");
            return process.GetPhysicalProcessId(pHostProcessId);
        }

        /// <summary>
        /// DEPRECATED. DO NOT USE.
        /// </summary>
        int IDebugProgramNode2.GetHostMachineName_V7(out string pbstrHostMachineName)
        {
            pbstrHostMachineName = null;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// DEPRECATED. DO NOT USE.
        /// </summary>
        int IDebugProgramNode2.Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Gets the name and identifier of the debug engine (DE) running a program.
        /// </summary>
        public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.GetEngineInfo");
            pbstrEngine = DebugEngine.Name;
            pguidEngine = new Guid(GuidList.Strings.guidDot42DebuggerId);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// DEPRECATED. DO NOT USE.
        /// </summary>
        int IDebugProgramNode2.DetachDebugger_V7()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.EnumCodeContexts");
            throw new NotImplementedException();
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.GetMemoryBytes");
            throw new NotImplementedException();
        }

        public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.GetDisassemblyStream");
            ppDisassemblyStream = new DebugDisassemblyStream(this, ((DebugCodeContext)pCodeContext), dex.Value);
            return VSConstants.S_OK;
        }

        public int EnumModules(out IEnumDebugModules2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.EnumModules");
            ppEnum = new ModuleEnum(MainModule);
            return VSConstants.S_OK;
        }

        public int GetENCUpdate(out object ppUpdate)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.GetENCUpdate");
            ppUpdate = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.EnumCodePaths");
            throw new NotImplementedException();
        }

        public int WriteDump(enum_DUMPTYPE dumptype, string pszDumpUrl)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.WriteDump");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the program. The thread is returned to give the debugger information on which thread the user is viewing when executing.
        /// </summary>
        public int ExecuteOnThread(IDebugThread2 pThread)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProgram2.ExecuteOnThread");
            // Resume VM
            ResumeAsync();
            return VSConstants.S_OK;
        }

        public int Stop()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugProgram.Stop");
            throw new NotImplementedException();
        }

        public int WatchForThreadStep(IDebugProgram2 pOriginatingProgram, uint dwTid, int fWatch, uint dwFrame)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugProgram.WatchForThreadStep");
            return VSConstants.S_OK;
        }

        public int WatchForExpressionEvaluationOnThread(IDebugProgram2 pOriginatingProgram, uint dwTid, uint dwEvalFlags, IDebugEventCallback2 pExprCallback, int fWatch)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugProgram.WatchForExpressionEvaluationOnThread");
            throw new NotImplementedException();
        }

        public int OnAttach(ref Guid guidProgramId)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugProgram.OnAttach");
            throw new NotImplementedException();
        }

        private Dex LoadDex()
        {
            var apk = new ApkFile(apkPath);
            var dex = apk.Load("classes.dex");
            return Dex.Read(new MemoryStream(dex));
        }
    }
}
