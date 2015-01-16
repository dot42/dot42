namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// ref to an element type
    /// </summary>
    public sealed class XByReferenceType : XTypeSpecification
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public XByReferenceType(XTypeReference elementType)
            : base(elementType)
        {
        }

        /// <summary>
        /// What kind of reference is this?
        /// </summary>
        public override XTypeReferenceKind Kind { get { return XTypeReferenceKind.ByReferenceType; } }

        /// <summary>
        /// Name of the member (including all)
        /// </summary>
        public override string GetFullName(bool noGenerics)
        {
            return "ref " + ElementType.GetFullName(noGenerics); 
        }

        /// <summary>
        /// Is this an byref type?
        /// </summary>
        public override bool IsByReference
        {
            get { return true; }
        }
    }
}
