using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.XModel;
using Dot42.Utility;

namespace Dot42.CompilerLib.Ast.Optimizer
{
    /// <summary>
    /// Assigns C# types to IL expressions.
    /// </summary>
    /// <remarks>
    /// Types are inferred in a bidirectional manner:
    /// The expected type flows from the outside to the inside, the actual inferred type flows from the inside to the outside.
    /// </remarks>
    internal partial class TypeAnalysis
    {
        private readonly DecompilerContext context;
        private readonly XTypeSystem typeSystem;
        private readonly XModule module;
        private readonly List<ExpressionToInfer> allExpressions = new List<ExpressionToInfer>();
        private readonly DefaultDictionary<AstVariable, List<ExpressionToInfer>> assignmentExpressions = new DefaultDictionary<AstVariable, List<ExpressionToInfer>>(_ => new List<ExpressionToInfer>());
        private readonly HashSet<AstVariable> singleLoadVariables = new HashSet<AstVariable>();

        public static void Run(DecompilerContext context, AstBlock method)
        {
            var ta = new TypeAnalysis(context);
            ta.CreateDependencyGraph(method);
            ta.IdentifySingleLoadVariables();
            ta.RunInference();
        }

        private TypeAnalysis(DecompilerContext context)
        {
            this.context = context;
            module = context.CurrentModule;
            typeSystem = module.TypeSystem;
        }

#region CreateDependencyGraph

        /// <summary>
        /// Creates the "ExpressionToInfer" instances (=nodes in dependency graph)
        /// </summary>
        /// <remarks>
        /// We are using a dependency graph to ensure that expressions are analyzed in the correct order.
        /// </remarks>
        private void CreateDependencyGraph(AstNode node)
        {
            var catchBlock = node as AstTryCatchBlock.CatchBlock;
            if (catchBlock != null && catchBlock.ExceptionVariable != null && catchBlock.ExceptionType != null && catchBlock.ExceptionVariable.Type == null)
            {
                catchBlock.ExceptionVariable.Type = catchBlock.ExceptionType;
            }
            var expr = node as AstExpression;
            if (expr != null)
            {
                var expressionToInfer = new ExpressionToInfer(expr);
                allExpressions.Add(expressionToInfer);
                FindNestedAssignments(expr, expressionToInfer);

                if ((expr.Code == AstCode.Stloc) && ((AstVariable) expr.Operand).Type == null)
                {
                    assignmentExpressions[(AstVariable)expr.Operand].Add(expressionToInfer);
                }
                return;
            }
            foreach (var child in node.GetChildren())
            {
                CreateDependencyGraph(child);
            }
        }

        private void FindNestedAssignments(AstExpression expr, ExpressionToInfer parent)
        {
            foreach (var arg in expr.Arguments)
            {
                if (arg.Code == AstCode.Stloc)
                {
                    var expressionToInfer = new ExpressionToInfer(arg);
                    allExpressions.Add(expressionToInfer);
                    FindNestedAssignments(arg, expressionToInfer);
                    var v = (AstVariable)arg.Operand;
                    if (v.Type == null)
                    {
                        assignmentExpressions[v].Add(expressionToInfer);
                        // the instruction that consumes the stloc result is handled as if it was reading the variable
                        parent.Dependencies.Add(v);
                    }
                }
                else
                {
                    AstVariable v;
                    if (arg.Match(AstCode.Ldloc, out v) && (v.Type == null))
                    {
                        parent.Dependencies.Add(v);
                    }
                    FindNestedAssignments(arg, parent);
                }
            }
        }

#endregion // CreateDependencyGraph

        /// <summary>
        /// Find all variables that are assigned to exactly a single time:
        /// </summary>
        private void IdentifySingleLoadVariables()
        {
            var q = (from expr in allExpressions
                    from v in expr.Dependencies
                    group expr by v).ToArray();
            foreach (var g in q)
            {
                var v = g.Key;
                if ((g.Count() == 1) && g.Single().Expression.GetSelfAndChildrenRecursive<AstExpression>().Count(e => e.Operand == v) == 1)
                {
                    singleLoadVariables.Add(v);
                    // Mark the assignments as dependent on the type from the single load:
                    foreach (var assignment in assignmentExpressions[v])
                    {
                        assignment.DependsOnSingleLoad = v;
                    }
                }
            }
        }

        /// <summary>
        /// Run inference for all expressions.
        /// </summary>
        private void RunInference()
        {
            // Two flags that allow resolving cycles:
            var ignoreSingleLoadDependencies = false;
            var assignVariableTypesBasedOnPartialInformation = false;
            while (allExpressions.Any(x => !x.Done))
            {
                var inferredOneOrMoreExpressions = false;
                foreach (var expr in allExpressions.Where(x => !x.Done))
                {
                    if (expr.Dependencies.TrueForAll(v => v.Type != null) )
                        //((expr.DependsOnSingleLoad == null) || (expr.DependsOnSingleLoad.Type != null) || ignoreSingleLoadDependencies))
                    //if (expr.Dependencies.TrueForAll(v => v.Type != null || singleLoadVariables.Contains(v)) &&
                    //    ((expr.DependsOnSingleLoad == null) || (expr.DependsOnSingleLoad.Type != null) || ignoreSingleLoadDependencies))
                    {
                        RunInference(expr.Expression);
                        expr.Done = true;
                        inferredOneOrMoreExpressions = true;
                    }
                }

                // Did we infer the type or one or more expressions?
                if (!inferredOneOrMoreExpressions)
                {
                    if (ignoreSingleLoadDependencies)
                    {
                        if (assignVariableTypesBasedOnPartialInformation)
                            throw new InvalidOperationException("Could not infer any expression");
                        assignVariableTypesBasedOnPartialInformation = true;
                    }
                    else
                    {
                        // We have a cyclic dependency; we'll try if we can resolve it by ignoring single-load dependencies.
                        // This can happen if the variable was not actually assigned an expected type by the single-load instruction.
                        ignoreSingleLoadDependencies = true;
                        continue;
                    }
                }
                else
                {
                    assignVariableTypesBasedOnPartialInformation = false;
                    ignoreSingleLoadDependencies = false;
                }

                // Now infer types for variables:
                foreach (var pair in assignmentExpressions.Where(x => x.Key.Type == null))
                {
                    var v = pair.Key;
                    if (assignVariableTypesBasedOnPartialInformation
                            ? pair.Value.Any(e => e.Done)
                            : pair.Value.All(e => e.Done))
                    {
                        XTypeReference inferredType = null;
                        foreach (var expr in pair.Value)
                        {
                            Debug.Assert(expr.Expression.Code == AstCode.Stloc);
                            var assignedValue = expr.Expression.Arguments.Single();
                            if (assignedValue.InferredType != null)
                            {
                                if (inferredType == null)
                                {
                                    inferredType = assignedValue.InferredType;
                                }
                                else
                                {
                                    // pick the common base type
                                    inferredType = TypeWithMoreInformation(inferredType, assignedValue.InferredType);
                                }
                            }
                        }
                        if (inferredType == null)
                        {
                            continue;
                            //inferredType = typeSystem.Object;
                        }
                        v.Type = inferredType;
                        // Assign inferred type to all the assignments (in case they used different inferred types):
                        foreach (ExpressionToInfer expr in pair.Value)
                        {
                            expr.Expression.InferredType = inferredType;
                            // re-infer if the expected type has changed
                            InferTypeForExpression(expr.Expression.Arguments.Single(), inferredType);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Infer the type of the given expression and it's arguments if they are no Stloc.
        /// </summary>
        private void RunInference(AstExpression expr)
        {
            var anyArgumentIsMissingExpectedType = expr.Arguments.Any(a => a.ExpectedType == null);
            if ((expr.InferredType == null) || anyArgumentIsMissingExpectedType)
            {
                InferTypeForExpression(expr, expr.ExpectedType, anyArgumentIsMissingExpectedType);
            }
            foreach (var arg in expr.Arguments.Where(arg => arg.Code != AstCode.Stloc))
            {
                RunInference(arg);
            }
        }

        /// <summary>
        /// Infers the C# type of <paramref name="expr"/>.
        /// </summary>
        /// <param name="expr">The expression</param>
        /// <param name="expectedType">The expected type of the expression</param>
        /// <param name="forceInferChildren">Whether direct children should be inferred even if its not necessary. (does not apply to nested children!)</param>
        /// <returns>The inferred type</returns>
        private XTypeReference InferTypeForExpression(AstExpression expr, XTypeReference expectedType, bool forceInferChildren = false)
        {
            if ((expectedType != null) && !IsSameType(expr.ExpectedType, expectedType))
            {
                expr.ExpectedType = expectedType;
                if (expr.Code != AstCode.Stloc) // stloc is special case and never gets re-evaluated
                {
                    forceInferChildren = true;
                }
            }
            if (forceInferChildren || (expr.InferredType == null))
            {
                expr.InferredType = DoInferTypeForExpression(expr, expectedType, forceInferChildren);
            }
            return expr.InferredType;
        }

        XTypeReference DoInferTypeForExpression(AstExpression expr, XTypeReference expectedType, bool forceInferChildren = false)
        {
            switch (expr.Code)
            {
                #region Logical operators
                case AstCode.NullCoalescing:
                    return InferBinaryArguments(expr.Arguments[0], expr.Arguments[1], expectedType, forceInferChildren);
                #endregion
                #region Variable load/store
                case AstCode.Stloc:
                    {
                        AstVariable v = (AstVariable)expr.Operand;
                        if (forceInferChildren)
                        {
                            // do not use 'expectedType' in here!
                            InferTypeForExpression(expr.Arguments.Single(), v.Type);
                        }
                        return v.Type;
                    }
                case AstCode.Ldloc:
                    {
                        AstVariable v = (AstVariable)expr.Operand;
                        if (v.Type == null && singleLoadVariables.Contains(v))
                        {
                            v.Type = expectedType;
                        }
                        return v.Type;
                    }
                case AstCode.Ldloca:
                    return new XByReferenceType(((AstVariable)expr.Operand).Type);
                #endregion
                #region Call / NewObj
                case AstCode.Call:
                case AstCode.Callvirt:
                case AstCode.CallIntf:
                case AstCode.CallSpecial:
                    {
                        var method = (XMethodReference)expr.Operand;
                        if (forceInferChildren)
                        {
                            for (int i = 0; i < expr.Arguments.Count; i++)
                            {
                                if (i == 0 && method.HasThis)
                                {
                                    InferTypeForExpression(expr.Arguments[0], MakeRefIfValueType(method.DeclaringType, expr.GetPrefix(AstCode.Constrained)));
                                }
                                else
                                {
                                    InferTypeForExpression(expr.Arguments[i], SubstituteTypeArgs(method.Parameters[method.HasThis ? i - 1 : i].ParameterType, method));
                                }
                            }
                        }
                        return SubstituteTypeArgs(method.ReturnType, method);
                    }
                case AstCode.Newobj:
                    {
                        var ctor = (XMethodReference)expr.Operand;
                        if (forceInferChildren)
                        {
                            for (int i = 0; i < ctor.Parameters.Count; i++)
                            {
                                InferTypeForExpression(expr.Arguments[i], SubstituteTypeArgs(ctor.Parameters[i].ParameterType, ctor));
                            }
                        }
                        return ctor.DeclaringType;
                    }
                case AstCode.New:
                    return (XTypeReference)expr.Operand;
                #endregion
                #region Load/Store Fields
                case AstCode.Ldfld:
                    if (forceInferChildren)
                    {
                        InferTypeForExpression(expr.Arguments[0], MakeRefIfValueType(((XFieldReference)expr.Operand).DeclaringType, expr.GetPrefix(AstCode.Constrained)));
                    }
                    return GetFieldType((XFieldReference)expr.Operand);
                case AstCode.Ldsfld:
                    return GetFieldType((XFieldReference)expr.Operand);
                case AstCode.Ldflda:
                    if (forceInferChildren)
                    {
                        InferTypeForExpression(expr.Arguments[0], MakeRefIfValueType(((XFieldReference)expr.Operand).DeclaringType, expr.GetPrefix(AstCode.Constrained)));
                    }
                    return new XByReferenceType(GetFieldType((XFieldReference)expr.Operand));
                case AstCode.Ldsflda:
                    return new XByReferenceType(GetFieldType((XFieldReference)expr.Operand));
                case AstCode.Stfld:
                    if (forceInferChildren)
                    {
                        InferTypeForExpression(expr.Arguments[0], MakeRefIfValueType(((XFieldReference)expr.Operand).DeclaringType, expr.GetPrefix(AstCode.Constrained)));
                        InferTypeForExpression(expr.Arguments[1], GetFieldType((XFieldReference)expr.Operand));
                    }
                    return GetFieldType((XFieldReference)expr.Operand);
                case AstCode.Stsfld:
                    if (forceInferChildren)
                        InferTypeForExpression(expr.Arguments[0], GetFieldType((XFieldReference)expr.Operand));
                    return GetFieldType((XFieldReference)expr.Operand);
                #endregion
                #region Reference/Pointer instructions
                case AstCode.Ldind_Ref:
                    return UnpackPointer(InferTypeForExpression(expr.Arguments[0], null));
                case AstCode.Stind_Ref:
                    if (forceInferChildren)
                    {
                        var elementType = UnpackPointer(InferTypeForExpression(expr.Arguments[0], null));
                        InferTypeForExpression(expr.Arguments[1], elementType);
                    }
                    return null;
                case AstCode.Ldobj:
                    {
                        var type = (XTypeReference)expr.Operand;
                        if (expectedType != null)
                        {
                            int infoAmount = GetInformationAmount(expectedType);
                            if (infoAmount == 1 && GetInformationAmount(type) == 8)
                            {
                                // A bool can be loaded from both bytes and sbytes.
                                type = expectedType;
                            }
                            if (infoAmount >= 8 && infoAmount <= 64 && infoAmount == GetInformationAmount(type))
                            {
                                // An integer can be loaded as another integer of the same size.
                                // For integers smaller than 32 bit, the signs must match (as loading performs sign extension)
                                if (infoAmount >= 32 || IsSigned(expectedType) == IsSigned(type))
                                    type = expectedType;
                            }
                        }
                        if (forceInferChildren)
                        {
                            if (InferTypeForExpression(expr.Arguments[0], new XByReferenceType(type)) is XPointerType)
                                InferTypeForExpression(expr.Arguments[0], new XPointerType(type));
                        }
                        return type;
                    }
                case AstCode.Stobj:
                    {
                        var operandType = (XTypeReference)expr.Operand;
                        var pointerType = InferTypeForExpression(expr.Arguments[0], new XByReferenceType(operandType));
                        XTypeReference elementType;
                        if (pointerType is XPointerType)
                            elementType = ((XPointerType)pointerType).ElementType;
                        else if (pointerType is XByReferenceType)
                            elementType = ((XByReferenceType)pointerType).ElementType;
                        else
                            elementType = null;
                        if (elementType != null)
                        {
                            // An integer can be stored in any other integer of the same size.
                            int infoAmount = GetInformationAmount(elementType);
                            if (infoAmount == 1 && GetInformationAmount(operandType) == 8)
                                operandType = elementType;
                            else if (infoAmount == GetInformationAmount(operandType) && IsSigned(elementType) != null && IsSigned(operandType) != null)
                                operandType = elementType;
                        }
                        if (forceInferChildren)
                        {
                            if (pointerType is XPointerType)
                                InferTypeForExpression(expr.Arguments[0], new XPointerType(operandType));
                            else if (!IsSameType(operandType, expr.Operand as XTypeReference))
                                InferTypeForExpression(expr.Arguments[0], new XByReferenceType(operandType));
                            InferTypeForExpression(expr.Arguments[1], operandType);
                        }
                        return operandType;
                    }
                case AstCode.Initobj:
                    return null;
                case AstCode.DefaultValue:
                    return (XTypeReference)expr.Operand;
                case AstCode.Localloc:
                    if (forceInferChildren)
                    {
                        InferTypeForExpression(expr.Arguments[0], typeSystem.Int);
                    }
                    if (expectedType is XPointerType)
                        return expectedType;
                    else
                        return typeSystem.IntPtr;
                case AstCode.Sizeof:
                    return typeSystem.Int;
                case AstCode.PostIncrement:
                case AstCode.PostIncrement_Ovf:
                case AstCode.PostIncrement_Ovf_Un:
                    {
                        var elementType = UnpackPointer(InferTypeForExpression(expr.Arguments[0], null));
                        if (forceInferChildren && elementType != null)
                        {
                            // Assign expected type to the child expression
                            InferTypeForExpression(expr.Arguments[0], new XByReferenceType(elementType));
                        }
                        return elementType;
                    }
                case AstCode.Mkrefany:
                    if (forceInferChildren)
                    {
                        InferTypeForExpression(expr.Arguments[0], (XTypeReference)expr.Operand);
                    }
                    return typeSystem.TypedReference;
                case AstCode.Refanytype:
                    if (forceInferChildren)
                    {
                        InferTypeForExpression(expr.Arguments[0], typeSystem.TypedReference);
                    }
                    return new XTypeReference.SimpleXTypeReference(module, "System", "RuntimeTypeHandle", null, true, null);
                case AstCode.Refanyval:
                    if (forceInferChildren)
                    {
                        InferTypeForExpression(expr.Arguments[0], typeSystem.TypedReference);
                    }
                    return new XByReferenceType((XTypeReference)expr.Operand);
                case AstCode.AddressOf:
                    {
                        var t = InferTypeForExpression(expr.Arguments[0], UnpackPointer(expectedType));
                        return t != null ? new XByReferenceType(t) : null;
                    }
                case AstCode.ValueOf:
                    return GetNullableTypeArgument(InferTypeForExpression(expr.Arguments[0], CreateNullableType(expectedType)));
                case AstCode.NullableOf:
                    return CreateNullableType(InferTypeForExpression(expr.Arguments[0], GetNullableTypeArgument(expectedType)));
                case AstCode.Ldthis:
                    return context.DeclaringType;
                #endregion
                #region Arithmetic instructions
                case AstCode.Not: // bitwise complement
                case AstCode.Neg:
                    return InferTypeForExpression(expr.Arguments.Single(), expectedType);
                case AstCode.Add:
                    return InferArgumentsInAddition(expr, null, expectedType);
                case AstCode.Sub:
                    return InferArgumentsInSubtraction(expr, null, expectedType);
                case AstCode.Mul:
                case AstCode.Or:
                case AstCode.And:
                case AstCode.Xor:
                    return InferArgumentsInBinaryOperator(expr, null, expectedType);
                case AstCode.Add_Ovf:
                    return InferArgumentsInAddition(expr, true, expectedType);
                case AstCode.Sub_Ovf:
                    return InferArgumentsInSubtraction(expr, true, expectedType);
                case AstCode.Mul_Ovf:
                case AstCode.Div:
                case AstCode.Rem:
                    return InferArgumentsInBinaryOperator(expr, true, expectedType);
                case AstCode.Add_Ovf_Un:
                    return InferArgumentsInAddition(expr, false, expectedType);
                case AstCode.Sub_Ovf_Un:
                    return InferArgumentsInSubtraction(expr, false, expectedType);
                case AstCode.Mul_Ovf_Un:
                case AstCode.Div_Un:
                case AstCode.Rem_Un:
                    return InferArgumentsInBinaryOperator(expr, false, expectedType);
                case AstCode.Shl:
                    if (forceInferChildren)
                        InferTypeForExpression(expr.Arguments[1], typeSystem.Int);
                    if (expectedType != null && (
                        expectedType.Kind == XTypeReferenceKind.Int || expectedType.Kind == XTypeReferenceKind.UInt ||
                        expectedType.Kind == XTypeReferenceKind.Long || expectedType.Kind == XTypeReferenceKind.ULong)
                       )
                        return NumericPromotion(InferTypeForExpression(expr.Arguments[0], expectedType));
                    else
                        return NumericPromotion(InferTypeForExpression(expr.Arguments[0], null));
                case AstCode.Shr:
                case AstCode.Shr_Un:
                    {
                        if (forceInferChildren)
                            InferTypeForExpression(expr.Arguments[1], typeSystem.Int);
                        var type = NumericPromotion(InferTypeForExpression(expr.Arguments[0], null));
                        XTypeReference expectedInputType = null;
                        if (type != null)
                        {
                            switch (type.Kind)
                            {
                                case XTypeReferenceKind.Int:
                                    if (expr.Code == AstCode.Shr_Un)
                                        expectedInputType = typeSystem.UInt;
                                    break;
                                case XTypeReferenceKind.UInt:
                                    if (expr.Code == AstCode.Shr)
                                        expectedInputType = typeSystem.Int;
                                    break;
                                case XTypeReferenceKind.Long:
                                    if (expr.Code == AstCode.Shr_Un)
                                        expectedInputType = typeSystem.ULong;
                                    break;
                                case XTypeReferenceKind.ULong:
                                    if (expr.Code == AstCode.Shr)
                                        expectedInputType = typeSystem.ULong;
                                    break;
                            }
                        }
                        if (expectedInputType != null)
                        {
                            InferTypeForExpression(expr.Arguments[0], expectedInputType);
                            return expectedInputType;
                        }
                        else
                        {
                            return type;
                        }
                    }
                case AstCode.CompoundAssignment:
                    {
                        var op = expr.Arguments[0];
                        if (op.Code == AstCode.NullableOf) op = op.Arguments[0].Arguments[0];
                        var varType = InferTypeForExpression(op.Arguments[0], null);
                        if (forceInferChildren)
                        {
                            InferTypeForExpression(expr.Arguments[0], varType);
                        }
                        return varType;
                    }
                #endregion
                #region Constant loading instructions
                case AstCode.Ldnull:
                    return typeSystem.Object;
                case AstCode.Ldstr:
                    return typeSystem.String;
                case AstCode.Ldftn:
                case AstCode.Ldvirtftn:
                    return typeSystem.IntPtr;
                case AstCode.Ldc_I4:
                    if (IsBoolean(expectedType) && ((int)expr.Operand == 0 || (int)expr.Operand == 1))
                        return typeSystem.Bool;
                    if (expectedType is XPointerType && (int)expr.Operand == 0)
                        return expectedType;
                    if (IsIntegerOrEnum(expectedType) && OperandFitsInType(expectedType, (int)expr.Operand))
                        return expectedType;
                    else
                        return typeSystem.Int;
                case AstCode.Ldc_I8:
                    if (expectedType is XPointerType && (long)expr.Operand == 0)
                        return expectedType;
                    if (IsIntegerOrEnum(expectedType) && GetInformationAmount(expectedType) >= NativeInt)
                        return expectedType;
                    else
                        return typeSystem.Long;
                case AstCode.Ldc_R4:
                    return typeSystem.Float;
                case AstCode.Ldc_R8:
                    return typeSystem.Double;
                case AstCode.Ldc_Decimal:
                    return new XTypeReference.SimpleXTypeReference(module, "System", "Decimal", null, true, null);
                case AstCode.Ldtoken:
                    if (expr.Operand is XTypeReference)
                        return new XTypeReference.SimpleXTypeReference(module, "System", "RuntimeTypeHandle", null, true, null);
                    else if (expr.Operand is XFieldReference)
                        return new XTypeReference.SimpleXTypeReference(module, "System", "RuntimeFieldHandle", null, true, null);
                    else
                        return new XTypeReference.SimpleXTypeReference(module, "System", "RuntimeMethodHandle", null, true, null);
                case AstCode.Arglist:
                    return new XTypeReference.SimpleXTypeReference(module, "System", "RuntimeArgumentHandle", null, true, null);
                case AstCode.LdClass:
                case AstCode.TypeOf:
                case AstCode.BoxedTypeOf:
                    return typeSystem.Type;
                #endregion
                #region Array instructions
                case AstCode.Newarr:
                    if (forceInferChildren)
                        InferTypeForExpression(expr.Arguments.Single(), typeSystem.Int);
                    return new XArrayType((XTypeReference)expr.Operand);
                case AstCode.InitEnumArray:
                    {
                        var arrayType = InferTypeForExpression(expr.Arguments[0], null) as XArrayType;
                        return arrayType;
                    }
                case AstCode.InitStructArray:
                    {
                        var arrayType = InferTypeForExpression(expr.Arguments[0], null) as XArrayType;
                        return arrayType;
                    }
                case AstCode.MultiNewarr:
                    if (forceInferChildren)
                    {
                        foreach (var arg in expr.Arguments)
                        {
                            InferTypeForExpression(arg, typeSystem.Int);
                        }
                    }
                    return (XTypeReference)expr.Operand;
                case AstCode.InitArray:
                    var initArrayData = (InitArrayData)expr.Operand;
                    return initArrayData.ArrayType;
                case AstCode.Ldlen:
                    return typeSystem.Int;
                case AstCode.Ldelem_U1:
                case AstCode.Ldelem_U2:
                case AstCode.Ldelem_U4:
                case AstCode.Ldelem_I1:
                case AstCode.Ldelem_I2:
                case AstCode.Ldelem_I4:
                case AstCode.Ldelem_I8:
                case AstCode.Ldelem_R4:
                case AstCode.Ldelem_R8:
                case AstCode.Ldelem_I:
                case AstCode.Ldelem_Ref:
                    {
                        var arrayType = InferTypeForExpression(expr.Arguments[0], null) as XArrayType;
                        if (forceInferChildren)
                        {
                            InferTypeForExpression(expr.Arguments[1], typeSystem.Int);
                        }
                        return arrayType != null ? arrayType.ElementType : null;
                    }
                case AstCode.Ldelem_Any:
                    if (forceInferChildren)
                    {
                        InferTypeForExpression(expr.Arguments[1], typeSystem.Int);
                    }
                    return (XTypeReference)expr.Operand;
                case AstCode.Ldelema:
                    {
                        var arrayType = InferTypeForExpression(expr.Arguments[0], null) as XArrayType;
                        if (forceInferChildren)
                            InferTypeForExpression(expr.Arguments[1], typeSystem.Int);
                        return arrayType != null ? new XByReferenceType(arrayType.ElementType) : null;
                    }
                case AstCode.Stelem_I:
                case AstCode.Stelem_I1:
                case AstCode.Stelem_I2:
                case AstCode.Stelem_I4:
                case AstCode.Stelem_I8:
                case AstCode.Stelem_R4:
                case AstCode.Stelem_R8:
                case AstCode.Stelem_Ref:
                case AstCode.Stelem_Any:
                    {
                        var arrayType = InferTypeForExpression(expr.Arguments[0], null) as XArrayType;
                        if (forceInferChildren)
                        {
                            InferTypeForExpression(expr.Arguments[1], typeSystem.Int);
                            if (arrayType != null)
                            {
                                InferTypeForExpression(expr.Arguments[2], arrayType.ElementType);
                            }
                        }
                        return arrayType != null ? arrayType.ElementType : null;
                    }
                #endregion
                #region Conversion instructions
                case AstCode.Conv_I1:
                case AstCode.Conv_Ovf_I1:
                case AstCode.Conv_Ovf_I1_Un:
                    return HandleConversion(8, true, expr.Arguments[0], expectedType, typeSystem.SByte);
                case AstCode.Conv_I2:
                case AstCode.Conv_Ovf_I2:
                case AstCode.Conv_Ovf_I2_Un:
                    return HandleConversion(16, true, expr.Arguments[0], expectedType, typeSystem.Short);
                case AstCode.Conv_I4:
                case AstCode.Conv_Ovf_I4:
                case AstCode.Conv_Ovf_I4_Un:
                    return HandleConversion(32, true, expr.Arguments[0], expectedType, typeSystem.Int);
                case AstCode.Conv_I8:
                case AstCode.Conv_Ovf_I8:
                case AstCode.Conv_Ovf_I8_Un:
                    return HandleConversion(64, true, expr.Arguments[0], expectedType, typeSystem.Long);
                case AstCode.Conv_U1:
                case AstCode.Conv_Ovf_U1:
                case AstCode.Conv_Ovf_U1_Un:
                    return HandleConversion(8, false, expr.Arguments[0], expectedType, typeSystem.Byte);
                case AstCode.Conv_U2:
                case AstCode.Conv_Ovf_U2:
                case AstCode.Conv_Ovf_U2_Un:
                    return HandleConversion(16, false, expr.Arguments[0], expectedType, typeSystem.UShort);
                case AstCode.Conv_U4:
                case AstCode.Conv_Ovf_U4:
                case AstCode.Conv_Ovf_U4_Un:
                    return HandleConversion(32, false, expr.Arguments[0], expectedType, typeSystem.UInt);
                case AstCode.Conv_U8:
                case AstCode.Conv_Ovf_U8:
                case AstCode.Conv_Ovf_U8_Un:
                    return HandleConversion(64, false, expr.Arguments[0], expectedType, typeSystem.ULong);
                case AstCode.Conv_I:
                case AstCode.Conv_Ovf_I:
                case AstCode.Conv_Ovf_I_Un:
                    return HandleConversion(NativeInt, true, expr.Arguments[0], expectedType, typeSystem.IntPtr);
                case AstCode.Conv_U:
                case AstCode.Conv_Ovf_U:
                case AstCode.Conv_Ovf_U_Un:
                    return HandleConversion(NativeInt, false, expr.Arguments[0], expectedType, typeSystem.UIntPtr);
                case AstCode.Int_to_ubyte:
                    return HandleConversion(8, false, expr.Arguments[0], expectedType, typeSystem.Byte);
                case AstCode.Int_to_ushort:
                    return HandleConversion(16, false, expr.Arguments[0], expectedType, typeSystem.UShort);
                case AstCode.Conv_R4:
                    if (forceInferChildren)
                    {
                        InferTypeForExpression(expr.Arguments[0], typeSystem.Float);
                    }
                    return typeSystem.Float;
                case AstCode.Conv_R8:
                    if (forceInferChildren)
                    {
                        InferTypeForExpression(expr.Arguments[0], typeSystem.Double);
                    }
                    return typeSystem.Double;
                case AstCode.Conv_R_Un:
                    return (expectedType != null && expectedType.Kind == XTypeReferenceKind.Float) ? typeSystem.Float : typeSystem.Double;
                case AstCode.Castclass:
                case AstCode.Unbox_Any:
                    return (XTypeReference)expr.Operand;
                case AstCode.Unbox:
                    return new XByReferenceType((XTypeReference)expr.Operand);
                case AstCode.Isinst:
                    {
                        // isinst performs the equivalent of a cast only for reference types;
                        // value types still need to be unboxed after an isinst instruction
                        var tr = (XTypeReference)expr.Operand;
                        return tr.IsValueType ? typeSystem.Object : tr;
                    }
                case AstCode.InstanceOf:
                    {
                        return typeSystem.Bool;
                    }
                case AstCode.Box:
                    {
                        var tr = (XTypeReference)expr.Operand;
                        if (forceInferChildren)
                            InferTypeForExpression(expr.Arguments.Single(), tr);
                        return tr.IsValueType ? typeSystem.Object : tr;
                    }
                #endregion
                #region Comparison instructions
                case AstCode.Ceq:
                case AstCode.Cne:
                    if (forceInferChildren)
                        InferArgumentsInBinaryOperator(expr, null, null);
                    return typeSystem.Bool;
                case AstCode.Clt:
                case AstCode.Cgt:
                case AstCode.Cle:
                case AstCode.Cge:
                    if (forceInferChildren)
                        InferArgumentsInBinaryOperator(expr, true, null);
                    return typeSystem.Bool;
                case AstCode.Clt_Un:
                case AstCode.Cgt_Un:
                case AstCode.Cle_Un:
                case AstCode.Cge_Un:
                    if (forceInferChildren)
                        InferArgumentsInBinaryOperator(expr, false, null);
                    return typeSystem.Bool;
                #endregion
                #region Branch instructions
                case AstCode.Brtrue:
                case AstCode.Brfalse:
                    if (forceInferChildren)
                        InferTypeForExpression(expr.Arguments.Single(), typeSystem.Bool);
                    return null;
                case AstCode.BrIfEq:
                case AstCode.BrIfNe:
                case AstCode.BrIfGe:
                case AstCode.BrIfGt:
                case AstCode.BrIfLe:
                case AstCode.BrIfLt:
                    if (forceInferChildren)
                        InferTypeForExpression(expr.Arguments.Single(), typeSystem.Int);
                    return null;
                case AstCode.__Beq:
                case AstCode.__Bne_Un:
                    if (forceInferChildren)
                        InferArgumentsInBinaryOperator(expr, null, null);
                    return null;
                case AstCode.Br:
                case AstCode.Leave:
                case AstCode.Endfinally:
                case AstCode.Switch:
                case AstCode.LookupSwitch:
                case AstCode.Throw:
                case AstCode.Rethrow:
                case AstCode.LoopOrSwitchBreak:
                    return null;
                case AstCode.Ret:
                    if (forceInferChildren && expr.Arguments.Count == 1)
                        InferTypeForExpression(expr.Arguments[0], context.ReturnType);
                    return null;
                #endregion
                case AstCode.Pop:
                    return null;
                case AstCode.Wrap:
                case AstCode.Dup:
                    {
                        var arg = expr.Arguments.Single();
                        return arg.ExpectedType = InferTypeForExpression(arg, expectedType);
                    }
                case AstCode.LdGenericInstanceField:
                case AstCode.LdGenericInstanceTypeArgument:
                case AstCode.LdGenericInstanceMethodArgument:
                case AstCode.StGenericInstanceField:
                    return expr.ExpectedType;
                //    return new XArrayType(new XTypeReference.SimpleXTypeReference(module, "System", "Type", null, false, null));
                default:
                    Debug.WriteLine("Type Inference: Can't handle expression " + expr.Code.GetName());
                    return null;
            }
        }

        /// <summary>
        /// Wraps 'type' in a ByReferenceType if it is a value type. If a constrained prefix is specified,
        /// returns the constrained type wrapped in a ByReferenceType.
        /// </summary>
        XTypeReference MakeRefIfValueType(XTypeReference type, AstExpressionPrefix constrainedPrefix)
        {
            if (constrainedPrefix != null)
                return new XByReferenceType((XTypeReference)constrainedPrefix.Operand);
            if (type.IsValueType)
                return new XByReferenceType(type);
            else
                return type;
        }

        /// <summary>
        /// Promotes primitive types smaller than int32 to int32.
        /// </summary>
        /// <remarks>
        /// Always promotes to signed int32.
        /// </remarks>
        XTypeReference NumericPromotion(XTypeReference type)
        {
            if (type == null)
                return null;
            switch (type.Kind)
            {
                case XTypeReferenceKind.SByte:
                case XTypeReferenceKind.Short:
                case XTypeReferenceKind.Byte:
                case XTypeReferenceKind.UShort:
                    return typeSystem.Int;
                default:
                    return type;
            }
        }

        XTypeReference HandleConversion(int targetBitSize, bool targetSigned, AstExpression arg, XTypeReference expectedType, XTypeReference targetType)
        {
            if (targetBitSize >= NativeInt && expectedType is XPointerType)
            {
                InferTypeForExpression(arg, expectedType);
                return expectedType;
            }
            var argType = InferTypeForExpression(arg, null);
            if (targetBitSize >= NativeInt && argType is XByReferenceType)
            {
                // conv instructions on managed references mean that the GC should stop tracking them, so they become pointers:
                var ptrType = new XPointerType(((XByReferenceType)argType).ElementType);
                InferTypeForExpression(arg, ptrType);
                return ptrType;
            }
            else if (targetBitSize >= NativeInt && argType is XPointerType)
            {
                return argType;
            }
            var resultType = (GetInformationAmount(expectedType) == targetBitSize && IsSigned(expectedType) == targetSigned) ? expectedType : targetType;
            arg.ExpectedType = resultType; // store the expected type in the argument so that AstMethodBodyBuilder will insert a cast
            return resultType;
        }

        public static XTypeReference GetFieldType(XFieldReference fieldReference)
        {
            return SubstituteTypeArgs(UnpackModifiers(fieldReference.FieldType), fieldReference);
        }

        public static XTypeReference SubstituteTypeArgs(XTypeReference type, XMemberReference member)
        {
            if (type is XTypeSpecification)
            {
                var arrayType = type as XArrayType;
                if (arrayType != null)
                {
                    var elementType = SubstituteTypeArgs(arrayType.ElementType, member);
                    if (elementType != arrayType.ElementType)
                    {
                        var newArrayType = new XArrayType(elementType, arrayType.Dimensions);
                        return newArrayType;
                    }
                    else
                    {
                        return type;
                    }
                }
                var refType = type as XByReferenceType;
                if (refType != null)
                {
                    var elementType = SubstituteTypeArgs(refType.ElementType, member);
                    return elementType != refType.ElementType ? new XByReferenceType(elementType) : type;
                }
                var giType = type as XGenericInstanceType;
                if (giType != null)
                {
                    var genericArgs = giType.GenericArguments.ToArray();
                    var isChanged = false;
                    for (int i = 0; i < giType.GenericArguments.Count; i++)
                    {
                        var argType = SubstituteTypeArgs(giType.GenericArguments[i], member);
                        if (genericArgs[i] != argType)
                        {
                            isChanged = true;
                            genericArgs[i] = argType;
                        }
                    }
                    return isChanged ? new XGenericInstanceType(giType.ElementType, genericArgs) : type;
                }
                var optmodType = type as XOptionalModifierType;
                if (optmodType != null)
                {
                    var elementType = SubstituteTypeArgs(optmodType.ElementType, member);
                    return elementType != optmodType.ElementType ? new XOptionalModifierType(optmodType.ModifierType, elementType) : type;
                }
                var reqmodType = type as XRequiredModifierType;
                if (reqmodType != null)
                {
                    var elementType = SubstituteTypeArgs(reqmodType.ElementType, member);
                    return elementType != reqmodType.ElementType ? new XRequiredModifierType(reqmodType.ModifierType, elementType) : type;
                }
                var ptrType = type as XPointerType;
                if (ptrType != null)
                {
                    var elementType = SubstituteTypeArgs(ptrType.ElementType, member);
                    return elementType != ptrType.ElementType ? new XPointerType(elementType) : type;
                }
            }
            var gp = type as XGenericParameter;
            if (gp != null)
            {
                var declaringType = member.DeclaringType;
                if (declaringType is XArrayType)
                {
                    return ((XArrayType)declaringType).ElementType;
                }
                if (gp.Owner is XMethodReference)
                {
                    var giMethod = member as XGenericInstanceMethod;
                    if (giMethod != null)
                        return giMethod.GenericArguments[gp.Position];
                }
                else if (declaringType.IsGenericInstance)
                {
                    return ((XGenericInstanceType)declaringType).GenericArguments[gp.Position];
                }
            }
            return type;
        }

        static XTypeReference UnpackPointer(XTypeReference pointerOrManagedReference)
        {
            var refType = pointerOrManagedReference as XByReferenceType;
            if (refType != null)
                return refType.ElementType;
            var ptrType = pointerOrManagedReference as XPointerType;
            if (ptrType != null)
                return ptrType.ElementType;
            return null;
        }

        internal static XTypeReference UnpackModifiers(XTypeReference type)
        {
            while (type is XOptionalModifierType || type is XRequiredModifierType)
                type = type.ElementType;
            return type;
        }

        static XTypeReference GetNullableTypeArgument(XTypeReference type)
        {
            var t = type as XGenericInstanceType;
            return IsNullableType(t) ? t.GenericArguments[0] : type;
        }

        XGenericInstanceType CreateNullableType(XTypeReference type)
        {
            if (type == null) return null;
            return new XGenericInstanceType(new XTypeReference.SimpleXTypeReference(module, "System", "Nullable`1", null, true, new[] { "T" }), new[] { type });
        }

        XTypeReference InferArgumentsInBinaryOperator(AstExpression expr, bool? isSigned, XTypeReference expectedType)
        {
            return InferBinaryArguments(expr.Arguments[0], expr.Arguments[1], expectedType);
        }

        XTypeReference InferArgumentsInAddition(AstExpression expr, bool? isSigned, XTypeReference expectedType)
        {
            AstExpression left = expr.Arguments[0];
            AstExpression right = expr.Arguments[1];
            var leftPreferred = DoInferTypeForExpression(left, expectedType);
            if (leftPreferred is XPointerType)
            {
                left.InferredType = left.ExpectedType = leftPreferred;
                InferTypeForExpression(right, typeSystem.IntPtr);
                return leftPreferred;
            }
            var rightPreferred = DoInferTypeForExpression(right, expectedType);
            if (rightPreferred is XPointerType)
            {
                InferTypeForExpression(left, typeSystem.IntPtr);
                right.InferredType = right.ExpectedType = rightPreferred;
                return rightPreferred;
            }
            return InferBinaryArguments(left, right, expectedType, leftPreferred: leftPreferred, rightPreferred: rightPreferred);
        }

        XTypeReference InferArgumentsInSubtraction(AstExpression expr, bool? isSigned, XTypeReference expectedType)
        {
            AstExpression left = expr.Arguments[0];
            AstExpression right = expr.Arguments[1];
            XTypeReference leftPreferred = DoInferTypeForExpression(left, expectedType);
            if (leftPreferred is XPointerType)
            {
                left.InferredType = left.ExpectedType = leftPreferred;
                InferTypeForExpression(right, typeSystem.IntPtr);
                return leftPreferred;
            }
            return InferBinaryArguments(left, right, expectedType, leftPreferred: leftPreferred);
        }

        XTypeReference InferBinaryArguments(AstExpression left, AstExpression right, XTypeReference expectedType, bool forceInferChildren = false, XTypeReference leftPreferred = null, XTypeReference rightPreferred = null)
        {
            if (leftPreferred == null) leftPreferred = DoInferTypeForExpression(left, expectedType, forceInferChildren);
            if (rightPreferred == null) rightPreferred = DoInferTypeForExpression(right, expectedType, forceInferChildren);
            if (IsSameType(leftPreferred, rightPreferred))
            {
                return left.InferredType = right.InferredType = left.ExpectedType = right.ExpectedType = leftPreferred;
            }
            else if (IsSameType(rightPreferred, DoInferTypeForExpression(left, rightPreferred, forceInferChildren)))
            {
                return left.InferredType = right.InferredType = left.ExpectedType = right.ExpectedType = rightPreferred;
            }
            else if (IsSameType(leftPreferred, DoInferTypeForExpression(right, leftPreferred, forceInferChildren)))
            {
                // re-infer the left expression with the preferred type to reset any conflicts caused by the rightPreferred type
                DoInferTypeForExpression(left, leftPreferred, forceInferChildren);
                return left.InferredType = right.InferredType = left.ExpectedType = right.ExpectedType = leftPreferred;
            }
            else
            {
                left.ExpectedType = right.ExpectedType = TypeWithMoreInformation(leftPreferred, rightPreferred);
                left.InferredType = DoInferTypeForExpression(left, left.ExpectedType, forceInferChildren);
                right.InferredType = DoInferTypeForExpression(right, right.ExpectedType, forceInferChildren);
                return left.ExpectedType;
            }
        }

        /// <summary>
        /// Return the type which has the most information.
        /// </summary>
        private static XTypeReference TypeWithMoreInformation(XTypeReference leftPreferred, XTypeReference rightPreferred)
        {
            var left = GetInformationAmount(leftPreferred);
            var right = GetInformationAmount(rightPreferred);
            if (left < right) return rightPreferred;
            if (left > right) return leftPreferred;
            if (leftPreferred.IsSystemObject())
            {
                // Cases where ldnull is followed by something more specific
                return rightPreferred;
            }
            // TODO
            return leftPreferred;
        }

        /// <summary>
        /// Information amount used for IntPtr.
        /// </summary>
        private const int NativeInt = 33; // treat native int as between int32 and int64

        private static int GetInformationAmount(XTypeReference type)
        {
            if (type == null)
                return 0;
            if (type.IsValueType && !IsArrayPointerOrReference(type))
            {
                // value type might be an enum
                XTypeDefinition typeDef;
                if (type.TryResolve(out typeDef) && typeDef.IsEnum)
                {
                    var underlyingType = typeDef.Fields.Single(f => f.IsRuntimeSpecialName && !f.IsStatic).FieldType;
                    return GetInformationAmount(underlyingType);
                }
            }
            switch (type.Kind)
            {
                case XTypeReferenceKind.Void:
                    return 0;
                case XTypeReferenceKind.Bool:
                    return 1;
                case XTypeReferenceKind.SByte:
                case XTypeReferenceKind.Byte:
                    return 8;
                case XTypeReferenceKind.Char:
                case XTypeReferenceKind.Short:
                case XTypeReferenceKind.UShort:
                    return 16;
                case XTypeReferenceKind.Int:
                case XTypeReferenceKind.UInt:
                case XTypeReferenceKind.Float:
                    return 32;
                case XTypeReferenceKind.Long:
                case XTypeReferenceKind.ULong:
                case XTypeReferenceKind.Double:
                    return 64;
                case XTypeReferenceKind.IntPtr:
                case XTypeReferenceKind.UIntPtr:
                    return NativeInt;
                default:
                    return 100; // we consider structs/objects to have more information than any primitives
            }
        }

        public static bool IsBoolean(XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Bool.IsSame(type);
        }

        public static bool IsIntegerOrEnum(XTypeReference type)
        {
            return IsSigned(type) != null;
        }

        public static bool IsEnum(XTypeReference type)
        {
            // Arrays/Pointers/ByReference resolve to their element type, but we don't want to consider those to be enums
            // However, GenericInstanceTypes, ModOpts etc. should be considered enums.
            if (type == null || IsArrayPointerOrReference(type))
                return false;
            // unfortunately we cannot rely on type.IsValueType here - it's not set when the instruction operand is a typeref (as opposed to a typespec)
            XTypeDefinition typeDef;
            return type.TryResolve(out typeDef) && typeDef.IsEnum;
        }

        static bool? IsSigned(XTypeReference type)
        {
            if (type == null || IsArrayPointerOrReference(type))
                return null;
            // unfortunately we cannot rely on type.IsValueType here - it's not set when the instruction operand is a typeref (as opposed to a typespec)
            XTypeDefinition typeDef;
            if (type.TryResolve(out typeDef) && typeDef.IsEnum)
            {
                var underlyingType = typeDef.Fields.Single(f => f.IsRuntimeSpecialName && !f.IsStatic).FieldType;
                return IsSigned(underlyingType);
            }
            switch (type.Kind)
            {
                case XTypeReferenceKind.SByte:
                case XTypeReferenceKind.Short:
                case XTypeReferenceKind.Int:
                case XTypeReferenceKind.Long:
                case XTypeReferenceKind.IntPtr:
                    return true;
                case XTypeReferenceKind.Byte:
                case XTypeReferenceKind.Char:
                case XTypeReferenceKind.UShort:
                case XTypeReferenceKind.UInt:
                case XTypeReferenceKind.ULong:
                case XTypeReferenceKind.UIntPtr:
                    return false;
                default:
                    return null;
            }
        }

        static bool OperandFitsInType(XTypeReference type, int num)
        {
            XTypeDefinition typeDef;
            if (type.TryResolve(out typeDef) && typeDef.IsEnum)
            {
                type = typeDef.Fields.Single(f => f.IsRuntimeSpecialName && !f.IsStatic).FieldType;
            }
            switch (type.Kind)
            {
                case XTypeReferenceKind.SByte:
                    return sbyte.MinValue <= num && num <= sbyte.MaxValue;
                case XTypeReferenceKind.Short:
                    return short.MinValue <= num && num <= short.MaxValue;
                case XTypeReferenceKind.Byte:
                    return byte.MinValue <= num && num <= byte.MaxValue;
                case XTypeReferenceKind.Char:
                    return char.MinValue <= num && num <= char.MaxValue;
                case XTypeReferenceKind.UShort:
                    return ushort.MinValue <= num && num <= ushort.MaxValue;
                default:
                    return true;
            }
        }

        static bool IsArrayPointerOrReference(XTypeReference type)
        {
            var typeSpec = type as XTypeSpecification;
            while (typeSpec != null)
            {
                if (typeSpec is XArrayType || typeSpec is XPointerType || typeSpec is XByReferenceType)
                    return true;
                typeSpec = typeSpec.ElementType as XTypeSpecification;
            }
            return false;
        }

        internal static bool IsNullableType(XTypeReference type)
        {
            return (type != null) && type.IsSystemNullable();
        }

        public static TypeCode GetTypeCode(XTypeReference type)
        {
            if (type == null)
                return TypeCode.Empty;
            switch (type.Kind)
            {
                case XTypeReferenceKind.Bool:
                    return TypeCode.Boolean;
                case XTypeReferenceKind.Char:
                    return TypeCode.Char;
                case XTypeReferenceKind.SByte:
                    return TypeCode.SByte;
                case XTypeReferenceKind.Byte:
                    return TypeCode.Byte;
                case XTypeReferenceKind.Short:
                    return TypeCode.Int16;
                case XTypeReferenceKind.UShort:
                    return TypeCode.UInt16;
                case XTypeReferenceKind.Int:
                    return TypeCode.Int32;
                case XTypeReferenceKind.UInt:
                    return TypeCode.UInt32;
                case XTypeReferenceKind.Long:
                    return TypeCode.Int64;
                case XTypeReferenceKind.ULong:
                    return TypeCode.UInt64;
                case XTypeReferenceKind.Float:
                    return TypeCode.Single;
                case XTypeReferenceKind.Double:
                    return TypeCode.Double;
                default:
                    if (type.IsSystemString())
                        return TypeCode.String;
                    return TypeCode.Object;
            }
        }

        /// <summary>
        /// Clears the type inference data on the method.
        /// </summary>
        public static void Reset(AstBlock method)
        {
            foreach (var expr in method.GetSelfAndChildrenRecursive<AstExpression>())
            {
                expr.InferredType = null;
                expr.ExpectedType = null;
                var v = expr.Operand as AstVariable;
                if (v != null && v.IsGenerated)
                {
                    v.Type = null;
                }
            }
        }

        public static bool IsSameType(XTypeReference type1, XTypeReference type2)
        {
            if (type1 == type2)
                return true;
            if (type1 == null || type2 == null)
                return false;
            return type1.IsSame(type2);
        }
    }
}
