using System.ComponentModel.Composition;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Mapped;
using Dot42.JvmClassLib;
using MethodDefinition = Dot42.JvmClassLib.MethodDefinition;

namespace Dot42.FrameworkBuilder.Mapped
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangLongBuilder : JavaBoxedBuilder 
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangLongBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangLongBuilder(ClassFile cf)
            : base(cf, "System.Int64", "java/lang/Long")
        {
        }

        /// <summary>
        /// Gets the name of the given method
        /// </summary>
        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            if (method.Name == "ParseLong")
            {
                renamer.Rename(method, "Parse");
            }
        }

        /// <summary>
        /// Should the given method be implemented?
        /// </summary>
        protected override bool ShouldImplement(MethodDefinition method, TargetFramework target)
        {
            if (method.Name == "getLong")
            {
                if (method.Descriptor == "(Ljava/lang/String;Ljava/lang/Long;)Ljava/lang/Long;")
                    return false;
            }
            return base.ShouldImplement(method, target);
        }
    }
}
