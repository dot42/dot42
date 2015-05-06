using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dot42.ImportJarLib.Model;
using Dot42.Utility;
using Mono.Cecil;
using Mono.Cecil.Cil;

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

            foreach (var getMethod in getters)
            {
                // Get the name of the property
                var name = GetPropertyName(getMethod);

                // other clashes must be handled in later.
                if(getMethod.InterfaceType == null)
                    typeDef.Fields.Where(x => x.Name == name).ForEach(RenameClashingField);

                // Create property
                var prop = new NetPropertyDefinition
                {
                    Name = name, 
                    Getter = getMethod, 
                    Description = getMethod.Description,
                };

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
                if (setMethod.InterfaceType == null)
                    typeDef.Fields.Where(x => x.Name == name).ForEach(RenameClashingField);

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
            RemoveDuplicateProperties();
            RemoveClashingProperties();
            FixPropertyFinalState();
        }

        /// <summary>
        /// Create a property name from the given getter.
        /// </summary>
        protected virtual string GetPropertyName(NetMethodDefinition method)
        {
            var name = method.Name;

            bool isSetter = method.Name.StartsWith("Set") || method.Name.StartsWith("set_");

            name = name.StartsWith("get_") ? name.Substring(4) 
                 : name.StartsWith("Get")  ? name.Substring(3)
                 : name.StartsWith("set_") ? name.Substring(4) 
                 : name.StartsWith("Set")  ? name.Substring(3) 
                 : name;

            bool isBool = (isSetter ? method.Parameters[0].ParameterType : method.ReturnType)
                         .IsBoolean();

            if (isBool && AddIsPrefixToBoolProperty(name, method))
            {
                name = "Is" + name;
            }

            if (!(char.IsLetter(name[0]) || (name[0] == '_')))
                name = "_" + name;

            return name;
        }

        protected virtual bool AddIsPrefixToBoolProperty(string name, NetMethodDefinition method)
        {
            // We can't really tell if the "Is" Prefix was omitted because of 
            // careless naming or because the actual name is more to the point.
            // http://docstore.mik.ua/orelly/java-ent/jnut/ch06_02.htm
            // http://stackoverflow.com/questions/5322648/for-a-boolean-field-what-is-the-naming-convention-for-its-getter-setter
            // http://stackoverflow.com/questions/11941485/java-naming-convention-for-boolean-variable-names-writerenabled-vs-writerisenab
            // http://stackoverflow.com/questions/4851337/setter-for-a-boolean-variable-named-like-isactive

            var excludedPrefixes = new[] { "Is", "Can", "Has", "Use" };
            var namePrefix = GetNamePrefix(name);
            return !excludedPrefixes.Contains(namePrefix);
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

            var prefix = GetNamePrefix(name);
            return prefix == "Get" || prefix == "Has" || prefix == "Can" || prefix == "Is" || prefix == "get_";
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

            var prefix = GetNamePrefix(name);

            return prefix == "Set" || prefix == "set_";
        }

        /// <summary>
        /// Find the first matching set method.
        /// </summary>
        protected virtual NetMethodDefinition FindSetter(NetMethodDefinition getMethod, IEnumerable<NetMethodDefinition> setters)
        {
            var getName = getMethod.Name;
            var getPrefix = GetNamePrefix(getName);
            
            if (getPrefix == "Has" || getPrefix == "Can")
                return null;

            if(getPrefix != null)
                getName = getName.Substring(getPrefix.Length);

            var name1 = "Set" + getName;
            var name2 = "set_" + getName;

            var type = getMethod.ReturnType;

            var possibleMatch = setters.Where(x => (x.Name == name1 || x.Name == name2)
                                                && x.Parameters[0].ParameterType.AreSame(type)
                                                && x.InterfaceType.AreSame(getMethod.InterfaceType)
                                                && x.HasSameVisibility(getMethod))
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
        /// 
        /// Will also fix property visibility.
        /// </summary>
        private void FixOverridenProperties(MethodRenamer methodRenamer)
        {
            if (!typeDef.Properties.Any())
                return;

            // handle overiding properties
            var allBaseTypes = typeDef.GetBaseTypes(true);
            var allBaseProperties = allBaseTypes.SelectMany(x => x.GetElementType().Properties).ToList();

            var overridingProperties = typeDef.Properties.Where(x => IsAnyOverride(x) && !DoNotFixOverridenProperty(x)).ToList();

            for (int i = 0; i < overridingProperties.Count; ++i)
            {
                var prop = overridingProperties[i];

                var sameBaseProperties = allBaseProperties.Where(prop.AreSame).ToList();
                var getter = prop.Getter;
                var setter = prop.Setter;

                // Check for existing property in base class
                if (sameBaseProperties.Count == 0)
                {
                    // implement as normal method.
                    RemoveProperty(prop);
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

                // We  must implement the property, since we inherited it. 
                // Fix up any clashes.

                // clashes with explicit implementations should not occur.
                if ((prop.Getter ?? prop.Setter).InterfaceType != null)
                    continue;

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

                foreach (var clash in typeDef.Properties.Where(x => x != prop && x.Name == propName).ToList())
                {
                    if (clash.Parameters.Count != prop.Parameters.Count)
                        continue;
                    if ((clash.Getter ?? clash.Setter).InterfaceType != null)
                        continue;

                    if (prop.PropertyType.AreSame(clash.PropertyType) && 
                        prop.MainMethod.CreateReason == "TypeBuilder.AddAbstractInterfaceMethods")
                    {
                        // it appears that the method does have an implementation!
                        typeDef.Properties.Remove(prop);
                        typeDef.Methods.Remove(prop.Getter);
                        typeDef.Methods.Remove(prop.Setter);
                        overridingProperties.RemoveAt(i);
                        i -= 1;
                        continue;
                    }

                    if (overridingProperties.Contains(clash))
                    {
                        // This would have probably been removed later anyways.
                        //Console.Error.WriteLine("Warning: duplicate property names {0}::{1}. Not generating property for one of the clashes.", typeDef.FullName, propName);
                        RemoveProperty(clash);
                        overridingProperties.Remove(clash); // this must come after us.
                        continue;
                    }

                    // else remove other property
                    RemoveProperty(clash);
                }  
                
                if (typeDef.Methods.Any(x => x != getter && x != setter && x.Name == propName))
                {
                    Console.Error.WriteLine("Warning: Inherited property {0}::{1} clashes with methods. Prepending \"Invoke\" prefix to methods.", typeDef.FullName, propName);
                    typeDef.Methods.Where(x => x != getter && x != setter && x.Name == propName)
                                   .ForEach(m => methodRenamer.Rename(m, "Invoke" + m.Name));
                }
            }
        }

        private void RemoveProperty(NetPropertyDefinition prop)
        {
            if (prop.Getter != null) prop.Getter.Property = null;
            if (prop.Setter != null) prop.Setter.Property = null;
            typeDef.Properties.Remove(prop);
        }

        private void RemoveClashingProperties()
        {
            // remove all properties with clashes
            foreach (var clash in typeDef.Properties.Where(p => IsNameClash(typeDef, p)).ToList())
            {
                RemoveProperty(clash);
            }
        }

        private void FixPropertyFinalState()
        {
            // remove all properties with clashes
            foreach (var prop in typeDef.Properties)
            {
                var getter = prop.Getter;
                var setter = prop.Setter;

                if(getter == null || setter == null)
                    continue;

                if (getter.IsFinal != setter.IsFinal)
                {
                    Console.Error.WriteLine("Warning: Property Getters/Setters {0} {1}/{2} have different virtual status. Importing both as virtual.", typeDef.FullName, getter.OriginalJavaName, setter.OriginalJavaName);
                    getter.IsVirtual = true;
                    setter.IsVirtual = true;
                }
            }
        }

        private void RemoveDuplicateProperties()
        {
            // Remove all properties with duplicate names.
            // note that the less-important properties 
            // (i.e. those with only a lone setter)
            // come later in the list.

            var props = typeDef.Properties.ToList();
            for (int i = 0; i < props.Count; ++i)
            {
                var prop = props[i];

                if (prop.MainMethod.InterfaceType != null)
                    continue;

                foreach (var clash in typeDef.Properties.Where(x => x != prop && x.Name == prop.Name).ToList())
                {
                    if (clash.Parameters.Count != prop.Parameters.Count)
                        continue;
                    if (clash.MainMethod.InterfaceType != null)
                        continue;

                    RemoveProperty(clash);
                    props.Remove(clash);
                }
            }
        }

        public static bool IsNameClash(NetTypeDefinition typeDef, NetPropertyDefinition prop)
        {
            if ((prop.Getter ?? prop.Setter).InterfaceType != null)
                return false;

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

        /// <summary>
        /// Will return everyting up to the first non-lower 
        /// character or underscore, starting from the second character.
        /// returns null if none found.
        /// </summary>
        private static string GetNamePrefix(string name)
        {
            string namePrefix = null;

            for (int i = 1; i < name.Length; ++i)
            {
                if (!char.IsLower(name[i]) && name[i] != '_')
                {
                    namePrefix = name.Substring(0, i);
                    break;
                }
            }

            //namePrefix = namePrefix ?? name;
            return namePrefix;
        }

        private static void RenameClashingField(NetFieldDefinition field)
        {
            field.Name = "@" + char.ToLower(field.Name[0]) + field.Name.Substring(1);
        }

        private string GetUniqueName(string originalName, IList<string> names)
        {
            var result = originalName;
            var index = 0;
            while (names.Contains(result))
            {
                result = originalName + (++index);
            }
            return result;
        }
    }
}
