using System.ComponentModel.Composition;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Mapped;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Android
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangReflectAnnotatedElementBuilder: AndroidBuilder
    {
        public JavaLangReflectAnnotatedElementBuilder() : this(ClassFile.Empty) { }

        internal JavaLangReflectAnnotatedElementBuilder(ClassFile cf)
            : base(cf, "java/lang/reflect/AnnotatedElement")
        {
        }

        protected override PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new NoPropertyBuilder(typeDef, this);
        }
    }
}
