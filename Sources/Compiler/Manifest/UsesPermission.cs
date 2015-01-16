using System.Collections.Generic;
using System.Xml.Linq;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string UsesPermissionAttribute = "UsesPermissionAttribute";

        /// <summary>
        /// Create all uses-permission elements
        /// </summary>
        private void CreateUsesPermission(XElement manifest)
        {
            // Generate xml
            var permissions = new Dictionary<string, string>();
            foreach (var attr in assembly.GetAttributesFromAllAssemblies(UsesPermissionAttribute))
            {
                var name = (string)attr.ConstructorArguments[0].Value;
                if (permissions.ContainsKey(name))
                    continue;

                permissions[name] = name;
                var element = new XElement("uses-permission");
                element.AddAttr("name", Namespace, name);

                manifest.Add(element);
            }
        }
    }
}
