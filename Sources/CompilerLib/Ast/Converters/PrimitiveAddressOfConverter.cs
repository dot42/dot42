using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert ldloca, ldflda, lfsflda, addressof in the context of call arguments
    /// </summary>
    internal static class PrimitiveAddressOfConverter
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, MethodSource currentMethod, AssemblyCompiler compiler)
        {
            foreach (var node in ast.GetExpressions(x => (x.Code == AstCode.Call) || (x.Code == AstCode.Calli) || (x.Code == AstCode.Callvirt)))
            {
                var method = (XMethodReference)node.Operand;
                XMethodDefinition methodDef;
                if (method.TryResolve(out methodDef) && methodDef.IsConstructor && methodDef.DeclaringType.IsPrimitive && (node.Arguments.Count == 2))
                {
                    // primitive.ctor(addressof_primitive, value) -> primitive.cast(value)
                    var locVar = node.Arguments[0].Operand;
                    node.Arguments.RemoveAt(0);
                    node.SetCode(AstCode.Stloc).SetType(node.Arguments[0].GetResultType()).Operand = locVar;
                }
                else
                {
                    for (var i = 0; i < node.Arguments.Count; i++)
                    {
                        ProcessArgument(node, method, i, currentMethod, compiler.Module);
                    }
                }
            }
        }

        /// <summary>
        /// Process an argument of the given call node.
        /// </summary>
        private static
        void ProcessArgument(AstExpression callNode, XMethodReference methodRef, int argumentIndex, MethodSource currentMethod, XModule assembly)
        {
            var node = callNode.Arguments[argumentIndex];

            // Should we do something?
            switch (node.Code)
            {
                case AstCode.Ldloca: // Parameter
                case AstCode.AddressOf: // Local variable
                case AstCode.Ldflda: // Instance field
                case AstCode.Ldsflda: // Static field
                case AstCode.Ldelema: // Array
                    break;
                case AstCode.Ldloc:
                    if (!node.MatchThis() || !currentMethod.IsDotNet)
                        return;
                    break;
                default:
                    return;
            }

            // Process argument
            var method = methodRef.Resolve();
            var argIsThis = !method.IsStatic && (argumentIndex == 0);
            var parameterIndex = method.IsStatic ? argumentIndex : argumentIndex - 1;
            var parameter = argIsThis ? null : method.Parameters[parameterIndex];
            var argType = argIsThis ? method.DeclaringType : parameter.ParameterType;
            //var argAttrs = argIsThis ? ParameterAttributes.None : parameter.Attributes;
            var argIsByRef = argType.IsByReference;
            var argIsOut = argIsThis ? false : argIsByRef && (parameter.Kind == XParameterKind.Output); // argIsByRef && argAttrs.HasFlag(ParameterAttributes.Out);
            var argIsGenByRefParam = argIsByRef && argType.ElementType.IsGenericParameter;
            switch (node.Code)
            {
                case AstCode.Ldloca: // Parameter
                    {
                        var variable = ((AstVariable)node.Operand);
                        if (variable.Type.IsPrimitive || argIsByRef)
                        {
                            // Box first
                            var ldloc = new AstExpression(node.SourceLocation, AstCode.Ldloc, node.Operand) { InferredType = variable.Type };
                            if (argIsByRef)
                            {
                                var stloc = new AstExpression(node.SourceLocation, AstCode.Stloc, node.Operand) { InferredType = variable.Type };
                                stloc.Arguments.Add(GetValueOutOfByRefArray(node, variable.Type, argIsGenByRefParam, assembly));
                                ConvertToByRefArray(node, variable.Type, ldloc, stloc, argIsOut, argIsGenByRefParam, argType.ElementType, assembly);
                            }
                            else
                            {
                                ConvertToBox(node, variable.Type, ldloc);
                            }
                        }
                        else if (variable.Type.IsGenericParameter)
                        {
                            // Convert to ldarg
                            var ldloc = new AstExpression(node.SourceLocation, AstCode.Ldloc, node.Operand) { InferredType = variable.Type };
                            callNode.Arguments[argumentIndex] = ldloc;
                        }
                    }
                    break;
                case AstCode.Ldloc: // this
                    {
                        var variable = ((AstVariable)node.Operand);
                        if (argIsThis && (variable.Type.IsByReference))
                        {
                            node.SetType(variable.Type.ElementType);
                        }
                        else if (argIsByRef)
                        {
                            var ldclone = new AstExpression(node);
                            var stExpr = new AstExpression(node.SourceLocation, AstCode.Nop, null);
                            var elementType = variable.Type;
                            if (elementType.IsByReference) elementType = elementType.ElementType;
                            ConvertToByRefArray(node, elementType, ldclone, stExpr, argIsOut, argIsGenByRefParam, argType.ElementType, assembly);
                        }
                    }
                    break;
                case AstCode.AddressOf: // Local variable
                    {
                        var arg = node.Arguments[0];
                        var type = arg.GetResultType();
                        var typeDef = type.Resolve();
                        if (typeDef.IsPrimitive)
                        {
                            if (argIsByRef)
                            {
                                throw new CompilerException("Unsupported use of AddressOf by byref argument");
                            }
                            else
                            {
                                ConvertToBox(node, type, arg);
                            }
                        }
                    }
                    break;
                case AstCode.Ldflda: // Instance field
                case AstCode.Ldsflda: // Static field
                    {
                        var fieldRef = (XFieldReference)node.Operand;
                        var field = fieldRef.Resolve();
                        if (field.FieldType.IsPrimitive || argIsByRef)
                        {
                            // Box first
                            var ldfldCode = (node.Code == AstCode.Ldflda) ? AstCode.Ldfld : AstCode.Ldsfld;
                            var ldfld = new AstExpression(node.SourceLocation, ldfldCode, node.Operand) { InferredType = field.FieldType };
                            ldfld.Arguments.AddRange(node.Arguments);
                            if (argIsByRef)
                            {
                                var stfldCode = (node.Code == AstCode.Ldflda) ? AstCode.Stfld : AstCode.Stsfld;
                                var stfld = new AstExpression(node.SourceLocation, stfldCode, node.Operand) { InferredType = field.FieldType };
                                stfld.Arguments.AddRange(node.Arguments); // instance
                                stfld.Arguments.Add(GetValueOutOfByRefArray(node, field.FieldType, argIsGenByRefParam, assembly)); // value
                                ConvertToByRefArray(node, field.FieldType, ldfld, stfld, argIsOut, argIsGenByRefParam, argType.ElementType, assembly);
                            }
                            else
                            {
                                ConvertToBox(node, field.FieldType, ldfld);
                            }
                        }
                    }
                    break;
                case AstCode.Ldelema: // Array element
                    {
                        var array = node.Arguments[0];
                        var arrayType = array.GetResultType();
                        var type = arrayType.ElementType;
                        if (type.IsPrimitive || argIsByRef)
                        {
                            // Box first
                            var ldElemCode = type.GetLdElemCode();
                            var ldelem = new AstExpression(node.SourceLocation, ldElemCode, node.Operand) { InferredType = type };
                            ldelem.Arguments.AddRange(node.Arguments);
                            if (argIsByRef)
                            {
                                var stelemCode = type.GetStElemCode();
                                var stelem = new AstExpression(node.SourceLocation, stelemCode, node.Operand) { InferredType = type };
                                stelem.Arguments.AddRange(node.Arguments);
                                stelem.Arguments.Add(GetValueOutOfByRefArray(node, type, argIsGenByRefParam, assembly));
                                ConvertToByRefArray(node, type, ldelem, stelem, argIsOut, argIsGenByRefParam, argType.ElementType, assembly);
                            }
                            else
                            {
                                ConvertToBox(node, type, ldelem);
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Convert the given node to a box operation with given type and argument.
        /// </summary>
        private static void ConvertToBox(AstExpression node, XTypeReference boxType, AstExpression argument)
        {
            node.Code = AstCode.Box;
            node.Operand = boxType;
            node.InferredType = boxType;
            node.ExpectedType = boxType;
            node.Arguments.Clear();
            node.Arguments.Add(argument);
        }

        /// <summary>
        /// Convert the given node to a array creation operation with given element type and argument.
        /// </summary>
        private static void ConvertToByRefArray(AstExpression node, XTypeReference elementType, AstExpression argument, AstExpression storeArgument, bool isOut, bool argIsGenParam, XTypeReference sourceType, XModule assembly)
        {
            var arrayElementType = argIsGenParam ? assembly.TypeSystem.Object : elementType;

            if (argIsGenParam && ((!isOut) || (elementType == null) || !elementType.IsSame(argument.GetResultType())))
            {
                argument = new AstExpression(node.SourceLocation, AstCode.Box, elementType, argument);
            }

            node.Code = isOut ? AstCode.ByRefOutArray : AstCode.ByRefArray;
            node.Operand = arrayElementType;
            node.InferredType = new XByReferenceType(arrayElementType);
            node.ExpectedType = new XByReferenceType(arrayElementType);
            node.Arguments.Clear();
            node.Arguments.Add(argument);
            node.Arguments.Add(new AstExpression(node.SourceLocation, AstCode.Nop, sourceType)); // how else can be keep this information?
            node.StoreByRefExpression = storeArgument;
        }

        /// <summary>
        /// Create an expression that results that value that is stored in a byref array.
        /// </summary>
        private static AstExpression GetValueOutOfByRefArray(AstExpression byRefArrayNode, XTypeReference elementType, bool argIsGenParam, XModule assembly)
        {
            var arrayElementType = argIsGenParam ? assembly.TypeSystem.Object : elementType;

            var sp = byRefArrayNode.SourceLocation;
            var index = new AstExpression(sp, AstCode.Ldc_I4, 0) { InferredType = assembly.TypeSystem.Int };
            var ldelem = new AstExpression(sp, arrayElementType.GetLdElemCode(), null) { InferredType = arrayElementType };
            ldelem.Arguments.Add(byRefArrayNode);
            ldelem.Arguments.Add(index);

            if (!argIsGenParam)
                return ldelem;

            return new AstExpression(byRefArrayNode.SourceLocation, AstCode.Unbox, elementType, ldelem);
        }
    }
}
