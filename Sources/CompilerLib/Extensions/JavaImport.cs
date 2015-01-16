using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.FrameworkDefinitions;
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
        /// Gets the first method the given method overrides that has a JavaImport attribute.
        /// </summary>
        internal static MethodDefinition GetJavaImportBaseMethod(this MethodDefinition method)
        {
            while (true)
            {
                var baseMethod = method.GetBaseMethod();
                if (baseMethod == null)
                    return null;

                if (baseMethod.GetJavaImportAttribute() != null)
                    return baseMethod;

                method = baseMethod;
            }
        }

        /// <summary>
        /// Gets the first method the given method overrides from an implemented interface that has a JavaImport attribute.
        /// </summary>
        internal static MethodDefinition GetJavaImportBaseInterfaceMethod(this MethodDefinition method)
        {
            var iMethod = method.GetBaseInterfaceMethod();
            if (iMethod == null)
                return null;

            if (iMethod.GetJavaImportAttribute() != null)
                return iMethod;

            return null;
        }

        /// <summary>
        /// Gets the first JavaImport attribute (if any);
        /// </summary>
        /*internal static CustomAttribute GetJavaImportAttribute(this ICustomAttributeProvider provider, bool failOnNotFound = false)
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
        }*/

        /// <summary>
        /// Is there any JavaImport attribute attached to the given provider?
        /// </summary>
        internal static bool HasJavaImportAttribute(this ICustomAttributeProvider provider)
        {
            return (provider.GetJavaImportAttribute() != null);
        }
    }
}
