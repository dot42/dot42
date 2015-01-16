using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dot42.FrameworkDefinitions
{
    /// <summary>
    /// Info about a single framework.
    /// </summary>
    public class FrameworkInfo
    {
        private readonly List<SkinInfo> skins = new List<SkinInfo>();
        private readonly string folder;
        private readonly FrameworkIni ini;

        /// <summary>
        /// Default ctor
        /// </summary>
        public FrameworkInfo(string folder)
        {
            this.folder = folder;
            ini = new FrameworkIni(folder);
            var skinsRoot = Path.Combine(folder, "skins");
            if (Directory.Exists(skinsRoot))
            {
                skins.AddRange(Directory.GetDirectories(skinsRoot).Select(x => new SkinInfo(x)));
            }
        }

        /// <summary>
        /// Gets the platform name
        /// </summary>
        public string Name { get { return Path.GetFileName(folder); } }

        /// <summary>
        /// Does this framework matches with a framework assembly version?
        /// </summary>
        public bool MatchesFrameworkVersion(Version version)
        {
            var expected = string.Format("v{0}", version);
            var name = Name;
            return string.Equals(name, expected, StringComparison.OrdinalIgnoreCase) ||
                   expected.StartsWith(name + ".", StringComparison.OrdinalIgnoreCase);

        }

        /// <summary>
        /// Gets the framework descriptor
        /// </summary>
        public FrameworkIni Descriptor { get { return ini; } }

        /// <summary>
        /// Default skin (can be null)
        /// </summary>
        public SkinInfo DefaultSkin
        {
            get
            {
                var name = ini["sdk.skin.default"];
                SkinInfo skin = null;
                if (!string.IsNullOrEmpty(name))
                {
                    skin = skins.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                }
                return skin ?? skins.FirstOrDefault();
            }
        }

        /// <summary>
        /// Skins supports in this framework
        /// </summary>
        public IEnumerable<SkinInfo> Skins { get { return skins; } }

        /// <summary>
        /// Gets the folder containing this framework.
        /// </summary>
        public string Folder { get { return folder; } }

        /// <summary>
        /// Convert to human readable string.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} (API level {1})", Name, Descriptor.ApiLevel);
        }
    }
}
