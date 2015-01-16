using System.Collections.Generic;
using System.Xml.Linq;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string UsesLibraryAttribute = "UsesLibraryAttribute";

        /// <summary>
        /// Create all uses-library elements
        /// </summary>
        private void CreateUsesLibrary(XElement application)
        {
            // Collect all attributes
            var libraries = new Dictionary<string, bool>();
            foreach (var attr in assembly.GetAttributesFromAllAssemblies(UsesLibraryAttribute))
            {
                var name = attr.GetValue<string>(0, "Name");
                var required = attr.GetValue(1, "Required", true);

                if (!libraries.ContainsKey(name) || required)
                {
                    libraries[name] = required;
                }
            }

            // Create XML
            foreach (var entry in libraries)
            {
                var name = entry.Key;
                var required = entry.Value;

                var element = new XElement("uses-library");
                element.AddAttr("name", Namespace, name);
                element.AddAttrIfNotDefault("required", Namespace, required, true);

                application.Add(element);
            }
        }
    }
}
