using System.Xml.Linq;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string PermissionGroupAttribute = "PermissionGroupAttribute";

        /// <summary>
        /// Create all permission-group elements
        /// </summary>
        private void CreatePermissionGroup(XElement manifest)
        {
            // Create services
            foreach (var attr in assembly.GetAttributes(PermissionGroupAttribute))
            {
                var permissionGroup = new XElement("permission-group");
                manifest.Add(permissionGroup);

                permissionGroup.AddAttr("name", Namespace, attr.GetValue<string>(0, "Name"));
                permissionGroup.AddAttrIfNotEmpty("label", Namespace, attr.GetValue<string>("Label"), FormatStringOrLiteral);
                permissionGroup.AddAttrIfNotEmpty("description", Namespace, attr.GetValue<string>("Description"), FormatString);
                permissionGroup.AddAttrIfNotEmpty("icon", Namespace, attr.GetValue<string>("Icon"), FormatDrawable);
            }
        }
    }
}
