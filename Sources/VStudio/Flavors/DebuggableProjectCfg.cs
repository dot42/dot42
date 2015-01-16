using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dot42.VStudio.Flavors
{
    /// <summary>
    /// Implementation of IVsDebuggableProjectCfg
    /// </summary>
    internal class DebuggableProjectCfg : IVsDebuggableProjectCfg
    {
        private readonly Dot42Project project;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebuggableProjectCfg(Dot42Project project)
        {
            this.project = project;
        }

        /// <summary>
        /// Starts the debugger.
        /// </summary>
        int IVsDebuggableProjectCfg.DebugLaunch(uint grfLaunch)
        {
            project.DebugLaunch(grfLaunch);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines whether the debugger can be launched, given the state of the launch flags.
        /// </summary>
        int IVsDebuggableProjectCfg.QueryDebugLaunch(uint grfLaunch, out int pfCanLaunch)
        {
            pfCanLaunch = 1;
            return VSConstants.S_OK;
        }

        int IVsCfg.get_DisplayName(out string pbstrDisplayName)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_IsDebugOnly(out int pfIsDebugOnly)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_IsReleaseOnly(out int pfIsReleaseOnly)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.EnumOutputs(out IVsEnumOutputs ppIVsEnumOutputs)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.OpenOutput(string szOutputCanonicalName, out IVsOutput ppIVsOutput)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_ProjectCfgProvider(out IVsProjectCfgProvider ppIVsProjectCfgProvider)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_BuildableProjectCfg(out IVsBuildableProjectCfg ppIVsBuildableProjectCfg)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_CanonicalName(out string pbstrCanonicalName)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_Platform(out Guid pguidPlatform)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_IsPackaged(out int pfIsPackaged)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_IsSpecifyingOutputSupported(out int pfIsSpecifyingOutputSupported)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_TargetCodePage(out uint puiTargetCodePage)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_UpdateSequenceNumber(ULARGE_INTEGER[] puliUSN)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_RootURL(out string pbstrRootURL)
        {
            throw new NotImplementedException();
        }

        int IVsDebuggableProjectCfg.get_DisplayName(out string pbstrDisplayName)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_IsDebugOnly(out int pfIsDebugOnly)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_IsReleaseOnly(out int pfIsReleaseOnly)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.EnumOutputs(out IVsEnumOutputs ppIVsEnumOutputs)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.OpenOutput(string szOutputCanonicalName, out IVsOutput ppIVsOutput)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_ProjectCfgProvider(out IVsProjectCfgProvider ppIVsProjectCfgProvider)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_BuildableProjectCfg(out IVsBuildableProjectCfg ppIVsBuildableProjectCfg)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_CanonicalName(out string pbstrCanonicalName)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_Platform(out Guid pguidPlatform)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_IsPackaged(out int pfIsPackaged)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_IsSpecifyingOutputSupported(out int pfIsSpecifyingOutputSupported)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_TargetCodePage(out uint puiTargetCodePage)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_UpdateSequenceNumber(ULARGE_INTEGER[] puliUSN)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_RootURL(out string pbstrRootURL)
        {
            throw new NotImplementedException();
        }

        int IVsProjectCfg.get_DisplayName(out string pbstrDisplayName)
        {
            throw new NotImplementedException();
        }

        int IVsCfg.get_IsDebugOnly(out int pfIsDebugOnly)
        {
            throw new NotImplementedException();
        }

        int IVsCfg.get_IsReleaseOnly(out int pfIsReleaseOnly)
        {
            throw new NotImplementedException();
        }
    }
}
