using System.Collections.Generic;
using Dot42.Utility;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Android Debug Bridge interaction
    /// </summary>
    partial class Adb
    {
        /// <summary>
        /// Process runner for the install APK command.
        /// </summary>
        private class InstallApkRunner : ProcessRunner
        {
            private bool certificateFailure;
            private bool alreadyExists;
            private bool invalidApk;
            private bool invalidUri;
            private bool couldntCopy;
            private readonly List<string> failures = new List<string>();

            /// <summary>
            /// Default ctor
            /// </summary>
            public InstallApkRunner(string command, IEnumerable<string> arguments)
                : base(command, arguments)
            {
            }

            /// <summary>
            /// Called when new output is available.
            /// </summary>
            protected override void OnOutput(string line)
            {
                if (string.IsNullOrEmpty(line))
                    return;
                if (line.Contains("INSTALL_PARSE_FAILED_INCONSISTENT_CERTIFICATES"))
                    certificateFailure = true;
                if (line.Contains("INSTALL_FAILED_ALREADY_EXISTS"))
                    alreadyExists = true;
                if (line.Contains("INSTALL_FAILED_INVALID_APK"))
                    invalidApk = true;
                if (line.Contains("INSTALL_FAILED_INVALID_URI"))
                    invalidUri = true;
                if (line.Contains("INSTALL_FAILED_COULDNT_COPY"))
                    couldntCopy = true;
                if (line.Contains("INSTALL_FAILED"))
                    failures.Add(line);
            }

            /// <summary>
            /// Check the result of this runner.
            /// </summary>
            public void CheckResult(Adb adb, out bool retry, out bool uninstall)
            {
                retry = false;
                uninstall = false;

                if (certificateFailure)
                {
                    // Remove first, then retry
                    adb.Log("Certificate mismatch. Uninstalling...");
                    uninstall = true;
                    retry = true;
                }
                else if (alreadyExists)
                {
                    // This should not happen because we install with reinstall flag on.
                    throw new AdbException("Install failed. The package already exists.");
                }
                else if (invalidApk)
                {
                    // Compile/format/verify error
                    throw new AdbException("Install failed. Invalid APK. Check the device log for details.");
                }
                else if (invalidUri)
                {
                    // ? error
                    throw new AdbException("Install failed. Invalid URI. Check the device log for details.");
                }
                else if (couldntCopy)
                {
                    // ? error
                    throw new AdbException("Install failed. Couldn't copy to final destination. Check the device log for details.");
                }
                else if (failures.Count > 0)
                {
                    // Unknown error
                    throw new AdbException(string.Format("Install failed ({0}). Check the device log for details.", failures[0]));
                }
            }
        }
    }
}
