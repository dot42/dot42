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
    internal sealed class AndroidAppActivityBuilder : AndroidBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public AndroidAppActivityBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AndroidAppActivityBuilder(ClassFile cf)
            : base(cf, "android/app/Activity")
        {
        }

        /// <summary>
        /// Create a property builder for this type builder.
        /// </summary>
        protected override PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new SkipSpecifiedPropertyBuilder (typeDef, this, "setTitle", "getTitle");
        }

        public override void ModifyMethodName(NetMethodDefinition method, MethodRenamer renamer)
        {
            if(method.Name == "GetTitle")
                renamer.Rename(method, "JavaGetTitle");
            else
                base.ModifyMethodName(method, renamer);
        }
    }
}
