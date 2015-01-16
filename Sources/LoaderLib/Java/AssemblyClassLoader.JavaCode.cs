using System.IO;
using Dot42.JvmClassLib;
using Mono.Cecil;

namespace Dot42.LoaderLib.Java
{
    /// <summary>
    /// Load jar file from JavaCode attributes included in assemblies.
    /// </summary>
    partial class AssemblyClassLoader
    {
        /// <summary>
        /// Data of a JavaCodeAttribute+Resource
        /// </summary>
        internal sealed class JavaCode
        {
            private readonly EmbeddedResource resource;
            private JarFile resolved;

            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaCode(EmbeddedResource resource)
            {
                this.resource = resource;
            }

            /// <summary>
            /// Gets the loaded jar file.
            /// </summary>
            public JarFile Resolve(IClassLoader nextClassLoader)
            {
                return resolved ?? (resolved = new JarFile(new MemoryStream(resource.GetResourceData()), "javacode-" + resource.Name, nextClassLoader));
            }
        }
    }
}
