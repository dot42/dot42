using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dot42.VStudio.Flavors
{
    /// <summary>
    /// Implementation of IVsProjectFlavorCfg.
    /// </summary>
    internal class ProjectFlavorCfg : IVsProjectFlavorCfg
    {
        private readonly Dot42Project project;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ProjectFlavorCfg(Dot42Project project)
        {
            this.project = project;
        }

        /// <summary>
        /// Provides access to a configuration interfaces such as <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsBuildableProjectCfg2"/> or <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsDebuggableProjectCfg"/>.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="iidCfg">[in] Interface identifier of the <paramref name="ppCfg"/> to access.</param><param name="ppCfg">[out, iid_is(iidCfg)] Pointer to the configuration interface identified by <paramref name="iidCfg"/>.</param>
        public int get_CfgType(ref Guid iidCfg, out IntPtr ppCfg)
        {
            ppCfg = IntPtr.Zero;

            // See if this is an interface we support
            if (iidCfg == typeof(IVsDebuggableProjectCfg).GUID)
            {
                var projectCfg = new DebuggableProjectCfg(project);
                ppCfg = Marshal.GetComInterfaceForObject(projectCfg, typeof (IVsDebuggableProjectCfg));
            }

            // If not supported
            return (ppCfg == IntPtr.Zero) ? VSConstants.E_NOINTERFACE : VSConstants.S_OK;
        }

        /// <summary>
        /// Closes the <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsProjectFlavorCfg"/> object.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        public int Close()
        {
            return VSConstants.S_OK;
        }
    }
}
