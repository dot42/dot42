namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Generic parameter
    /// </summary>
    public abstract class XGenericParameter : XTypeReference
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal XGenericParameter(XModule module)
            : base(module, false, null)
        {
        }

        /// <summary>
        /// Name of the member
        /// </summary>
        public sealed override string Name
        {
            get
            {
                var prefix = (Owner is XTypeReference) ? "!" : "!!";
                return prefix + Position;
            }        
        }

        /// <summary>
        /// Create a fullname of this type reference.
        /// </summary>
        public override string GetFullName(bool noGenerics)
        {
            return noGenerics ? "System.Object" : Name;
            //if (!noGenerics) return Name;

            //var c = Constraints;

            //if (c.Length == 0 || (c.Length > 1 && c[0].Resolve().IsInterface))
            //    return "System.Object";

            //return c[0].GetFullName(true);
        }

        /// <summary>
        /// Gets the owner of this generic parameter.
        /// </summary>
        public abstract IXGenericParameterProvider Owner { get; }

        /// <summary>
        /// Is this a generic parameter?
        /// </summary>
        public override bool IsGenericParameter { get { return true; } }

        /// <summary>
        /// Gets the index of this generic parameter in the owners list of generic parameters.
        /// </summary>
        public abstract int Position { get; }

        public virtual XTypeReference[] Constraints { get {  return new XTypeReference[0];} } 

        /// <summary>
        /// What kind of reference is this?
        /// </summary>
        public override XTypeReferenceKind Kind { get { return XTypeReferenceKind.GenericParameter; } }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public override bool TryResolve(out XTypeDefinition type)
        {
            type = null;
            return false;
        }

        /// <summary>
        /// Is this generic parameter the same as the given other?
        /// </summary>
        public bool IsSame(XGenericParameter other)
        {
            return (Position == other.Position);
        }

        /// <summary>
        /// Simple implementation
        /// </summary>
        public sealed class SimpleXGenericParameter : XGenericParameter
        {
            private readonly IXGenericParameterProvider owner;
            private readonly int position;

            /// <summary>
            /// Default ctor
            /// </summary>
            internal SimpleXGenericParameter(IXGenericParameterProvider owner, int position)
                : base(owner.Module)
            {
                this.owner = owner;
                this.position = position;
            }

            /// <summary>
            /// Gets the owner of this generic parameter.
            /// </summary>
            public override IXGenericParameterProvider Owner
            {
                get { return owner; }
            }

            /// <summary>
            /// Gets the index of this generic parameter in the owners list of generic parameters.
            /// </summary>
            public override int Position
            {
                get { return position; }
            }
        }
    }
}
