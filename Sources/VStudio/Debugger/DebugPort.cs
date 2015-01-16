using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    public sealed class DebugPort : IDebugDefaultPort2, IDebugPortNotify2, IConnectionPoint, IConnectionPointContainer
    {
        private readonly IDebugPortRequest2 request;
        private readonly DebugPortSupplier supplier;
        private readonly Guid guid;
        private readonly List<DebugProcess> processes = new List<DebugProcess>();
        private readonly EventSinkCollection eventSinks = new EventSinkCollection();

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DebugPort(DebugPortSupplier supplier, IDebugPortRequest2 request)
        {
            this.supplier = supplier;
            this.request = request;
            guid = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the guid of this port
        /// </summary>
        internal Guid Guid { get { return guid; } }

        /// <summary>
        /// Gets the port name.
        /// </summary>
        public int GetPortName(out string pbstrName)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPort.GetPortName");
            return request.GetPortName(out pbstrName);
        }

        /// <summary>
        /// Gets the port identifier.
        /// </summary>
        public int GetPortId(out Guid pguidPort)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPort.GetPortId");
            pguidPort = guid;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the description of a port that was previously used to create the port (if available).
        /// </summary>
        public int GetPortRequest(out IDebugPortRequest2 ppRequest)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPort.GetPortRequest");
            ppRequest = request;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the port supplier for this port.
        /// </summary>
        public int GetPortSupplier(out IDebugPortSupplier2 ppSupplier)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPort.GetPortSupplier");
            ppSupplier = supplier;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the specified process running on a port.
        /// </summary>
        public int GetProcess(AD_PROCESS_ID ProcessId, out IDebugProcess2 ppProcess)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPort.GetProcess");
            var process = (ProcessId.ProcessIdType == (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM) ?
                processes.FirstOrDefault(x => x.ProcessId == ProcessId.dwProcessId) :
                processes.FirstOrDefault(x => x.ProcessGuid == ProcessId.guidProcessId);
            ppProcess = process;
            return (process != null) ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        /// <summary>
        /// Returns a list of all the processes running on a port.
        /// </summary>
        public int EnumProcesses(out IEnumDebugProcesses2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPort.EnumProcesses");
            ppEnum = new ProcessEnum(processes.Cast<IDebugProcess2>().ToArray());
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This method gets an IDebugPortNotify2 interface for this port.
        /// </summary>
        public int GetPortNotify(out IDebugPortNotify2 ppPortNotify)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPort.GetPortNotify");
            ppPortNotify = this;
            return VSConstants.S_OK;
        }

        public int GetServer(out IDebugCoreServer3 ppServer)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPort.GetPortNotify");
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method determines whether this port is on the local machine.
        /// </summary>
        public int QueryIsLocal()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPort.QueryIsLocal");
            return VSConstants.S_OK; // Yes, this is local
        }

        /// <summary>
        /// Record a process launch using this port.
        /// </summary>
        internal void RecordProcess(DebugProcess process)
        {
            processes.Add(process);
        }

        /// <summary>
        /// Registers a program that can be debugged with the port it is running on.
        /// </summary>
        int IDebugPortNotify2.AddProgramNode(IDebugProgramNode2 pProgramNode)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugPortNotify2.AddProgramNode");
            IDebugProcess2 proc;
            var pid = new AD_PROCESS_ID[1];
            ErrorHandler.ThrowOnFailure(pProgramNode.GetHostPid(pid));
            ErrorHandler.ThrowOnFailure(((IDebugPort2)this).GetProcess(pid[0], out proc));

            // Our implementation conflates ProgramNode and Program,
            // perhaps erroneously.
            var program = (IDebugProgram2)pProgramNode;

            SendEvent(null, this, proc, null, new ProcessCreateEvent());
            SendEvent(null, this, proc, program, new ProgramCreateEvent());
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Unregisters a program that can be debugged from the port it is running on.
        /// </summary>
        int IDebugPortNotify2.RemoveProgramNode(IDebugProgramNode2 pProgramNode)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "IDebugPortNotify2.RemoveProgramNode");
            var program = pProgramNode as DebugProgram;
            if (program == null)
                return VSConstants.E_INVALIDARG;
            SendEvent(null, this, program.Process, program, new ProgramDestroyEvent(0));
            SendEvent(null, this, program.Process, null, new ProcessDestroyEvent());
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Send the given event to all event sinks
        /// </summary>
        private void SendEvent(IDebugCoreServer2 server, IDebugPort2 port, IDebugProcess2 process, IDebugProgram2 program, BaseEvent @event)
        {
            var iid = @event.IID;
            DLog.Debug(DContext.VSDebuggerEvent, "DebugPort Event {0} {1}", @event.GetType().Name, iid);
            foreach (var eventSink in eventSinks)
            {
                var events = eventSink as IDebugPortEvents2;
                if (events != null)
                {
                    var rc = events.Event(server, port, process, program, @event, ref iid);
                    if (!ErrorHandler.Succeeded(rc))
                    {
                        DLog.Error(DContext.VSDebuggerEvent, "DebugPort Event failed {0}", rc);
                    }
                }
            }
        }

        void IConnectionPoint.GetConnectionInterface(out Guid pIID)
        {
            pIID = typeof (IDebugPortEvents2).GUID;
        }

        void IConnectionPoint.GetConnectionPointContainer(out IConnectionPointContainer ppCPC)
        {
            ppCPC = this;
        }

        void IConnectionPoint.Advise(object pUnkSink, out uint pdwCookie)
        {
            pdwCookie = eventSinks.Add(pUnkSink);
        }

        void IConnectionPoint.Unadvise(uint dwCookie)
        {
            eventSinks.RemoveAt(dwCookie);
        }

        void IConnectionPoint.EnumConnections(out IEnumConnections ppEnum)
        {
            throw new NotImplementedException();
        }

        void IConnectionPointContainer.EnumConnectionPoints(out IEnumConnectionPoints ppEnum)
        {
            // This doesn't need to be implemented; all we care about
            // is FindConnectionPoint().
            throw new NotImplementedException();
        }

        /// <summary/>
        /// <param name="riid"/><param name="ppCP"/>
        void IConnectionPointContainer.FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP)
        {
            ppCP = null;
            if (riid == typeof(IDebugPortEvents2).GUID)
            {
                ppCP = this;
            }
        }
    }
}
