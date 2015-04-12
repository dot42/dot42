using System;
using System.Diagnostics;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Entry point for dot42 related Ast conversions.
    /// </summary>
    public static class TargetConverters
    {
        /// <summary>
        /// Perform all dot42 related Ast conversions.
        /// </summary>
        public static void Convert(DecompilerContext context, AstBlock ast, MethodSource currentMethod, AssemblyCompiler compiler)
        {
            if (ast.IsOptimizedForTarget)
                return;            
#if DEBUG
            //Debugger.Launch();
            if ((currentMethod.Method != null) && (currentMethod.Method.Name.Equals("runTest", StringComparison.OrdinalIgnoreCase)))
            {
                //Debugger.Launch();
            }
#endif

            IntPtrConverter.Convert(ast, compiler);
            TypeOfConverter.Convert(ast, compiler);
            BranchOptimizer.Convert(ast);
            CompoundAssignmentConverter.Convert(ast);
            // keep this order
            InterlockedConverter.Convert(ast,compiler);
            ByReferenceParamConverter.Convert(context, ast, compiler);
            // end
            CompareUnorderedConverter.Convert(ast);
            EnumConverter.Convert(ast, compiler);
            EnumOptimizer.Convert(ast, compiler);


            // Keep this order
            NullableConverter.Convert(ast, compiler);
            PrimitiveAddressOfConverter.Convert(ast, currentMethod, compiler);
            StructCallConverter.Convert(ast, compiler);
            // end

            InitializeStructVariablesConverter.Convert(ast);
            DelegateConverter.Convert(ast);
            LdcWideConverter.Convert(ast);
            LdLocWithConversionConverter.Convert(ast);
            ConvertAfterLoadConversionConverter.Convert(ast);
            ConvertBeforeStoreConversionConverter.Convert(ast);
            CleanupConverter.Convert(ast);

            GenericsConverter.Convert(ast, currentMethod);

            // Expand cast expressions
            CastConverter.Convert(ast, currentMethod, compiler);

            // Expand generic instance information
            GenericInstanceConverter.Convert(ast, currentMethod, compiler);

        }
    }
}
