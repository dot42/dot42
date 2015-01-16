using Dot42.Utility;

namespace Dot42.AdbLib
{
    /// <summary>
    /// ADB related exception
    /// </summary>
    public class AdbException : Dot42Exception
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public AdbException(string message) : base(message)
        {
        }
    }
}
