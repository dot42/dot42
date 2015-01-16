namespace Dot42.DexLib
{
    /// <summary>
    /// Type reference for out or ref parameters.
    /// </summary>
    public class ByReferenceType : ArrayType 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ByReferenceType(TypeReference elementType)
            : base(elementType)
        {
        }
    }
}