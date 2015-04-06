using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dot42.ImportJarLib.Model;
using Dot42.Utility;
using Mono.Cecil;

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
            //if (typeDef.IsInterface)
            //    return;

            var getters = typeDef.Methods.Where(IsGetter).ToList();
            var setters = typeDef.Methods.Where(IsSetter).ToList();
            var generatedNames = new HashSet<string>();

            foreach (var getMethod in getters)
            {
                // Get the name of the property
                var name = GetPropertyName(getMethod);

                // other clashes must be handled in FixOverrides.
                typeDef.Fields.Where(x => x.Name == name).ForEach(f => f.Name = "@" + f.Name.ToLowerInvariant());

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

                getMethod.Property = prop;

                // Find setter
                var setMethod = FindSetter(getMethod, setters);
                if (setMethod != null)
                {
                    prop.Setter = setMethod;
                    setMethod.Property = prop;

                    setters.Remove(setMethod);
                }

                // Add property
                typeDef.Properties.Add(prop);
            }

            // create properties for lone setters that override a base setter
            foreach (var setMethod in setters.Where(s => s.IsOverride))
            {
                var name = GetPropertyName(setMethod);

                // other clashes must be handled in FixOverrides
                typeDef.Fields.Where(x => x.Name == name).ForEach(f => f.Name = "@" + f.Name.ToLowerInvariant());

                if (!generatedNames.Add(name + "_" + (setMethod.Parameters.Count-1)))
                    continue;

                // Create property
                var prop = new NetPropertyDefinition { Name = name, Setter = setMethod, Description = setMethod.Description };
                setMethod.Property = prop;

                AddCustomAttributes(setMethod, prop.CustomAttributes);

                // Clone parameters
                if (setMethod.Parameters.Skip(1).Any())
                {
                    prop.Parameters.AddRange(setMethod.Parameters.Skip(1).Select(x => new NetParameterDefinition(x.Name, x.ParameterType, false)));
                    prop.Name = "this";
                }
                
                typeDef.Properties.Add(prop);
            }
        }

        /// <summary>
        /// Called in the finalize phase of the type builder.
        /// </summary>
        public void Finalize(TargetFramework target, MethodRenamer methodRenamer)
        {
            FixOverridenProperties(methodRenamer);
            RemoveClashingProperties();
        }

        /// <summary>
        /// Create a property name from the given getter.
        /// </summary>
        protected virtual string GetPropertyName(NetMethodDefinition method)
        {
            var name = method.Name;

            bool isSetter = method.Name.StartsWith("Set");

            name = name.StartsWith("Get") ? name.Substring(3) 
                 : name.StartsWith("Set") ? name.Substring(3) 
                 : name;

            if (!(char.IsLetter(name[0]) || (name[0] == '_')))
                name = "_" + name;

            bool isBool = (isSetter ? method.Parameters[0].ParameterType : method.ReturnType)
                         .IsBoolean();

            if (isBool && !name.StartsWith("Is") 
                       && !name.StartsWith("Has") 
                       && !name.StartsWith("Can"))
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

            bool startsWithGet = name.StartsWith("Get") || name.StartsWith("Has") || name.StartsWith("Can");
            bool startsWithIs = name.StartsWith("Is");

            if (!startsWithGet && !startsWithIs)
                return false;
            if ((startsWithGet && name.Length < 4) || (startsWithIs && name.Length < 3))
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
            var getName = getMethod.Name;
            
            if (getName.StartsWith("Has") || getName.StartsWith("Can"))
                return null;

            if (getName.StartsWith("Get"))
                getName = getName.Substring(3);
            else if(getName.StartsWith("Is"))
                getName = getName.Substring(1);

            var name = "Set" + getName;

            var type = getMethod.ReturnType;

            var possibleMatch = setters.Where(x => x.Name == name 
                                                && x.Parameters[0].ParameterType.AreSame(type)
                                                && x.IsFinal == getMethod.IsFinal)
                                       .ToList();

            if (possibleMatch.Count > 1)
            {
                Console.Error.WriteLine("Warning: more than one possible setter matches property {0}::{1}. Not generating setter.", typeDef.FullName, getMethod.Name);
                return null;
            }

            return possibleMatch.FirstOrDefault();
        }

        /// <summary>
        /// Remove all properties that override / implement a property that does not exist.
        /// </summary>
        /// <param name="methodRenamer"></param>
        private void FixOverridenProperties(MethodRenamer methodRenamer)
        {
            if (!typeDef.Properties.Any())
                return;

            var allBaseTypes = typeDef.GetBaseTypes(true);
            var allBaseProperties = allBaseTypes.SelectMany(x => x.GetElementType().Properties).ToList();
            // handle overiding getters.
            foreach (var prop in typeDef.Properties.Where(x => IsAnyOverride(x) && !DoNotFixOverridenProperty(x)).ToList())
            {
                var sameBaseProperties = allBaseProperties.Where(prop.AreSame).ToList();
                var getter = prop.Getter;
                var setter = prop.Setter;

                // Check for existing property in base class
                if (sameBaseProperties.Count == 0)
                {
                    // implement as normal method.
                    if (getter != null) getter.Property = null;
                    if (setter != null) setter.Property = null;
                    typeDef.Properties.Remove(prop);
                    continue;
                }

                // Check for base property with setter
                if (setter != null)
                {
                    if (sameBaseProperties.Any(x => x.Setter == null))
                    {
                        // Remove setter
                        setter.Property = null;
                        prop.Setter = null;

                        if (getter == null)
                        {
                            // remove property alltogether
                            typeDef.Properties.Remove(prop);
                            continue;
                        }

                    }
                }
                else
                {
                    // Check for any base property with setter
                }

                // we've got to implement the property, since we inherited it. 
                // fix up any clashes.
                var propName = prop.Name;
                if (propName == typeDef.Name)
                {
                    Console.Error.WriteLine("Warning: Not generating inherited property and methods for {0}::{1}: clash with type name.", typeDef.FullName,propName);
                    typeDef.Properties.Remove(prop);
                    typeDef.Methods.Remove(getter);
                    typeDef.Methods.Remove(setter);
                    continue;
                }

                if (typeDef.NestedTypes.Any(x => x.Name == propName))
                {
                    Console.Error.WriteLine("Warning: Inherited property {0}::{1} clashes with nested type. Renaming nested type, but consider renaming the property instead.", typeDef.FullName, propName);
                    typeDef.NestedTypes.Where(x => x.Name == propName).ForEach(t=>t.Name += "FixMe");
                }

                if (typeDef.Methods.Any(x => x != getter && x != setter && x.Name == propName))
                {
                    Console.Error.WriteLine("Warning: Inherited property {0}::{1} clashes with methods. Prepending \"Invoke\" prefix to methods.", typeDef.FullName, propName);
                    typeDef.Methods.Where(x => x != getter && x != setter && x.Name == propName)
                                   .ForEach(m => methodRenamer.Rename(m, "Invoke" + m.Name));
                }
               
            }
        }

        private void RemoveClashingProperties()
        {
            // remove all properties with clashes
            foreach (var prop in typeDef.Properties.Where(p => IsNameClash(typeDef, p)).ToList())
            {
                if (prop.Getter != null) prop.Getter.Property = null;
                if (prop.Setter != null) prop.Setter.Property = null;
                typeDef.Properties.Remove(prop);
            }
        }

        public static bool IsNameClash(NetTypeDefinition typeDef, NetPropertyDefinition prop)
        {
            var name = prop.Name;
            return prop.Name == typeDef.Name
                             || typeDef.NestedTypes.Any(x => x.Name == name)
                             || typeDef.Methods.Any(x => (x != prop.Getter && x != prop.Setter) && (x.Name == name));
        }

        private static bool IsAnyOverride(NetPropertyDefinition prop)
        {
            return (prop.Getter != null && prop.Getter.IsOverride)
                || (prop.Setter != null && prop.Setter.IsOverride);
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
