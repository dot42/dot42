using System.IO;
using Dot42.Utility;

namespace Dot42.AvdLib
{
    /// <summary>
    /// [name].ini file.
    /// </summary>
    public class InfoFile
    {
        private readonly string path;
        private readonly IniFile ini;

        /// <summary>
        /// Default ctor
        /// </summary>
        public InfoFile(string path)
        {
            this.path = System.IO.Path.GetFullPath(path);
            ini = File.Exists(path) ? new IniFile(path) : new IniFile();
        }

        /// <summary>
        /// Save this info file.
        /// </summary>
        public void Save()
        {
            var folder = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder) && (!Directory.Exists(folder)))
                Directory.CreateDirectory(folder);
            ini.Save(path);
        }

        /// <summary>
        /// Path of the AVD folder.
        /// </summary>
        public string Path
        {
            get { return ini[AvdConstants.AvdInfoPath]; }
            set { ini[AvdConstants.AvdInfoPath] = value; }
        }

        /// <summary>
        /// API Level target
        /// </summary>
        public string Target
        {
            get { return ini[AvdConstants.AvdInfoTarget]; }
            set { ini[AvdConstants.AvdInfoTarget] = value; }
        }
    }
}
