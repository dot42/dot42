using System.Xml.Linq;

namespace Dot42.Mapping
{
    /// <summary>
    /// Local variable info
    /// </summary>
    public class Variable : RegisterEntry
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public Variable(int register, string name)
            : base(register, name)
        {
        }

        /// <summary>
        /// XML ctor
        /// </summary>
        internal Variable(XElement e)
            : base(e)
        {
        }
    }
}
