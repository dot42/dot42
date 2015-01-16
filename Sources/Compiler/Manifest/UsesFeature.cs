using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string UsesFeatureAttribute = "UsesFeatureAttribute";
        private const string UsesOpenGLAttribute = "UsesOpenGLAttribute";

        /// <summary>
        /// Create all uses-feature elements
        /// </summary>
        private void CreateUsesFeature(XElement manifest)
        {
            // Collect all attributes
            var features = new Dictionary<string, bool>();
            foreach (var attr in assembly.GetAttributesFromAllAssemblies(UsesFeatureAttribute))
            {
                var name = attr.GetValue<string>(0, "Name");
                var required = attr.GetValue(1, "Required", true);

                if (!features.ContainsKey(name) || required)
                {
                    features[name] = required;
                }
            }

            // Generate uses-feature elements
            foreach (var entry in features)
            {
                var name = entry.Key;
                var required = entry.Value;

                var element = new XElement("uses-feature");
                element.AddAttr("name", Namespace, name);
                element.AddAttrIfNotDefault("required", Namespace, required, true);

                manifest.Add(element);
            }

            // Collect opengl version
            var maxVersion = -1;
            var hasVersion = false;
            foreach (var attr in assembly.GetAttributesFromAllAssemblies(UsesOpenGLAttribute))
            {
                var version = attr.GetValue<int>(0, "Version");
                maxVersion = Math.Max(maxVersion, version);
                hasVersion = true;
            }

            // Generate uses-feature element
            if (hasVersion)
            {
                var element = new XElement("uses-feature");
                element.AddAttr("glEsVersion", Namespace, maxVersion.ToString());

                manifest.Add(element);
            }
        }
    }
}
