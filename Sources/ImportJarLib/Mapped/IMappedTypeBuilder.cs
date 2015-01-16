using Dot42.JvmClassLib;

namespace Dot42.ImportJarLib.Mapped
{
    public interface IMappedTypeBuilder
    {
        /// <summary>
        /// Gets the name of the java class that this builder will map.
        /// </summary>
        string ClassName { get; }

        /// <summary>
        /// Create a new type builder for the given class.
        /// </summary>
        TypeBuilder Create(ClassFile cf);
    }
}
