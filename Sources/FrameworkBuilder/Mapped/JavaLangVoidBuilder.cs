using System.ComponentModel.Composition;
using System.Reflection;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Mapped;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Mapped
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangVoidBuilder : JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangVoidBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangVoidBuilder(ClassFile cf)
            : base(cf, "System.Void", "java/lang/Void")
        {
        }

        /// <summary>
        /// Implement members and setup references now that all types have been created
        /// </summary>
        public override void Implement(TargetFramework target)
        {
            base.Implement(target);
            TypeDefinition.IsStruct = true;
            //TypeDefinition.ClassSize = 1;
            //TypeDefinition.PackingSize = 0;
        }

        /// <summary>
        /// Should the given method be implemented?
        /// </summary>
        protected override bool ShouldImplement(MethodDefinition method, TargetFramework target)
        {
            return base.ShouldImplement(method, target) && (method.Name != "<init>");
        }

        /// <summary>
        /// Create type attributes
        /// </summary>
        protected override TypeAttributes GetAttributes(ClassFile cf, bool hasFields)
        {
            return TypeAttributes.SequentialLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed |
                   TypeAttributes.BeforeFieldInit | TypeAttributes.Public;
        }
    }
}
