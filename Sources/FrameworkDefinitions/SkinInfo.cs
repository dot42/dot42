using System.IO;
using Dot42.Utility;

namespace Dot42.FrameworkDefinitions
{
    /// <summary>
    /// Info about a single skin.
    /// </summary>
    public class SkinInfo
    {
        private readonly string folder;
        private readonly IniFile ini;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal SkinInfo(string folder)
        {
            this.folder = folder;
            var path = Path.Combine(folder, "hardware.ini");
            ini = File.Exists(path) ? new IniFile(path) : new IniFile();
        }

        /// <summary>
        /// Gets the platform name
        /// </summary>
        public string Name { get { return Path.GetFileName(folder); } }

        /// <summary>
        /// Gets the folder containing this skin relative to the SDK ROOT
        /// </summary>
        public string RelativeFolder { get { return folder.MakeRelativeTo(Locations.ProgramFiles); } }

        /// <summary>
        /// Gets the hardware descriptor
        /// </summary>
        public IniFile Hardware { get { return ini; } }

        /// <summary>
        /// Convert to human readable string.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}
