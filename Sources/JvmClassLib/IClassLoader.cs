using System.Collections.Generic;

namespace Dot42.JvmClassLib
{
    public interface IClassLoader
    {
        /// <summary>
        /// Load a class with the given name
        /// </summary>
        bool TryLoadClass(string className, out ClassFile result);

        /// <summary>
        /// Gets all package names found in this loader.
        /// </summary>
        IEnumerable<string> Packages { get; }
    }
}
