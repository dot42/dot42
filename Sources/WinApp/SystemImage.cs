using System.IO;
using Dot42.Utility;

namespace Dot42.Gui
{
    /// <summary>
    /// Info about a single framework.
    /// </summary>
    internal class SystemImage
    {
        private readonly string folder;
        private readonly IniFile ini;

        /// <summary>
        /// Default ctor
        /// </summary>
        private SystemImage(string folder)
        {
            this.folder = folder;
            ini= new IniFile(Path.Combine(folder, "source.properties"));
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal static SystemImage TryLoad(string folder)
        {
            try
            {
                return new SystemImage(folder);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the folder containing this system image relative to the SDK root.
        /// </summary>
        public string RelativeFolder { get { return folder.MakeRelativeTo(Locations.SdkRoot); } }

        /// <summary>
        /// Gets a human readable descriptor.
        /// </summary>
        public string Descriptor { get { return ini["Pkg.Desc"]; } }

        /// <summary>
        /// Gets the target api level.
        /// </summary>
        public int ApiLevel
        {
            get
            {
                int value;
                return ini.TryGetIntValue("AndroidVersion.ApiLevel", out value) ? value : -1;
            }
        }

        /// <summary>
        /// Gets the ABI name.
        /// </summary>
        public string Abi
        {
            get { return ini["SystemImage.Abi"]; }
        }

        /// <summary>
        /// Convert to human readable string.
        /// </summary>
        public override string ToString()
        {
            return Descriptor;
        }
    }
}
