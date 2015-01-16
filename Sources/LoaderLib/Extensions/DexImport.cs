using System;
using System.Linq;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;

namespace Dot42.LoaderLib.Extensions
{
    /// <summary>
    /// JRef related extension methods
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Gets the first DexImport attribute (if any);
        /// </summary>
        public static CustomAttribute GetDexImportAttribute(this ICustomAttributeProvider provider, bool failOnNotFound = false)
        {
            CustomAttribute result;
            if (!provider.HasCustomAttributes)
            {
                result = null;
            }
            else 
            {
                result = provider.CustomAttributes.FirstOrDefault(x =>
                    (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                    (x.AttributeType.Name == AttributeConstants.DexImportAttributeName));
            }
            if ((result == null) && failOnNotFound)
            {
                throw new ArgumentException(string.Format("No {0} attribute found in {1}", AttributeConstants.DexImportAttributeName, provider));
            }
            return result;
        }

        /// <summary>
        /// Gets the first DexImport or JavaImport attribute (if any);
        /// </summary>
        public static CustomAttribute GetDexOrJavaImportAttribute(this ICustomAttributeProvider provider, bool failOnNotFound = false)
        {
            CustomAttribute result;
            if (!provider.HasCustomAttributes)
            {
                result = null;
            }
            else
            {
                result = provider.CustomAttributes.FirstOrDefault(x =>
                    (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                    ((x.AttributeType.Name == AttributeConstants.DexImportAttributeName) || (x.AttributeType.Name == AttributeConstants.JavaImportAttributeName)));
            }
            if ((result == null) && failOnNotFound)
            {
                throw new ArgumentException(string.Format("No {0} or {1} attribute found in {2}", AttributeConstants.DexImportAttributeName, AttributeConstants.JavaImportAttributeName, provider));
            }
            return result;
        }

        /// <summary>
        /// Gets the first JavaImport attribute (if any);
        /// </summary>
        public static CustomAttribute GetJavaImportAttribute(this ICustomAttributeProvider provider, bool failOnNotFound = false)
        {
            CustomAttribute result;
            if (!provider.HasCustomAttributes)
            {
                result = null;
            }
            else
            {
                result = provider.CustomAttributes.FirstOrDefault(x =>
                    (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                    (x.AttributeType.Name == AttributeConstants.JavaImportAttributeName));
            }
            if ((result == null) && failOnNotFound)
            {
                throw new ArgumentException(string.Format("No {0} attribute found in {1}", AttributeConstants.JavaImportAttributeName, provider));
            }
            return result;
        }
    }
}
