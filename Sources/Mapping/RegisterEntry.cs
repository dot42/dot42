using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.Mapping
{
    /// <summary>
    /// Base class for register mapping entries.
    /// </summary>
    public abstract class RegisterEntry
    {
        private readonly int register;
        private readonly string name;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected RegisterEntry(int register, string name)
        {
            this.register = register;
            this.name = name;
        }

        /// <summary>
        /// XML ctor
        /// </summary>
        internal RegisterEntry(XElement e)
        {
            this.name = e.GetAttribute("n");
            register = int.Parse(e.GetAttribute("r"));
        }

        /// <summary>
        /// Register number
        /// </summary>
        public int Register
        {
            get { return register; }
        }

        /// <summary>
        /// Variable/parameter name
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Convert to XML element
        /// </summary>
        internal virtual XElement ToXml(string elementName)
        {
            var e = new XElement(elementName,
                new XAttribute("n", Name),
                new XAttribute("r", Register.ToString()));
            return e;
        }
    }
}
