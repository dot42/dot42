using System;
using Microsoft.Build.Framework;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to install an APK file.
    /// </summary>
    public class InstallBar : BarDeployTask
    {
        private int errorCount;

        /// <summary>
        /// Package (.apk) to install
        /// </summary>
        [Required]
        public ITaskItem Package { get; set; }

        /// <summary>
        /// Execute the install.
        /// </summary>
        public override bool Execute()
        {
            try
            {
                var deployer = CreateDefaultDeployer();
                deployer.Install(Package.ItemSpec, false);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }
    }
}
