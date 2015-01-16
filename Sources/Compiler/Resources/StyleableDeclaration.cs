using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.Compiler.Resources
{
    /// <summary>
    /// Wrapper around &lt;declare-styleable&gt;
    /// </summary>
    public class StyleableDeclaration
    {
        private readonly string name;
        private readonly List<string> attributeNames;

        /// <summary>
        /// Default ctor
        /// </summary>
        private StyleableDeclaration(string name, IEnumerable<string> attributeNames)
        {
            this.name = name;
            this.attributeNames = attributeNames.ToList();
        }

        public string Name
        {
            get { return name; }
        }

        public IEnumerable<string> AttributeNames
        {
            get { return attributeNames; }
        }

        /// <summary>
        /// Yield all declare-styleable elements from the given xml file.
        /// </summary>
        public static IEnumerable<StyleableDeclaration> Parse(XDocument document)
        {
            var root = document.Root;
            if (root.Name.LocalName != "resources")
                yield break;

            foreach (var element in root.Elements("declare-styleable"))
            {
                var name = GetName(element);
                if (name == null)
                    continue;

                var attributeNames = element.Elements("attr").Select(GetName).Where(x => x != null);
                yield return new StyleableDeclaration(name, attributeNames);
            }
        }

        /// <summary>
        /// Gets the value of the "name" attribute from the given element.
        /// </summary>
        private static string GetName(XElement element)
        {
            var nameAttr = element.Attribute("name");
            return (nameAttr != null) ? nameAttr.Value : null;
        }
    }
}
