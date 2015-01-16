using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Generic instance
    /// </summary>
    public sealed class XGenericInstanceMethod : XMethodReference, IXGenericInstance
    {
        private readonly XMethodReference elementMethod;
        private readonly ReadOnlyCollection<XTypeReference> genericArguments;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XGenericInstanceMethod(XMethodReference elementMethod, IEnumerable<XTypeReference> genericArguments)
            : base(elementMethod.DeclaringType)
        {
            this.elementMethod = elementMethod;
            this.genericArguments = genericArguments.ToList().AsReadOnly();
            if (this.genericArguments.Count != GenericParameters.Count)
                throw new ArgumentException("Mismatch in generic parameter/argument count");
        }

        /// <summary>
        /// Name of the member
        /// </summary>
        public override string Name
        {
            get { return elementMethod.Name; }
        }

        /// <summary>
        /// Gets the method without array/generic modifiers
        /// </summary>
        public XMethodReference ElementMethod { get { return elementMethod; } }

        /// <summary>
        /// Gets the non-generic method.
        /// </summary>
        public override XMethodReference GetElementMethod()
        {
            return elementMethod.GetElementMethod();
        }

        /// <summary>
        /// Is this an instance method ref?
        /// </summary>
        public override bool HasThis
        {
            get { return elementMethod.HasThis; }
        }

        /// <summary>
        /// Return type of the method
        /// </summary>
        public override XTypeReference ReturnType
        {
            get { return elementMethod.ReturnType; }
        }

        /// <summary>
        /// Parameters of the method
        /// </summary>
        public override ReadOnlyCollection<XParameter> Parameters
        {
            get { return elementMethod.Parameters; }
        }

        /// <summary>
        /// Gets all generic parameters
        /// </summary>
        public override ReadOnlyCollection<XGenericParameter> GenericParameters
        {
            get { return elementMethod.GenericParameters; }
        }

        /// <summary>
        /// Gets all generic arguments
        /// </summary>
        public ReadOnlyCollection<XTypeReference> GenericArguments { get { return genericArguments; } }

        /// <summary>
        /// Is this a generic instance?
        /// </summary>
        public override bool IsGenericInstance
        {
            get { return true; }
        }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public override bool TryResolve(out XMethodDefinition method)
        {
            return ElementMethod.TryResolve(out method);
        }

        /// <summary>
        /// Is this generic parameter the same as the given other?
        /// </summary>
        public bool IsSame(XGenericInstanceMethod other)
        {
            if (!ElementMethod.IsSame(other.ElementMethod) || (GenericArguments.Count != other.GenericArguments.Count))
                return false;
            return !GenericArguments.Where((t, i) => !t.IsSame(other.GenericArguments[i])).Any();
        }
    }
}
