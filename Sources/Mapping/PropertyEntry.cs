using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.Mapping
{
    /// <summary>
    /// Property typed entry
    /// </summary>
    public sealed class PropertyEntry : MemberEntry
    {
        private readonly string signature;

        /// <summary>
        /// XML ctor
        /// </summary>
        internal PropertyEntry(XElement e)
            : base(e)
        {
            signature = e.GetAttribute("signature");
            if (string.IsNullOrEmpty(signature)) { signature = "()"; }
        }

        /// <summary>
        /// Convert to XML element
        /// </summary>
        internal override XElement ToXml(string elementName)
        {
            var e = base.ToXml(elementName);
            e.Add(new XAttribute("signature", signature));
            return e;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public PropertyEntry(string name, string type, string dexName, string dexType, string signature)
            : base(name, type, dexName, dexType)
        {
            this.signature = signature;
        }

        /// <summary>
        /// Property signature in .NET
        /// </summary>
        public string Signature { get { return signature; } }
    }
}
