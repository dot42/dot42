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
    internal sealed class JavaLangMathBuilder : JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangMathBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangMathBuilder(ClassFile cf)
            : base(cf, "System.Math", "java/lang/Math")
        {
        }

        /// <summary>
        /// Gets the name of the given method
        /// </summary>
        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            if (method.Name == "Ceil")
            {
                renamer.Rename(method, "Ceiling");
            }
            else if (method.Name == "IEEEremainder")
            {
                renamer.Rename(method, "IEEERemainder");
            }
            else if (method.Name == "Abs")
            {
                renamer.Rename(method, "JavaAbs");
            }
            else if (method.Name == "Round")
            {
                renamer.Rename(method, "JavaRound");
            }
            else if (method.Name == "Pow")
            {
                renamer.Rename(method, "JavaPow");
            }
        }
    }
}
