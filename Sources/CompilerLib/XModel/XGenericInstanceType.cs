using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Generic instance
    /// </summary>
    public sealed class XGenericInstanceType : XTypeSpecification, IXGenericInstance
    {
        private readonly ReadOnlyCollection<XTypeReference> genericArguments;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XGenericInstanceType(XTypeReference elementType, IEnumerable<XTypeReference> genericArguments)
            : base(elementType)
        {
            this.genericArguments = genericArguments.ToList().AsReadOnly();
            if (this.genericArguments.Count != GenericParameters.Count)
                throw new ArgumentException("Mismatch in generic parameter/argument count");
        }

        /// <summary>
        /// Gets all generic arguments
        /// </summary>
        public ReadOnlyCollection<XTypeReference> GenericArguments { get { return genericArguments; } }

        /// <summary>
        /// What kind of reference is this?
        /// </summary>
        public override XTypeReferenceKind Kind { get { return XTypeReferenceKind.GenericInstanceType; } }

        /// <summary>
        /// Is this a generic instance?
        /// </summary>
        public override bool IsGenericInstance { get { return true; } }

        /// <summary>
        /// Is this generic parameter the same as the given other?
        /// </summary>
        public bool IsSame(XGenericInstanceType other)
        {
            if (!ElementType.IsSame(other.ElementType) || (GenericArguments.Count != other.GenericArguments.Count))
                return false;
            return !GenericArguments.Where((t, i) => !t.IsSame(other.GenericArguments[i])).Any();
        }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public override bool TryResolve(out XTypeDefinition type)
        {
            return ElementType.TryResolve(out type);
        }

        /// <summary>
        /// Create a method reference for the given method using this this as declaring type.
        /// Usually the method will be returned, unless this type is a generic instance type.
        /// </summary>
        public override XMethodReference CreateReference(XMethodDefinition method)
        {
            return new XMethodReference.Simple(method.Name, method.HasThis, method.ReturnType, this, method.Parameters, method.GenericParameters.Select(x => x.Name));
        }
    }
}
