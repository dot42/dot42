using System.Collections.Generic;
using System.Linq;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Reference to an object type.
    /// </summary>
    public sealed class ObjectTypeReference : TypeReference
    {
        private readonly string className;
        private readonly List<TypeArgument> arguments;
        private readonly ObjectTypeReference prefix;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ObjectTypeReference(string className, IEnumerable<TypeArgument> arguments, ObjectTypeReference prefix = null)
        {
            this.className = className;
            this.prefix = prefix;
            this.arguments = (arguments != null) ? arguments.ToList() : null;
        }

        /// <summary>
        /// Gets the actual type.
        /// </summary>
        public override string ClassName { get { return className; } }

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
        public override bool IsObjectType { get { return true; } }

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
        public override IEnumerable<TypeArgument> Arguments
        {
            get { return arguments ?? Enumerable.Empty<TypeArgument>(); }
        }

        /// <summary>
        /// List of class type signature for declaring type (if any)
        /// </summary>
        public ObjectTypeReference Prefix
        {
            get { return prefix; }
        }

        /// <summary>
        /// Name of equivalent type in .NET.
        /// </summary>
        public override string ClrTypeName { get { return JvmClassLib.ClassName.JavaClassNameToClrTypeName(className); } }
    }
}
