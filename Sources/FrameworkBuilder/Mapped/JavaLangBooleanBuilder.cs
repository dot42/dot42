using System.ComponentModel;
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
    internal sealed class JavaLangBooleanBuilder : JavaBoxedBuilder 
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangBooleanBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangBooleanBuilder(ClassFile cf)
            : base(cf, "System.Boolean", "java/lang/Boolean")
        {
        }

        /// <summary>
        /// Gets the name of the given method
        /// </summary>
        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            if (method.Name == "ParseBoolean")
                renamer.Rename(method, "JavaParse");
        }
    }
}
