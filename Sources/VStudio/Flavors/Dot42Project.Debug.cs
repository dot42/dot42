using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Dot42.FrameworkDefinitions;
using Dot42.Ide.Debugger;
using Microsoft.VisualStudio.Shell.Interop;

#if ANDROID
using Dot42.ApkLib;
using PackageFile = Dot42.ApkLib.ApkFile;
#elif BB
using Dot42.BarLib;
using PackageFile = Dot42.BarLib.BarFile;
#endif

namespace Dot42.VStudio.Flavors
{
    partial class Dot42Project
    {
        private DateTime _lastDeployApkTimestamp;
        /// <summary>
        /// Starts the debugger.
        /// </summary>
        internal void DebugLaunch(uint grfLaunch)
        {
            // Open the package
            PackageFile packageFile;
            var packagePath = PackagePath;
            bool needsDeploy = false;

            try
            {
                packageFile = new PackageFile(packagePath);
                
                var curTime = File.GetLastWriteTimeUtc(packagePath);
                if (curTime != _lastDeployApkTimestamp)
                {
                    _lastDeployApkTimestamp = curTime;
                    needsDeploy = true;
                }
            }
            catch (Exception ex)
            {
                _lastDeployApkTimestamp = DateTime.MinValue;
                MessageBox.Show(string.Format("Failed to open package because: {0}", ex.Message));
                return;
            }

            // Show device selection
            var debuggable = (grfLaunch != (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug);
            var ide = Package.Ide;

#if ANDROID
            int minSdkVersion;
            if (!packageFile.Manifest.TryGetMinSdkVersion(out minSdkVersion))
                minSdkVersion = -1;
            var targetFramework = (minSdkVersion > 0) ? Frameworks.Instance.GetBySdkVersion(minSdkVersion) : null;
            var targetVersion = (targetFramework != null) ? targetFramework.Name : null;
            var runner = new ApkRunner(ide, packagePath, packageFile.Manifest.PackageName, packageFile.Manifest.Activities.Select(x => x.Name).FirstOrDefault(), debuggable, (int) grfLaunch);
#elif BB
            var targetFramework = Frameworks.Instance.FirstOrDefault();
            var targetVersion = (targetFramework != null) ? targetFramework.Name : null;
            var runner = new BarRunner(ide, packagePath, packageFile.Manifest.PackageName, "--default--", debuggable, (int)grfLaunch);
#endif
            
            using (var dialog = new DeviceSelectionDialog(ide, runner.DeviceIsCompatible, targetVersion))
            {
                var rc= dialog.ShowDialog();
                switch (rc)
                {
                    case DialogResult.OK:
                        var device = dialog.SelectedDevice;
                        if (Package.Ide.LastUsedUniqueId == device.UniqueId)
                        {
                            runner.DeployApk = needsDeploy;
                        }
                        else
                        {
                            _lastDeployApkTimestamp = DateTime.MinValue;
                            Package.Ide.LastUsedUniqueId = device.UniqueId;
                        }

                        using (var xDialog = new LauncherDialog(packagePath, debuggable))
                        {
                            var stateDialog = xDialog;
                            stateDialog.Cancel += (s, x) => runner.Cancel();
                            stateDialog.Load += (s, x) => runner.Run(device, stateDialog.SetState);
                            stateDialog.ShowDialog();
                        }
                        break;
                    case DialogResult.Retry:
                        // Change project target version
                        try
                        {
                            ChangeAndroidTargetVersion(dialog.NewProjectTargetVersion);
                            MessageBox.Show(string.Format("Changed target framework version to {0}, to match the Android version of your device.", dialog.NewProjectTargetVersion));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(string.Format("Failed to change target framework version because: {0}", ex.Message));
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the full path of the APK resulting from the current configuration.
        /// </summary>
        private string PackagePath
        {
            get
            {
                var config = this.GetActiveProjectCfg(_innerVsHierarchy);
                var outputGroup = config.GetBuildOutputGroup();
                var keyOutput = outputGroup.GetKeyOutput();
                var targetPath = keyOutput.GetProperty("OUTPUTLOC");
                var targetDir = string.IsNullOrEmpty(targetPath) ? string.Empty : Path.GetDirectoryName(targetPath);
#if ANDROID
                var packageFilename = GetMSBuildProperty("ApkFilename", null);
#elif BB
                var packageFilename = GetMSBuildProperty("BarFilename", null);
                if (packageFilename == null)
                {
                    packageFilename = GetMSBuildProperty("ApkFilename", null);
                    if (packageFilename != null)
                    {
                        packageFilename = Path.ChangeExtension(packageFilename, ".bar");
                    }
                }
#endif
                return Path.Combine(targetDir, packageFilename);
            }
        }
    }
}
