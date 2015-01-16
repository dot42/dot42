using System.Collections.Generic;
using System.Linq;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;

namespace Dot42.LoaderLib.Extensions
{
    /// <summary>
    /// JRef related extension methods
    /// </summary>
    public static partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Gets all JavaCode attributes (if any);
        /// </summary>
        internal static IEnumerable<CustomAttribute> GetJavaCodeAttributes(this ICustomAttributeProvider provider)
        {
            if (!provider.HasCustomAttributes)
            {
                return Enumerable.Empty<CustomAttribute>();
            }
            return provider.CustomAttributes.Where(x =>
                                                   (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                                                   (x.AttributeType.Name == AttributeConstants.JavaCodeAttributeName));
        }
    }
}
