using Dot42.Utility;

namespace Dot42.ImportJarLib
{
    public class SdkProperties : IniFile
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public SdkProperties(string path)
            : base(path)
        {
        }
    }
}
