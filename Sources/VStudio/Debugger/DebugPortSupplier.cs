using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    [ComVisible(true)]
    [Guid(GuidList.Strings.guidDot42PortSupplierClsid)]
    //[Obfuscation] // COM needs a name
    public class DebugPortSupplier : IDebugPortSupplier2
    {
        internal const string Name = "dot42 Port Supplier";
        private readonly Dictionary<string, DebugPort> ports = new Dictionary<string, DebugPort>();

        /// <summary>
        /// Gets the name of this port supplier
        /// </summary>
        public int GetPortSupplierName(out string pbstrName)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPortSupplier.GetPortSupplierName");
            pbstrName = Name;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the id of this port supplier
        /// </summary>
        public int GetPortSupplierId(out Guid pguidPortSupplier)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPortSupplier.GetPortSupplierId");
            pguidPortSupplier = new Guid(GuidList.Strings.guidDot42PortSupplierId);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets a port by it's guid.
        /// </summary>
        public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPortSupplier.GetPort");
            var guid = guidPort;
            var port = ports.Values.FirstOrDefault(x => x.Guid == guid);
            ppPort = port;
            return (port != null) ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        /// <summary>
        /// Enumerate all ports
        /// </summary>
        public int EnumPorts(out IEnumDebugPorts2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPortSupplier.EnumPorts");
            ppEnum = new PortEnum(ports.Values.Cast<IDebugPort2>().ToArray());
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Can ports be added?
        /// </summary>
        public int CanAddPort()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPortSupplier.CanAddPort");
            return VSConstants.S_FALSE;
        }

        /// <summary>
        /// Add a port.
        /// </summary>
        public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPortSupplier.AddPort");
            string name;
            ErrorHandler.ThrowOnFailure(pRequest.GetPortName(out name));
            var port = new DebugPort(this, pRequest);
            ports[name] = port;
            ppPort = port;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Remove the given port.
        /// </summary>
        public int RemovePort(IDebugPort2 pPort)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugPortSupplier.RemovePort");
            string name;
            if (!ErrorHandler.Succeeded(pPort.GetPortName(out name)))
                return VSConstants.S_FALSE;
            return ports.Remove(name) ? VSConstants.S_OK : VSConstants.S_FALSE;
        }
    }
}
