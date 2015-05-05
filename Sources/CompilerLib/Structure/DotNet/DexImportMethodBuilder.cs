using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using Dot42.Utility;
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

            
            if (method.IsAbstract || method.IsVirtual)
            {
                // generate a warning, since these methods will never be called 
                // as virtual. Do not generate a warning if we know the compiler
                // will handle the call.

                if (method.DeclaringType.FullName == "System.Array")
                {
                    // all array methods should be redirected by the compiler.
                    return;
                }
                var intfMethod = method.GetBaseInterfaceMethod();
                if (intfMethod != null && intfMethod.DeclaringType.FullName == "System.IFormattable")
                {
                    // this is handled by the compiler.
                    return;
                }

                DLog.Warning(DContext.CompilerCodeGenerator, "Abstract or virtual .NET method '{0}' in DexImport class '{1}'. Unless specially handled by the compiler, this will never be called virtually.", method.Name, method.DeclaringType.FullName);
            }
        }

        /// <summary>
        /// Ensure that the name of the method is unique.
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
