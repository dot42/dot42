using System.IO;
using Dot42.Utility;

namespace Dot42.FrameworkDefinitions
{
    /// <summary>
    /// Creator of framework.ini file.
    /// </summary>
    public class FrameworkIni 
    {
        private readonly string folder;
        private const string FileName = "Framework.ini";
        private readonly IniFile ini;

        /// <summary>
        /// Default ctor
        /// </summary>
        public FrameworkIni(string folder) 
        {
            this.folder = folder;
            var path = Path.Combine(folder, FileName);
            ini = File.Exists(path) ? new IniFile(path) : new IniFile();
        }

        /// <summary>
        /// Access to all properties.
        /// </summary>
        public string this[string key]
        {
            get { return ini[key]; }
            set { ini[key] = value; }
        }

        /// <summary>
        /// Platform target.
        /// </summary>
        public string Target
        {
            get { return ini["target"]; }
            set { ini["target"] = value; }
        }

        /// <summary>
        /// Platform API level.
        /// </summary>
        public int ApiLevel
        {
            get
            {
                int result;
                return ini.TryGetIntValue("api.level", out result) ? result : 0;
            }
            set { ini["api.level"] = value.ToString(); }
        }

        /// <summary>
        /// Save this file.
        /// </summary>
        public void Save()
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            ini.Save(Path.Combine(folder, FileName));
        }
    }
}
