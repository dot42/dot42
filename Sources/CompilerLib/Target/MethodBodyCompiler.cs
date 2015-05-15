using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Converters;
using Dot42.CompilerLib.Ast.Optimizer;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Target
{
    public enum StopAstConversion
    {
        None,
        AfterILConversion,
        AfterOptimizing,
        AfterIntPtrConverter,
        AfterTypeOfConverter,
        AfterBranchOptimizer,
        AfterCompoundAssignmentConverter,
        AfterInterlockedConverter,
        AfterByReferenceParamConverter,
        AfterCompareUnorderedConverter,
        AfterEnumConverter,
        AfterEnumOptimizer,
        AfterNullableConverter,
        AfterPrimitiveAddressOfConverter,
        AfterStructCallConverter,
        AfterInitializeStructVariablesConverter,
        AfterDelegateConverter,
        AfterLdcWideConverter,
        AfterLdLocWithConversionConverter,
        AfterConvertAfterLoadConversionConverter,
        AfterConvertBeforeStoreConversionConverter,
        AfterCleanupConverter,
        AfterGenericsConverter,
        AfterCastConverter,
        AfterGenericInstanceConverter,
    }

    /// <summary>
    /// Compile IL to Dex code.
    /// </summary>
    internal abstract class MethodBodyCompiler
    {
        /// <summary>
        /// Convert the given method into optimized Ast format.
        /// </summary>
        internal protected static AstNode CreateOptimizedAst(AssemblyCompiler compiler, MethodSource source, bool generateSetNextInstructionCode, StopAstConversion debugStop = StopAstConversion.None)
        {
            // Build AST
            DecompilerContext context;
            AstBlock ast;
            if (source.IsDotNet)
            {
                context = new DecompilerContext(source.Method);
                var astBuilder = new IL2Ast.AstBuilder(source.ILMethod, true, context);
                var children = astBuilder.Build();
                ast = new AstBlock(children.Select(x => x.SourceLocation).FirstOrDefault(), children);
                if ((source.ILMethod.IsConstructor) && (source.Method.DeclaringType.Fields.Any(x => x.FieldType.IsEnum() || x.Name.EndsWith(NameConstants.Atomic.FieldUpdaterPostfix))))
                {
                    // Ensure all fields are initialized
                    AddFieldInitializationCode(compiler, source, ast);
                }
                if (source.Method.NeedsGenericInstanceTypeParameter && (source.Name == ".ctor"))
                {
                    // Add code to safe the generic instance type parameter into the generic instance field.
                    AddGenericInstanceFieldInitializationCode(ast);
                }
            }
            else if (source.IsJava)
            {
                var astBuilder = new Java2Ast.AstBuilder(compiler.Module, source.JavaMethod, source.Method.DeclaringType, true);
                context = new DecompilerContext(source.Method);
                ast = astBuilder.Build();
            }
            else if (source.IsAst)
            {
                context = new DecompilerContext(source.Method);
                ast = source.Ast;
            }
            else
            {
                throw new NotSupportedException("Unknown source");
            }

            if (debugStop == StopAstConversion.AfterILConversion) return ast;

            // Optimize AST
            var astOptimizer = new AstOptimizer(context, ast);
            astOptimizer.Optimize();

            if (debugStop == StopAstConversion.AfterOptimizing) return ast;

            // Optimize AST towards the target
            TargetConverters.Convert(context, ast, source, compiler, debugStop);

            if(generateSetNextInstructionCode)
                SetNextInstructionGenerator.Convert(ast, source, compiler);

            // Return return
            return ast;
        }

        /// <summary>
        /// Add initialization code to the given constructor for non-initialized struct fields.
        /// </summary>
        private static void AddFieldInitializationCode(AssemblyCompiler compiler, MethodSource ctor, AstBlock ast)
        {
            List<XFieldDefinition> fieldsToInitialize;
            var declaringType = ctor.Method.DeclaringType;
            var fieldsThatMayNeedInitialization = declaringType.Fields.Where(x => x.IsReachable && x.FieldType.IsEnum());
            var index = 0;
            if (ctor.Method.IsStatic)
            {
                var atomicUpdaters = declaringType.Fields.Where(x => x.Name.EndsWith(NameConstants.Atomic.FieldUpdaterPostfix));
                foreach (var field in atomicUpdaters)
                {
                    var type = field.FieldType.Resolve();
                    var factory = type.Methods.First(m => m.Name == "NewUpdater" && m.IsStatic 
                                                      && (m.Parameters.Count==2 || m.Parameters.Count==3));
                    var baseFieldName = field.Name.Substring(0, field.Name.Length - NameConstants.Atomic.FieldUpdaterPostfix.Length);

                    var declaringTypeExpr = new AstExpression(ast.SourceLocation, AstCode.TypeOf, field.DeclaringType);
                    var ldBaseFieldNameExpr = new AstExpression(ast.SourceLocation, AstCode.Ldstr, baseFieldName);

                    AstExpression createExpr;
                    if (factory.Parameters.Count == 2)
                    {
                        // doesn't need the original field type.
                        createExpr = new AstExpression(ast.SourceLocation, AstCode.Call, factory,
                                                            declaringTypeExpr, ldBaseFieldNameExpr)
                                                .SetType(factory.ReturnType);
                    }
                    else
                    {
                        // needs the original field type; use the element type for generics.
                        var originalField = declaringType.Fields.Single(f => f.Name==baseFieldName);
                        var originalFieldType = originalField.FieldType;
                        if (!originalFieldType.IsArray)
                            originalFieldType = originalFieldType.GetElementType();

                        var fieldTypeExpr = new AstExpression(ast.SourceLocation, AstCode.TypeOf, originalFieldType);
                        createExpr = new AstExpression(ast.SourceLocation, AstCode.Call, factory,
                                                            declaringTypeExpr, fieldTypeExpr, ldBaseFieldNameExpr)
                                                .SetType(factory.ReturnType);
                    }
                    var initExpr = new AstExpression(ast.SourceLocation, AstCode.Stsfld, field, createExpr);
                    ast.Body.Insert(index++, initExpr);
                }

                fieldsToInitialize = fieldsThatMayNeedInitialization.Where(x => x.IsStatic && !IsInitialized(ast, x)).ToList();
                foreach (var field in fieldsToInitialize)
                {
                    var defaultExpr = new AstExpression(ast.SourceLocation, AstCode.DefaultValue, field.FieldType);
                    var initExpr = new AstExpression(ast.SourceLocation, AstCode.Stsfld, field, defaultExpr);
                    ast.Body.Insert(index++, initExpr);
                }

            }
            else
            {
                // If there is a this ctor being called, we do not have to initialize here.
                var thisCalls = ast.GetSelfAndChildrenRecursive<AstExpression>(x => (x.Code == AstCode.Call) && ((XMethodReference) x.Operand).DeclaringType.IsSame(declaringType));
                if (thisCalls.Any(x => ((XMethodReference) x.Operand).Name == ".ctor"))
                    return;
                fieldsToInitialize = fieldsThatMayNeedInitialization.Where(x => !x.IsStatic && !IsInitialized(ast, x)).ToList();
                if (fieldsToInitialize.Any())
                {
                    var baseCall = FindFirstBaseCtorCall(ast, declaringType);
                    var initExpressions = new List<AstExpression>();
                    foreach (var field in fieldsToInitialize)
                    {
                        var thisExpr = new AstExpression(ast.SourceLocation, AstCode.Ldthis, null);
                        var defaultExpr = new AstExpression(ast.SourceLocation, AstCode.DefaultValue, field.FieldType);
                        initExpressions.Add(new AstExpression(ast.SourceLocation, AstCode.Stfld, field, thisExpr, defaultExpr));
                    }
                    InsertAfter(ast, baseCall, initExpressions);
                }
            }
        }

        /// <summary>
        /// Add generic instance field initialization code.
        /// </summary>
        private static void AddGenericInstanceFieldInitializationCode(AstBlock ast)
        {
            var initExpr = new AstExpression(ast.SourceLocation, AstCode.StGenericInstanceField, null,
                new AstExpression(ast.SourceLocation, AstCode.LdGenericInstanceTypeArgument, null));
            InsertAfter(ast, null, new[] { initExpr });
        }

        /// <summary>
        /// Is the given field initialized in the given code?
        /// </summary>
        private static bool IsInitialized(AstBlock ast, XFieldDefinition field)
        {
            var storeCode = field.IsStatic ? AstCode.Stsfld : AstCode.Stfld;
            var initExpressions = ast.GetSelfAndChildrenRecursive<AstExpression>(x => (x.Code == storeCode) && ((XFieldReference)x.Operand).IsSame(field)).ToList();
            return (initExpressions.Any());
        }

        /// <summary>
        /// Find and return the first call (expression) to the base ctor within the given Ast.
        /// </summary>
        private static AstExpression FindFirstBaseCtorCall(AstBlock ast, XTypeDefinition declaringType)
        {
            var baseCalls = ast.GetSelfAndChildrenRecursive<AstExpression>(x => (x.Code == AstCode.Call) && ((XMethodReference)x.Operand).DeclaringType.IsSame(declaringType.BaseType));
            return baseCalls.FirstOrDefault(x => ((XMethodReference)x.Operand).Name == ".ctor");
        }

        /// <summary>
        /// Insert all expressions after the given marker.
        /// </summary>
        private static void InsertAfter(AstBlock ast, AstExpression marker, IEnumerable<AstExpression> expressions)
        {
            if (marker != null)
            {
                var block = ast.GetSelfAndChildrenRecursive<AstBlock>(b => b.Body.Contains(marker)).First();
                var index = block.Body.IndexOf(marker);
                block.Body.InsertRange(index + 1, expressions);
            }
            else
            {
                ast.Body.InsertRange(0, expressions);
            }
        }
    }
}
