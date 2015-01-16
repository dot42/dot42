using System.Xml.Linq;

namespace Dot42.Mapping
{
    /// <summary>
    /// Event typed entry
    /// </summary>
    public sealed class EventEntry : MemberEntry
    {
        /// <summary>
        /// XML ctor
        /// </summary>
        internal EventEntry(XElement e)
            : base(e)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal EventEntry(string name, string type, string dexName, string dexType)
            : base(name, type, dexName, dexType)
        {
        }
    }
}
