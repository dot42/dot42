using System.Collections.Generic;
using System.Linq;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Dot42 attribute related extension methods
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Is there any Ignore attribute attached to the given provider?
        /// </summary>
        internal static bool HasIgnoreAttribute(this ICustomAttributeProvider provider)
        {
            if (!provider.HasCustomAttributes)
                return false;
            return provider.CustomAttributes.Any(x =>
                (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                (x.AttributeType.Name == AttributeConstants.IgnoreAttributeName));
        }

        /// <summary>
        /// Is there any Include attribute attached to the given provider?
        /// </summary>
        internal static IEnumerable<CustomAttribute> GetIncludeAttributes(this ICustomAttributeProvider provider)
        {
            if (!provider.HasCustomAttributes)
                return Enumerable.Empty<CustomAttribute>();
            return provider.CustomAttributes.Where(x =>
                (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) 
                && (x.AttributeType.Name == AttributeConstants.IncludeAttributeName
                 || x.AttributeType.Name == AttributeConstants.IncludeTypeAttributeName));
        }

        /// <summary>
        /// Is there any Include attribute attached to the given provider?
        /// </summary>
        internal static bool HasIncludeAttribute(this ICustomAttributeProvider provider)
        {
            return provider.GetIncludeAttributes().Any();
        }

        internal static bool HasSerializationMethodAttribute(this MethodDefinition method)
        {
            return method.CustomAttributes
                .Any(a => a.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace
                       && a.AttributeType.Name == AttributeConstants.SerializationMethodAttributeName);
        }

        /// <summary>
        /// Is there any EventHandler attached to the given provider?
        /// </summary>
        internal static bool HasEventHandlerAttribute(this ICustomAttributeProvider provider)
        {
            if (!provider.HasCustomAttributes)
                return false;
            return provider.CustomAttributes.Any(x =>
                (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                (x.AttributeType.Name == AttributeConstants.EventHandlerAttributeName));
        }

        /// <summary>
        /// Is there any CustomView attribute attached to the given provider?
        /// </summary>
        internal static bool HasCustomViewAttribute(this ICustomAttributeProvider provider)
        {
            if (!provider.HasCustomAttributes)
                return false;
            return provider.CustomAttributes.Any(x =>
                (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                (x.AttributeType.Name == AttributeConstants.CustomViewAttributeName));
        }

        /// <summary>
        /// Is there any [NUnit.Framework.Test] attribute attached to the given provider?
        /// </summary>
        internal static bool HasNUnitTestAttribute(this ICustomAttributeProvider provider)
        {
            if (!provider.HasCustomAttributes)
                return false;
            return provider.CustomAttributes.Any(x => (x.AttributeType.FullName == AttributeConstants.NUnitTestAttributeFullName));
        }

        /// <summary>
        /// Is there any [NUnit.Framework.SetUp] attribute attached to the given provider?
        /// </summary>
        internal static bool HasNUnitSetUpAttribute(this ICustomAttributeProvider provider)
        {
            if (!provider.HasCustomAttributes)
                return false;
            return provider.CustomAttributes.Any(x => (x.AttributeType.FullName == AttributeConstants.NUnitSetUpAttributeFullName));
        }

        /// <summary>
        /// Is there any [NUnit.Framework.TearDown] attribute attached to the given provider?
        /// </summary>
        internal static bool HasNUnitTearDownAttribute(this ICustomAttributeProvider provider)
        {
            if (!provider.HasCustomAttributes)
                return false;
            return provider.CustomAttributes.Any(x => (x.AttributeType.FullName == AttributeConstants.NUnitTearDownAttributeFullName));
        }

        /// <summary>
        /// Is there any [NUnit.Framework.TestFixture] attribute attached to the given provider?
        /// </summary>
        internal static bool HasNUnitTestFixtureAttribute(this ICustomAttributeProvider provider)
        {
            if (!provider.HasCustomAttributes)
                return false;
            return provider.CustomAttributes.Any(x => (x.AttributeType.FullName == AttributeConstants.NUnitTestFixtureAttributeFullName));
        }
    }
}
