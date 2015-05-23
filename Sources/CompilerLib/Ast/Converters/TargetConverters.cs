using System;
using System.Diagnostics;
using Dot42.CompilerLib.Target;

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
        public static void Convert(DecompilerContext context, AstBlock ast, MethodSource currentMethod, AssemblyCompiler compiler, StopAstConversion stop = StopAstConversion.None)
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
            if (stop == StopAstConversion.AfterIntPtrConverter) return;
            
            TypeOfConverter.Convert(ast, compiler);
            if (stop == StopAstConversion.AfterTypeOfConverter) return;
            
            // TODO: check if we actually need this optimizer, as we have a more throughoutful
            //       optimizer as the last step.
            BranchOptimizer.Convert(ast);
            if (stop == StopAstConversion.AfterBranchOptimizer) return;

            CompoundAssignmentConverter.Convert(ast);
            if (stop == StopAstConversion.AfterCompoundAssignmentConverter) return;

            // keep this order
            InterlockedConverter.Convert(ast, currentMethod, compiler);
            if (stop == StopAstConversion.AfterInterlockedConverter) return;
            
            ByReferenceParamConverter.Convert(context, ast, compiler);
            if (stop == StopAstConversion.AfterByReferenceParamConverter) return;

            // end
            CompareUnorderedConverter.Convert(ast);
            if (stop == StopAstConversion.AfterCompareUnorderedConverter) return;
            
            EnumConverter.Convert(ast, compiler);
            if (stop == StopAstConversion.AfterEnumConverter) return;
            
            EnumOptimizer.Convert(ast, compiler);
            if (stop == StopAstConversion.AfterEnumOptimizer) return;

            // Keep this order
            NullableConverter.Convert(ast, compiler);
            if (stop == StopAstConversion.AfterNullableConverter) return;
            
            PrimitiveAddressOfConverter.Convert(ast, currentMethod, compiler);
            if (stop == StopAstConversion.AfterPrimitiveAddressOfConverter) return;
            
            StructCallConverter.Convert(ast, compiler);
            if (stop == StopAstConversion.AfterStructCallConverter) return;
            // end

            
            InitializeStructVariablesConverter.Convert(ast);
            if (stop == StopAstConversion.AfterInitializeStructVariablesConverter) return;
            
            DelegateConverter.Convert(ast);
            if (stop == StopAstConversion.AfterDelegateConverter) return;
            
            LdcWideConverter.Convert(ast);
            if (stop == StopAstConversion.AfterLdcWideConverter) return;
            
            LdLocWithConversionConverter.Convert(ast);
            if (stop == StopAstConversion.AfterLdLocWithConversionConverter) return;
            
            ConvertAfterLoadConversionConverter.Convert(ast);
            if (stop == StopAstConversion.AfterConvertAfterLoadConversionConverter) return;
            
            ConvertBeforeStoreConversionConverter.Convert(ast);
            if (stop == StopAstConversion.AfterConvertBeforeStoreConversionConverter) return;

            CleanupConverter.Convert(ast);
            if (stop == StopAstConversion.AfterCleanupConverter) return;

            GenericsConverter.Convert(ast, currentMethod, compiler.Module.TypeSystem);
            if (stop == StopAstConversion.AfterGenericsConverter) return;

            // Expand cast expressions
            CastConverter.Convert(ast, currentMethod, compiler);
            if (stop == StopAstConversion.AfterCastConverter) return;
            
            // Expand generic instance information
            GenericInstanceConverter.Convert(ast, currentMethod, compiler);
            if (stop == StopAstConversion.AfterGenericInstanceConverter) return;
            
            // run the branch optimizer again. (do we need the first invocation?)
            BranchOptimizer2.Convert(ast, compiler);
            if (stop == StopAstConversion.AfterBranchOptimizer2) return;

        }
    }
}
