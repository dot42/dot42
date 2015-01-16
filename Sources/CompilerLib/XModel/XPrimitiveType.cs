namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Primitive type
    /// </summary>
    public sealed class XPrimitiveType : XTypeReference
    {
        private readonly XTypeReferenceKind kind;
        private readonly string name;
        private XTypeDefinition resolved;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal XPrimitiveType(XModule module, XTypeReferenceKind kind, string name)
            : base(module, true, null)
        {
            this.kind = kind;
            this.name = name;
        }

        /// <summary>
        /// Name of the member
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Create a fullname of this type reference.
        /// </summary>
        public override string GetFullName(bool noGenerics)
        {
            return "System." + name;
        }

        /// <summary>
        /// Gets the namespace of this type.
        /// Returns the namespace of the declaring type for nested types.
        /// </summary>
        /*public override string Namespace
        {
            get { return "System"; }
        }*/

        /// <summary>
        /// What kind of reference is this?
        /// </summary>
        public override XTypeReferenceKind Kind { get { return kind; } }

        /// <summary>
        /// Is this a primitive type?
        /// </summary>
        public override bool IsPrimitive { get { return true; } }

        /// <summary>
        /// Gets the deepest <see cref="XTypeReference.ElementType"/>.
        /// </summary>
        public override XTypeReference GetElementType()
        {
            return this;
        }

        /// <summary>
        /// Gets the type without array/generic modifiers
        /// </summary>
        public override XTypeReference ElementType
        {
            get { return this; }
        }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public override bool TryResolve(out XTypeDefinition type)
        {
            if (resolved == null)
            {
                if (!base.TryResolve(out resolved)) 
                    throw new XResolutionException(this);
            }
            type = resolved;
            return true;
        }
    }
}
