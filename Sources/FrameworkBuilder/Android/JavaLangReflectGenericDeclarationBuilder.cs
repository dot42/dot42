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
    internal sealed class JavaLangReflectGenericDeclarationBuilder: AndroidBuilder
    {
        public JavaLangReflectGenericDeclarationBuilder() : this(ClassFile.Empty) { }

        internal JavaLangReflectGenericDeclarationBuilder(ClassFile cf)
            : base(cf, "java/lang/reflect/GenericDeclaration")
        {
        }

        protected override PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new NoPropertyBuilder(typeDef, this);
        }
    }
}
