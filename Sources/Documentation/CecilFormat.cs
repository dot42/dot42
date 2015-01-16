using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Dot42.Documentation
{
    public class CecilFormat
    {
        private static readonly Dictionary<string, string> typeReplacements = new Dictionary<string, string> {
            { typeof(Byte).FullName, "byte" },
            { typeof(SByte).FullName, "sbyte" },
            { typeof(Boolean).FullName, "bool" },
            { typeof(Char).FullName, "char" },
            { typeof(Int16).FullName, "short" },
            { typeof(Int32).FullName, "int" },
            { typeof(Int64).FullName, "long" },
            { typeof(UInt16).FullName, "ushort" },
            { typeof(UInt32).FullName, "uint" },
            { typeof(UInt64).FullName, "ulong" },
            { typeof(Single).FullName, "float" },
            { typeof(Double).FullName, "double" },
            { typeof(String).FullName, "string" },
            { typeof(Object).FullName, "object" },
            { typeof(void).FullName, "void" },
        };

        /// <summary>
        /// Gets the human readable name of the given type including generic arguments.
        /// </summary>
        public static string GetTypeName(TypeReference type)
        {
            return GetTypeName(type, true, true);
        }

        /// <summary>
        /// Gets the human readable name of the given type.
        /// </summary>
        public static string GetTypeName(TypeReference type, bool includeGenericParameters, bool includeGenericArguments)
        {
            if (type.IsNested)
            {
                var result = GetTypeName(type.DeclaringType, includeGenericParameters, includeGenericArguments) + "." +
                             StripGenericParameterCount(type.Name);
                if (includeGenericParameters)
                    result += GetGenericParameters(type);
                return result;
            }
            if (type.IsArray)
            {
                return GetTypeName(((ArrayType) type).ElementType, includeGenericParameters, includeGenericArguments) + "[]";
            }
            if (type.IsGenericParameter) return type.Name;
            if (type.IsGenericInstance)
            {
                var git = (GenericInstanceType) type;
                var arguments = includeGenericArguments ?
                    git.GenericArguments.Select(x => GetTypeName(x, includeGenericParameters, includeGenericArguments)) :
                    git.ElementType.GenericParameters.Select(x => x.Name);
                return GetTypeName(git.ElementType, false, includeGenericArguments) + '<' + string.Join(",", arguments) + '>';
            }
            if (type.IsByReference)
            {
                return "ref " + GetTypeName(((ByReferenceType) type).ElementType, includeGenericParameters, includeGenericArguments);
            }
            var fullName = type.FullName;
            string replacement;
            if (typeReplacements.TryGetValue(fullName, out replacement))
                return replacement;
            return StripGenericParameterCount(fullName) + (includeGenericParameters ? GetGenericParameters(type) : string.Empty);
        }

        /// <summary>
        /// Gets the human readable name of the given type.
        /// </summary>
        public static string GetShortTypeName(TypeReference type)
        {
            return GetShortTypeName(type, true, true);
        }

        /// <summary>
        /// Gets the human readable name of the given type.
        /// </summary>
        public static string GetShortTypeName(TypeReference type, bool includeGenericParameters, bool includeGenericArguments)
        {
            if (type.IsNested)
            {
                var result = GetShortTypeName(type.DeclaringType, includeGenericParameters, includeGenericArguments) + "." + StripGenericParameterCount(type.Name);
                if (includeGenericParameters) result += GetGenericParameters(type);
                return result;
            }
            if (type.IsArray)
            {
                return GetShortTypeName(((ArrayType)type).ElementType) + "[]";
            }
            if (type.IsGenericParameter) return type.Name;
            if (type.IsGenericInstance)
            {
                var git = (GenericInstanceType)type;
                var arguments = includeGenericArguments ?
                    git.GenericArguments.Select(x => GetShortTypeName(x, includeGenericParameters, includeGenericArguments)) :
                    git.ElementType.GenericParameters.Select(x => x.Name);
                return GetShortTypeName(git.ElementType, false, false) + '<' + string.Join(",", arguments) + '>';
            }
            if (type.IsByReference)
            {
                return "ref " + GetShortTypeName(((ByReferenceType)type).ElementType);
            }
            var fullName = type.FullName;
            string replacement;
            if (typeReplacements.TryGetValue(fullName, out replacement))
                return replacement;
            var prefix = (type.DeclaringType != null) ? GetShortTypeName(type.DeclaringType) + "." : string.Empty;
            var name = StripGenericParameterCount(type.Name);
            if (includeGenericParameters) name += GetGenericParameters(type);
            return prefix + name;
        }

        /// <summary>
        /// Remove the generic parameter count postfix from the given name
        /// </summary>
        private static string StripGenericParameterCount(string name)
        {
            var index = name.IndexOf('`');
            if (index > 0)
                return name.Substring(0, index);
            return name;
        }

        /// <summary>
        /// Format the generic parameters of given provider.
        /// </summary>
        private static string GetGenericParameters(IGenericParameterProvider provider)
        {
            if (!provider.HasGenericParameters) return string.Empty;
            return "<" + string.Join(", ", provider.GenericParameters.Select(x => x.Name)) + ">";
        }
    }
}
