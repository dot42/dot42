using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Dot42.CecilExtensions
{
    /// <summary>
    /// Resolve generics extension methods
    /// </summary>
    public class GenericsResolver
    {
        private readonly TypeDefinition context;
        private Dictionary<TypeDefinition, GenericInstanceType> typeToGit;

        /// <summary>
        /// Default ctor
        /// </summary>
        public GenericsResolver(TypeDefinition context)
        {
            this.context = context;
        }

        /// <summary>
        /// Resolve the given generic parameter to a generic argument.
        /// </summary>
        public TypeReference Resolve(GenericParameter gp)
        {
            var owner = gp.Owner as TypeReference;
            if (owner == null)
                return gp; // Do not resolve generic method parameters for now

            // Try to resolve the owner
            var ownerDef = owner.GetElementType().Resolve();
            if (ownerDef == null)
                return gp;

            // Try to find the owner in our map
            var map = GetTypeToGit();
            GenericInstanceType git;
            if (!map.TryGetValue(ownerDef, out git))
            {
                // No mapping found
                return gp;
            }

            // Replace with the given generic argument
            var result = git.GenericArguments[gp.Position];
            var resultAsGp = result as GenericParameter;
            return (resultAsGp != null) ? Resolve(resultAsGp) : result;
        }

        /// <summary>
        /// Gets the map from type definition to generic instance type.
        /// </summary>
        private Dictionary<TypeDefinition, GenericInstanceType> GetTypeToGit()
        {
            if (typeToGit == null)
            {
                // Build the map
                var map = new Dictionary<TypeDefinition, GenericInstanceType>();
                BuildTypeToGit(map, context);
                typeToGit = map;
            }
            return typeToGit;
        }

        /// <summary>
        /// Gets the map from type definition to generic instance type.
        /// </summary>
        private static void BuildTypeToGit(Dictionary<TypeDefinition, GenericInstanceType> map, TypeDefinition type)
        {
            var remainingTypes = new List<TypeDefinition>();

            // Check base type
            if (type.BaseType != null)
            {
                var baseTypeDef = type.BaseType.GetElementType().Resolve();
                if ((baseTypeDef != null) && !map.ContainsKey(baseTypeDef))
                {
                    var baseTypeAsGit = type.BaseType as GenericInstanceType;
                    if (baseTypeAsGit != null)
                    {
                        map.Add(baseTypeDef, baseTypeAsGit);
                    }
                    remainingTypes.Add(baseTypeDef);
                }
            }

            // Check interfaces
            if (type.HasInterfaces)
            {
                foreach (var intf in type.Interfaces.Select(x => x.InterfaceType))
                {
                    var intfDef = intf.GetElementType().Resolve();
                    if ((intfDef != null) && !map.ContainsKey(intfDef))
                    {
                        var intfAsGit = intf as GenericInstanceType;
                        if (intfAsGit != null)
                        {
                            map.Add(intfDef, intfAsGit);
                        }
                        remainingTypes.Add(intfDef);
                    }
                }
            }

            // Recurse into types
            remainingTypes.ForEach(x => BuildTypeToGit(map, x));
        }
    }
}
