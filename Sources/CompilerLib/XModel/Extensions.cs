using System;
using System.Linq;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Is the given type of the given kind?
        /// </summary>
        public static bool Is(this XTypeReference type, XTypeReferenceKind kind)
        {
            return (type != null) && (type.Kind == kind);
        }

        /// <summary>
        /// Is the given type of the given kind?
        /// </summary>
        public static bool Is(this XTypeReference type, XTypeReferenceKind kind1, XTypeReferenceKind kind2)
        {
            return (type != null) && 
                ((type.Kind == kind1) || (type.Kind == kind2));
        }

        /// <summary>
        /// Is the given type of the given kind?
        /// </summary>
        public static bool Is(this XTypeReference type, params XTypeReferenceKind[] kinds)
        {
            return (type != null) && (Array.IndexOf(kinds, type.Kind) >= 0);
        }

        /// <summary>
        /// Is the given type a bool?
        /// </summary>
        public static bool IsBoolean(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Bool.IsSame(type);
        }

        /// <summary>
        /// Is the given type a byte?
        /// </summary>
        public static bool IsByte(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Byte.IsSame(type);
        }

        /// <summary>
        /// Is the given type a sbyte?
        /// </summary>
        public static bool IsSByte(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.SByte.IsSame(type);
        }

        /// <summary>
        /// Is the given type a char?
        /// </summary>
        public static bool IsChar(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Char.IsSame(type);
        }

        /// <summary>
        /// Is the given type a short?
        /// </summary>
        public static bool IsInt16(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Short.IsSame(type);
        }

        /// <summary>
        /// Is the given type a ushort?
        /// </summary>
        public static bool IsUInt16(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.UShort.IsSame(type);
        }

        /// <summary>
        /// Is the given type a int?
        /// </summary>
        public static bool IsInt32(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Int.IsSame(type);
        }

        /// <summary>
        /// Is the given type a uint?
        /// </summary>
        public static bool IsUInt32(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.UInt.IsSame(type);
        }

        /// <summary>
        /// Is the given type a long?
        /// </summary>
        public static bool IsInt64(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Long.IsSame(type);
        }

        /// <summary>
        /// Is the given type a ulong?
        /// </summary>
        public static bool IsUInt64(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.ULong.IsSame(type);
        }

        /// <summary>
        /// Is the given type a IntPtr?
        /// </summary>
        public static bool IsIntPtr(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.IntPtr.IsSame(type);
        }

        /// <summary>
        /// Is the given type a float?
        /// </summary>
        public static bool IsFloat(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Float.IsSame(type);
        }

        /// <summary>
        /// Is the given type a double?
        /// </summary>
        public static bool IsDouble(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Double.IsSame(type);
        }

        /// <summary>
        /// Is the given type void?
        /// </summary>
        public static bool IsVoid(this XTypeReference type)
        {
            return (type != null) && ((type.FullName == "System.Void") || type.Module.TypeSystem.Void.IsSame(type));
        }

        /// <summary>
        /// Is the given type a wide primitive (long, ulong, double)?
        /// </summary>
        public static bool IsWide(this XTypeReference type)
        {
            if (type == null) return false;
            var ts = type.Module.TypeSystem;
            return ts.Long.IsSame(type) || ts.ULong.IsSame(type) || ts.Double.IsSame(type);
        }

        /// <summary>
        /// Is the given type System.Type?
        /// </summary>
        public static bool IsSystemType(this XTypeReference type)
        {
            return (type != null) && (type.FullName == "System.Type");
        }

        /// <summary>
        /// Is the given type a reference to System.Object?
        /// </summary>
        public static bool IsSystemObject(this XTypeReference type)
        {
            return (type != null) && (type.FullName == "System.Object");
        }

        /// <summary>
        /// Is the given type a reference to System.String?
        /// </summary>
        public static bool IsSystemString(this XTypeReference type)
        {
            return (type != null) && (type.FullName == "System.String");
        }

        /// <summary>
        /// Is the given type a reference to System.Decimal?
        /// </summary>
        public static bool IsSystemDecimal(this XTypeReference type)
        {
            return (type != null) && (type.FullName == "System.Decimal");
        }

        /// <summary>
        /// Is the given type a reference to System.Nullable`1?
        /// </summary>
        public static bool IsSystemNullable(this XTypeReference type)
        {
            return (type != null) && (type.FullName == "System.Nullable`1");
        }

        /// <summary>
        /// Is the given type a reference to System.Array?
        /// </summary>
        public static bool IsSystemArray(this XTypeReference type)
        {
            return (type != null) && (type.FullName == "System.Array");
        }

        /// <summary>
        /// Is the given type a reference to System.IFormattable?
        /// </summary>
        public static bool IsSystemIFormattable(this XTypeReference type)
        {
            return (type != null) && (type.FullName == "System.IFormattable");
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.ICollection?
        /// </summary>
        public static bool IsSystemCollectionsICollection(this XTypeReference type)
        {
            return (type != null) && (type.FullName == "System.Collections.ICollection");
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.IEnumerable?
        /// </summary>
        public static bool IsSystemCollectionsIEnumerable(this XTypeReference type)
        {
            return (type != null) && (type.FullName == "System.Collections.IEnumerable");
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.IList?
        /// </summary>
        public static bool IsSystemCollectionsIList(this XTypeReference type)
        {
            return (type != null) && (type.FullName == "System.Collections.IList");
        }

        /// <summary>
        /// Does the given type extend from System.MulticastDelegate?
        /// </summary>
        public static bool IsDelegate(this XTypeDefinition type)
        {
            while (true)
            {
                var baseType = type.BaseType;
                if (baseType == null)
                    break;
                if (!baseType.TryResolve(out type))
                    break;
                if (type.FullName == typeof(System.MulticastDelegate).FullName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Is the given type System.Collections.ICollection or a derived interface?
        /// </summary>
        public static bool ExtendsICollection(this XTypeReference type)
        {
            if (type.FullName == "System.Collections.ICollection")
            {
                return true;
            }

            XTypeDefinition typeDef;
            if (!type.TryResolve(out typeDef) || !typeDef.IsInterface)
                return false;
            return typeDef.Interfaces.Any(ExtendsICollection);
        }

        /// <summary>
        /// Is the given type IEnumerable or a derived interface?
        /// </summary>
        public static bool ExtendsIEnumerable(this XTypeReference type)
        {
            if (type.FullName == "System.Collections.IEnumerable")
            {
                return true;
            }

            XTypeDefinition typeDef;
            if (!type.TryResolve(out typeDef) || !typeDef.IsInterface)
                return false;
            return typeDef.Interfaces.Any(ExtendsIEnumerable);
        }

        /// <summary>
        /// Is the given type System.Collections.IList, System.Collections.Generic.IList(T) or a derived interface?
        /// </summary>
        public static bool ExtendsIList(this XTypeReference type)
        {
            var fullName = type.FullName;
            if ((fullName == "System.Collections.IList") ||
                (fullName == "System.Collections.Generic.IList`1"))
            {
                return true;
            }

            XTypeDefinition typeDef;
            if (!type.TryResolve(out typeDef) || !typeDef.IsInterface)
                return false;
            return typeDef.Interfaces.Any(ExtendsIList);
        }

        /// <summary>
        /// Is the given type an array of a generic parameter?
        /// </summary>
        public static bool IsGenericParameterArray(this XTypeReference type)
        {
            if (!type.IsArray)
                return false;
            return type.ElementType.IsGenericParameter;
        }

        /// <summary>
        /// Is the given type an array of a primitive elements?
        /// </summary>
        public static bool IsPrimitiveArray(this XTypeReference type)
        {
            if (!type.IsArray)
                return false;
            return type.ElementType.IsPrimitive;
        }

        /// <summary>
        /// Is the given type a type definition or a normal type reference?
        /// </summary>
        public static bool IsDefinitionOrReferenceOrPrimitive(this XTypeReference type)
        {
            if (type.IsDefinition || type.IsPrimitive)
                return true;
            return (type.Kind == XTypeReferenceKind.TypeReference);
        }

        /// <summary>
        /// Is the given type System.Nullable&lt;T&gt;?
        /// </summary>
        public static bool IsNullableT(this XTypeReference type)
        {
            return (type.FullName == "System.Nullable`1");
        }

        /// <summary>
        /// Is the given type an enum?
        /// </summary>
        public static bool IsEnum(this XTypeReference type)
        {
            XTypeDefinition typeDef;
            return type.IsEnum(out typeDef);
        }

        /// <summary>
        /// Is the given type an enum?
        /// </summary>
        public static bool IsEnum(this XTypeReference type, out XTypeDefinition typeDef)
        {
            typeDef = null;
            if (type == null)
                return false;
            return type.TryResolve(out typeDef) && typeDef.IsEnum;
        }

        /// <summary>
        /// Is the given type a struct?
        /// </summary>
        public static bool IsStruct(this XTypeReference type)
        {
            XTypeDefinition typeDef;
            return type.IsStruct(out typeDef);
        }

        /// <summary>
        /// Is the given type a struct?
        /// </summary>
        public static bool IsStruct(this XTypeReference type, out XTypeDefinition typeDef)
        {
            typeDef = null;
            if (type == null)
                return false;
            return type.TryResolve(out typeDef) && typeDef.IsStruct;
        }

        /// <summary>
        /// Is the given type a base class of the given child?
        /// </summary>
        public static bool IsBaseOf(this XTypeDefinition type, XTypeDefinition child)
        {
            while (child != null)
            {
                if (child.BaseType == null)
                    return false;
                XTypeDefinition baseType;
                if (!child.BaseType.TryResolve(out baseType))
                    return false;
                if (baseType.IsSame(type))
                    return true;
                child = baseType;
            }
            return false;
        }
    }
}
