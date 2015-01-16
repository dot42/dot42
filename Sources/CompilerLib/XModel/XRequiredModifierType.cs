namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Required type
    /// </summary>
    public sealed class XRequiredModifierType : XTypeSpecification
    {
        private readonly XTypeReference modifierType;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XRequiredModifierType(XTypeReference modifierType, XTypeReference elementType)
            : base(elementType)
        {
            this.modifierType = modifierType;
        }

        /// <summary>
        /// What kind of reference is this?
        /// </summary>
        public override XTypeReferenceKind Kind { get { return XTypeReferenceKind.RequiredModifierType; } }

        /// <summary>
        /// Name of the member (including all)
        /// </summary>
        public override string GetFullName(bool noGenerics)
        {
            return "reqmod " + ElementType.GetFullName(noGenerics); 
        }

        /// <summary>
        /// Gets the underlying modifier type bits.
        /// </summary>
        public XTypeReference ModifierType
        {
            get { return modifierType; }
        }
    }
}
