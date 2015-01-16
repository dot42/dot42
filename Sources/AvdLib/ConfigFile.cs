using System.IO;
using Dot42.Utility;

namespace Dot42.AvdLib
{
    /// <summary>
    /// AVD/config.ini file.
    /// </summary>
    public class ConfigFile
    {
        private readonly string folder;
        private readonly IniFile ini;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ConfigFile(string folder)
        {
            this.folder = folder;
            var path = Path.Combine(folder, AvdConstants.ConfigIni);
            ini = File.Exists(path) ? new IniFile(path) : new IniFile();
            if (!File.Exists(path))
            {
                CpuArch = AvdConstants.DefaultCpuArch;
                CpuModel = AvdConstants.DefaultCpuModel;
            }
        }

        /// <summary>
        /// Save this config file.
        /// </summary>
        public void Save()
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, AvdConstants.ConfigIni);
            ini.Save(path);
        }

        /// <summary>
        /// Access to any property
        /// </summary>
        public string this[string key]
        {
            get { return ini[key]; }
            set { ini[key] = value; }
        }

        /// <summary>
        /// Abi type of this avd
        /// </summary>
        public string AbiType
        {
            get { return ini[AvdConstants.AvdIniAbiType]; }
            set { ini[AvdConstants.AvdIniAbiType] = value; }
        }

        /// <summary>
        /// CPU architecture of this avd
        /// </summary>
        public string CpuArch
        {
            get { return ini[AvdConstants.AvdIniCpuArch]; }
            set { ini[AvdConstants.AvdIniCpuArch] = value; }
        }

        /// <summary>
        /// CPU architecture of this avd
        /// </summary>
        public string CpuModel
        {
            get { return ini[AvdConstants.AvdIniCpuModel]; }
            set { ini[AvdConstants.AvdIniCpuModel] = value; }
        }

        /// <summary>
        /// SDK-relative path of the skin folder, if any, or a 320x480 like constant for a numeric skin size.
        /// </summary>
        public string SkinPath
        {
            get { return ini[AvdConstants.AvdIniSkinPath]; }
            set { ini[AvdConstants.AvdIniSkinPath] = value; }
        }

        /// <summary>
        /// UI name for this skin.
        /// This setting id ignored by the emulator. It is only used to give a friendlier name to the skin.
        /// If missing, use <see cref="SkinPath"/>instead.
        /// </summary>
        public string SkinName
        {
            get { return ini[AvdConstants.AvdIniSkinName]; }
            set { ini[AvdConstants.AvdIniSkinName] = value; }
        }

        /// <summary>
        /// Path to the sdcard file.
        /// If missing, the default name "sdcard.img" will be used for the sdcard, if there's such
        /// a file.
        /// </summary>
        public string SdcardPath
        {
            get { return ini[AvdConstants.AvdIniSdcardPath]; }
            set { ini[AvdConstants.AvdIniSdcardPath] = value; }
        }

        /// <summary>
        /// Size of the SD card.
        /// This property is for UI purposes only. It is not used by the emulator.
        /// </summary>
        public string SdcardSize
        {
            get { return ini[AvdConstants.AvdIniSdcardSize]; }
            set { ini[AvdConstants.AvdIniSdcardSize] = value; }
        }

        /// <summary>
        /// The first path where the emulator looks for system images. Typically this is the path to the add-on system image or
        /// the path to the platform system image if there's no add-on.
        /// The emulator looks at <see cref="Images1"/> before <see cref="Images2"/>.
        /// </summary>
        public string Images1
        {
            get { return ini[AvdConstants.AvdIniImages1]; }
            set { ini[AvdConstants.AvdIniImages1] = value; }
        }

        /// <summary>
        /// AVD/config.ini key name representing the second path where the emulator looks
        /// for system images. Typically this is the path to the platform system image.
        /// </summary>
        public string Images2
        {
            get { return ini[AvdConstants.AvdIniImages2]; }
            set { ini[AvdConstants.AvdIniImages2] = value; }
        }

        /// <summary>
        /// Indicates the presence of the snapshots file.
        /// This property is for UI purposes only. It is not used by the emulator.
        /// </summary>
        public string SnapshotPresent
        {
            get { return ini[AvdConstants.AvdIniSnapshotPresent]; }
            set { ini[AvdConstants.AvdIniSnapshotPresent] = value; }
        }
    }
}
