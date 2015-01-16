using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Dot42.Utility
{
    /// <summary>
    /// File/folder locations
    /// </summary>
    public static class Locations
    {
        private const string AndroidAvdFolder = @".android\avd";
        private const string AndroidSystemImagesFolder = @"system-images";
        private const string DeviceCenterExeName = "dot42DevCenter.exe";
        private const string AdbExeName = "adb.exe";
        private const string AaptExeName = "aapt.exe";

        /// <summary>
        /// Current target. Must be set by apps.
        /// </summary>
        public static Targets? Target;

        /// <summary>
        /// Set the current target from string.
        /// </summary>
        public static Targets SetTarget(string value)
        {
            // Detect target
            Targets target;
            switch ((value ?? string.Empty).ToUpperInvariant())
            {
                case "":
                case "ANDROID":
                    target = Targets.Android;
                    break;
                case "BLACKBERRY":
                    target = Targets.BlackBerry;
                    break;
                default:
                    throw new ArgumentException("Unknown target " + value);
            }
            Target = target;
            return target;
        }

        /// <summary>
        /// Is the current target set to Android?
        /// </summary>
        public static bool IsAndroid { get { return (Target.HasValue && Target.Value == Targets.Android); } }

        /// <summary>
        /// Is the current target set to BlackBerry?
        /// </summary>
        public static bool IsBlackBerry { get { return (Target.HasValue && Target.Value == Targets.BlackBerry); } }

        /// <summary>
        /// Gets the program files location.
        /// </summary>
        public static string ProgramFiles
        {
            get
            {
                var folder = Path.GetDirectoryName(typeof(Locations).Assembly.Location);
                if (!string.IsNullOrEmpty(folder))
                {
                    if (IsProgramFilesRoot(folder))
                        return folder;
#if DEBUG
                    folder = Path.GetFullPath(Path.Combine(folder, @"..\..\..\..\Build\Application"));
                    if (IsProgramFilesRoot(folder))
                        return folder;
#endif
                }

                // Try get environment
                folder = Environment.GetEnvironmentVariable("Dot42Folder");
                if (!string.IsNullOrEmpty(folder) && IsProgramFilesRoot(folder))
                {
                    return folder;
                }

                // Try registry
                folder = GetExtensionsPathFromRegistry();
                if (!string.IsNullOrEmpty(folder) && IsProgramFilesRoot(folder))
                {
                    return folder;
                }

                throw new InvalidOperationException("Cannot find program files root");
            }
        }

        /// <summary>
        /// Gets the full path of Dot42 Device Center
        /// </summary>
        public static string DeviceCenter
        {
            get
            {
                var path = Path.Combine(ProgramFiles, DeviceCenterExeName);
                if (File.Exists(path))
                    return path;
                // Search in path.
                var envPath = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(envPath))
                {
                    var folders = envPath.Split(new[] { Path.PathSeparator });
                    path = folders.Select(x => Path.Combine(x, DeviceCenterExeName)).FirstOrDefault(File.Exists);
                    if (path != null)
                        return path;
                }
                throw new InvalidOperationException("Cannot find Device Center");
            }
        }

        /// <summary>
        /// Gets the folder containing the android tools.
        /// </summary>
        public static string AndroidTools
        {
            get { return Path.Combine(ProgramFiles, "Tools"); }
        }

        /// <summary>
        /// Gets the full path of adb.exe.
        /// </summary>
        public static string Adb
        {
            get
            {
                var path = Path.Combine(AndroidPlatformTools, AdbExeName);
                if (File.Exists(path))
                    return path;
                // Search in path.
                var envPath = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(envPath))
                {
                    var folders = envPath.Split(new[] { Path.PathSeparator });
                    path = folders.Select(x => Path.Combine(x, AdbExeName)).FirstOrDefault(File.Exists);
                    if (path != null)
                        return path;
                }
                throw new InvalidOperationException("Cannot find adb");
            }
        }

        /// <summary>
        /// Gets the full path of aapt.exe.
        /// </summary>
        public static string Aapt
        {
            get
            {
                var path = Path.Combine(AndroidPlatformTools, AaptExeName);
                if (File.Exists(path))
                    return path;
                // Search in path.
                var envPath = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(envPath))
                {
                    var folders = envPath.Split(new[] { Path.PathSeparator });
                    path = folders.Select(x => Path.Combine(x, AaptExeName)).FirstOrDefault(File.Exists);
                    if (path != null)
                        return path;
                }
                throw new InvalidOperationException("Cannot find aapt.exe");
            }
        }

        /// <summary>
        /// Gets the folder containing the android platform tools.
        /// </summary>
        public static string AndroidPlatformTools
        {
            get { return Path.Combine(ProgramFiles, "Platform-Tools"); }
        }

        /// <summary>
        /// Gets the folder containing the framework definitions.
        /// </summary>
        public static string Frameworks
        {
            get { return Path.Combine(ProgramFiles, "Frameworks"); }
        }

        /// <summary>
        /// Gets the root folder containing all AVD info files.
        /// </summary>
        public static string Avds
        {
            get { return Path.Combine(SdkHome, AndroidAvdFolder); }
        }

        /// <summary>
        /// Gets the root folder containing all system images.
        /// </summary>
        public static string SystemImages
        {
            get { return Path.Combine(SdkRoot, AndroidSystemImagesFolder); }
        }

        /// <summary>
        /// Gets the folder in the users local application data for this app.
        /// </summary>
        public static string LocalAppData
        {
            get
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appData, "Dot42");
            }
        }

        /// <summary>
        /// Gets the root folder for user related SDK info (e.g. AVD's).
        /// </summary>
        public static string SdkHome
        {
            get
            {
                var root = Environment.GetEnvironmentVariable("DOT42_AVD_HOME");
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
                    return root;
                return Path.Combine(LocalAppData, "Sdk");
            }
        }

        /// <summary>
        /// Gets the root folder for user related SDK info (e.g. system images).
        /// </summary>
        public static string SdkRoot
        {
            get
            {
                var root = Environment.GetEnvironmentVariable("DOT42_AVD_HOME");
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
                    return root;
                return Path.Combine(LocalAppData, "Sdk");
            }
        }

        /// <summary>
        /// Is the given folder a valid program files folder?
        /// </summary>
        private static bool IsProgramFilesRoot(string folder)
        {
            return Directory.Exists(Path.Combine(folder, "Tools")) &&
                   Directory.Exists(Path.Combine(folder, "Frameworks"));
        }

        /// <summary>
        /// Try to get the extensions path from the registry.
        /// </summary>
        private static string GetExtensionsPathFromRegistry()
        {
            if (!Target.HasValue)
                return null;
            string keyName;
            switch (Target.Value)
            {
                case Targets.Android:
                    keyName = @"Software\dot42\Android";
                    break;
                case Targets.BlackBerry:
                    keyName = @"Software\dot42\BlackBerry";
                    break;
                default:
                    throw new ArgumentException("Unknown target " + (int)Target.Value);
            }
           
            var roots = new[] { Registry.CurrentUser, Registry.LocalMachine };
            // Try registry
            foreach (var root in roots)
            {
                using (var key = root.OpenSubKey(keyName))
                {
                    if (key != null)
                    {
                        var path = key.GetValue("ExtensionsPath") as string;
                        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                            return path;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Return the folder with the Dot42 samples, or null if not found.
        /// </summary>
        public static string SamplesFolder(Targets target)
        {
            if (!SamplesFolderIsPossible)
                return null;
            var root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrEmpty(root))
                return null;
            string folder;
            switch (target)
            {
                case Targets.Android:
                    folder = Path.Combine(root, @"dot42\Android\Samples");
                    break;
                case Targets.BlackBerry:
                    folder = Path.Combine(root, @"dot42\BlackBerry\Samples");
                    break;
                default:
                    throw new ArgumentException("Unknown target " + (int)target);
            }
            return folder;
        }

        /// <summary>
        /// Is it possible to create a samples folder?
        /// </summary>
        public static bool SamplesFolderIsPossible
        {
            get
            {
                string root;
                try
                {
                    root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                catch
                {
                    root = null;
                }
                return !string.IsNullOrEmpty(root);
            }
        }
    }
}
