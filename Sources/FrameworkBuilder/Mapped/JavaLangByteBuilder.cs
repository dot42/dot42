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
    internal sealed class JavaLangByteBuilder : JavaBoxedBuilder 
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangByteBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangByteBuilder(ClassFile cf)
            : base(cf, "System.SByte", "java/lang/Byte")
        {
        }

        /// <summary>
        /// Gets the name of the given method
        /// </summary>
        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            if (method.Name == "ParseByte")
                renamer.Rename(method, "Parse");
        }
    }
}
