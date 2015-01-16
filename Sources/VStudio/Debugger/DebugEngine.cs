using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dot42.DebuggerLib.Model;
using Dot42.Ide.Debugger;
using Dot42.Mapping;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    [ComVisible(true)]
    [Guid(GuidList.Strings.guidDot42DebugEngineClsid)]
    [Description(Name)]
    //[Obfuscation] // COM needs a name
    public class DebugEngine : IDebugEngine3, IDebugEngineLaunch2
    {
        internal const string Name = "Dot42 Debug Engine";

        private string registryRoot;
        private EngineEventCallback eventCallback;
        private DebugProgram program;
        private Action<LauncherStates, string> stateUpdate;
        private bool destroying = false;
        private readonly ExceptionBehaviorMap exceptionBehaviorMap = new ExceptionBehaviorMap();

        /// <summary>
        /// Gets the currently running program.
        /// Can be null.
        /// </summary>
        internal DebugProgram Program { get { return program; } }

        /// <summary>
        /// Retrieves a list of all programs being debugged by a debug engine (DE).
        /// </summary>
        public int EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.EnumPrograms");
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Creates a pending breakpoint in the debug engine (DE).
        /// </summary>
        public int CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.CreatePendingBreakpoint");
            ppPendingBP = new DebugPendingBreakpoint(pBPRequest, this);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Specifies how the debug engine (DE) should handle a given exception.
        /// </summary>
        public int SetException(EXCEPTION_INFO[] pException)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.SetException");
            var copyToProgram = false;
            foreach (var info in pException)
            {
                if (info.guidType == GuidList.Guids.guidDot42DebuggerId)
                {
                    if (info.bstrExceptionName == ExceptionConstants.TopLevelName)
                    {
                        exceptionBehaviorMap.DefaultStopOnThrow = info.dwState.HasFlag(enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE);
                        exceptionBehaviorMap.DefaultStopUncaught = info.dwState.HasFlag(enum_EXCEPTION_STATE.EXCEPTION_STOP_USER_UNCAUGHT);
                        copyToProgram = true;
                    }
                    else
                    {
                        var behavior = new ExceptionBehavior(
                            info.bstrExceptionName,
                            info.dwState.HasFlag(enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE),
                            info.dwState.HasFlag(enum_EXCEPTION_STATE.EXCEPTION_STOP_USER_UNCAUGHT));
                        exceptionBehaviorMap[info.bstrExceptionName] = behavior;
                        copyToProgram = true;
                    }
                }
            }
            if (copyToProgram)
            {
                CopyExceptionMapToProgram();
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Removes the specified exception so it is no longer handled by the debug engine.
        /// </summary>
        public int RemoveSetException(EXCEPTION_INFO[] pException)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.RemoveSetException");
            var copyToProgram = false;
            foreach (var info in pException)
            {
                if (info.guidType == GuidList.Guids.guidDot42DebuggerId)
                {
                    if (info.bstrExceptionName == ExceptionConstants.TopLevelName)
                    {
                        exceptionBehaviorMap.ResetDefaults();
                        copyToProgram = true;
                    }
                    else
                    {
                        exceptionBehaviorMap[info.bstrExceptionName] = null;
                        copyToProgram = true;
                    }
                }
            }
            if (copyToProgram)
            {
                CopyExceptionMapToProgram();
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Removes the list of exceptions the IDE has set for a particular run-time architecture or language.
        /// </summary>
        public int RemoveAllSetExceptions(ref Guid guidType)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.RemoveAllSetExceptions");

            // Remove all custom exception behavior
            exceptionBehaviorMap.ResetAll();
            CopyExceptionMapToProgram();

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Copy the current state of the exception behavior map to the current program.
        /// </summary>
        private void CopyExceptionMapToProgram()
        {
            var currentProgram = program;
            if (currentProgram != null)
            {
                currentProgram.ExceptionManager.ExceptionBehaviorMap.CopyFrom(exceptionBehaviorMap);
            }            
        }

        /// <summary>
        /// Gets the GUID of the debug engine (DE)
        /// </summary>
        public int GetEngineId(out Guid pguidEngine)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.GetEngineId");
            pguidEngine = new Guid(GuidList.Strings.guidDot42DebuggerId);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Informs a debug engine (DE) that the program specified has been atypically terminated and that the DE should clean up all references 
        /// to the program and send a program destroy event.
        /// </summary>
        public int DestroyProgram(IDebugProgram2 pProgram)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.DestroyProgram");

            // Avoid recursion
            if (destroying)
                return VSConstants.S_OK;
            destroying = true;

            var program = pProgram as DebugProgram;
            if (program == null)
                return VSConstants.E_INVALIDARG;
            var port = (IDebugPortNotify2) program.Process.Port;
            port.RemoveProgramNode(program);

            if (this.program == program)
            {
                // Detach
                this.program = null;
            }

            //eventCallback.Send(program, new ProgramDestroyEvent(0));
            return VSConstants.S_OK;
        }

        public int ContinueFromSynchronousEvent(IDebugEvent2 pEvent)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.ContinueFromSynchronousEvent {0}", pEvent);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Sets the locale of the debug engine (DE).
        /// </summary>
        public int SetLocale(ushort wLangID)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.SetLocale");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Sets the registry root for the debug engine (DE).
        /// </summary>
        public int SetRegistryRoot(string pszRegistryRoot)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.SetRegistryRoot");
            registryRoot = pszRegistryRoot;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This method sets a registry value known as a metric
        /// </summary>
        public int SetMetric(string pszMetric, object varValue)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.SetMetric");
            return VSConstants.S_OK;
        }

        public int CauseBreak()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.CauseBreak");
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine3.SetSymbolPath(string szSymbolSearchPath, string szSymbolCachePath, uint Flags)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugEngine.SetSymbolPath");
            return VSConstants.S_OK;
        }

        int IDebugEngine3.LoadSymbols()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugEngine.SetSymbolPath");
            return VSConstants.S_OK;
        }

        int IDebugEngine3.SetJustMyCodeState(int fUpdate, uint dwModules, JMC_CODE_SPEC[] rgJMCSpec)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugEngine.SetJustMyCodeState");
            return VSConstants.S_OK;
        }

        int IDebugEngine3.SetEngineGuid(ref Guid guidEngine)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugEngine.SetEngineGuid");
            return VSConstants.S_OK;
        }

        int IDebugEngine3.SetAllExceptions(enum_EXCEPTION_STATE dwState)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugEngine.SetAllExceptions");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Launch the actual debug process.
        /// </summary>
        public int LaunchSuspended(string pszServer, IDebugPort2 pPort, string pszExe, string pszArgs, string pszDir, string bstrEnv, string pszOptions, enum_LAUNCH_FLAGS dwLaunchFlags, uint hStdInput, uint hStdOutput, uint hStdError, IDebugEventCallback2 pCallback, out IDebugProcess2 ppProcess)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.LaunchSuspended");

            ppProcess = null;
            var port = pPort as DebugPort;
            if (pPort == null)
                return VSConstants.E_INVALIDARG;

            // Create event callback
            eventCallback = new EngineEventCallback(this, pCallback);

            // Notify creation
            eventCallback.Send(new EngineCreateEvent(this));

            // Get debugger
            var guid = new Guid(pszOptions);
            var debugger = Launcher.GetAndRemoveDebugger(guid, out stateUpdate);

            // Load map file
            var mapFilePath = Path.ChangeExtension(pszExe, ".d42map");
            var mapFile = File.Exists(mapFilePath) ? new MapFile(mapFilePath) : new MapFile();
            
            // Create new process
            var process = new DebugProcess(this, port, debugger, Environment.TickCount, guid, pszExe, mapFile, eventCallback);
            var program = process.Program;
            process.Terminated += (s, x) => ((IDebugEngine2)this).DestroyProgram(program);

            // Record process
            ((DebugPort)pPort).RecordProcess(process);
            // Return result
            ppProcess = process;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Resumes process execution.
        /// </summary>
        public int ResumeProcess(IDebugProcess2 pProcess)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.ResumeProcess");
            var process = pProcess as DebugProcess;
            if (process == null)
                return VSConstants.E_INVALIDARG;

            try
            {
                var program = process.Program;
                ((IDebugPortNotify2) process.Port).AddProgramNode(program);

                // Suspend and prepare the VM
                var debugger = process.Debugger;
                var suspend = process.Debugger.VirtualMachine.SuspendAsync();
                var prepare = suspend.ContinueWith(t => {
                    t.ForwardException();
                    return debugger.PrepareAsync();
                }).Unwrap();
                var loadThreads = prepare.ContinueWith(t => {
                    t.ForwardException();
                    return program.ThreadManager.RefreshAsync();
                }).Unwrap();
                loadThreads.ContinueWith(t => {
                    t.ForwardException();

                    // Notify module
                    eventCallback.Send(program, new ModuleLoadEvent(program.MainModule, "Loading module", true));
                    eventCallback.Send(program, new SymbolSearchEvent(program.MainModule, "Symbols loaded", enum_MODULE_INFO_FLAGS.MIF_SYMBOLS_LOADED));

                    var mainThread = program.ThreadManager.MainThread();
                    if (mainThread != null)
                    {
                        // Threads loaded
                        // Load complete
                        eventCallback.Send(mainThread, new LoadCompleteEvent());
                        eventCallback.Send(mainThread, new EntryPointEvent());

                        // Resume now
                        debugger.VirtualMachine.ResumeAsync();

                        // We're fully attached now
                        if (stateUpdate != null) stateUpdate(LauncherStates.Attached, string.Empty);
                        stateUpdate = null;
                    }
                    else
                    {
                        DLog.Error(DContext.VSDebuggerLauncher, "No main thread found");
                    }
                });

                return VSConstants.S_OK;
            }
            catch (Exception ex)
            {
                DLog.Error(DContext.VSDebuggerLauncher, "ResumeProcess failed", ex);
                return VSConstants.E_FAIL;
            }
        }

        /// <summary>
        /// Attaches a debug engine (DE) to a program or programs. Called by the session debug manager (SDM) when the DE is running in-process to the SDM.
        /// </summary>
        public int Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 pCallback, enum_ATTACH_REASON dwReason)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.Attach");

            // Save program
            program = rgpPrograms[0] as DebugProgram;
            if (program == null)
                return VSConstants.E_INVALIDARG;

            // Update program state
            CopyExceptionMapToProgram();

            //eventCallback.Send(process, new ProcessCreateEvent());
            eventCallback.Send(program, new ProgramCreateEvent());
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines if a process can be terminated.
        /// </summary>
        public int CanTerminateProcess(IDebugProcess2 pProcess)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.CanTerminateProcess");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Terminates a process.
        /// </summary>
        public int TerminateProcess(IDebugProcess2 pProcess)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugEngine2.TerminateProcess");
            return pProcess.Terminate();
        }
    }
}
