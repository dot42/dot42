using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using Dot42.ApkLib;
using Dot42.CompilerLib;

namespace Dot42.Compiler.Resources
{
    /// <summary>
    /// Process Layout resource files.
    /// </summary>
    internal sealed class LayoutResourceProcessor : XmlResourceProcessor
    {
        private readonly NameConverter nameConverter;
        private readonly HashSet<string> customClassNames = new HashSet<string>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public LayoutResourceProcessor(NameConverter nameConverter)
        {
            this.nameConverter = nameConverter;
        }

        /// <summary>
        /// Gets the names of all custom classes used in this layout file.
        /// </summary>
        public HashSet<string> CustomClassNames
        {
            get { return customClassNames; }
        }

        /// <summary>
        /// Process this XML resource.
        /// </summary>
        /// <param name="document">The XML document</param>
        /// <returns>True if changes were made, false otherwise</returns>
        protected override bool Process(XDocument document)
        {
            return Process(document.Root);
        }

        /// <summary>
        /// Process the given element, all it's attributes and it's child elements.
        /// </summary>
        /// <returns>True if changes were made, false otherwise</returns>
        private bool Process(XElement element)
        {
            var result = false;

            // Process android:name / class
            var localName = element.Name.LocalName;
            if ((localName == "fragment") || (localName == "view"))
            {
                var nameAttr = element.Attribute(XName.Get("name", AndroidConstants.AndroidNamespace));
                if (nameAttr != null)
                {
                    var typeName = nameAttr.Value;
                    customClassNames.Add(typeName);
                    nameAttr.Value = nameConverter.GetConvertedFullName(typeName);
                    result = true;
                }
                var classAttr = element.Attribute("class");
                if (classAttr != null)
                {
                    var typeName = classAttr.Value;
                    customClassNames.Add(typeName);
                    classAttr.Value = nameConverter.GetConvertedFullName(typeName);
                    result = true;
                }
            }
            else
            {
                customClassNames.Add(localName);
            }

            // Process custom element names
            if (localName.StartsWith(nameConverter.RootNamespaceDot))
            {
                // Class name used as element name
                element.Name = nameConverter.GetConvertedFullName(localName);
                result = true;
            }
            
            // Process child elements
            foreach (var child in element.Elements())
            {
                result |= Process(child);
            }

            return result;
        }
    }
}
