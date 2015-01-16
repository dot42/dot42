using System.Collections.Generic;
using System.IO;
using Dot42.ImportJarLib.Model;

namespace Dot42.ImportJarLib
{
    public interface ICodeGeneratorContext
    {
        /// <summary>
        /// Add code to the header of a source file.
        /// </summary>
        void CreateSourceFileHeader(TextWriter writer);

        /// <summary>
        /// If true, all methods are generated as extern.
        /// </summary>
        bool GenerateExternalMethods { get; }

        /// <summary>
        /// If true, will output debug releated comments
        /// </summary>
        bool GenerateDebugComments { get; }

        /// <summary>
        /// Gets all roots that can come our of <see cref="GetNamespaceRoot"/>
        /// </summary>
        IEnumerable<string> PossibleNamespaceRoots { get; }

        /// <summary>
        /// Gets the root part of the namespace.
        /// The result of this method is used for the name of the source file in which the type is placed.
        /// </summary>
        string GetNamespaceRoot(NetTypeDefinition type);
    }
}
