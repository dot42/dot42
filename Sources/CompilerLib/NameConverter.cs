using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.JvmClassLib;
using Dot42.Utility;
using Mono.Cecil;
using FieldReference = Mono.Cecil.FieldReference;

namespace Dot42.CompilerLib
{
    /// <summary>
    /// Class used to convert the rootnamespace of types such that it will always begin
    /// with the package name.
    /// </summary>
    public class NameConverter 
    {
        private readonly string packageName;
        private readonly string rootNamespaceDot;
        private readonly Dictionary<XFieldDefinition, DexLib.FieldDefinition> xFieldMap = new Dictionary<XFieldDefinition, DexLib.FieldDefinition>();
        private readonly Dictionary<XMethodDefinition, DexLib.MethodDefinition> xMethodMap = new Dictionary<XMethodDefinition, DexLib.MethodDefinition>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public NameConverter(string packageName, string rootNamespace)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentException("packageName is empty");
            this.packageName = packageName;
            rootNamespaceDot = rootNamespace + ".";
        }

        /// <summary>
        /// Returns the root namespace postfixed with a '.' 
        /// </summary>
        public string RootNamespaceDot { get { return rootNamespaceDot; } }

        /// <summary>
        /// Create the classname for the Nullable base class for the given type.
        /// </summary>
        public static string GetNullableBaseClassName(XTypeDefinition type)
        {
            return type.Name + "__Nullable";
        }

        /// <summary>
        /// Get's the namespace of the given type after conversion.
        /// </summary>
        public string GetConvertedNamespace(XTypeDefinition type)
        {
            if (type.IsNested)
            {
                return GetConvertedNamespace(type.DeclaringType);
            }
            var ns = type.Namespace;
            return ConvertNamespace(ns);
        }

        /// <summary>
        /// Get's the namespace of the given type after conversion.
        /// </summary>
        public string GetConvertedNamespace(ClassFile type)
        {
            /*if (type.IsNested)
            {
                return GetConvertedNamespace(type.DeclaringType);
            }*/
            return ClassName.JavaClassNameToClrTypeName(type.Package);
        }

        /// <summary>
        /// Get's the namespace of the given type after conversion.
        /// </summary>
        public string GetConvertedNamespace(Type type)
        {
            if (type.IsNested)
            {
                return GetConvertedNamespace(type.DeclaringType);
            }
            var ns = type.Namespace;
            return ConvertNamespace(ns);
        }

        /// <summary>
        /// Get's the namespace of the given type after conversion.
        /// </summary>
        private string ConvertNamespace(string ns)
        {
            // Empty namespace => prefix package name
            if (string.IsNullOrEmpty(ns))
                return packageName;

#if LOWER_ONLY
            // Empty root namespace => prefix package name
            if (string.IsNullOrEmpty(rootNamespace))
                return packageName + '.' + ns;

            // Namespace == root namespace => change to package name
            if (ns == rootNamespace)
                return packageName;

            // Namespace is child of root namespace => replace root namespace with package name
            if (ns.StartsWith(rootNamespaceDot))
                return packageName + '.' + ns.Substring(rootNamespaceDot.Length);

            // default => prefix with package name
            return packageName + '.' + ns;
#else
            //return ns;
            return ns.Substring(0, 1).ToLower() + ns.Substring(1);
#endif
        }

        /// <summary>
        /// Get's the full name of the given type after conversion.
        /// </summary>
        public string GetConvertedFullName(XTypeDefinition type)
        {
            if (type.IsNested)
            {
                var declaringTypeName = GetConvertedFullName(type.DeclaringType);
                return declaringTypeName + Dex.NestedClassSeparator + GetConvertedName(type);
            }

            var ns = GetConvertedNamespace(type);
            return ns + '.' + GetConvertedName(type);
        }

        /// <summary>
        /// Get's the full name of the given type after conversion.
        /// </summary>
        public string GetConvertedFullName(Type type)
        {
            if (type.IsNested)
            {
                var declaringTypeName = GetConvertedFullName(type.DeclaringType);
                return declaringTypeName + Dex.NestedClassSeparator + GetConvertedName(type);
            }

            var ns = GetConvertedNamespace(type);
            return ns + '.' + GetConvertedName(type);
        }

        /// <summary>
        /// Get's the full name of the given type's full name after conversion.
        /// </summary>
        public string GetConvertedFullName(string fullName)
        {
            var index = fullName.LastIndexOf('.');
            var sourceNamespace = (index > 0) ? fullName.Substring(0, index) : string.Empty;
            var sourceName = (index > 0) ? fullName.Substring(index + 1) : fullName;

            var ns = ConvertNamespace(sourceNamespace);
            return ns + '.' + ConvertTypeName(sourceName);
        }

        /// <summary>
        /// Gets the converted name of the given type.
        /// </summary>
        public static string GetConvertedName(XTypeReference type)
        {
            return ConvertTypeName(type.Name);
        }

        /// <summary>
        /// Gets the converted name of the given type.
        /// </summary>
        public static string GetConvertedName(Type type)
        {
            return ConvertTypeName(type.Name);
        }

        /// <summary>
        /// Gets the converted name of the given type.
        /// </summary>
        public static string GetConvertedName(ClassFile type)
        {
            return ConvertTypeName(type.Name);
        }

        /// <summary>
        /// Gets the converted name of the given field.
        /// </summary>
        private static string ConvertTypeName(string name)
        {
            var sb = new StringBuilder(name.Length);
            var addPostfix = false;
            foreach (var ch in name)
            {
                if (IsSimpleNameChar(ch))
                {
                    sb.Append(ch);
                }
                else if (ch == '`')
                {
                    // replace with an allowed similar char. (ʹ)
                    sb.Append('\x02b9');
                }
                else
                {
                    addPostfix = true;
                }
            }
            if (addPostfix)
            {
                sb.Append(GetHashPostfix(name));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Is the given character allowed in a simple name?
        /// </summary>
        private static bool IsSimpleNameChar(char value)
        {
            return
                ((value >= 'A') && (value <= 'Z')) ||
                ((value >= 'a') && (value <= 'z')) ||
                ((value >= '0') && (value <= '9')) ||
                (value == '$') ||
                //(value == '-') ||
                (value == '_') ||
                ((value >= '\u00a1') && (value <= '\u1fff')) ||
                ((value >= '\u2010') && (value <= '\u2027')) ||
                ((value >= '\u2030') && (value <= '\ud7ff')) ||
                ((value >= '\ue000') && (value <= '\uffef')) ||
                ((value >= '\u00a1') && (value <= '\u1fff'));
        }

        /// <summary>
        /// Gets the converted name of the given field.
        /// </summary>
        public static string GetConvertedName(FieldReference field)
        {
            var name = field.Name;
            var originalName = name;
            name = name.Replace('<', '_');
            name = name.Replace('>', '_');
            if (name != originalName)
            {
                // Add hash to ensure unique
                name = name + GetHashPostfix(originalName);
            }
            return name;
        }

        /// <summary>
        /// Gets the converted name of the given field.
        /// </summary>
        public static string GetConvertedName(JvmClassLib.FieldDefinition field)
        {
            var name = field.Name;
            return name;
        }

        /// <summary>
        /// Gets the converted name of the given property.
        /// </summary>
        public static string GetConvertedName(PropertyReference property)
        {
            var name = property.Name;
            var originalName = name;
            name = name.Replace('<', '_');
            name = name.Replace('>', '_');
            if (name != originalName)
            {
                // Add hash to ensure unique
                name = name + GetHashPostfix(originalName);
            }
            return name;
        }

        /// <summary>
        /// Gets the converted name of the given field.
        /// </summary>
        public static string GetConvertedName(XMethodReference method)
        {
            var name = method.Name;
            // Handle special names
            switch (name)
            {
                case ".ctor":
                    return "<init>";
                case ".cctor":
                    return "<clinit>";
            }

            // Handle properties in custom views.
            var methodDef = method.Resolve();
            if (methodDef.DeclaringType.HasCustomViewAttribute())
            {
                if (methodDef.IsSetter && name.StartsWith("set_"))
                {
                    name = "set" + name.Substring(4);
                }
                else if (methodDef.IsGetter && name.StartsWith("get_"))
                {
                    name = "get" + name.Substring(4);
                }
            }

            // Avoid special characters
            var originalName = name;
            name = name.Replace('<', '_');
            name = name.Replace('>', '_');
            name = name.Replace('.', '_');

            if (name != originalName)
            {
                // Add hash to ensure unique
                name = name + '_' + GetHashPostfix(originalName);
            }

            return name;
        }

        /// <summary>
        /// Convert a name into a simple name.
        /// </summary>
        public static string GetSimpleName(string name)
        {
            var index = name.LastIndexOfAny(new[] { Dex.NestedClassSeparator, '/', '.' });
            if (index >= 0)
                return name.Substring(index + 1);
            return name;
        }

        /// <summary>
        /// Gets the namespace of the package (== the name of the package).
        /// </summary>
        public string PackageName { get { return packageName; } }

        /// <summary>
        /// Record the given field mapping
        /// </summary>
        internal void Record(XFieldDefinition xField, DexLib.FieldDefinition dField)
        {
            xFieldMap.Add(xField, dField);
        }

        /// <summary>
        /// Record the given method mapping
        /// </summary>
        internal void Record(XMethodDefinition xMethod, DexLib.MethodDefinition dMethod)
        {
            xMethodMap.Add(xMethod, dMethod);
        }

        /// <summary>
        /// Record the given field mapping
        /// </summary>
        internal DexLib.FieldDefinition GetField(XFieldDefinition xField)
        {
            DexLib.FieldDefinition dfield;
            if (xFieldMap.TryGetValue(xField, out dfield))
                return dfield;
            throw new ArgumentException(string.Format("Field {0} not found", xField));
        }

        public List<XMethodDefinition> XMethods { get { return xMethodMap.Keys.ToList(); } }

        /// <summary>
        /// Record the given method mapping
        /// </summary>
        internal DexLib.MethodDefinition GetMethod(XMethodDefinition xMethod)
        {
            DexLib.MethodDefinition dmethod;
            if (xMethodMap.TryGetValue(xMethod, out dmethod))
                return dmethod;
            /*var javaImportAttr = ilMethod.GetJavaImportAttribute();
            if (javaImportAttr != null)
            {
                string memberName;
                string descriptor;
                string className;
                javaImportAttr.GetDexOrJavaImportNames(ilMethod, out memberName, out descriptor, out className);
                var javaMethod = javaMethodMap.Keys.FirstOrDefault(x => (x.Name == memberName) && (x.Descriptor == descriptor) && (x.DeclaringClass.ClassName == className));
                if (javaMethod != null)
                {
                    return GetMethod(javaMethod);
                }
            }*/
            throw new ArgumentException(string.Format("Method {0} not found", xMethod));
        }

        private static string GetHashPostfix(string originalName)
        {
            // return a persistant hash code so that the code stays stable
            // and comparable between different compiler versions.
            // also 4 characters should suffice.
            return "_" + Math.Abs(HashCodeUtility.GetPersistentHashCode(originalName))
                             .ToString("0000").Substring(0, 4);
        }
    }
}
