using Dot42.Utility;

namespace Dot42.BarDeployLib
{
    /// <summary>
    /// BAR deployment related exception
    /// </summary>
    public class BarDeployException : Dot42Exception
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public BarDeployException(string message) : base(message)
        {
        }
    }
}
