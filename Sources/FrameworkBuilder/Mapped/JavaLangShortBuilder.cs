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
    internal sealed class JavaLangShortBuilder : JavaBoxedBuilder 
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangShortBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangShortBuilder(ClassFile cf)
            : base(cf, "System.Int16", "java/lang/Short")
        {
        }

        /// <summary>
        /// Gets the name of the given method
        /// </summary>
        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            if (method.Name == "ParseShort")
            {
                renamer.Rename(method, "Parse");
            }
        }
    }
}
