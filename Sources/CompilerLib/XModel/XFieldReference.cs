using System.Linq;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Reference to a field
    /// </summary>
    public abstract class XFieldReference : XMemberReference
    {
        private XFieldDefinition resolvedField;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected XFieldReference(XTypeReference declaringType)
            : base(declaringType.Module, declaringType)
        {
        }

        /// <summary>
        /// Type of field
        /// </summary>
        public abstract XTypeReference FieldType { get; }

        /// <summary>
        /// Name of the member (including all)
        /// </summary>
        public sealed override string FullName
        {
            get { return FieldType.FullName + " " + DeclaringType.FullName + "." + Name; }
        }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public virtual bool TryResolve(out XFieldDefinition field)
        {
            if (resolvedField != null)
            {
                field = resolvedField;
                return true;
            }
            field = null;
            XTypeDefinition declaringType;
            if (!DeclaringType.GetElementType().TryResolve(out declaringType))
                return false;
            if (!declaringType.TryGet(this, out field))
                return false;
            // Cache for later
            resolvedField = field;
            declaringType.AddFlushAction(() => resolvedField = null);
            return true;
        }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// Throw an exception is the resolution failed.
        /// </summary>
        public XFieldDefinition Resolve()
        {
            XFieldDefinition fieldDef;
            if (TryResolve(out fieldDef))
                return fieldDef;
            throw new XResolutionException(this);
        }

        /// <summary>
        /// Is this reference equal to the given other reference?
        /// </summary>
        public bool IsSame(XFieldReference other)
        {
            return DeclaringType.IsSame(other.DeclaringType) && IsSameExceptDeclaringType(other);
        }

        /// <summary>
        /// Is this reference equal to the given other reference?
        /// </summary>
        public virtual bool IsSameExceptDeclaringType(XFieldReference other)
        {
            return (Name == other.Name) && FieldType.IsSame(other.FieldType);
        }

        /// <summary>
        /// Simple implementation
        /// </summary>
        public sealed class Simple : XFieldReference
        {
            private readonly string name;
            private readonly XTypeReference fieldType;

            /// <summary>
            /// Default ctor
            /// </summary>
            public Simple(string name, XTypeReference fieldType, XTypeReference declaringType)
                : base(declaringType)
            {
                this.name = name;
                this.fieldType = fieldType;
            }

            /// <summary>
            /// Name of the member
            /// </summary>
            public override string Name
            {
                get { return name; }
            }

            /// <summary>
            /// Type of field
            /// </summary>
            public override XTypeReference FieldType
            {
                get { return fieldType; }
            }
        }
    }
}
