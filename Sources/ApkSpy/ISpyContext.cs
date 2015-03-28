using Dot42.Mapping;

namespace Dot42.ApkSpy
{
    internal interface ISpyContext : ISpySettings
    {
        /// <summary>
        /// Gets the map file loaded with the current file.
        /// Can return null.
        /// </summary>
        MapFile MapFile { get; }

#if DEBUG || ENABLE_SHOW_AST
        /// <summary>
        /// Gets the XModule
        /// </summary>
        XModule Module { get; }

        /// <summary>
        /// Gets the temp assembly.
        /// </summary>
        AssemblyDefinition Assembly { get; }

        /// <summary>
        /// Gets the classloader
        /// </summary>
        AssemblyClassLoader ClassLoader { get; }
#endif
    }
}
