using System.ComponentModel.Composition;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Mapped;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Mapped
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangReflectConstructorBuilder: JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangReflectConstructorBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangReflectConstructorBuilder(ClassFile cf)
            : base(cf, "System.Reflection.JavaConstructor", "java/lang/reflect/Constructor")
        {

        }

        /// <summary>
        /// Add generic parameters to my type?
        /// </summary>
        protected override bool AddGenericParameters
        {
            get { return false; }
        }
    }
}
