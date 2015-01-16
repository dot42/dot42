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
    internal sealed class JavaLangObjectBuilder : JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangObjectBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangObjectBuilder(ClassFile cf)
            : base(cf, "System.Object", "java/lang/Object")
        {
        }

        /// <summary>
        /// Helps in sorting type builders
        /// </summary>
        public override int Priority { get { return 0; } }

        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            switch (method.Name)
            {
                case "Wait":
                    renamer.Rename(method, "JavaWait");
                    break;
                case "GetClass":
                    renamer.Rename(method, "GetType");
                    break;
            }
        }

        protected override bool ShouldImplement(MethodDefinition method, TargetFramework target)
        {
            if (method.Name == "clone")
                return false;
            return base.ShouldImplement(method, target);
        }
    }
}
