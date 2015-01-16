using System.Xml;
using System.Xml.Linq;

namespace Dot42.Utility
{
    public static class XmlExtensions
    {
        /// <summary>
        /// Gets the formatted source position of the given XML object.
        /// </summary>
        public static string FormatLineInfo(this XObject xObject)
        {
            var info = xObject as IXmlLineInfo;
            if ((info == null) || (!info.HasLineInfo()))
                return "?";
            return string.Format("Ln {0}, Col {1}", info.LineNumber, info.LinePosition);
        }

        /// <summary>
        /// Gets the value of an attribute.
        /// </summary>
        public static string GetAttribute(this XElement element, XName name)
        {
            var attr = element.Attribute(name);
            return (attr != null) ? attr.Value : null;
        }

        /// <summary>
        /// Gets the inner value of an element.
        /// </summary>
        public static string GetElementValue(this XElement element, XName name)
        {
            var x = element.Element(name);
            return (x != null) ? x.Value : null;
        }
    }
}
