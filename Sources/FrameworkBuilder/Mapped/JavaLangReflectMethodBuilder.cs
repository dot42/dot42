using System.ComponentModel.Composition;
using Dot42.ImportJarLib.Mapped;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Mapped
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangReflectMethodBuilder: JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangReflectMethodBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangReflectMethodBuilder(ClassFile cf)
            : base(cf, "System.Reflection.JavaMethod", "java/lang/reflect/Method")
        {
        }

        /// <summary>
        /// Should the given interface implementation method be left out?
        /// </summary>
        protected override bool IgnoreInterfaceMethod(ImportJarLib.Model.NetMethodDefinition method, ImportJarLib.Model.NetMethodDefinition interfaceMethod)
        {
            if (method.Name == "GetTypeParameters")
                return true;
            return base.IgnoreInterfaceMethod(method, interfaceMethod);
        }
    }
}
