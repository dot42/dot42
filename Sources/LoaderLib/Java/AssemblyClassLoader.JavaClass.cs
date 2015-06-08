using System;
using System.IO;
using Dot42.JvmClassLib;

namespace Dot42.LoaderLib.Java
{
    /// <summary>
    /// Load java classes from JavaClass attributes included in assemblies.
    /// </summary>
    partial class AssemblyClassLoader
    {
        /// <summary>
        /// Data of a JavaClassAttribute
        /// </summary>
        internal sealed class JavaClass
        {
            private readonly string className;
            private readonly JavaCode javaCode;
            private ClassFile resolved;

            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaClass(string className, JavaCode javaCode)
            {
                this.className = className;
                this.javaCode = javaCode;
            }

            /// <summary>
            /// Resolve the payload into a class file.
            /// </summary>
            public ClassFile Resolve(IClassLoader loader, Action<ClassFile> classLoaded)
            {
                if (resolved == null)
                {
                    // Load the class
                    var raw = javaCode.Resolve(loader).GetResource(className + ".class");
                    resolved = new ClassFile(new MemoryStream(raw), loader);
                    if (classLoaded != null)
                    {
                        classLoaded(resolved);
                    }
                }
                return resolved;
            }

            public ClassSource ClassSource { get { return javaCode.ClassSource; } }

        }
    }
}
