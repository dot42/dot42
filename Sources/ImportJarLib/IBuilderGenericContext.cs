using Dot42.ImportJarLib.Model;

namespace Dot42.ImportJarLib
{
    public interface IBuilderGenericContext
    {
        /// <summary>
        /// Resolve the given generic parameter into a type reference.
        /// </summary>
        bool TryResolveTypeParameter(string name, TargetFramework target, out NetTypeReference type);

        /// <summary>
        /// Full typename of the context without any generic types.
        /// </summary>
        string FullTypeName { get; }
    }
}
