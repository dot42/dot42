using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Dot42.Utility;
using Microsoft.Win32;

namespace Dot42.Shared.UI
{
    /// <summary>
    /// Server Deployment wizard specific settings
    /// </summary>
    [Obfuscation(Feature = "cleanup")]
    public sealed class UserPreferences 
    {        
        private static UserPreferences instance;
        private readonly CustomSettingsProvider provider = new CustomSettingsProvider();
        private readonly object cacheLock = new object();
        private readonly Dictionary<string, Entry> cache = new Dictionary<string, Entry>();

        #region Public statics

        /// <summary>
        /// Gets the global instance.
        /// </summary>
        public static UserPreferences Preferences
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof (UserPreferences))
                    {
                        if (instance == null)
                        {
                            instance = new UserPreferences();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Save the preferences now
        /// </summary>
        public static void SaveNow()
        {
            if (instance != null)
            {
                instance.Save();
            }
        }

        #endregion

        /// <summary>
        /// Location of main window
        /// </summary>
        public Point ApplicationWindowLocation
        {
            get { return Get("ApplicationWindowLocation", new Point(50,50)); }
            set { Set("ApplicationWindowLocation", value); }
        }

        /// <summary>
        /// Size of main window
        /// </summary>
        [UserScopedSetting,
         DefaultSettingValue("900,600")]
        public Size ApplicationWindowSize
        {
            get { return Get("ApplicationWindowSize", new Size(900, 600)); }
            set { Set("ApplicationWindowSize", value); }
        }

        /// <summary>
        /// Is main window maximized
        /// </summary>
        public bool ApplicationWindowMaximized
        {
            get { return Get("ApplicationWindowMaximized", false); }
            set { Set("ApplicationWindowMaximized", value); }
        }

        /// <summary>
        /// Location of main window
        /// </summary>
        public Point LogCatWindowLocation
        {
            get { return Get("LogCatWindowLocation", new Point(200, 200)); }
            set { Set("LogCatWindowLocation", value); }
        }

        /// <summary>
        /// Size of main window
        /// </summary>
        public Size LogCatWindowSize
        {
            get { return Get("LogCatWindowSize", new Size(900, 700)); }
            set { Set("LogCatWindowSize", value); }
        }

        /// <summary>
        /// Is main window maximized
        /// </summary>
        public bool LogCatWindowMaximized
        {
            get { return Get("LogCatWindowMaximized", false); }
            set { Set("LogCatWindowMaximized", value); }
        }

        /// <summary>
        /// Log level filter
        /// </summary>
        public bool LogCatShowVerbose
        {
            get { return Get("LogCatShowVerbose", false); }
            set { Set("LogCatShowVerbose", value); }
        }

        /// <summary>
        /// Log level filter
        /// </summary>
        public bool LogCatShowDebug
        {
            get { return Get("LogCatShowDebug", false); }
            set { Set("LogCatShowDebug", value); }
        }

        /// <summary>
        /// Log level filter
        /// </summary>
        public bool LogCatShowInfo
        {
            get { return Get("LogCatShowInfo", true); }
            set { Set("LogCatShowInfo", value); }
        }

        /// <summary>
        /// Log level filter
        /// </summary>
        public bool LogCatShowWarning
        {
            get { return Get("LogCatShowWarning", true); }
            set { Set("LogCatShowWarning", value); }
        }

        /// <summary>
        /// Log level filter
        /// </summary>
        public bool LogCatShowError
        {
            get { return Get("LogCatShowError", true); }
            set { Set("LogCatShowError", value); }
        }

        /// <summary>
        /// Log level filter
        /// </summary>
        public bool LogCatShowAssert
        {
            get { return Get("LogCatShowAssert", true); }
            set { Set("LogCatShowAssert", value); }
        }

        /// <summary>
        /// Serial of the preferred device.
        /// The preferred device is that device that is selected when it comes online.
        /// </summary>
        public string PreferredDeviceSerial
        {
            get { return Get("PreferredDeviceSerial", string.Empty); }
            set { Set("PreferredDeviceSerial", value ?? string.Empty); }
        }

        /// <summary>
        /// Address of device to connect to.
        /// </summary>
        public string DeviceConnectionAddress
        {
            get { return Get("DeviceConnectionAddress", string.Empty); }
            set { Set("DeviceConnectionAddress", value ?? string.Empty); }
        }

        /// <summary>
        /// ShowStartTabOnStartup
        /// </summary>
        public bool ShowStartTabOnStartup
        {
            get { return Get("ShowStartTabOnStartup", true); }
            set { Set("ShowStartTabOnStartup", value); }
        }

        /// <summary>
        /// Default folder to look for framework assemblies.
        /// </summary>
        public string AssemblyCheckFrameworkFolder
        {
            get { return Get("AssemblyCheckFrameworkFolder", string.Empty); }
            set { Set("AssemblyCheckFrameworkFolder", value ?? string.Empty); }
        }

        /// <summary>
        /// Last imported jar.
        /// </summary>
        public string JarImportPath
        {
            get { return Get("JarImportPath", string.Empty); }
            set { Set("JarImportPath", value ?? string.Empty); }
        }

        /// <summary>
        /// Last imported jar's lib name.
        /// </summary>
        public string JarImportLibName
        {
            get { return Get("JarImportLibName", string.Empty); }
            set { Set("JarImportLibName", value ?? string.Empty); }
        }

        /// <summary>
        /// Last imported jar's type map path.
        /// </summary>
        public string JarImportTypeMapPath
        {
            get { return Get("JarImportTypeMapPath", string.Empty); }
            set { Set("JarImportTypeMapPath", value ?? string.Empty); }
        }

        /// <summary>
        /// Last imported jar's output folder.
        /// </summary>
        public string JarImportOutputFolder
        {
            get { return Get("JarImportOutputFolder", string.Empty); }
            set { Set("JarImportOutputFolder", value ?? string.Empty); }
        }

        /// <summary>
        /// Last used framework version (for creating new projects).
        /// </summary>
        public string FrameworkVersion
        {
            get { return Get("FrameworkVersion", string.Empty); }
            set { Set("FrameworkVersion", value ?? string.Empty); }
        }

        /// <summary>
        /// Last used certificate path (for creating new projects).
        /// </summary>
        public string CertificatePath
        {
            get { return Get("CertificatePath", string.Empty); }
            set { Set("CertificatePath", value ?? string.Empty); }
        }

        /// <summary>
        /// Last used certificate thumb print (for creating new projects).
        /// </summary>
        public string CertificateThumbPrint
        {
            get { return Get("CertificateThumbPrint", string.Empty); }
            set { Set("CertificateThumbPrint", value ?? string.Empty); }
        }

        /// <summary>
        /// Load a preference value
        /// </summary>
        private T Get<T>(string key, T defaultValue)
        {
            // Load from cache
            lock (cacheLock)
            {
                Entry entry;
                if (cache.TryGetValue(key, out entry))
                    return (T) entry.Value;

                // Not in cache, load from provider
                var descriptor = TypeDescriptor.GetConverter(typeof(T));
                entry = new Entry(descriptor);
                var serializedValue = provider.GetValue(key);
                T result = defaultValue;
                if (serializedValue != null)
                {
                    result = (T) descriptor.ConvertFromString(serializedValue);
                }
                entry.Value = result;
                cache[key] = entry;
                return result;
            }
        }

        /// <summary>
        /// Save a preference value
        /// </summary>
        private void Set<T>(string key, T value)
        {
            // Store in from cache
            lock (cacheLock)
            {
                Entry entry;
                if (cache.TryGetValue(key, out entry))
                {
                    entry.Value = value;
                    return;
                }

                // Not in cache, create entry
                var descriptor = TypeDescriptor.GetConverter(typeof(T));
                entry = new Entry(descriptor) { Value = value };
                cache[key] = entry;
            }
        }

        /// <summary>
        /// Save changes
        /// </summary>
        private void Save()
        {
            lock (cacheLock)
            {
                foreach (var entry in cache)
                {
                    var serialized = entry.Value.SerializedValue;
                    provider.SetValue(entry.Key, serialized);
                }
            }
        }

        /// <summary>
        /// Bind the location and size of the main window to these preferences.
        /// </summary>
        public void AttachMainWindow(Form form, Size minSize)
        {
            var p = new MainWindowPrefs(form, minSize);
            p.Attach();
        }

        /// <summary>
        /// Bind the location and size of the log window to these preferences.
        /// </summary>
        public void AttachLogCatWindow(Form form, Size minSize)
        {
            var p = new LogCatWindowPrefs(form, minSize);
            p.Attach();
        }

        private sealed class Entry
        {
            private readonly TypeConverter converter;

            /// <summary>
            /// Default ctor
            /// </summary>
            public Entry(TypeConverter converter)
            {
                this.converter = converter;
            }

            /// <summary>
            /// Activate value
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Value as stored in the provider
            /// </summary>
            public string SerializedValue
            {
                get { return (Value == null) ? string.Empty : converter.ConvertToString(Value); }
            }
        }

        /// <summary>
        /// Windows preferences for the main window.
        /// </summary>
        private sealed class MainWindowPrefs : WindowPreferences
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            public MainWindowPrefs(Form form, Size minSize)
                : base(form, minSize)
            {
            }

            /// <summary>
            /// Location of window
            /// </summary>
            public override Point Location
            {
                get { return Preferences.ApplicationWindowLocation; }
                set { Preferences.ApplicationWindowLocation = value; }
            }

            /// <summary>
            /// Size of window
            /// </summary>
            public override Size Size
            {
                get { return Preferences.ApplicationWindowSize; }
                set { Preferences.ApplicationWindowSize = value; }
            }

            /// <summary>
            /// Is window maximized
            /// </summary>
            public override bool Maximized
            {
                get { return Preferences.ApplicationWindowMaximized; }
                set { Preferences.ApplicationWindowMaximized = value; }
            }
        }

        /// <summary>
        /// Windows preferences for the logcat window.
        /// </summary>
        private sealed class LogCatWindowPrefs : WindowPreferences
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            public LogCatWindowPrefs(Form form, Size minSize)
                : base(form, minSize)
            {
            }

            /// <summary>
            /// Location of window
            /// </summary>
            public override Point Location
            {
                get { return Preferences.LogCatWindowLocation; }
                set { Preferences.LogCatWindowLocation = value; }
            }

            /// <summary>
            /// Size of window
            /// </summary>
            public override Size Size
            {
                get { return Preferences.LogCatWindowSize; }
                set { Preferences.LogCatWindowSize = value; }
            }

            /// <summary>
            /// Is window maximized
            /// </summary>
            public override bool Maximized
            {
                get { return Preferences.LogCatWindowMaximized; }
                set { Preferences.LogCatWindowMaximized = value; }
            }
        }

        /// <summary>
        /// Settings provider for dot42.
        /// </summary>
        internal sealed class CustomSettingsProvider 
        {
            private const string V1_KEY = RegistryConstants.PREFERENCES;
            private static readonly string[] registerKeys = new[] {V1_KEY};

            /// <summary>
            /// Try to load a value.
            /// </summary>
            /// <returns>Null if not found</returns>
            public string GetValue(string key)
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(GetSubKeyPath(false)))
                {
                    if (regKey == null)
                        return null;
                    return regKey.GetValue(key) as string;
                }
            }

            /// <summary>
            /// Try to load a value.
            /// </summary>
            /// <returns>Null if not found</returns>
            public void SetValue(string key, string value)
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(GetSubKeyPath(true)))
                {
                    if (regKey != null)
                        regKey.SetValue(key, value);
                }
            }

            /// <summary>
            /// Try to read the latest path
            /// </summary>
            private string GetSubKeyPath(bool writable)
            {
                // Always write to latest key
                if (writable)
                {
                    return registerKeys[0];
                }

                // Read from the latest available key
                foreach (var key in registerKeys)
                {
                    var regKey = Registry.CurrentUser.OpenSubKey(key);
                    if (regKey != null)
                    {
                        regKey.Close();
                        return key;
                    }
                }

                // No key found, use latest
                return registerKeys[0];
            }
        }
    }
}