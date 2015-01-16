using System.Linq;
using System.ServiceModel.Web;
using Mono.Cecil;

namespace Dot42.WcfTools.Extensions
{
    /// <summary>
    /// WCF related extension methods
    /// </summary>
    internal static class WcfExtensions
    {
        /// <summary>
        /// Is there a ServiceContractAttribute on the given type?
        /// </summary>
        internal static bool HasServiceContractAttribute(this TypeDefinition type)
        {
            if (!type.HasCustomAttributes)
                return false;
            return type.CustomAttributes.Any(x => x.AttributeType.FullName == WcfAttributeConstants.ServiceContractAttribute);
        }

        internal static string GetStringProperty(this CustomAttribute customAttribute, string propertyName)
        {
            var nameProperty = customAttribute.Properties.FirstOrDefault(x => x.Name == propertyName);
            if (nameProperty.Name == propertyName)
            {
                return nameProperty.Argument.Value as string;
            }

            return null;
        }

        internal static WebMessageFormat GetWebMessageFormatProperty(this CustomAttribute customAttribute, string propertyName)
        {
            var nameProperty = customAttribute.Properties.FirstOrDefault(x => x.Name == propertyName);
            if (nameProperty.Name == propertyName)
            {
                return (WebMessageFormat)nameProperty.Argument.Value;
            }

            return WebMessageFormat.Xml;
        }

        internal static bool? GetBooleanProperty(this CustomAttribute customAttribute, string propertyName)
        {
            var nameProperty = customAttribute.Properties.FirstOrDefault(x => x.Name == propertyName);
            if (nameProperty.Name == propertyName)
            {
                return (bool)nameProperty.Argument.Value;
            }

            return null;
        }

        internal static int? GetIntProperty(this CustomAttribute customAttribute, string propertyName)
        {
            var nameProperty = customAttribute.Properties.FirstOrDefault(x => x.Name == propertyName);
            if (nameProperty.Name == propertyName)
            {
                return (int) nameProperty.Argument.Value;
            }

            return null;
        }
    }
}
