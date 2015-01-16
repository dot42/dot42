using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.Mapping
{
    /// <summary>
    /// Field, property or event entry in map file
    /// </summary>
    public abstract class MemberEntry
    {
        private readonly string name;
        private readonly string type;
        private readonly string dexName;
        private readonly string dexType;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected MemberEntry(string name, string type, string dexName, string dexType)
        {
            this.name = name;
            this.type = type;
            this.dexName = dexName;
            this.dexType = dexType;
        }

        /// <summary>
        /// XML ctor
        /// </summary>
        protected MemberEntry(XElement e)
        {
            name = e.GetAttribute("name");
            type = e.GetAttribute("type");
            dexName = e.GetAttribute("dname");
            dexType = e.GetAttribute("dtype");
        }

        /// <summary>
        /// Convert to XML element
        /// </summary>
        internal virtual XElement ToXml(string elementName)
        {
            return new XElement(elementName,
                new XAttribute("name", name),
                new XAttribute("type", type),
                new XAttribute("dname", dexName),
                new XAttribute("dtype", dexType));
        }

        /// <summary>
        /// Name of member in .NET
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Type of member in .NET (field type, property type)
        /// </summary>
        public string Type { get { return type; } }

        /// <summary>
        /// Name of member in Dex
        /// </summary>
        public string DexName { get { return dexName; } }

        /// <summary>
        /// Type of member in Dex (field type, property type)
        /// </summary>
        public string DexType { get { return dexType; } }
    }
}
