using System;
using System.Text.RegularExpressions;

namespace Dot42.AvdLib
{
    internal static class AvdConstants
    {
        public const String AvdFolderExtension = ".avd";
        public const String AvdInfoExtension = ".ini";

        public const String AvdInfoPath = "path";
        public const String AvdInfoTarget = "target";

        /// <summary>
        /// AVD/config.ini key name representing the abi type of the specific avd
        /// </summary>
        public const String AvdIniAbiType = "abi.type";

        /// <summary>
        /// AVD/config.ini key name representing the CPU architecture of the specific avd
        /// </summary>
        public const String AvdIniCpuArch = "hw.cpu.arch";

        /// <summary>
        /// AVD/config.ini key name representing the CPU architecture of the specific avd
        /// </summary>
        public const String AvdIniCpuModel = "hw.cpu.model";

        /// <summary>
        /// AVD/config.ini key name representing the SDK-relative path of the skin folder, if any,
        /// or a 320x480 like constant for a numeric skin size.
        /// </summary>
        public const String AvdIniSkinPath = "skin.path";

        /// <summary>
        /// AVD/config.ini key name representing an UI name for the skin.
        /// This config key is ignored by the emulator. It is only used by the SDK manager or
        /// tools to give a friendlier name to the skin.
        /// If missing, use the {@link #AVD_INI_SKIN_PATH} key instead.
        /// </summary>
        public const String AvdIniSkinName = "skin.name";
        
        /// <summary>
        /// AVD/config.ini key name representing the path to the sdcard file.
        /// If missing, the default name "sdcard.img" will be used for the sdcard, if there's such
        /// a file.
        /// </summary>
        public const String AvdIniSdcardPath = "sdcard.path";

        /// <summary>
        /// AVD/config.ini key name representing the size of the SD card.
        /// This property is for UI purposes only. It is not used by the emulator.
        /// </summary>
        public const String AvdIniSdcardSize = "sdcard.size";

        /// <summary>
        /// AVD/config.ini key name representing the first path where the emulator looks
        /// for system images. Typically this is the path to the add-on system image or
        /// the path to the platform system image if there's no add-on.
        /// The emulator looks at {@link #AVD_INI_IMAGES_1} before {@link #AVD_INI_IMAGES_2}.
        /// </summary>
        public const String AvdIniImages1 = "image.sysdir.1";

        /// <summary>
        /// AVD/config.ini key name representing the second path where the emulator looks
        /// for system images. Typically this is the path to the platform system image.
        /// </summary>
        public const String AvdIniImages2 = "image.sysdir.2";

        /// <summary>
        /// AVD/config.ini key name representing the presence of the snapshots file.
        /// This property is for UI purposes only. It is not used by the emulator.
        /// </summary>
        public const String AvdIniSnapshotPresent = "snapshot.present";

        /// <summary>
        /// Pattern to match pixel-sized skin "names", e.g. "320x480".
        /// </summary>
        public static readonly Regex NumericSkinSize = new Regex("([0-9]{2,})x([0-9]{2,})");

        public const String UserdataImg = "userdata.img";
        public const String ConfigIni = "config.ini";
        public const String SdcardImg = "sdcard.img";
        public const String SnapshotsImg = "snapshots.img";

        public const string DefaultCpuArch = "arm";
        public const string DefaultCpuModel = "cortex-a8";
    }
}
