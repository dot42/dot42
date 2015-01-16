using System;
using System.IO;
using Dot42.AdbLib;
using Dot42.ApkLib;
using Dot42.DeviceLib;
using Dot42.Shared.UI;

namespace Dot42.Gui.Controls.Devices
{
    public sealed class InstallApkControl : ProgressControl, IProgressControl
    {
        private readonly IDevice device;
        private readonly string apkPath;
        private bool failed;

        /// <summary>
        /// Default ctor
        /// </summary>
        public InstallApkControl(IDevice device, string apkPath)
            : base(string.Format("Installing {0}", Path.GetFileName(apkPath)))
        {
            this.device = device;
            this.apkPath = apkPath;
        }

        /// <summary>
        /// Run the process.
        /// </summary>
        protected override void DoWork()
        {
            // Get APK package name
            var apk = new ApkFile(apkPath);
            var packageName = apk.Manifest.PackageName;

            // Now install
            var adb = new Adb { Logger = LogOutput };
            adb.InstallApk(device.Serial, apkPath, packageName, Adb.Timeout.InstallApk);
        }

        /// <summary>
        /// Call in the GUI thread when an error has occurred in <see cref="ProgressControl.DoWork"/>.
        /// </summary>
        protected override void ShowError(Exception error)
        {
            failed = true;
            LogOutput(string.Format("\n\nFailed to install {0} because: {1}.", apkPath, error.Message));
        }

        /// <summary>
        /// Prevent closing when failed.
        /// </summary>
        protected override void OnDone()
        {
            if (failed)
            {
                CloseButtonVisible = true;
                ProgressBarVisible = false;
            }
            else
            {
                base.OnDone();
            }
        }

        protected override void OnClose()
        {
            base.OnDone();
        }
    }
}
