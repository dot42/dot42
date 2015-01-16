using System.Linq;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using MethodDefinition = Mono.Cecil.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Method builder for methods added to JR classes.
    /// </summary>
    internal class DexImportMethodBuilder : MethodBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public DexImportMethodBuilder(AssemblyCompiler compiler, MethodDefinition method)
            : base(compiler, method)
        {
        }

        /// <summary>
        /// Add the given method to its declaring class.
        /// </summary>
        protected override void AddMethodToDeclaringClass(ClassDefinition declaringClass, DexLib.MethodDefinition dmethod, DexTargetPackage targetPackage)
        {
            var generatedCodeClass = targetPackage.GetOrCreateGeneratedCodeClass();
            UpdateName(dmethod, generatedCodeClass);
            dmethod.Owner = generatedCodeClass;
            generatedCodeClass.Methods.Add(dmethod);
        }

        /// <summary>
        /// Add the given method to its declaring class.
        /// </summary>
        protected override void SetAccessFlags(DexLib.MethodDefinition dmethod, MethodDefinition method)
        {
            dmethod.IsStatic = true;
            dmethod.IsPublic = true;
        }

        /// <summary>
        /// Ensure that the name of the field is unique.
        /// </summary>
        private static void UpdateName(DexLib.MethodDefinition method, ClassDefinition declaringClass)
        {
            var baseName = method.Name;
            if (baseName == "<init>") baseName = "NetCtor";
            var name = baseName;
            var postfix = 0;
            while (true)
            {
                if (declaringClass.Methods.All(x => x.Name != name))
                {
                    method.Name = name;
                    return;
                }
                name = baseName + ++postfix;
            }
        }
    }
}
