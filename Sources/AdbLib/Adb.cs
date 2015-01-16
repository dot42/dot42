using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dot42.DeviceLib;
using Dot42.Utility;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Android Debug Bridge interaction
    /// </summary>
    public partial class Adb
    {
        internal static readonly IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5037);

        /// <summary>
        /// Name of log streams
        /// </summary>
        public static class LogNames
        {
            public const string Main = "main";
            public const string Radio = "radio";
            public const string Events = "events";
            public const string System = "system";
        }

        /// <summary>
        /// Default timeout constants.
        /// </summary>
        public static class Timeout
        {
            /// <summary>
            /// Timeout for a start-server command.
            /// </summary>
            public const int StartServer = 10 * 1000;

            /// <summary>
            /// Timeout for a kill-server command.
            /// </summary>
            public const int KillServer = 10 * 1000;

            /// <summary>
            /// Timeout for a devices command.
            /// </summary>
            public const int GetDevices = 10 * 1000;

            /// <summary>
            /// Timeout for a GetJdwpProcessIds command
            /// </summary>
            public const int GetJdwpProcessIds = 10 * 1000;

            /// <summary>
            /// Timeout for a wait-for-device command.
            /// </summary>
            public const int WaitForDevice = 60 * 1000;

            /// <summary>
            /// Timeout for an install command.
            /// </summary>
            public const int InstallApk = 20 * 1000;

            /// <summary>
            /// Timeout for an uninstall command.
            /// </summary>
            public const int UninstallApk = 20 * 1000;

            /// <summary>
            /// Timeout for an am shell start command.
            /// </summary>
            public const int StartActivity = 20 * 1000;

            /// <summary>
            /// Timeout for an connect command.
            /// </summary>
            public const int Connect = 20 * 1000;

            /// <summary>
            /// Timeout for an am shell getprop command.
            /// </summary>
            public const int GetProperty = 5 * 1000;
        }

        /// <summary>
        /// Action called to collect ADB output.
        /// </summary>
        public Action<string> Logger { get; set; }

        /// <summary>
        /// Start an instance of the ADB server.
        /// </summary>
        public bool StartServer(int timeout)
        {
            var runner = new ProcessRunner(Locations.Adb, "start-server") { Logger = Logger };
            return (runner.Run(timeout) == 0);            
        }

        /// <summary>
        /// Kill any instance of the ADB server.
        /// </summary>
        public bool KillServer(int timeout)
        {
            var runner = new ProcessRunner(Locations.Adb, "kill-server") { Logger = Logger };
            return (runner.Run(timeout) == 0);
        }

        /// <summary>
        /// Wait for the android device to become ready.
        /// </summary>
        /// <returns>True on success, false otherwise</returns>
        public bool WaitForDevice(string serial, int timeout)
        {
            var runner = new ProcessRunner(Locations.Adb, AddSerial(serial, "wait-for-device")) { Logger = Logger };
            return (runner.Run(timeout) == 0);
        }

        /// <summary>
        /// Install the APK on the device.
        /// Throws an exception when installation fails.
        /// </summary>
        public void InstallApk(string serial, string apkPath, string packageName, int timeout)
        {
            InstallApk(serial, apkPath, () => packageName, true, timeout);
        }

        /// <summary>
        /// Install the APK on the device.
        /// Throws an exception when installation fails.
        /// </summary>
        public void InstallApk(string serial, string apkPath, Func<string> getPackageName, bool reInstall, int timeout)
        {
            int exitCode;
            var runner = DoInstallApk(serial, apkPath, reInstall, timeout, out exitCode);
            bool retry;
            bool uninstall;
            runner.CheckResult(this, out retry, out uninstall);
            if (retry)
            {
                if (uninstall)
                {
                    // Remove first, then retry
                    var packageName = (getPackageName != null) ? getPackageName() : null;
                    if (string.IsNullOrEmpty(packageName))
                    {
                        throw new AdbException("Install needs a retry, please uninstall manually and re-install.");
                    }
                    Log("Uninstalling...");
                    UninstallApk(serial, packageName, Adb.Timeout.UninstallApk);
                }

                // Retry
                Log("Retrying...");
                runner = DoInstallApk(serial, apkPath, reInstall, timeout, out exitCode);
            }

            if (exitCode != 0)
            {
                throw new AdbException(string.Format("Failed to install because: {0}", runner.Output));
            }
        }


        /// <summary>
        /// Install the APK on the device.
        /// Throws an exception when installation fails.
        /// </summary>
        private InstallApkRunner DoInstallApk(string serial, string apkPath, bool reInstall, int timeout, out int exitCode)
        {
            var args = AddSerial(serial, "install", reInstall ? "-r" : null, apkPath).Where(x => x != null);
            var runner = new InstallApkRunner(Locations.Adb, args) 
            {
                Logger = Logger
            };
            exitCode = runner.Run(timeout);
            return runner;
        }

        /// <summary>
        /// Uninstall the APK from the device.
        /// Throws an exception when uninstallation fails.
        /// </summary>
        public void UninstallApk(string serial, string packageName, int timeout)
        {
            var runner = new ProcessRunner(Locations.Adb, AddSerial(serial, "uninstall", packageName)) { Logger = Logger };
            var exitCode = runner.Run(timeout);
            if (exitCode != 0)
            {
                throw new AdbException(string.Format("Failed to uninstall because: {0}", runner.Output));
            }
        }

        /// <summary>
        /// Start an given activity in the given package.
        /// Throws an exception when starting fails.
        /// </summary>
        public void StartActivity(IDevice device, string packageName, string activityName, bool debuggable, int timeout, IStartActivityListener listener)
        {
            var component = packageName + "/" + activityName;
            var receiver = new StartActivityReceiver(listener);
            using (var socket = new AdbRequest(EndPoint))
            {
                try
                {
                    var args = new List<string>();
                    args.Add("start");
                    if (debuggable) args.Add("-D");
                    args.Add("-n");
                    args.Add(component);
                    socket.ExecuteShellCommand(receiver, device, timeout, "am", args.ToArray());
                }
                catch (Exception ex)
                {
                    throw new AdbException(string.Format("Failed to start activity because: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Connect local port to JDWP thread on VM process pid.
        /// </summary>
        public void ForwardJdwp(IDevice device, int localPort, int pid)
        {
            using (var socket = new AdbRequest(EndPoint))
            {
                try
                {
                    socket.ForwardJdwp(device, localPort, pid);
                }
                catch (Exception ex)
                {
                    throw new AdbException(string.Format("Failed to start activity because: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Connect to a networked device.
        /// </summary>
        /// <returns>True on success, false otherwise</returns>
        public bool Connect(string host, string port, int timeout)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("host empty");
            var hostArg = host;
            if (!string.IsNullOrEmpty(port))
                hostArg = hostArg + ":" + port;
            var runner = new ProcessRunner(Locations.Adb, "connect", hostArg) { Logger = Logger, LogCommand = true };
            return (runner.Run(timeout) == 0);
        }

        /// <summary>
        /// Gets the currently connected devices.
        /// </summary>
        public static IEnumerable<AndroidDevice> GetDevices(int timeout)
        {
            try
            {
                using (var socket = new AdbDevicesRequest(EndPoint))
                {
                    return socket.Devices();
                }
            }
            catch (Exception ex)
            {
                throw new AdbException(string.Format("Failed to list devices because: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Gets the currently available jdwp process id's on the given device.
        /// </summary>
        public static IEnumerable<int> GetJdwpProcessIds(IDevice device, int timeout)
        {
            try
            {
                using (var socket = new AdbJdwpRequest(EndPoint))
                {
                    return socket.JdwpProcessIds(device);
                }
            }
            catch (Exception ex)
            {
                throw new AdbException(string.Format("Failed to list jdwp pid's because: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Get a system property of a device.
        /// Throws an exception when the request fails.
        /// </summary>
        public static string GetProperty(IDevice device, string propertyName, int timeout, bool errorIfFailed = true)
        {
            var receiver = new StringShellOutputReceiver();
            using (var socket = new AdbRequest(EndPoint))
            {
                try
                {
                    socket.ExecuteShellCommand(receiver, device, timeout, "getprop", propertyName);
                }
                catch (Exception ex)
                {
                    if (errorIfFailed)
                    {
                        throw new AdbException(string.Format("Failed to get property because: {0}", ex.Message));
                    }
                    return null;
                }
            }
            var result = receiver.ToString().Trim(' ', '\t', '\n', '\r');
            return string.IsNullOrEmpty(result) ? null : result;
        }

        /// <summary>
        /// Keep reading log messages until cancelled by the listener or closed.
        /// </summary>
        public void RunLogService(ILogListener listener, IDevice device, string logName)
        {
            using (var socket = new AdbRequest(EndPoint))
            {
                socket.RunLogService(listener, device, logName);
            }            
        }

        /// <summary>
        /// Add the "-s serial" arguments if it is given to the given list of arguments.
        /// </summary>
        private static IEnumerable<string> AddSerial(string serial, params string[] args)
        {
            if (!string.IsNullOrEmpty(serial))
            {
                yield return "-s";
                yield return serial;
            }
            foreach (var arg in args)
            {
                if (arg != null)
                    yield return arg;
            }
        }

        /// <summary>
        /// Add a message to the log (if any).
        /// </summary>
        private void Log(string line)
        {
            if (Logger != null)
            {
                Logger(line);
            }
        }
    }
}
