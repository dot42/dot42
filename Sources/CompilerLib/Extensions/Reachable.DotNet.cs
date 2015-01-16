using Dot42.Cecil;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Mono.Cecil related extension methods
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Mark the given reference reachable
        /// </summary>
        internal static void MarkReachable(this MemberReference memberRef, IReachableContext context)
        {
            if ((memberRef != null) && (!memberRef.IsReachable))
            {
                memberRef.SetReachable(context);
            }
        }
        /// <summary>
        /// Mark all fields with the given name reachable.
        /// </summary>
        internal static void MarkFieldsReachable(this TypeReference type, string fieldName, ReachableContext context)
        {
            while ((type != null) && (context.Contains(type)))
            {
                TypeDefinition typeDef = context.GetTypeDefinition(type);
                if (typeDef != null)
                {
                    if (typeDef.HasFields)
                    {
                        foreach (FieldDefinition field in typeDef.Fields)
                        {
                            if (field.Name == fieldName)
                            {
                                field.MarkReachable(context);
                            }
                        }
                    }
                    type = typeDef.BaseType;
                }
                else
                {
                    type = null;
                }
            }
        }

        /// <summary>
        /// Mark all properties with the given name reachable.
        /// </summary>
        internal static void MarkPropertiesReachable(this TypeReference type, string propertyName, ReachableContext context)
        {
            while ((type != null) && (context.Contains(type)))
            {
                TypeDefinition typeDef = context.GetTypeDefinition(type);
                if (typeDef != null)
                {
                    if (typeDef.HasProperties)
                    {
                        foreach (PropertyDefinition prop in typeDef.Properties)
                        {
                            if (prop.Name == propertyName)
                            {
                                prop.MarkReachable(context);
                            }
                        }
                    }
                    type = typeDef.BaseType;
                }
                else
                {
                    type = null;
                }
            }
        }
    }
}
