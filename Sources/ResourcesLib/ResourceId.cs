using System;
using System.Linq;

namespace Dot42.ResourcesLib
{
    /// <summary>
    /// Wrapper for a resource reference.
    /// Supports android and dot42 formats.
    /// </summary>
    public sealed class ResourceId
    {
        private readonly string packageName;
        private readonly bool create;
        private readonly ResourceType type;
        private readonly string name;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ResourceId(string packageName, bool create, ResourceType type, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            this.packageName = string.IsNullOrEmpty(packageName) ? null : packageName;
            this.create = create;
            this.type = type;
            this.name = name;
        }

        /// <summary>
        /// Package part
        /// </summary>
        public string PackageName
        {
            get { return packageName; }
        }

        /// <summary>
        /// Does this resource have a package part?
        /// </summary>
        public bool HasPackageName { get { return (packageName != null); } }

        /// <summary>
        /// Auto create id if needed ('+')
        /// </summary>
        public bool Create
        {
            get { return create; }
        }

        /// <summary>
        /// Type of resource
        /// </summary>
        public ResourceType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Name part
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Convert to a string representation.
        /// </summary>
        public string ToString(ResourceIdFormat format)
        {
            //var normalizedName = ResourceExtensions.GetNormalizedResourceName(name);
            var normalizedName = name;
            switch (format)
            {
                case ResourceIdFormat.AndroidXml:
                    if (HasPackageName)
                        return string.Format("@{0}:{1}/{2}", packageName, type.GetXmlName(), normalizedName);
                    return string.Format("@{0}/{1}", type.GetXmlName(), normalizedName);
                case ResourceIdFormat.AndroidXmlWithCreate:
                    return string.Format("@{0}{1}/{2}", create ? "+" : "", type.GetXmlName(), normalizedName);
                default:
                    throw new ArgumentException(string.Format("Unknown resource id format {0}", (int)format));
            }
        }

        /// <summary>
        /// Convert to standard Android XML format.
        /// </summary>
        public override string ToString()
        {
            return ToString(ResourceIdFormat.AndroidXml);
        }

        /// <summary>
        /// Try to parse the given string into a resource id.
        /// </summary>
        public static bool TryParse(string value, ResourceType defaultType, out ResourceId resourceId)
        {
            resourceId = null;
            if (string.IsNullOrEmpty(value))
                return false;
            value = value.Trim();
            if (value.Length == 0)
                return false;

            // Skip leading '@' 
            var hasAt = (value[0] == '@');
            if (hasAt)
            {
                value = value.Substring(1);
                if (value.Length == 0)
                    return false;
            }

            // Look for package prefix
            var index = value.IndexOf(':');
            string packageName = null;
            if (index > 0)
            {
                packageName = value.Substring(0, index);
                value = value.Substring(index + 1);
            }
            var hasPackageName = (packageName != null);
            
            // Look for '+' 
            if (value.Length == 0)
                return false;
            var create = false;
            if (value[0] == '+')
            {
                create = true;
                value = value.Substring(1);
                if (value.Length == 0)
                    return false;
            }
            
            // Look for type
            index = value.IndexOf('/');
            ResourceType type;
            if (index <= 0)
            {
                // Did not find a resource type, try to use default
                if (defaultType == ResourceType.Unknown)
                    return false;
                type = defaultType;
            }
            else
            {
                // Found a resource type, try to parse it
                var typeName = value.Substring(0, index);
                value = value.Substring(index + 1);
                if (!TryParseType(typeName, out type))
                    return false;
                if (value.Length == 0)
                    return false;
            }
            
            // Remainder is name
            if (!(hasPackageName))
                value = ResourceExtensions.GetNormalizedResourceName(value, type);
            var name = value;
            resourceId = new ResourceId(packageName, create, type, name);
            return true;
        }

        /// <summary>
        /// Try to parse the given value to a resource type.
        /// </summary>
        private static bool TryParseType(string value, out ResourceType type)
        {
            type = 0;
            if (string.IsNullOrEmpty(value))
                return false;
            // Try by android xml name
            var info = ResourceExtensions.GetResourceTypeInfos().FirstOrDefault(x => x.XmlName == value);
            if (info != null)
            {
                type = info.Type;
                return true;
            }
            // Try by Dot42 ID name
            info = ResourceExtensions.GetResourceTypeInfos().FirstOrDefault(x => x.Dot42Name == value);
            if (info != null)
            {
                type = info.Type;
                return true;
            }
            // Try by R nested class name
            info = ResourceExtensions.GetResourceTypeInfos().FirstOrDefault(x => x.RNestedClassName == value);
            if (info != null)
            {
                type = info.Type;
                return true;
            }
            // Not found
            return false;
        }
    }
}
