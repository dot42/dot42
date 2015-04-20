using System.CodeDom;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Add/expand generic unboxing
    /// </summary>
    internal static class GenericsConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, MethodSource currentMethod, XTypeSystem typeSystem)
        {
            // Expand typeof
            foreach (var pair in ast.GetExpressionPairs())
            {
                var node = pair.Expression;
                switch (node.Code)
                {
                    case AstCode.Ldfld:
                    //case AstCode.Ldsfld: // NOT YET
                        {
                            var field = (XFieldReference) node.Operand;
                            UnboxIfGeneric(field.FieldType, node, typeSystem);
                        }
                        break;
                    case AstCode.Stfld:
                        {
                            var field = (XFieldReference)node.Operand;
                            BoxIfGeneric(field.FieldType, node.Arguments[1]);
                        }
                        break;
                    case AstCode.Call:
                    case AstCode.Calli:
                    case AstCode.Callvirt:
                        {
                            var method = (XMethodReference)node.Operand;
                            if ((!method.ReturnType.IsVoid()) && (pair.Parent != null))
                            {
                                UnboxIfGeneric(method.ReturnType, node, typeSystem);
                            }
                        }
                        break;
                    case AstCode.Ret:
                        {
                            if (node.Arguments.Count > 0)
                            {
                                var expectedType = currentMethod.Method.ReturnType;
                                BoxIfGeneric(expectedType, node.Arguments[0]);
                            }
                        }
                        break;
                    case AstCode.ByRefArray:
                    case AstCode.ByRefOutArray:
                    {
                        if (node.Arguments.Count > 1)
                        {
                            var originalType = (XTypeReference) node.Arguments[1].Operand;
                            UnboxByRefIfGeneric(originalType, node.StoreByRefExpression, typeSystem);
                        }
                    }
                    break;
                }
            }
        }

        private static void UnboxIfGeneric(XTypeReference type, AstExpression node, XTypeSystem typeSystem)
        {
            if (type.IsGenericParameter || type.IsGenericParameterArray())
            {
                var resultType = node.GetResultType();
                if (!type.IsByReference && resultType.IsByReference)
                {
                    var elementType = resultType.ElementType;

                    var clone = new AstExpression(node);
                    node.SetCode(AstCode.SimpleCastclass).SetArguments(clone).Operand = elementType;
                }
                else
                {
                    if (!TreatAsStruct(type, resultType))
                    {
                        var clone = new AstExpression(node);
                        node.SetCode(AstCode.UnboxFromGeneric).SetArguments(clone).Operand = type;
                    }
                    else
                    {
                        ConvertUnboxStruct(node, resultType, typeSystem);
                    }
                }
            }
        }

        private static void UnboxByRefIfGeneric(XTypeReference type, AstExpression node, XTypeSystem typeSystem)
        {
            if (!type.IsGenericParameter)
                return;
            
            var resultType = node.InferredType ?? node.ExpectedType;
            if (resultType == null)
                return;

            if (!TreatAsStruct(type, resultType))
                return;

            // find the first unbox, which should be our target.
            var unbox = node.GetSelfAndChildrenRecursive<AstExpression>( n => n.Code == AstCode.Unbox).FirstOrDefault();

            if (unbox == null)
                return;

            ConvertUnboxStruct(unbox, resultType, typeSystem);
        }

        private static void BoxIfGeneric(XTypeReference type, AstExpression node)
        {
            // TODO: CLR allows return-by-reference, though C# does not. Do we need to handle this here?

            if (type.IsGenericParameter)
            {
                var resultType = node.GetResultType();
                if (resultType.IsPrimitive)
                {
                    var clone = new AstExpression(node);
                    node.SetCode(AstCode.BoxToGeneric)
                                .SetArguments(clone)
                                .Operand = type;
                }
            }
            else if (type.IsGenericParameterArray())
            {
                var resultType = node.GetResultType().ElementType;
                if (resultType.IsPrimitive)
                {
                    var clone = new AstExpression(node);
                    node.SetCode(AstCode.BoxToGeneric).SetArguments(clone).Operand = type;
                }
            }
        }

        private static void ConvertUnboxStruct(AstExpression node, XTypeReference resultType, XTypeSystem typeSystem)
        {
            // Structs must never be null. We have to handle structs here, since a newobj 
            // might need generic arguments. These would be difficult to provide at "UnboxFromGeneric",
            // but will be automatically filled in by the GenericInstanceConverter

            // convert to (temp$ = (T)x) != null ? temp$ : new T();

            // replace any unbox, but keep if otherwise.
            var clone = node.Code == AstCode.Unbox ? new AstExpression(node.Arguments[0]) : new AstExpression(node);

            // make sure we don't evaluate the expression twice.
            var tempVar = new AstGeneratedVariable("temp$", "") { Type = typeSystem.Object };

            // T(x)
            var txExpr = new AstExpression(node.SourceLocation, AstCode.SimpleCastclass, resultType, clone)
                                .SetType(resultType);

            // temporary storage
            var storeTempVar = new AstExpression(node.SourceLocation, AstCode.Stloc, tempVar, txExpr) { ExpectedType = resultType };
            var loadTempVar = new AstExpression(node.SourceLocation, AstCode.Ldloc, tempVar)
                                .SetType(resultType);

            // new T()
            var constructor = StructCallConverter.GetDefaultValueCtor(resultType.Resolve());
            var newExpr = new AstExpression(node.SourceLocation, AstCode.Newobj, constructor)
                                .SetType(resultType);

            // Combine
            var conditional = new AstExpression(node.SourceLocation, AstCode.Conditional, resultType,
                                                storeTempVar, loadTempVar, newExpr)
                                .SetType(resultType);

            node.CopyFrom(conditional);
        }

        private static bool TreatAsStruct(XTypeReference type, XTypeReference resultType)
        {
            bool isStruct = resultType.IsStruct();
            bool isNullable = false;

            if (isStruct)
            {
                var gp = type as XGenericParameter;
                if (gp != null)
                {
                    var typeRef = gp.Owner as XTypeReference;
                    if (typeRef != null) isNullable = typeRef.IsSystemNullable();
                }
            }

            return isStruct && !isNullable;
        }
    }
}
