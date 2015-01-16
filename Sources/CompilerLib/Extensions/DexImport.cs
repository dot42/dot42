using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.FrameworkDefinitions;
using Dot42.ImportJarLib.Extensions;
using Dot42.LoaderLib.Extensions;
using Mono.Cecil;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// JRef related extension methods
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Gets the first method the given method overrides that has a DexImport attribute.
        /// </summary>
        internal static MethodDefinition GetDexImportBaseMethod(this MethodDefinition method)
        {
            while (true)
            {
                var baseMethod = method.GetBaseMethod();
                if (baseMethod == null)
                    return null;

                if (baseMethod.GetDexImportAttribute() != null)
                    return baseMethod;

                method = baseMethod;
            }
        }

        /// <summary>
        /// Gets the first method the given method overrides from an implemented interface that has a DexImport attribute.
        /// </summary>
        internal static MethodDefinition GetDexImportBaseInterfaceMethod(this MethodDefinition method)
        {
            var iMethod = method.GetBaseInterfaceMethod();
            if (iMethod == null)
                return null;

            if (iMethod.GetDexImportAttribute() != null)
                return iMethod;

            return null;
        }

        /// <summary>
        /// Gets the first DexName attribute (if any);
        /// </summary>
        internal static CustomAttribute GetDexNameAttribute(this ICustomAttributeProvider provider)
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
                    (x.AttributeType.Name == AttributeConstants.DexNameAttributeName));
            }
            return result;
        }

        /// <summary>
        /// Gets the first DexNative attribute (if any);
        /// </summary>
        internal static CustomAttribute GetDexNativeAttribute(this ICustomAttributeProvider provider)
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
                    (x.AttributeType.Name == AttributeConstants.DexNativeAttributeName));
            }
            return result;
        }

        /// <summary>
        /// Is there any JRef attribute attached to the given provider?
        /// </summary>
        internal static bool HasDexImportAttribute(this ICustomAttributeProvider provider)
        {
            return (provider.GetDexImportAttribute() != null);
        }

        /// <summary>
        /// Is there any DexNative attribute attached to the given provider?
        /// </summary>
        internal static bool HasDexNativeAttribute(this ICustomAttributeProvider provider)
        {
            return (provider.GetDexNativeAttribute() != null);
        }

        /// <summary>
        /// Gets the first ResourceId attribute (if any);
        /// </summary>
        internal static CustomAttribute GetResourceIdAttribute(this ICustomAttributeProvider provider)
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
                    (x.AttributeType.Name == AttributeConstants.ResourceIdAttributeName));
            }
            return result;
        }

        /// <summary>
        /// Is there any ResourceId attribute attached to the given provider?
        /// </summary>
        internal static bool HasResourceIdAttribute(this ICustomAttributeProvider provider)
        {
            return (provider.GetResourceIdAttribute() != null);
        }
    }
}
