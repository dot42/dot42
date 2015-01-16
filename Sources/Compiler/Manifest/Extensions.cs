using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;

namespace Dot42.Compiler.Manifest
{
    internal static class Extensions
    {
        /// <summary>
        /// Gets a list of Dot42 attributes with given name. 
        /// The namespace of the attribute is always "Dot42".
        /// </summary>
        internal static IEnumerable<CustomAttribute> GetAttributes(this ICustomAttributeProvider provider, string attributeName)
        {
            if (!provider.HasCustomAttributes)
                return Enumerable.Empty<CustomAttribute>();
            return provider.CustomAttributes.Where(x => x.AttributeType.FullName == "Dot42.Manifest." + attributeName);
        }

        /// <summary>
        /// Gets a list of Dot42 attributes with given name from the given assembly and all assemblies that are referenced. 
        /// The namespace of the attribute is always "Dot42".
        /// </summary>
        internal static IEnumerable<CustomAttribute> GetAttributesFromAllAssemblies(this AssemblyDefinition assembly, string attributeName)
        {
            var attributes = new List<CustomAttribute>();
            var visited = new Dictionary<string, string>();
            CollectAttributes(assembly, attributeName, attributes, visited);
            return attributes;
        }

        /// <summary>
        /// Collect matching custom attributes.
        /// </summary>
        private static void CollectAttributes(AssemblyDefinition assembly, string attributeName, List<CustomAttribute> attributes, Dictionary<string, string> visited)
        {
            // Collect in assembly
            visited[assembly.Name.Name] = assembly.Name.Name;
            attributes.AddRange(assembly.GetAttributes(attributeName));

            // Collect in assembly references
            foreach (var name in assembly.MainModule.AssemblyReferences)
            {
                // Already visited?
                if (visited.ContainsKey(name.Name))
                    continue;

                // Resolve the assembly
                visited[name.Name] = name.Name; // May seem duplicate, but mscorlib can also be corlib.
                var otherAssembly = assembly.MainModule.AssemblyResolver.Resolve(name);
                CollectAttributes(otherAssembly, attributeName, attributes, visited);
            }
        }

        /// <summary>
        /// Gets a value from a custom attribute.
        /// First it will look for a ctor argument.
        /// If that is not available, it will look for a property by the given name.
        /// </summary>
        /// <param name="attribute">Attribute to get a value from</param>
        /// <param name="ctorArgumentIndex">0 based index in ctor arguments (use -1 to avoid ctor argument lookup)</param>
        /// <param name="propertyName">Name of the property to look for</param>
        /// <param name="defaultValue">Value returned in no value was found</param>
        internal static T GetValue<T>(this CustomAttribute attribute, int ctorArgumentIndex, string propertyName, T defaultValue = default(T))
        {
            T value;
            if (TryGetValue(attribute, ctorArgumentIndex, propertyName, out value))
                return value;
            return defaultValue;
        }

        /// <summary>
        /// Gets a value from a custom attribute.
        /// First it will look for a ctor argument.
        /// If that is not available, it will look for a property by the given name.
        /// </summary>
        /// <param name="attribute">Attribute to get a value from</param>
        /// <param name="ctorArgumentIndex">0 based index in ctor arguments (use -1 to avoid ctor argument lookup)</param>
        /// <param name="propertyName">Name of the property to look for</param>
        /// <param name="value">Value is returned in this parameter (if found)</param>
        /// <returns>True if found, false otherwise</returns>
        internal static bool TryGetValue<T>(this CustomAttribute attribute, int ctorArgumentIndex, string propertyName, out T value)
        {
            if (attribute != null)
            {
                if ((ctorArgumentIndex >= 0) && (ctorArgumentIndex < attribute.ConstructorArguments.Count))
                {
                    value = ConvertArgumentValue<T>(attribute.ConstructorArguments[ctorArgumentIndex].Value);
                    return true;
                }
                foreach (var p in attribute.Properties.Where(x => x.Name == propertyName))
                {
                    var argValue = p.Argument.Value;
                    value = ConvertArgumentValue<T>(argValue);
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        /// <summary>
        /// Convert an attribute argument value to type T.
        /// </summary>
        private static T ConvertArgumentValue<T>(object value)
        {
            if (value is T)
                return (T) value;
            var typeT = typeof (T);
            if (typeT.IsArray)
            {
                var arrayValue = value as CustomAttributeArgument[];
                if (arrayValue != null)
                {
                    var list = arrayValue.Select(x => Convert.ChangeType(x.Value, typeT.GetElementType())).ToList();
                    var array = Array.CreateInstance(typeT.GetElementType(), list.Count);
                    for (var i = 0; i < list.Count; i++)
                    {
                        array.SetValue(list[i], i);
                    }
                    return (T) ((object)array);
                }
            }
            throw new ArgumentException("Unknown attribute value: " + value);
        }

        /// <summary>
        /// Gets a value from a custom attribute.
        /// It will look for a property by the given name.
        /// </summary>
        /// <param name="attribute">Attribute to get a value from</param>
        /// <param name="propertyName">Name of the property to look for</param>
        /// <param name="defaultValue">Value returned in no value was found</param>
        internal static T GetValue<T>(this CustomAttribute attribute, string propertyName, T defaultValue = default(T))
        {
            return attribute.GetValue(-1, propertyName, defaultValue);
        }

        /// <summary>
        /// Gets a value from a custom attribute.
        /// It will look for a property by the given name.
        /// </summary>
        /// <param name="attribute">Attribute to get a value from</param>
        /// <param name="propertyName">Name of the property to look for</param>
        /// <param name="value">Value is returned in this parameter (if found)</param>
        /// <returns>True if found, false otherwise</returns>
        internal static bool TryGetValue<T>(this CustomAttribute attribute, string propertyName, out T value)
        {
            return attribute.TryGetValue(-1, propertyName, out value);
        }

        /// <summary>
        /// Add an attribute with given name and value.
        /// </summary>
        internal static void AddAttr(this XElement element, string attributeLocalName, string attributeNamespace, string value)
        {
            var name = string.IsNullOrEmpty(attributeNamespace)
                           ? XName.Get(attributeLocalName)
                           : XName.Get(attributeLocalName, attributeNamespace);
            element.Add(new XAttribute(name, value ?? string.Empty));
        }

        /// <summary>
        /// Add an attribute with given name if the value is not empty.
        /// </summary>
        internal static void AddAttrIfNotEmpty(this XElement element, string attributeLocalName, string attributeNamespace, string value, Func<string, string> formatter = null)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (formatter != null)
                {
                    value = formatter(value);
                }
                AddAttr(element, attributeLocalName, attributeNamespace, value);
            }
        }

        /// <summary>
        /// Add an attribute with given name if the value is not empty.
        /// </summary>
        internal static void AddAttrIfNotEmpty<T>(this XElement element, string attributeLocalName, string attributeNamespace, T value, Func<T, string> formatter = null)
            where T : class
        {
            if (value == null)
                return;
            var sValue = value as string;
            if ((sValue != null) && (sValue.Length == 0))
                return;
            if (formatter != null)
            {
                sValue = formatter(value);
            }
            else
            {
                sValue = value.ToString();
            }
            AddAttr(element, attributeLocalName, attributeNamespace, sValue);
        }

        /// <summary>
        /// Add a boolean attribute with given name if the value is not the given default value
        /// </summary>
        internal static void AddAttrIfNotDefault(this XElement element, string attributeLocalName, string attributeNamespace, bool value, bool defaultValue)
        {
            if (value != defaultValue)
            {
                AddAttr(element, attributeLocalName, attributeNamespace, value.ToString(CultureInfo.InvariantCulture).ToLower());
            }
        }

        /// <summary>
        /// Add a boolean attribute with given name if the value is not the given default value
        /// </summary>
        internal static void AddAttrIfNotDefault(this XElement element, string attributeLocalName, string attributeNamespace, int value, int defaultValue, Func<int, string> formatter = null)
        {
            if (value != defaultValue)
            {
                var valueAsString = (formatter != null) ? formatter(value) : value.ToString(CultureInfo.InvariantCulture);
                AddAttr(element, attributeLocalName, attributeNamespace, valueAsString);
            }
        }

        /// <summary>
        /// Add a boolean attribute with given name if the value is not the given default value
        /// </summary>
        internal static void AddAttrIfNotDefault(this XElement element, string attributeLocalName, string attributeNamespace, long value, long defaultValue, Func<long, string> formatter = null)
        {
            if (value != defaultValue)
            {
                var valueAsString = (formatter != null) ? formatter(value) : value.ToString(CultureInfo.InvariantCulture);
                AddAttr(element, attributeLocalName, attributeNamespace, valueAsString);
            }
        }

        /// <summary>
        /// Add a boolean attribute with given name if the value is not the given default value
        /// </summary>
        internal static void AddAttrIfFound(this XElement element, string attributeLocalName, string attributeNamespace, CustomAttribute attr, string propertyName)
        {
            bool value;
            if (attr.TryGetValue<bool>(propertyName, out value))
            {
                AddAttr(element, attributeLocalName, attributeNamespace, value.ToString(CultureInfo.InvariantCulture).ToLower());
            }
        }
    }
}
