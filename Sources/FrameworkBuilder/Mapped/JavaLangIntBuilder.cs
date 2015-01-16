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
    internal sealed class JavaLangIntBuilder : JavaBoxedBuilder 
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangIntBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangIntBuilder(ClassFile cf)
            : base(cf, "System.Int32", "java/lang/Integer")
        {
        }

        /// <summary>
        /// Gets the name of the given method
        /// </summary>
        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            if (method.Name == "ParseInt")
            {
                renamer.Rename(method, "JavaParse");
            }
        }

        /// <summary>
        /// Should the given method be implemented?
        /// </summary>
        protected override bool ShouldImplement(MethodDefinition method, TargetFramework target)
        {
            if (method.Name == "getInteger")
            {
                if (method.Descriptor == "(Ljava/lang/String;Ljava/lang/Integer;)Ljava/lang/Integer;")
                    return false;
            }
            return base.ShouldImplement(method, target);
        }
    }
}
