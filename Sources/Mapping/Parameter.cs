using System.Xml.Linq;

namespace Dot42.Mapping
{
    /// <summary>
    /// Method parameter info
    /// </summary>
    public class Parameter : RegisterEntry
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public Parameter(int register, string name)
            : base(register, name)
        {
        }

        /// <summary>
        /// XML ctor
        /// </summary>
        internal Parameter(XElement e)
            : base(e)
        {
        }
    }
}
