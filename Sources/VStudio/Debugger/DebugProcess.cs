using System;
using System.IO;
using Dot42.Mapping;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    internal sealed class DebugProcess : IDebugProcess2, IDebugProcess3, IDebugProcessEx2
    {
        private readonly DebugEngine engine;
        private readonly DebugPort port;
        private readonly DebuggerLib.Debugger debugger;
        private readonly int processId;
        private readonly string apkPath;
        private readonly DebugProgram program;
        private readonly EngineEventCallback eventCallback;
        private readonly DateTime creationDate;
        private Guid languageGuid;
        private readonly Guid guid;

        /// <summary>
        /// Fired when the underlying program has been terminated.
        /// </summary>
        public event EventHandler Terminated;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DebugProcess(DebugEngine engine, DebugPort port, DebuggerLib.Debugger debugger, int processId, Guid guid, string apkPath, MapFile mapFile, EngineEventCallback eventCallback)
        {
            this.engine = engine;
            this.port = port;
            this.debugger = debugger;
            this.processId = processId;
            this.guid = guid;
            this.apkPath = apkPath;
            this.eventCallback = eventCallback;
            creationDate = DateTime.Now;
            program = new DebugProgram(this, debugger, apkPath, mapFile, eventCallback);
            program.Terminated += OnProgramTerminated;
        }

        /// <summary>
        /// Gets a reference to the containing engine
        /// </summary>
        internal DebugEngine Engine { get { return engine; } }

        /// <summary>
        /// Gets the containing port.
        /// </summary>
        internal DebugPort Port { get { return port; } }

        /// <summary>
        /// Gets my program
        /// </summary>
        internal DebugProgram Program { get { return program; } }

        /// <summary>
        /// Gets the id of this process.
        /// </summary>
        internal int ProcessId { get { return processId; } }

        /// <summary>
        /// Gets the GUID of this process.
        /// </summary>
        internal Guid ProcessGuid { get { return guid; } }

        /// <summary>
        /// Gets the low level debug connection
        /// </summary>
        public DebuggerLib.Debugger Debugger
        {
            get { return debugger; }
        }

        /// <summary>
        /// Gets a description of the process.
        /// </summary>
        public int GetInfo(enum_PROCESS_INFO_FIELDS fields, PROCESS_INFO[] pProcessInfo)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetInfo {0}", fields);

            var foundFields = (enum_PROCESS_INFO_FIELDS)0;
            var info = new PROCESS_INFO();
            
            if ((fields & enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME) != 0)
            {
                info.bstrFileName = apkPath;
                foundFields |= enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME;
            }
            if ((fields & enum_PROCESS_INFO_FIELDS.PIF_BASE_NAME) != 0)
            {
                info.bstrBaseName = Path.GetFileName(apkPath);
                foundFields |= enum_PROCESS_INFO_FIELDS.PIF_BASE_NAME;
            }
            if ((fields & enum_PROCESS_INFO_FIELDS.PIF_TITLE) != 0)
            {
                info.bstrTitle = Path.GetFileName(apkPath);
                foundFields |= enum_PROCESS_INFO_FIELDS.PIF_TITLE;
            }
            if ((fields & enum_PROCESS_INFO_FIELDS.PIF_PROCESS_ID) != 0)
            {
                var arr = new AD_PROCESS_ID[0];
                GetPhysicalProcessId(arr);
                info.ProcessId = arr[0];
                foundFields |= enum_PROCESS_INFO_FIELDS.PIF_PROCESS_ID;
            }
            if ((fields & enum_PROCESS_INFO_FIELDS.PIF_SESSION_ID) != 0)
            {
                info.dwSessionId = 1;
                foundFields |= enum_PROCESS_INFO_FIELDS.PIF_SESSION_ID;
            }
            if ((fields & enum_PROCESS_INFO_FIELDS.PIF_ATTACHED_SESSION_NAME) != 0)
            {
                info.bstrAttachedSessionName = "<deprecated>";
                foundFields |= enum_PROCESS_INFO_FIELDS.PIF_ATTACHED_SESSION_NAME;
            }
            if ((fields & enum_PROCESS_INFO_FIELDS.PIF_CREATION_TIME) != 0)
            {
                var fileTime = DateTime.Now.ToFileTime();
                info.CreationTime.dwLowDateTime = (uint) (fileTime & 0xFFFFFFFF);
                info.CreationTime.dwHighDateTime = (uint) (fileTime >> 32);
                foundFields |= enum_PROCESS_INFO_FIELDS.PIF_CREATION_TIME;
            }
            if ((fields & enum_PROCESS_INFO_FIELDS.PIF_FLAGS) != 0)
            {
                pProcessInfo[0].Flags = GetProcessStatus();
                foundFields |= enum_PROCESS_INFO_FIELDS.PIF_FLAGS;
            }
            pProcessInfo[0].Fields = foundFields;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the status of this process.
        /// </summary>
        private enum_PROCESS_INFO_FLAGS GetProcessStatus()
        {
            //return enum_PROCESS_INFO_FLAGS.PIFLAG_PROCESS_RUNNING | enum_PROCESS_INFO_FLAGS.PIFLAG_DEBUGGER_ATTACHED;
            return enum_PROCESS_INFO_FLAGS.PIFLAG_DEBUGGER_ATTACHED;
        }

        /// <summary>
        /// Enumerate all programs
        /// </summary>
        public int EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.EnumPrograms");
            ppEnum = new ProgramEnum(new[] { program });
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the title, friendly name, or file name of the process.
        /// </summary>
        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrName)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetName");
            switch (gnType)
            {
                case enum_GETNAME_TYPE.GN_FILENAME:
                case enum_GETNAME_TYPE.GN_MONIKERNAME:
                    pbstrName = apkPath;
                    break;
                case enum_GETNAME_TYPE.GN_NAME:
                case enum_GETNAME_TYPE.GN_BASENAME:
                case enum_GETNAME_TYPE.GN_TITLE:
                    pbstrName = Path.GetFileName(apkPath);
                    break;
                case enum_GETNAME_TYPE.GN_URL:
                    pbstrName = "<url>";
                    break;
                case enum_GETNAME_TYPE.GN_STARTPAGEURL:
                    pbstrName = "<startpageurl>";
                    break;
                default:
                    pbstrName = null;
                    return VSConstants.S_FALSE;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the server that this process is running on.
        /// </summary>
        public int GetServer(out IDebugCoreServer2 ppServer)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetServer");
            ppServer = null;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Terminate the process.
        /// </summary>
        public int Terminate()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.Terminate");
            return ((IDebugProgram2)program).Terminate();
        }

        /// <summary>
        /// Underlying program has been terminated.
        /// </summary>
        private void OnProgramTerminated(object sender, EventArgs eventArgs)
        {
            // Notify process destroy
            //eventCallback.Send(this, new ProcessDestroyEvent());
            // Notify other listeners
            Terminated.Fire(this);
        }

        /// <summary>
        /// Attaches the session debug manager (SDM) to the process.
        /// </summary>
        public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.Attach");
            return HResults.E_ATTACH_DEBUGGER_ALREADY_ATTACHED;
        }

        /// <summary>
        /// Determines if the session debug manager (SDM) can detach the process.
        /// </summary>
        public int CanDetach()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.CanDetach");
            return VSConstants.S_FALSE;
        }

        /// <summary>
        /// Detaches the debugger from this process by detaching all of the programs in the process.
        /// </summary>
        public int Detach()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.Detach");
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Gets the system process identifier.
        /// </summary>
        public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetPhysicalProcessId");
            pProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;
            //pProcessId[0].dwProcessId = (uint) processId;
            pProcessId[0].dwProcessId = 0;
            pProcessId[0].guidProcessId = guid;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the GUID for this process.
        /// </summary>
        public int GetProcessId(out Guid pguidProcessId)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetProcessId");
            pguidProcessId = guid;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the name of the session that is debugging this process. An IDE can display this information to a user who is debugging a particular process on a particular machine.
        /// </summary>
        public int GetAttachedSessionName(out string pbstrSessionName)
        {
            // This method is deprecated, and its implementation should always return E_NOTIMPL.
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetAttachedSessionName");
            pbstrSessionName = null;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Enumerate all threads
        /// </summary>
        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.EnumThreads");
            return ((IDebugProgram2)program).EnumThreads(out ppEnum);
        }

        /// <summary>
        /// Cause a break in the program
        /// </summary>
        public int CauseBreak()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.CauseBreak");
            return ((IDebugProgram2)program).CauseBreak();
        }

        /// <summary>
        /// Gets the originating port.
        /// </summary>
        public int GetPort(out IDebugPort2 ppPort)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetPort");
            ppPort = port;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Begins execution of a process.
        /// </summary>
        int IDebugProcess3.Execute(IDebugThread2 pThread)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.Execute");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Continues execution of or stepping through a process.
        /// </summary>
        int IDebugProcess3.Continue(IDebugThread2 pThread)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.Continue");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Steps forward one instruction or statement in the process.
        /// </summary>
        int IDebugProcess3.Step(IDebugThread2 pThread, enum_STEPKIND stepKind, enum_STEPUNIT stepUnit)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess3.Step");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the reason that the process was launched for debugging.
        /// </summary>
        int IDebugProcess3.GetDebugReason(enum_DEBUG_REASON[] pReason)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetDebugReason");
            pReason[0] = enum_DEBUG_REASON.DEBUG_REASON_USER_LAUNCHED;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Sets the hosting language so that the debug engine can load the appropriate expression evaluator.
        /// </summary>
        int IDebugProcess3.SetHostingProcessLanguage(ref Guid guidLang)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.SetHostingProcessLanguage");
            languageGuid = guidLang;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Retrieves the language currently set for this process.
        /// </summary>
        int IDebugProcess3.GetHostingProcessLanguage(out Guid pguidLang)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetHostingProcessLanguage");
            pguidLang = languageGuid;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Disables Edit and Continue (ENC) for this process.
        /// </summary>
        int IDebugProcess3.DisableENC(EncUnavailableReason reason)
        {
            // A custom port supplier does not implement this method (it should always return E_NOTIMPL).
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.DisableENC");
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Get the ENC state for this process.
        /// </summary>
        int IDebugProcess3.GetENCAvailableState(EncUnavailableReason[] pReason)
        {
            // A custom port supplier does not implement this method (it should always return E_NOTIMPL).
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetENCAvaialableState");
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Retrieves an array of unique identifiers for available debug engines.
        /// </summary>
        int IDebugProcess3.GetEngineFilter(GUID_ARRAY[] pEngineArray)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugProcess2.GetEngineFilter");
            return VSConstants.E_NOTIMPL;
        }

        public int Attach(IDebugSession2 pSession)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugProcess.Attach");
            return VSConstants.S_OK;
        }

        public int Detach(IDebugSession2 pSession)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugProcess.Detach");
            return VSConstants.S_OK;
        }

        public int AddImplicitProgramNodes(ref Guid guidLaunchingEngine, Guid[] rgguidSpecificEngines, uint celtSpecificEngines)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugProcess.AddImplicitProgramNodes");
            return VSConstants.S_OK;
        }
    }
}
