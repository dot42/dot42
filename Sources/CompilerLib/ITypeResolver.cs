using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib
{
    public interface ITypeResolver<out TTypeRef>
    {
        /// <summary>
        /// Gets a dex type reference from the given IL type reference.
        /// </summary>
        TTypeRef GetTypeReference(XTypeReference type);
    }
}
