using System;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to uninstall a package.
    /// </summary>
    public class UninstallBar : BarDeployTask 
    {
        /// <summary>
        /// Id of the package to uninstall
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                var deployer = CreateDefaultDeployer();
                deployer.UninstallId(PackageId);
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
