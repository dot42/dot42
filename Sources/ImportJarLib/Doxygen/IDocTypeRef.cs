namespace Dot42.ImportJarLib.Doxygen
{
    public interface IDocTypeRef 
    {
        /// <summary>
        /// Resolve this reference into an XmlClass.
        /// </summary>
        IDocResolvedTypeRef Resolve(DocModel model);
    }
}
