using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Helper with build related extension methods
    /// </summary>
    internal static class BuilderExtensions
    {
        /// <summary>
        /// Resolve the given java based type reference to a Cecil based type reference.
        /// </summary>
        internal static NetTypeReference Resolve(this TypeReference jRef, TargetFramework target, IBuilderGenericContext gcontext, bool convertSignedBytes)
        {
            NetTypeReference result;
            if (TryResolve(jRef, target, gcontext, convertSignedBytes, out result))
                return result;
            throw new ArgumentException(string.Format("Unknown java type ref. {0}", jRef));
        }

        /// <summary>
        /// Resolve the given java based type reference to a Cecil based type reference.
        /// </summary>
        internal static bool TryResolve(this TypeReference jRef, TargetFramework target, IBuilderGenericContext gcontext, bool convertSignedBytes, out NetTypeReference result)
        {
            result = null;
            if (jRef.IsArray)
            {
                var aType = (ArrayTypeReference) jRef;
                NetTypeReference elementType;
                if (!aType.ElementType.TryResolve(target, gcontext, convertSignedBytes, out elementType))
                    return false;
                result = new NetArrayType(elementType);
                return true;
            }
            if (jRef.IsVoid)
            {
                result = target.TypeNameMap.GetByType(typeof(void));
                return true;
            }
            if (jRef.IsBaseType)
            {
                var bType = (BaseTypeReference) jRef;
                result = target.TypeNameMap.GetByType(bType.GetClrType(convertSignedBytes));
                return true;
            }
            if (jRef.IsObjectType)
            {
                return TryResolveObjectType((ObjectTypeReference) jRef, target, gcontext, out result);
            }
            if (jRef.IsTypeVariable)
            {
                var tRef = (TypeVariableReference) jRef;
                if (gcontext.TryResolveTypeParameter(tRef.ClassName, target, out result))
                    return true;
                result = target.TypeNameMap.Object; // Hack for incorrect behaving java classes
                return true;
            }
            return false;
            //throw new ArgumentException(string.Format("Unknown java type ref. {0}", jRef));
        }

        /// <summary>
        /// Resolve the given java based type reference to a Cecil based type reference.
        /// </summary>
        private static bool TryResolveObjectType(ObjectTypeReference jRef, TargetFramework target, IBuilderGenericContext gcontext, out NetTypeReference result)
        {
            var objecType = jRef;
            switch (objecType.ClassName)
            {
                case "java/lang/Exception":
                case "java/lang/Throwable":
                    result = target.TypeNameMap.GetByType(typeof(Exception));
                    return true;
                case "java/lang/Boolean":
                case "java/lang/Byte":
                case "java/lang/Character":
                case "java/lang/Double":
                case "java/lang/Float":
                case "java/lang/Integer":
                case "java/lang/Long":
                case "java/lang/Short":
                    result = new NetNullableType(target.TypeNameMap.GetType(objecType, target, gcontext));
                    return true;
                default:
                    return target.TypeNameMap.TryGetType(objecType, target, gcontext, out result);
            }
        }

        /// <summary>
        /// Is the given type java/lang/Void?
        /// </summary>
        internal static bool IsJavaLangVoid(this TypeReference type)
        {
            if (!type.IsObjectType)
                return false;
            return (((ObjectTypeReference) type).ClassName == "java/lang/Void");
        }

        /// <summary>
        /// Do type refer to System.Object?
        /// </summary>
        internal static bool IsObject(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.Object");
        }

        /// <summary>
        /// Do type refer to System.Boolean?
        /// </summary>
        internal static bool IsBoolean(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.Boolean");
        }

        /// <summary>
        /// Do type refer to System.Char?
        /// </summary>
        internal static bool IsChar(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.Char");
        }

        /// <summary>
        /// Do type refer to System.Double?
        /// </summary>
        internal static bool IsDouble(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.Double");
        }

        /// <summary>
        /// Do type refer to System.Byte?
        /// </summary>
        internal static bool IsByte(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.Byte");
        }

        /// <summary>
        /// Do type refer to System.SByte?
        /// </summary>
        internal static bool IsSByte(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.SByte");
        }

        /// <summary>
        /// Do type refer to System.Int16?
        /// </summary>
        internal static bool IsInt16(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.Int16");
        }

        /// <summary>
        /// Do type refer to System.Int32?
        /// </summary>
        internal static bool IsInt32(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.Int32");
        }

        /// <summary>
        /// Do type refer to System.Int64?
        /// </summary>
        internal static bool IsInt64(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.Int64");
        }

        /// <summary>
        /// Do type refer to System.Single?
        /// </summary>
        internal static bool IsSingle(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.Single");
        }

        /// <summary>
        /// Do type refer to System.String?
        /// </summary>
        internal static bool IsString(this NetTypeReference type)
        {
            return (StripGlobalPrefix(type.FullName) == "System.String");
        }

        /// <summary>
        /// Do type refer to System.Void?
        /// </summary>
        internal static bool IsVoid(this NetTypeReference type)
        {
            return (type == null) || (StripGlobalPrefix(type.FullName) == "System.Void");
        }

        /// <summary>
        /// Do type1 and type2 refer to the same type?
        /// </summary>
        /// <remarks>
        /// The types are also the same, if the Generic Parameters have a different name.
        /// MyClass&lt;T&gt; is the same as MyClass&lt;U&gt;.
        /// So we need to do some special processing to check this.
        /// </remarks>
        internal static bool AreSame(this NetTypeReference type1, NetTypeReference type2)
        {
            if ((type1 == null) && (type2 == null))
                return true;
            if ((type1 == null) || (type2 == null))
                return false;

            string fullName1 = type1.FullName;
            string fullName2 = type2.FullName;

            return AreSameGenericParameters(fullName1, fullName2, false);
        }

        /// <summary>
        /// Compare two type names. Also check for generic parameters, which need not have the same name.
        /// </summary>
        /// <param name="fullName1">Name of the first type.</param>
        /// <param name="fullName2">Name of the second type.</param>
        /// <param name="areGenericStrings">If we are already inside the &lt;...&gt; part, we can ignore parameter names.</param>
        /// <returns>True if te two type names are the same, false otherwise.</returns>
        private static bool AreSameGenericParameters(string fullName1, string fullName2, bool areGenericStrings)
        {
            if (fullName1.Equals(fullName2))
                return true;

            // Here we check for Generic Parameters. Remember that they can be nested.
            while (fullName1.Contains('<') || fullName2.Contains('<'))
            {
                string gp1 = string.Empty;
                string gp2 = string.Empty;
                int commaCount1 = 0;
                int commaCount2 = 0;
                int count = 0;
                for (int i = 0; i < fullName1.Length; i++)
                {
                    switch (fullName1[i])
                    {
                        case '<':
                            count++;
                            gp1 += '<';
                            fullName1 = fullName1.Remove(i, 1);
                            i--;
                            break;
                        case '>':
                            count--;
                            gp1 += '>';
                            fullName1 = fullName1.Remove(i, 1);
                            i--;
                            if (0 == count)
                                i = fullName1.Length;
                            break;
                        case ',':
                            if (0 != count)
                            {
                                commaCount1++;
                                gp1 += fullName1[i];
                                fullName1 = fullName1.Remove(i, 1);
                                i--;
                            }
                            break;
                        default:
                            if (0 != count)
                            {
                                gp1 += fullName1[i];
                                fullName1 = fullName1.Remove(i, 1);
                                i--;
                            }
                            break;
                    }
                }

                if (0 != count)
                    return false;

                for (int i = 0; i < fullName2.Length; i++)
                {
                    switch (fullName2[i])
                    {
                        case '<':
                            count++;
                            gp2 += '<';
                            fullName2 = fullName2.Remove(i, 1);
                            i--;
                            break;
                        case '>':
                            count--;
                            gp2 += '>';
                            fullName2 = fullName2.Remove(i, 1);
                            i--;
                            if (0 == count)
                                i = fullName2.Length;
                            break;
                        case ',':
                            if (0 != count)
                            {
                                commaCount2++;
                                gp2 += fullName2[i];
                                fullName2 = fullName2.Remove(i, 1);
                                i--;
                            }
                            break;
                        default:
                            if (0 != count)
                            {
                                gp2 += fullName2[i];
                                fullName2 = fullName2.Remove(i, 1);
                                i--;
                            }
                            break;
                    }
                }

                if (0 != count)
                    return false;

                if (!fullName1.Equals(fullName2))
                    return false;

                if (commaCount1 != commaCount2)
                    return false;

                if ((gp1.Length == 0 || gp2.Length == 0) && gp1.Length != gp2.Length)
                    return false; // TODO: check if this is correct, or if true is correct.


                gp1 = gp1.Substring(1, gp1.Length - 2);
                gp2 = gp2.Substring(1, gp2.Length - 2);

                if (!AreSameGenericParameters(gp1, gp2, true))
                    return false;
            }

            return fullName1.Equals(fullName2) || areGenericStrings;
        }

        /// <summary>
        /// Do prop1 and prop2 have the same name and signature?
        /// </summary>
        internal static bool AreSame(this NetPropertyDefinition prop1, NetPropertyDefinition prop2)
        {
            if (prop1.Name != prop2.Name)
                return false;
            if (!prop1.PropertyType.AreSame(prop2.PropertyType))
                return false;
            var count = prop1.Parameters.Count;
            if (count != prop2.Parameters.Count)
                return false;
            for (var i = 0; i < count; i++ )
            {
                if (!prop1.Parameters[i].ParameterType.AreSame(prop2.Parameters[i].ParameterType))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Are the two methods equal wrt name, parameters and type parameter count?
        /// </summary>
        internal static bool IsDuplicate(this NetMethodDefinition method1, NetMethodDefinition method2)
        {
            if (method1.Name != method2.Name)
                return false;
            if (method1.IsSignConverted != method2.IsSignConverted)
                return false;
            var count = method1.Parameters.Count;
            if (count != method2.Parameters.Count)
                return false;
            if (method1.GenericParameters.Count != method2.GenericParameters.Count)
                return false;
            for (var i = 0; i < count; i++)
            {
                if (!method1.Parameters[i].ParameterType.AreSame(method2.Parameters[i].ParameterType))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Are the two methods equal wrt name, parameters and type parameter count AND return type?
        /// </summary>
        internal static bool IsExactDuplicate(this NetMethodDefinition method1, NetMethodDefinition method2)
        {
            return method1.IsDuplicate(method2) && (method1.ReturnType.AreSame(method2.ReturnType));
        }

        /// <summary>
        /// Gets the base type + implemented interfaces of the given type.
        /// </summary>
        internal static IEnumerable<NetTypeReference> GetBaseTypes(this NetTypeDefinition type, bool recursive)
        {
            if (type.BaseType != null)
                yield return type.BaseType;
            foreach (var intf in type.Interfaces)
            {
                yield return intf;
            }
            if (recursive)
            {
                if ((type.BaseType != null))
                {
                    foreach (var t in type.BaseType.GetElementType().GetBaseTypes(true))
                    {
                        yield return t;
                    }
                }
                foreach (var t in type.Interfaces.Select(x => x.GetElementType()).SelectMany(intf => intf.GetBaseTypes(true)))
                {
                    yield return t;
                }
            }
        }

        /// <summary>
        /// Gets the declaring types of the given type.
        /// </summary>
        internal static IEnumerable<NetTypeDefinition> GetDeclaringTypes(this NetTypeDefinition type)
        {
            var declaringType = type.DeclaringType;
            while (declaringType != null)
            {
                yield return declaringType;
                declaringType = declaringType.DeclaringType;
            }
        }

        /// <summary>
        /// Gets the base type, parent base type, parent parent base type and so on.
        /// </summary>
        internal static IEnumerable<NetTypeReference> GetBaseTypesWithoutInterfaces(this NetTypeDefinition type)
        {
            while (type.BaseType != null)
            {
                yield return type.BaseType;
                type = type.BaseType.GetElementType();
            }
        }

        /// <summary>
        /// Remove any "global::" prefix from the given string.
        /// </summary>
        private static string StripGlobalPrefix(string s)
        {
            const string prefix = "global::";
            if (s.StartsWith(prefix))
                return s.Substring(prefix.Length);
            return s;
        }

        /// <summary>
        /// Does the given type contains is a signed byte or a signed byte array?
        /// </summary>
        internal static bool ContainsSignedByte(this TypeReference type)
        {
            if (type == null)
                return false;
            if (type.IsBaseType)
            {
                return (((BaseTypeReference) type).Type == BaseTypes.Byte);
            }
            if (type.IsArray)
            {
                return ((ArrayTypeReference) type).ElementType.ContainsSignedByte();
            }
            return false;
        }

        /// <summary>
        /// Does the given type is an unsigned byte or references on in its
        /// elements types or generic types?
        /// </summary>
        internal static bool ContainsUnsignedByte(this NetTypeReference type)
        {
            if (type == null)
                return false;

            if (type.IsByte())
                return true;

            return type.GetReferencedTypes().Any(r => r.IsByte());
        }

    }
}
