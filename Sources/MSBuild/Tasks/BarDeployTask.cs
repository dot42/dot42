using System;
using Dot42.BarDeployLib;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Base class for all BAR deployment based tasks.
    /// </summary>
    public abstract class BarDeployTask : Task
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        protected BarDeployTask()
        {
            Utils.InitializeLocations();
        }

        /// <summary>
        /// Create a deployer for the default device.
        /// </summary>
        protected BarDeployer CreateDefaultDeployer()
        {
            var device = BlackBerryDevices.Instance.DefaultDevice;
            if (device == null)
                throw new ArgumentException("Add a device first");
            return new BarDeployer(device) { Logger = OnLogger };
        }

        private void OnLogger(string s)
        {
            Log.LogMessage(MessageImportance.High, s);
        }
    }
}
