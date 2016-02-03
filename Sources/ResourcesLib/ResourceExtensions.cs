using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dot42.ResourcesLib
{
    public static class ResourceExtensions
    {
        private static readonly Dictionary<ResourceType, ResourceTypeInfo> ResourceTypeInfos = CreateDictionary(
            // Folder types
            new ResourceTypeInfo(ResourceType.Animation, "res\\anim", "anim", "Animation", "Animations"),
            new ResourceTypeInfo(ResourceType.Drawable, "res\\drawable", "drawable", "Drawable", "Drawables"),
            new ResourceTypeInfo(ResourceType.Layout, "res\\layout", "layout", "Layout", "Layouts"),
            new ResourceTypeInfo(ResourceType.Menu, "res\\menu", "menu", "Menu", "Menus"),
            new ResourceTypeInfo(ResourceType.Values, "res\\values", "values", "Value", "Values"),
            new ResourceTypeInfo(ResourceType.Xml, "res\\xml", "xml", "Xml", "Xmls"),
            new ResourceTypeInfo(ResourceType.Raw, "res\\raw", "raw", "Raw", "Raws"),
            // ID types
            new ResourceTypeInfo(ResourceType.Attr, "res\\values", "attr", "Attribute", "Attributes"),
            new ResourceTypeInfo(ResourceType.Bool, "res\\values", "bool", "Boolean", "Booleans"),
            new ResourceTypeInfo(ResourceType.Color, "res\\values", "color", "Color", "Colors"),
            new ResourceTypeInfo(ResourceType.Dimension, "res\\values", "dimen", "Dimension", "Dimensions"),
            new ResourceTypeInfo(ResourceType.Id, "res\\values", "id", "Id", "Ids"),
            new ResourceTypeInfo(ResourceType.Integer, "res\\values", "integer", "Integer", "Integers"),
            new ResourceTypeInfo(ResourceType.IntegerArray, "res\\values", "integer-array", "IntegerArray", "IntegerArrays"),
            new ResourceTypeInfo(ResourceType.TypedArray, "res\\values", "array", "TypedArray", "TypedArrays"),
            new ResourceTypeInfo(ResourceType.String, "res\\values", "string", "String", "Strings"),
            new ResourceTypeInfo(ResourceType.Style, "res\\values", "style", "Style", "Styles"),
            new ResourceTypeInfo(ResourceType.Plural, "res\\values", "plural", "Plural", "Plurals"),
            new ResourceTypeInfo(ResourceType.StringArray, "res\\values", "array", "StringArray", "StringArrays"),
            // Dot42 custom types
            new ResourceTypeInfo(ResourceType.Manifest, "", "", "", "")
            );

        /// <summary>
        /// Gets the information about all known resource types.
        /// </summary>
        internal static IEnumerable<ResourceTypeInfo> GetResourceTypeInfos()
        {
            return ResourceTypeInfos.Values;
        }

        /// <summary>
        /// Gets the name used in XML resources references.
        /// </summary>
        public static string GetXmlName(this ResourceType type)
        {
            ResourceTypeInfo info;
            if (ResourceTypeInfos.TryGetValue(type, out info))
                return info.XmlName;
            throw new ArgumentException("Unsupported type: " + (int)type);
        }

        /// <summary>
        /// Get a resource type from it's XML name.
        /// </summary>
        public static ResourceType GetResourceTypeFromXmlName(string xmlName)
        {
            return GetResourceTypeInfos().First(x => x.XmlName == xmlName).Type;
        }

        /// <summary>
        /// Gets the name used in Dot42 resources references.
        /// </summary>
        public static string GetDot42Name(this ResourceType type)
        {
            ResourceTypeInfo info;
            if (ResourceTypeInfos.TryGetValue(type, out info))
                return info.Dot42Name;
            throw new ArgumentException("Unsupported type: " + (int)type);
        }
        
        /// <summary>
        /// Gets the folder in which the given xml document will be stored.
        /// </summary>
        public static string GetRelativeFolder(this ResourceType type)
        {
            ResourceTypeInfo info;
            if (ResourceTypeInfos.TryGetValue(type, out info))
                return info.FolderName;
            throw new ArgumentException("Unsupported type: " + (int)type);
        }

        /// <summary>
        /// Convert the given resource filename into a valid resource name.
        /// All alternate qualifiers are removed, the extension is removed and the name is converted to lower case.
        /// </summary>
        public static string GetNormalizedResourceName(string resourceFile, ResourceType type)
        {
            var normalizeName = ((type < ResourceType.FirstIdType) || (type > ResourceType.LastIdType));
            return ConfigurationQualifiers.StripQualifiers(resourceFile, true, normalizeName);
        }

        /// <summary>
        /// Convert the given resource filename into a filename valid for a resource.
        /// All alternate qualifiers are removed and the name is converted to lower case.
        /// The extension is preserved.
        /// </summary>
        public static string GetNormalizedResourceFileName(string resourceFile)
        {
            return ConfigurationQualifiers.StripQualifiers(resourceFile, false, true);
        }

        /// <summary>
        /// Convert the given resource filename into a full path (in the given folder) valid for a resource.
        /// All alternate qualifiers are removed and the name is converted to lower case.
        /// The extension is preserved.
        /// </summary>
        public static string GetNormalizedResourcePath(string folder, string resourceFile, ResourceType resourceType)
        {
            // try the directory name first.
            string directoryName = Path.GetDirectoryName(resourceFile);
            var configQualifiers = ConfigurationQualifiers.Parse(directoryName);

            if (string.IsNullOrEmpty(configQualifiers.ToString()))
            {
                configQualifiers = ConfigurationQualifiers.Parse(resourceFile);
            }

            var outputFolder = Path.Combine(folder, resourceType.GetRelativeFolder() + configQualifiers);
            var normalizedResourceFileName = GetNormalizedResourceFileName(resourceFile);
            return Path.Combine(outputFolder, normalizedResourceFileName);
        }

        /// <summary>
        /// Convert a list of resource type info's to a dictionary.
        /// </summary>
        private static Dictionary<ResourceType, ResourceTypeInfo> CreateDictionary(params ResourceTypeInfo[] infos)
        {
            var dict = new Dictionary<ResourceType, ResourceTypeInfo>();
            foreach (var info in infos)
            {
                dict[info.Type] = info;
            }
            return dict;
        }

        /// <summary>
        /// Information about a type of resource.
        /// </summary>
        internal sealed class ResourceTypeInfo
        {
            private readonly ResourceType type;
            private readonly string folderName;
            private readonly string xmlName;
            private readonly string dot42Name;
            private readonly string rNestedClassName;

            public ResourceTypeInfo(ResourceType type, string folderName, string xmlName, string dot42Name, string rNestedClassName)
            {
                this.type = type;
                this.folderName = folderName;
                this.xmlName = xmlName;
                this.dot42Name = dot42Name;
                this.rNestedClassName = rNestedClassName;
            }

            public ResourceType Type
            {
                get { return type; }
            }

            public string FolderName
            {
                get { return folderName; }
            }

            public string XmlName
            {
                get { return xmlName; }
            }

            public string Dot42Name
            {
                get { return dot42Name; }
            }

            public string RNestedClassName
            {
                get { return rNestedClassName; }
            }
        }
    }
}
