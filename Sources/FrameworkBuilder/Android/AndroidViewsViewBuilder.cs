using System.ComponentModel.Composition;
using System.Linq;
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
    internal sealed class AndroidViewsViewsBuilder : AndroidBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public AndroidViewsViewsBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AndroidViewsViewsBuilder(ClassFile cf)
            : base(cf, "android/view/View")
        {
        }


        public override void ModifyMethodName(NetMethodDefinition method, MethodRenamer renamer)
        {
            if (method.OriginalJavaName == "getLayoutParams")
                renamer.Rename(method, "GetLayoutParameters");
            else if (method.OriginalJavaName == "setLayoutParams")
                renamer.Rename(method, "SetLayoutParameters");
            else
                base.ModifyMethodName(method, renamer);
        }
    }
}
