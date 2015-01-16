using System;

namespace Dot42.Utility
{
    public static class RegistryConstants
    {
        /// <summary>
        /// Registry base for application (C# for Android)
        /// </summary>
        internal static string ROOT(Targets target)
        {
            switch (target)
            {
                case Targets.Android:
                    return ROOT_ANDROID_1;
                case Targets.BlackBerry:
                    return ROOT_BLACKBERRY_1;
                default:
                    throw new ArgumentException("Unknown target " + (int)target);
            }
        }

        /// <summary>
        /// Registry root in version 1 (dot42 pre C# for Android)
        /// </summary>
        internal const string OldRoot1 = @"SOFTWARE\TallApplications\Dot42\v1";

        /// <summary>
        /// Registry root in version 1 C# for Android
        /// </summary>
        private const string PREFIX = @"SOFTWARE\dot42";

        /// <summary>
        /// Registry root in version 1 C# for Android
        /// </summary>
        private const string ROOT_ANDROID_1 = PREFIX + @"\Android\v1";

        /// <summary>
        /// Registry root in version 1 C# for BlackBerry
        /// </summary>
        private const string ROOT_BLACKBERRY_1 = PREFIX + @"\BlackBerry\v1";

        /// <summary>
        /// Registry root for recently used files.
        /// /MRU is appended in code
        /// </summary>
        internal static string MRU(Targets target) { return ROOT(target); }

        /// <summary>
        /// Registry root for license info
        /// </summary>
        public static string LICENSE(Targets target) { return ROOT(target) + @"\License"; }

        /// <summary>
        /// Registry root for license info
        /// </summary>
        public static readonly string BLACKBERRY_DEVICES = ROOT(Targets.BlackBerry) + @"\Devices"; 

        /// <summary>
        /// Registry root for license info
        /// </summary>
        public const string OLD_LICENSE_ANDROID = OldRoot1 + @"\License";

        /// <summary>
        /// Registry root for preferences
        /// </summary>
        public const string PREFERENCES = PREFIX + @"\Preferences"; 

        /// <summary>
        /// Registry root for logging settings (in current user)
        /// </summary>
        public const string LOGGING = PREFIX + @"\Logging"; 
    }
}
