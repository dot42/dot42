using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Reachable extensions
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Resolve type reference into type defition.
        /// Generic instance types are resolved to their element type.
        /// </summary>
        internal static TypeDefinition Resolve(this TypeReference type, ReachableContext context)
        {
            if (context.Contains(type))
            {
                var typeDef = type as TypeDefinition;
                if (typeDef != null) { return typeDef; }

                var git = type as GenericInstanceType;
                if (git != null) { return git.ElementType.Resolve(context); }

                return context.GetTypeDefinition(type);
            }

            // Type not in project, don't care about it.
            return null;
        }

        /// <summary>
        /// Try to resolve definition from reference.
        /// </summary>
        internal static EventDefinition Resolve(this EventReference evt, ReachableContext context)
        {
            var declType = evt.DeclaringType.Resolve(context);
            var resolver = new GenericsResolver(declType);
            return (declType == null) ? null : declType.Events.FirstOrDefault(x => x.AreSame(evt, resolver.Resolve));
        }

        /// <summary>
        /// Try to resolve definition from reference.
        /// </summary>
        internal static FieldDefinition Resolve(this FieldReference field, ReachableContext context)
        {
            var declType = field.DeclaringType.Resolve(context);
            var resolver = new GenericsResolver(declType);
            return (declType == null) ? null : declType.Fields.FirstOrDefault(x => x.AreSame(field, resolver.Resolve));
        }

        /// <summary>
        /// Try to resolve definition from reference.
        /// </summary>
        internal static PropertyDefinition Resolve(this PropertyReference prop, ReachableContext context)
        {
            var declType = prop.DeclaringType.Resolve(context);
            var resolver = new GenericsResolver(declType);
            return (declType == null) ? null : declType.Properties.FirstOrDefault(x => x.AreSame(prop, resolver.Resolve));
        }

        /// <summary>
        /// Try to resolve definition from reference.
        /// </summary>
        internal static MethodDefinition Resolve(this MethodReference method, ReachableContext context)
        {
            var declType = method.DeclaringType.Resolve(context);
            var resolver = new GenericsResolver(declType);
            return (declType == null) ? null : declType.Methods.FirstOrDefault(x => x.AreSame(method, resolver.Resolve));
        }

    }
}
