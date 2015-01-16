using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dot42.AdbLib;
using Dot42.Utility;

namespace Dot42.AvdLib
{
    /// <summary>
    /// Single android virtual device specification
    /// </summary>
    public class Avd
    {
        private const string Emulator = "emulator.exe";

        private readonly string rootFolder;
        private readonly string name;
        private readonly InfoFile infoFile;
        private readonly ConfigFile configFile;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Avd(string rootFolder, string name)
        {
            this.rootFolder = rootFolder;
            this.name = name;

            var infoPath = Path.Combine(rootFolder, name + AvdConstants.AvdInfoExtension);
            infoFile = new InfoFile(infoPath);
            var avdFolder = infoFile.Path ?? Path.Combine(rootFolder, name + AvdConstants.AvdFolderExtension);
            configFile = new ConfigFile(avdFolder);
            if (infoFile.Path == null)
            {
                infoFile.Path = avdFolder;
            }
        }

        /// <summary>
        /// Save settings to disk.
        /// </summary>
        public void Save()
        {
            configFile.Save();
            infoFile.Save();
        }

        /// <summary>
        /// Gets my name
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Gets/sets the target API.
        /// </summary>
        public string Target
        {
            get { return infoFile.Target; }
            set { infoFile.Target = value; }
        }

        /// <summary>
        /// Gets the target API level.
        /// </summary>
        public int TargetApiLevel
        {
            get
            {
                var target = Target;
                const string prefix = "android-";
                if (target.StartsWith(prefix))
                {
                    target = target.Substring(prefix.Length);
                }
                int result;
                if (int.TryParse(target, out result))
                    return result;
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Gets the config.ini file.
        /// </summary>
        public ConfigFile Config { get { return configFile; } }

        /// <summary>
        /// Remove this AVD and all its data from the given root folder.
        /// </summary>
        internal void Remove(string rootFolder)
        {
            var infoPath = Path.Combine(rootFolder, name + AvdConstants.AvdInfoExtension);
            var avdFolder = infoFile.Path ?? Path.Combine(rootFolder, name + AvdConstants.AvdFolderExtension);
            if (Directory.Exists(avdFolder))
            {
                Directory.Delete(avdFolder, true);
            }
            File.Delete(infoPath);
        }

        /// <summary>
        /// Start the emulator with this AVD
        /// </summary>
        public void Start(Action<string> log)
        {
            // Copy skins
            CopyNewFiles(Locations.Frameworks, Path.Combine(Locations.SdkRoot, Path.GetFileName(Locations.Frameworks)));

            // Prepare process
            var info = new ProcessStartInfo();
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.EnvironmentVariables["ANDROID_SDK_HOME"] = Locations.SdkHome;
            info.EnvironmentVariables["ANDROID_SDK_ROOT"] = Locations.SdkRoot;
            info.FileName = FindEmulator();
            info.Arguments = ProcessRunner.QuoteArgument("@" + name) + " -verbose";
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

#if DEBUG
            if (log != null)
            {
                log(string.Format("Filename: {0}", info.FileName));
                log(string.Format("Arguments: {0}", info.Arguments));
                foreach (var key in info.EnvironmentVariables.Keys.Cast<string>().OrderBy(x => x))
                {
                    log(string.Format("ENV[{0}]: {1}", key, info.EnvironmentVariables[key]));
                }
            }
#endif

            // Start process
            var p = Process.Start(info);
            if (log != null)
            {
                p.OutputDataReceived += (s, x) => log(x.Data);
                p.ErrorDataReceived += (s, x) => log(x.Data);
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }

            // Wait for a device
            new Adb().WaitForDevice(null, Adb.Timeout.WaitForDevice);

            if (p.HasExited && (p.ExitCode != 0))
            {
                throw new InvalidDataException(string.Format("Emulator exited with code {0}", p.ExitCode));
            }
        }

        /// <summary>
        /// Copy all new or modified files from source folder to destintation folder.
        /// </summary>
        private static void CopyNewFiles(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            // Copy files
            foreach (var file in Directory.GetFiles(sourceFolder))
            {
                var destFile = Path.Combine(destFolder, Path.GetFileName(file));
                if (!File.Exists(destFile) || (File.GetLastWriteTime(file) > File.GetLastWriteTime(destFile)))
                {
                    File.Copy(file, destFile, true);
                }
            }

            // Copy sub-folders
            foreach (var subFolder in Directory.GetDirectories(sourceFolder))
            {
                CopyNewFiles(subFolder, Path.Combine(destFolder, Path.GetFileName(subFolder)));   
            }
        }

        /// <summary>
        /// Locate the full path of the emulator.
        /// </summary>
        private static string FindEmulator()
        {
            var folder = Locations.AndroidTools;
            var path = Path.Combine(folder, Emulator);
            if (File.Exists(path))
                return path;
            // Search in path.
            var envPath = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(envPath))
            {
                var folders = envPath.Split(new[] {Path.PathSeparator});
                path = folders.Select(x => Path.Combine(x, Emulator)).FirstOrDefault(File.Exists);
                if (path != null)
                    return path;
            }
            throw new InvalidOperationException("Cannot find emulator");
        }
    }
}
