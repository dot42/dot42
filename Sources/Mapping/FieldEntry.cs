using System.Xml.Linq;

namespace Dot42.Mapping
{
    /// <summary>
    /// Field typed entry
    /// </summary>
    public sealed class FieldEntry : MemberEntry
    {
        /// <summary>
        /// XML ctor
        /// </summary>
        internal FieldEntry(XElement e)
            : base(e)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public FieldEntry(string name, string type, string dexName, string dexType)
            : base(name, type, dexName, dexType)
        {
        }
    }
}
