using System.ComponentModel.Composition;
using Dot42.ImportJarLib.Mapped;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Android
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangReflectTypeVariableBuilder: AndroidBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangReflectTypeVariableBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangReflectTypeVariableBuilder(ClassFile cf)
            : base(cf, "java/lang/reflect/TypeVariable")
        {

        }

        /// <summary>
        /// Don't add generic parameters, to help overcome the implemementing methods
        /// in Class, constructor, etc. specifying a more specific array.
        /// </summary>
        protected override bool AddGenericParameters
        {
            get { return false; }
        }
    }
}
