using System;
using System.Xml.Linq;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string PermissionAttribute = "PermissionAttribute";
        private static readonly EnumFormatter protectionLevelsOptions = new EnumFormatter(
            Tuple.Create(0, "normal"),
            Tuple.Create(1, "dangerous"),
            Tuple.Create(2, "signature"),
            Tuple.Create(3, "signatureOrSystem")
            );

        /// <summary>
        /// Create all permission elements
        /// </summary>
        private void CreatePermission(XElement manifest)
        {
            // Create services
            foreach (var attr in assembly.GetAttributes(PermissionAttribute))
            {
                var permission = new XElement("permission");
                manifest.Add(permission);

                permission.AddAttr("name", Namespace, attr.GetValue<string>(0, "Name"));
                permission.AddAttrIfNotEmpty("label", Namespace, attr.GetValue<string>("Label"), FormatStringOrLiteral);
                permission.AddAttrIfNotEmpty("description", Namespace, attr.GetValue<string>("Description"), FormatString);
                permission.AddAttrIfNotEmpty("icon", Namespace, attr.GetValue<string>("Icon"), FormatDrawable);
                permission.AddAttrIfNotEmpty("permissionGroup", Namespace, attr.GetValue<string>("PermissionGroup"));
                permission.AddAttrIfNotDefault("protectionLevel", Namespace, attr.GetValue<int>("ProtectionLevel"), 0, protectionLevelsOptions.Format);
            }
        }
    }
}
