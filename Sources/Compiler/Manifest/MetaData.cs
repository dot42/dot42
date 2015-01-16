using System.Xml.Linq;
using Mono.Cecil;
using ResourceType = Dot42.ResourcesLib.ResourceType;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string MetaDataAttribute = "MetaDataAttribute";
        
        /// <summary>
        /// Create all meta-data elements
        /// </summary>
        private void CreateMetaData(XElement parent, ICustomAttributeProvider provider)
        {
            // Create meta-data elements
            foreach (var attr in provider.GetAttributes(MetaDataAttribute))
            {
                var metaData = new XElement("meta-data");
                parent.Add(metaData);

                metaData.AddAttrIfNotEmpty("name", Namespace, attr.GetValue<string>(-1, "Name"));
                metaData.AddAttrIfNotEmpty("value", Namespace, attr.GetValue<string>(-1, "Value"));
                metaData.AddAttrIfNotEmpty("resource", Namespace, attr.GetValue<string>(-1, "Resource"), x => FormatResourceId(x, ResourceType.Unknown));
            }
        }
    }
}
