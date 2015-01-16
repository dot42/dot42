using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dot42.VStudio.Flavors
{
    /// <summary>
    /// VisualStudio utility methods.
    /// </summary>
    internal static class VSUtilities
    {
        /// <summary>
        /// Gets the global service provider.
        /// </summary>
        internal static IServiceProvider ServiceProvider()
        {
            return new PackageServiceProvider();
        }

        /// <summary>
        /// Gets the activate project configuration.
        /// </summary>
        internal static IVsProjectCfg GetActiveProjectCfg(this IServiceProvider serviceProvider, IVsHierarchy project)
        {
            var buildManager = (IVsSolutionBuildManager) serviceProvider.GetService(typeof (SVsSolutionBuildManager));
            if (buildManager == null)
                throw new InvalidOperationException("No solution build manager found");
            var ppIVsProjectCfg = new IVsProjectCfg[1];
            buildManager.FindActiveProjectCfg(IntPtr.Zero, IntPtr.Zero, project, ppIVsProjectCfg);
            return ppIVsProjectCfg[0];
        }

        /// <summary>
        /// Gets the build output group.
        /// </summary>
        internal static IVsOutputGroup2 GetBuildOutputGroup(this IVsProjectCfg projectCfg)
        {
            var projectCfg2 = projectCfg as IVsProjectCfg2;
            if (projectCfg2 == null)
                throw new InvalidOperationException("No project config2 found");
            IVsOutputGroup outputGroup;
            ErrorHandler.ThrowOnFailure(projectCfg2.OpenOutputGroup("Built", out outputGroup));
            return outputGroup as IVsOutputGroup2;
        }

        /// <summary>
        /// Gets the build output group.
        /// </summary>
        internal static IVsOutput2 GetKeyOutput(this IVsOutputGroup2 outputGroup)
        {
            if (outputGroup == null)
                throw new ArgumentNullException("outputGroup");
            IVsOutput2 output;
            ErrorHandler.ThrowOnFailure(outputGroup.get_KeyOutputObject(out output));
            return output;
        }

        /// <summary>
        /// Gets the build output group.
        /// </summary>
        internal static string GetDeployUrl(this IVsOutput2 output)
        {
            const string filePrefix = "file:///";
            if (output == null)
                throw new ArgumentNullException("output");
            string deployUrl;
            ErrorHandler.ThrowOnFailure(output.get_DeploySourceURL(out deployUrl));
            if ((deployUrl != null) && (deployUrl.StartsWith(filePrefix)))
            {
                deployUrl = deployUrl.Substring(filePrefix.Length);
            }
            return deployUrl;
        }

        /// <summary>
        /// Gets the build output group.
        /// </summary>
        internal static string GetRootRelativeUrl(this IVsOutput2 output)
        {
            const string filePrefix = "file:///";
            if (output == null)
                throw new ArgumentNullException("output");
            string url;
            ErrorHandler.ThrowOnFailure(output.get_RootRelativeURL(out url));
            if ((url != null) && (url.StartsWith(filePrefix)))
            {
                url = url.Substring(filePrefix.Length);
            }
            return url;
        }

        /// <summary>
        /// Gets a property from the given output.
        /// </summary>
        internal static string GetProperty(this IVsOutput2 output, string name)
        {
            if (output == null)
                throw new ArgumentNullException("output");
            object pvar;
            ErrorHandler.ThrowOnFailure(output.get_Property(name, out pvar));
            return pvar as string;
        }

        /// <summary>
        /// Get an output window. Create if needed.
        /// </summary>
        internal static IVsOutputWindowPane GetOutputPane(this IServiceProvider serviceProvider, Guid paneGuid,
                                                          string title, bool visible, bool clearWithSolution)
        {
            var output = (IVsOutputWindow) serviceProvider.GetService(typeof (SVsOutputWindow));
            IVsOutputWindowPane pane;

            // Create a new pane.
            output.CreatePane(
                ref paneGuid,
                title,
                Convert.ToInt32(visible),
                Convert.ToInt32(clearWithSolution));

            // Retrieve the new pane.
            output.GetPane(ref paneGuid, out pane);

            return pane;
        }

        /// <summary>
        /// Gets a project item from a given hierarchy + item id.
        /// </summary>
        internal static EnvDTE.ProjectItem GetProjectItemFromHierarchy(this IVsHierarchy pHier, uint dwItemId)
        {
            object oObj;
            ErrorHandler.ThrowOnFailure(pHier.GetProperty(dwItemId, (int) __VSHPROPID.VSHPROPID_ExtObject, out oObj));
            var oItem = oObj as EnvDTE.ProjectItem;
            if (oItem == null)
            {
                throw new ArgumentException("Extended object is not a ProjectItem");
            }
            return oItem;
        }

        private class PackageServiceProvider : IServiceProvider
        {
            /// <summary>
            /// Gets the service object of the specified type.
            /// </summary>
            /// <returns>
            /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
            /// </returns>
            /// <param name="serviceType">An object that specifies the type of service object to get. </param><filterpriority>2</filterpriority>
            public object GetService(Type serviceType)
            {
                return Package.GetGlobalService(serviceType);
            }
        }
    }
}
