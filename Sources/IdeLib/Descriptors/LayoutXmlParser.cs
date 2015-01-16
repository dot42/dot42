using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Dot42.Ide.Descriptors
{
    /// <summary>
    /// Parser for layout.xml
    /// </summary>
    internal sealed class LayoutXmlParser
    {
        private readonly Dictionary<string, LayoutElement> viewElements = new Dictionary<string, LayoutElement>();
        private readonly Dictionary<string, LayoutElement> viewGroupElements = new Dictionary<string, LayoutElement>();

        /// <summary>
        /// Default ctor
        /// </summary>
        internal LayoutXmlParser(Stream stream)
        {
            if (stream == null)
                return;
            var document = XDocument.Load(stream);
            foreach (var child in document.Root.Elements("view"))
            {
                var element = Parse(child, LayoutType.View);
                viewElements[element.Name] = element;
            }
            foreach (var child in document.Root.Elements("viewgroup"))
            {
                var element = Parse(child, LayoutType.ViewGroup);
                viewGroupElements[element.Name] = element;
            }
        }

        /// <summary>
        /// Lookup a layout element by the given short name.
        /// </summary>
        /// <returns>Null if not found</returns>
        internal LayoutElement FindElement(string shortName)
        {
            LayoutElement element;
            if (viewElements.TryGetValue(shortName, out element))
                return element;
            if (viewGroupElements.TryGetValue(shortName, out element))
                return element;
            return null;
        }

        /// <summary>
        /// Gets all view elements
        /// </summary>
        internal IEnumerable<LayoutElement> ViewElements
        {
            get { return viewElements.Values; }
        }

        /// <summary>
        /// Gets all viewgroup elements
        /// </summary>
        internal IEnumerable<LayoutElement> ViewGroupElements
        {
            get { return viewGroupElements.Values; }
        }

        /// <summary>
        /// Parse an attr element into an <see cref="AttributeDescriptor"/>.
        /// </summary>
        private static LayoutElement Parse(XElement element, LayoutType type)
        {
            var name = GetName(element);
            var isAbstract = GetAbstract(element);
            var superClassName = GetSuperClassName(element);
            return new LayoutElement(name, type, isAbstract, superClassName);
        }

        /// <summary>
        /// Gets the name attribute of the given element.
        /// </summary>
        private static string GetName(XElement element)
        {
            var attr = element.Attribute("name");
            return (attr != null) ? attr.Value : null;
        }

        /// <summary>
        /// Gets the super attribute of the given element.
        /// </summary>
        private static string GetSuperClassName(XElement element)
        {
            var attr = element.Attribute("super");
            return (attr != null) ? attr.Value : null;
        }

        /// <summary>
        /// Gets the format attribute of the given element.
        /// </summary>
        private static bool GetAbstract(XElement element)
        {
            var attr = element.Attribute("abstract");
            return (attr != null) && bool.Parse(attr.Value);
        }
    }
}
