using System.Collections.Generic;
using System.Linq;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Reference to a Void.
    /// </summary>
    public sealed class VoidTypeReference : TypeReference
    {
        public static readonly TypeReference Instance = new VoidTypeReference();

        /// <summary>
        /// Default ctor
        /// </summary>
        private VoidTypeReference()
        {
        }

        /// <summary>
        /// Is this a reference to an array type?
        /// </summary>
        public override bool IsArray { get { return false; } }

        /// <summary>
        /// Is this a reference to a base type?
        /// </summary>
        public override bool IsBaseType { get { return false; } }

        /// <summary>
        /// Does this type need 2 local variable slots instead of 1?
        /// </summary>
        public override bool IsWide { get { return false; } }

        /// <summary>
        /// Is this a reference to a normal type derived of java.lang.Object?
        /// </summary>
        public override bool IsObjectType { get { return false; } }

        /// <summary>
        /// Is this a reference to Void?
        /// </summary>
        public override bool IsVoid { get { return true; } }

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
        public override string ClassName { get { return "V"; } }

        /// <summary>
        /// Name of equivalent type in .NET.
        /// </summary>
        public override string ClrTypeName { get { return typeof(void).FullName; } }
    }
}
