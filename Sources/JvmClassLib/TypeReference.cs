using System.Collections.Generic;
using System.Diagnostics;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Reference to a type.
    /// </summary>
    [DebuggerDisplay("{ClrTypeName}")]
    public abstract class TypeReference : AbstractReference
    {
        /// <summary>
        /// Is this a reference to an array type?
        /// </summary>
        public abstract bool IsArray { get; }

        /// <summary>
        /// Is this a reference to a base type?
        /// </summary>
        public abstract bool IsBaseType { get; }

        /// <summary>
        /// Does this type need 2 local variable slots instead of 1?
        /// </summary>
        public abstract bool IsWide { get; }

        /// <summary>
        /// Is this a reference to a normal type derived of java.lang.Object?
        /// </summary>
        public abstract bool IsObjectType { get; }

        /// <summary>
        /// Is this a reference to Void?
        /// </summary>
        public abstract bool IsVoid { get; }

        /// <summary>
        /// Is this a reference to a generic type?
        /// </summary>
        public abstract bool IsTypeVariable { get; }

        /// <summary>
        /// Gets all type arguments
        /// </summary>
        public abstract IEnumerable<TypeArgument> Arguments { get; }
        
        /// <summary>
        /// Class Name in java terms.
        /// </summary>
        public abstract string ClassName { get; }

        /// <summary>
        /// Name of equivalent type in .NET.
        /// </summary>
        public abstract string ClrTypeName { get; }

        public override string ToString()
        {
            return ClassName;
        }
    }
}
