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
            private readonly string fileName;
            private JarFile resolved;
            private MemoryStream stream;
            private byte[] data;
            
            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaCode(EmbeddedResource resource, string fileName)
            {
                this.resource = resource;
                this.fileName = fileName;
            }

            public ClassSource ClassSource
            {
                get
                {
                    return new ClassSource(FileName, Data, resource.Name);
                }
            }

            /// <summary>
            /// Gets the loaded jar file.
            /// </summary>
            public JarFile Resolve(IClassLoader nextClassLoader)
            {
                return resolved ?? (resolved = new JarFile(new MemoryStream(Data),  FileName, nextClassLoader));
            }

            private byte[] Data { get { return data ?? (data = resource.GetResourceData()); } }

            private string FileName { get { return fileName ?? "javacode-" + resource.Name; } }
        }
    }
}
