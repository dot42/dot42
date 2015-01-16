using System;
using System.ComponentModel;
using System.Diagnostics;
using Dot42.AdbLib;
using Dot42.ApkLib;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to install an APK file.
    /// </summary>
    public class InstallApk : Task
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public InstallApk()
        {
            Utils.InitializeLocations();
            Reinstall = true;
        }

        /// <summary>
        /// Package (.apk) to install
        /// </summary>
        [Required]
        public ITaskItem Package { get; set; }

        /// <summary>
        /// Re-install keeping the data 
        /// </summary>
        [DefaultValue(true)]
        public bool Reinstall { get; set; }

        /// <summary>
        /// Execute the install.
        /// </summary>
        public override bool Execute()
        {
            try
            {
                var adb = new Adb();
                adb.Logger += s => { 
                    if(!string.IsNullOrEmpty(s))
                        Log.LogMessage(MessageImportance.High, s);
                };
                adb.InstallApk(null, Package.ItemSpec, GetApkName, true, Adb.Timeout.InstallApk);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }

        /// <summary>
        /// Gets the name of the APK package.
        /// </summary>
        private string GetApkName()
        {
            var apk = new ApkFile(Package.ItemSpec);
            return apk.Manifest.PackageName;
        }
    }
}
