using System.Diagnostics;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using MethodDefinition = Mono.Cecil.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Method builder for DllImport methods.
    /// </summary>
    internal class DllImportMethodBuilder : MethodBuilder
    {
        private readonly string dllName;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DllImportMethodBuilder(AssemblyCompiler compiler, MethodDefinition method, string dllName)
            : base(compiler, method)
        {
            this.dllName = dllName;
        }

        /// <summary>
        /// Add the given method to its declaring class.
        /// </summary>
        protected override void SetAccessFlags(DexLib.MethodDefinition dmethod, MethodDefinition method)
        {
            base.SetAccessFlags(dmethod, method);
            dmethod.IsNative = true;
        }

        /// <summary>
        /// Generate method code
        /// </summary>
        public override void GenerateCode(DexLib.ClassDefinition declaringClass, DexTargetPackage targetPackage)
        {
            // Do nothing
        }
    }
}
