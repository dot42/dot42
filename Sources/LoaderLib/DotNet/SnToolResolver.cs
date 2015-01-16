using System.Diagnostics;
using System.Linq;
using System.IO;
using Microsoft.Win32;

namespace Dot42.LoaderLib.DotNet
{
    /// <summary>
    /// Class to call the MS strong name utility.
    /// </summary>
    internal class SnToolResolver
    {
        private static string snExecutable = string.Empty;

        /// <summary>
        /// Initializes static members of the <see cref="SnToolResolver"/> class.
        /// </summary>
        static SnToolResolver()
        {
            // Get the registry key for the Microsoft SDKs.
            RegistryKey sdkList = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SDKs\\Windows");
            if (null != sdkList)
            {
                // Check for sn.exe starting from the latest version.
                foreach (string sdkVersion in sdkList.GetSubKeyNames().Reverse())
                {
                    // Open the Key for the specific SDK version.
                    RegistryKey sdk = sdkList.OpenSubKey(sdkVersion);
                    if (null != sdk)
                    {
                        // Get the installation path.
                        string installationPath = sdk.GetValue("InstallationFolder", "") as string;
                        if (!string.IsNullOrEmpty(installationPath))
                        {
                            // Check if the sn.exe exists under the bin folder.
                            FileInfo snInfo = new FileInfo(Path.Combine(installationPath, "bin", "sn.exe"));
                            if (snInfo.Exists)
                            {
                                // We've found it. Remember it and quit the loop.
                                SnToolResolver.snExecutable = snInfo.FullName;
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Use sn.exe to verify the given assembly.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to verify.</param>
        /// <returns>True if it verifies OK, false otherwise.</returns>
        public static bool VerifyAssembly(string assemblyName)
        {
            return SnToolResolver.RunSnTool(string.Format("-v \"{0}\"", assemblyName));
        }

        /// <summary>
        /// Call sn.exe with the given commandline parameters.
        /// </summary>
        /// <param name="arguments">The commandline paremeters to pass to sn.exe.</param>
        /// <returns>True if sn.exe returned 0, false otherwise.</returns>
        private static bool RunSnTool(string arguments)
        {
            if (!string.IsNullOrEmpty(SnToolResolver.snExecutable))
            {
                bool result = false;
                ProcessStartInfo psi = new ProcessStartInfo(SnToolResolver.snExecutable, arguments);
                if (psi != null)
                {
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        result = process.ExitCode == 0;
                    }
                }

                return result;
            }

            // We don't want to fail if sn is not installed.
            return true;
        }
    }
}
