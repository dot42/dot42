using System;
using System.Linq;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Type reference based on another element type
    /// </summary>
    public abstract class XTypeSpecification : XTypeReference
    {
        private readonly XTypeReference elementType;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected XTypeSpecification(XTypeReference elementType)
            : base(elementType.Module, false, elementType.GenericParameters.Select(x => x.Name))
        {
            if (elementType == null)
                throw new ArgumentNullException("elementType");
            this.elementType = elementType;
        }

        /// <summary>
        /// Name of the member
        /// </summary>
        public override string Name
        {
            get { return elementType.Name; }
        }

        /// <summary>
        /// Gets the deepest <see cref="XTypeReference.ElementType"/>.
        /// </summary>
        public override XTypeReference GetElementType()
        {
            return elementType.GetElementType();
        }

        /// <summary>
        /// Name of the member (including all)
        /// </summary>
        public override string GetFullName(bool noGenerics)
        {
            return elementType.GetFullName(noGenerics); 
        }

        /// <summary>
        /// Gets the type without array/generic modifiers
        /// </summary>
        public override XTypeReference ElementType
        {
            get { return elementType; }
        }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public override bool TryResolve(out XTypeDefinition type)
        {
            type = null;
            return false;
        }
    }
}
