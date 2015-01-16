namespace Dot42.ImportJarLib.Doxygen
{
    public interface IDocTypeNameResolver
    {
        /// <summary>
        /// Resolve java type name into .NET name.
        /// </summary>
        string ResolveTypeName(string className);
    }
}
