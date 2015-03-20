using System.Collections.Generic;
using System.Linq;
using Dot42.ImportJarLib.Model;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Go over the methods and convert them to properties when possible.
    /// </summary>
    public class PropertyBuilder
    {
        private readonly NetTypeDefinition typeDef;
        private readonly TypeBuilder declaringTypeBuilder;

        /// <summary>
        /// Default ctor
        /// </summary>
        public PropertyBuilder(NetTypeDefinition typeDef, TypeBuilder declaringTypeBuilder)
        {
            this.typeDef = typeDef;
            this.declaringTypeBuilder = declaringTypeBuilder;
        }

        /// <summary>
        /// Build properties for the given type
        /// </summary>
        internal void BuildProperties(TargetFramework target, MethodRenamer methodRenamer)
        {
            if (typeDef.IsInterface)
                return;

            var getters = typeDef.Methods.Where(IsGetter).ToList();
            var setters = typeDef.Methods.Where(IsSetter).ToList();
            var generatedNames = new HashSet<string>();

            foreach (var getMethod in getters)
            {
                // Get the name of the property
                var name = GetPropertyName(getMethod);

                // If there are other methods with same name, we do not create a property
                if (typeDef.Methods.Any(x => (x != getMethod) && (x.Name == name)))
                    continue;
                if (typeDef.NestedTypes.Any(x => x.Name == name))
                    continue;
                if (typeDef.Name == name)
                    continue;
                if (typeDef.Fields.Any(x => x.Name == name))
                    continue;
                if (!generatedNames.Add(name + "_" + getMethod.Parameters.Count))
                    continue;

                // Create property
                var prop = new NetPropertyDefinition { Name = name, Getter = getMethod, Description = getMethod.Description };

                AddCustomAttributes(getMethod, prop.CustomAttributes);
                
                // Clone parameters
                if (getMethod.Parameters.Any())
                {
                    prop.Parameters.AddRange(getMethod.Parameters.Select(x => new NetParameterDefinition(x.Name, x.ParameterType, false)));
                    prop.Name = "this";
                }

                // Rename getMethod if needed
                if (getMethod.Name == prop.Name)
                {
                    methodRenamer.Rename(getMethod, "Get" + getMethod.Name);
                }

                // Find setter
                var setMethod = FindSetter(getMethod, setters);
                if (setMethod != null)
                {
                    prop.Setter = setMethod;

                    // Rename setMethod if needed
                    if (setMethod.Name == prop.Name)
                    {
                        methodRenamer.Rename(setMethod, "Set" + setMethod.Name);
                    }
                }

                // Add property
                typeDef.Properties.Add(prop);
            }
        }

        /// <summary>
        /// Called in the finalize phase of the type builder.
        /// </summary>
        public void Finalize(TargetFramework target)
        {
            FixOverridenProperties();
        }
        
        /// <summary>
        /// Create a property name from the given getter.
        /// </summary>
        protected virtual string GetPropertyName(NetMethodDefinition getter)
        {
            var name = getter.Name;

            if (name == "GetTypeJava")
                return "Type";

            name = name.StartsWith("Get") ? name.Substring(3) : name;
            if (!(char.IsLetter(name[0]) || (name[0] == '_')))
                name = "_" + name;
            if (getter.ReturnType.IsBoolean() && !name.StartsWith("Is"))
                name = "Is" + name;
            
            

            return name;
        }

        /// <summary>
        /// Customize the custom attributes collection of the given method.
        /// </summary>
        protected virtual void AddCustomAttributes(NetMethodDefinition method, List<NetCustomAttribute> customAttributes)
        {
            //Nothing to do here
        }

        /// <summary>
        /// Is the given method a property get method?
        /// </summary>
        protected virtual bool IsGetter(NetMethodDefinition method)
        {
            if (method.Parameters.Count > 0)
                return false;
            if (method.ReturnType.IsVoid())
                return false;
            var name = method.Name;
            if (name == "GetHashCode")
                return false;
            if (!name.StartsWith("Get"))
                return false;
            if (name.Length < 4)
                return false;
            return true;
        }

        /// <summary>
        /// Is the given method a property set method?
        /// </summary>
        protected virtual bool IsSetter(NetMethodDefinition method)
        {
            if (method.Parameters.Count != 1)
                return false;
            if (!method.ReturnType.IsVoid())
                return false;
            var name = method.Name;
            if (!name.StartsWith("Set"))
                return false;
            if (name.Length < 4)
                return false;
            return true;
        }

        /// <summary>
        /// Find the first matching set method.
        /// </summary>
        protected virtual NetMethodDefinition FindSetter(NetMethodDefinition getMethod, IEnumerable<NetMethodDefinition> setters)
        {
            var name = "Set" + getMethod.Name.Substring(3);
            var type = getMethod.ReturnType;
            return setters.FirstOrDefault(x => x.Name == name && x.Parameters[0].ParameterType.AreSame(type));
        }

        /// <summary>
        /// Remove all properties that override a property that does not exist.
        /// </summary>
        private void FixOverridenProperties()
        {
            if (!typeDef.Properties.Any())
                return;

            var allBaseProperties = typeDef.GetBaseTypesWithoutInterfaces().SelectMany(x => x.GetElementType().Properties).ToList();
            foreach (var iterator in typeDef.Properties.Where(x => x.Getter.NeedsOverrideKeyword && !DoNotFixOverridenProperty(x)).ToList())
            {
                var prop = iterator;
                var sameBaseProperties = allBaseProperties.Where(prop.AreSame).ToList();

                // Check for existing property in base class
                if (sameBaseProperties.Count == 0)
                {
                    // Property has no override
                    typeDef.Properties.Remove(prop);
                    continue;
                }

                // Check for base property with setter
                if (prop.Setter != null)
                {
                    if (sameBaseProperties.Any(x => x.Setter == null))
                    {
                        // Remove setter
                        prop.Setter = null;
                    }
                } 
                else 
                {
                    // Check for any base property with setter
                }
            }
        }

        /// <summary>
        /// Should the given property be excluded from fixup?
        /// </summary>
        private static bool DoNotFixOverridenProperty(NetPropertyDefinition prop)
        {
            return (prop.Name == "Adapter");
        }
    }
}
