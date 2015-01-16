using System;
using Dot42.Utility;

namespace Dot42.ImportJarLib
{
    public class SourceProperties : IniFile
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public SourceProperties(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Gets the platform version.
        /// </summary>
        public string PlatformVersion
        {
            get
            {
                var result = GetValueThrowOnError("Platform.Version");
                if (result.IndexOf('.') < 0)
                    result += ".0";
                return "v" + result;
            }
        }

        /// <summary>
        /// Gets the version of the mscorlib assembly.
        /// </summary>
        public Version AssemblyVersion
        {
            get { return GetAssemblyVersion(0); }
        }

        /// <summary>
        /// Gets the file version of the mscorlib assembly.
        /// The revision number encodes the Dot42 revision number.
        /// </summary>
        public Version AssemblyFileVersion
        {
            get
            {
                var my = GetType().Assembly.GetName().Version;
                var myRev = Math.Max(0, my.Revision);
                if (myRev >= (1 << 16)) throw new InvalidOperationException("Revision number is too high");
                return GetAssemblyVersion(myRev);
            }
        }

        /// <summary>
        /// Gets the full informational version of the mscorlib assembly.
        /// </summary>
        public string AssemblyInformationalVersion
        {
            get
            {
                var platformVersion = GetValueThrowOnError("Platform.Version");
                if (platformVersion.IndexOf('.') < 0)
                    platformVersion += ".0";
                var my = GetType().Assembly.GetName().Version;
                return string.Format("{0}, Dot42 {1}", platformVersion, my);
            }
        }

        /// <summary>
        /// Gets the version of the mscorlib assembly.
        /// </summary>
        private Version GetAssemblyVersion(int revision)
        {
            var result = GetValueThrowOnError("Platform.Version");
            if (result.IndexOf('.') < 0)
                result += ".0";
            var tmp = new Version(result);
            return new Version(tmp.Major, Math.Max(0, tmp.Minor), Math.Max(0, tmp.Build), revision);
        }

        /// <summary>
        /// Gets the Android API level.
        /// </summary>
        public string ApiLevel
        {
            get { return GetValueThrowOnError("AndroidVersion.ApiLevel"); }
        }
    }
}
