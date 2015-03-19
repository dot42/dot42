using System.ComponentModel.Composition;
using Dot42.ImportJarLib.Mapped;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Mapped
{
#if false
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangReflectAccessibleObjectBuilder : JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangReflectAccessibleObjectBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangReflectAccessibleObjectBuilder(ClassFile cf)
            : base(cf, "System.Reflection.JavaAccessibleObject", "java/lang/reflect/AccessibleObject")
        {
        }
    }
#endif
}
