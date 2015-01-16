using System;
using System.Collections.Generic;
using System.Linq;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Reference to a type.
    /// </summary>
    public sealed class BaseTypeReference : TypeReference
    {
        private readonly char code;
        private readonly BaseTypes type;
        private readonly Type clrType;

        private static readonly BaseTypeReference[] baseTypes = new[] {
            new BaseTypeReference('B', BaseTypes.Byte, typeof(sbyte)), 
            new BaseTypeReference('C', BaseTypes.Char, typeof(char)), 
            new BaseTypeReference('D', BaseTypes.Double, typeof(double)), 
            new BaseTypeReference('F', BaseTypes.Float, typeof(float)), 
            new BaseTypeReference('I', BaseTypes.Int, typeof(int)), 
            new BaseTypeReference('J', BaseTypes.Long, typeof(long)), 
            new BaseTypeReference('S', BaseTypes.Short, typeof(short)), 
            new BaseTypeReference('Z', BaseTypes.Boolean, typeof(bool))
        };

        /// <summary>
        /// Default ctor
        /// </summary>
        private BaseTypeReference(char code, BaseTypes type, Type clrType)
        {
            this.code = code;
            this.type = type;
            this.clrType = clrType;
        }

        /// <summary>
        /// Try to lookup a base type reference by it's descriptor code.
        /// </summary>
        internal static bool TryGetByCode(char code, out BaseTypeReference type)
        {
            type = baseTypes.FirstOrDefault(x => x.code == code);
            return (type != null);
        }

        /// <summary>
        /// Try to lookup a base type reference by it's CLR equivalent.
        /// </summary>
        internal static bool TryGetByClrType(Type clrType, out BaseTypeReference type)
        {
            type = baseTypes.FirstOrDefault(x => x.clrType == clrType);
            return (type != null);
        }

        /// <summary>
        /// Code used in descriptors
        /// </summary>
        internal char Code { get { return code; } }

        /// <summary>
        /// Gets the actual type.
        /// </summary>
        public BaseTypes Type { get { return type; } }

        /// <summary>
        /// Type equivalent in .NET.
        /// </summary>
        public Type GetClrType(bool convertSignedByte)
        {
            if (convertSignedByte && (type == BaseTypes.Byte))
                return typeof (byte);
            return clrType;
        }

        /// <summary>
        /// Is this a reference to an array type?
        /// </summary>
        public override bool IsArray { get { return false; } }

        /// <summary>
        /// Is this a reference to a base type?
        /// </summary>
        public override bool IsBaseType { get { return true; } }

        /// <summary>
        /// Does this type need 2 local variable slots instead of 1?
        /// </summary>
        public override bool IsWide { get { return (type == BaseTypes.Double) || (type == BaseTypes.Long); } }

        /// <summary>
        /// Is this a reference to a normal type derived of java.lang.Object?
        /// </summary>
        public override bool IsObjectType { get { return false; } }

        /// <summary>
        /// Is this a reference to Void?
        /// </summary>
        public override bool IsVoid { get { return false; } }

        /// <summary>
        /// Is this a reference to a generic type?
        /// </summary>
        public override bool IsTypeVariable { get { return false; } }

        /// <summary>
        /// Gets all type arguments
        /// </summary>
        public override IEnumerable<TypeArgument> Arguments { get { return Enumerable.Empty<TypeArgument>(); } }

        /// <summary>
        /// Class Name in java terms.
        /// </summary>
        public override string ClassName { get { return Code.ToString(); } }

        /// <summary>
        /// Name of equivalent type in .NET.
        /// </summary>
        public override string ClrTypeName { get { return clrType.FullName; } }
    }
}
