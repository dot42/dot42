using System.Collections.Generic;
using System.Xml.Linq;

namespace Dot42.Compiler.Resources
{
    /// <summary>
    /// Process Values resource files.
    /// </summary>
    internal sealed class ValuesResourceProcessor : XmlResourceProcessor
    {
        private readonly List<StyleableDeclaration> styleableDeclarations = new List<StyleableDeclaration>();

        /// <summary>
        /// Process this XML resource.
        /// </summary>
        /// <param name="document">The XML document</param>
        /// <returns>True if changes were made, false otherwise</returns>
        protected override bool Process(XDocument document)
        {
            styleableDeclarations.AddRange(StyleableDeclaration.Parse(document));
            return false;
        }

        /// <summary>
        /// Gets the collected declarations
        /// </summary>
        public List<StyleableDeclaration> StyleableDeclarations { get { return styleableDeclarations; } }
    }
}
