namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// pointer to an element type
    /// </summary>
    public sealed class XPointerType : XTypeSpecification
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public XPointerType(XTypeReference elementType)
            : base(elementType)
        {
        }

        /// <summary>
        /// What kind of reference is this?
        /// </summary>
        public override XTypeReferenceKind Kind { get { return XTypeReferenceKind.PointerType; } }

        /// <summary>
        /// Name of the member (including all)
        /// </summary>
        public override string GetFullName(bool noGenerics)
        {
            return "ptr " + ElementType.GetFullName(noGenerics); 
        }
    }
}
