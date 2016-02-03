using System;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert ldloca, ldflda, lfsflda, addressof in the context of call arguments for non-primitive structs.
    /// </summary>
    internal static class StructCallConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, AssemblyCompiler compiler)
        {
            foreach (var pair in ast.GetExpressionPairs())
            {
                var node = pair.Expression;
                switch (node.Code)
                {
                    case AstCode.Call:
                    case AstCode.Calli:
                    case AstCode.Callvirt:
                        var methodRef = (XMethodReference) node.Operand;
                        XMethodDefinition method;
                        if (methodRef.TryResolve(out method))
                        {
                            var targetType = method.DeclaringType;
                            var prefix = node.GetPrefix(AstCode.Constrained);
                            if (prefix != null)
                            {
                                // Calls to object::ToString for value types have a constrained prefix with the target type in the prefix operand.
                                var prefixTypeRef = (XTypeReference) prefix.Operand;
                                XTypeDefinition prefixTypeDef;
                                if (prefixTypeRef.TryResolve(out prefixTypeDef))
                                {
                                    targetType = prefixTypeDef;
                                }
                            }
                            if (targetType.IsValueType && !targetType.IsPrimitive 
                                && !/*targetType*/method.DeclaringType.IsSystemNullable())
                            {
                                if (method.IsConstructor && (!node.Arguments[0].MatchThis()))
                                {
                                    // Convert ctor call
                                    ConvertCtorCall(node, methodRef);
                                }
                                else
                                {
                                    // Convert normal method call
                                    for (var i = 0; i < node.Arguments.Count; i++)
                                    {
                                        ProcessArgument(node, method, i, compiler.Module);
                                    }
                                }
                            }

                            // Clone all struct arguments
                            for (var i = 0; i < node.Arguments.Count; i++)
                            {
                                CloneStructArgument(node, method, i);
                            }
                        }
                        break;
                    case AstCode.DefaultValue:
                        {
                            var typeRef = (XTypeReference) node.Operand;
                            XTypeDefinition type;
                            if (typeRef.TryResolve(out type))
                            {
                                if (type.IsStruct)
                                {
                                    var defaultCtor = GetDefaultValueCtor(type);
                                    ConvertDefaultValue(node, defaultCtor);
                                }
                            }
                        }
                        break;
                    case AstCode.Newarr:
                        {
                            var typeRef = (XTypeReference) node.Operand;
                            XTypeDefinition type;
                            if (typeRef.TryResolve(out type) && type.IsValueType && !type.IsPrimitive &&
                                !type.IsSystemNullable())
                            {
                                var parentCode = (pair.Parent != null) ? pair.Parent.Code : AstCode.Nop;
                                if (type.IsEnum)
                                {
                                    // Initialize enum array
                                    if (parentCode != AstCode.InitEnumArray) // Avoid recursion
                                    {
                                        ConvertNewArrEnum(node);
                                    }
                                }
                                else
                                {
                                    // Initialize struct array
                                    if (parentCode != AstCode.InitStructArray) // Avoid recursion
                                    {
                                        var defaultCtor = GetDefaultValueCtor(type);
                                        ConvertNewArrStruct(node, defaultCtor);
                                    }
                                }
                            }
                        }
                        break;

                    case AstCode.Ldloca:
                        node.Code = AstCode.Ldloc;
                        break;
                    case AstCode.Ldflda:
                        node.Code = AstCode.Ldfld;
                        break;
                    case AstCode.Ldsflda:
                        node.Code = AstCode.Ldsfld;
                        break;
                    case AstCode.Ldelema:
                        node.Code = AstCode.Ldelem_Any;
                        break;
                    case AstCode.Ldobj:
                        if ((node.Arguments.Count == 1) && (node.Arguments[0].Code == AstCode.Ldloc))
                        {
                            node.CopyFrom(node.Arguments[0]);
                        }
                        break;

                    case AstCode.Stelem_Any:
                        {
                            var arrayType = node.Arguments[0].GetResultType() as XArrayType;
                            XTypeDefinition elementType;
                            if ((arrayType != null)
                                && (arrayType.ElementType.IsStruct(out elementType) 
                                && !elementType.IsImmutableStruct))
                            {
                                // Call $Clone
                                var cloneMethod = GetCloneMethod(elementType);
                                var valueArg = node.Arguments[2];
                                if (IsCloneNeeded(valueArg))
                                {
                                    var clone = new AstExpression(node.SourceLocation, AstCode.Call, arrayType.ElementType.CreateReference(cloneMethod), valueArg);
                                    node.Arguments[2] = clone;
                                }
                            }
                        }
                        break;
                    case AstCode.Stfld:
                    case AstCode.Stsfld:
                        {
                            var field = (XFieldReference) node.Operand;
                            XTypeDefinition fieldType;
                            if (field.FieldType.IsStruct(out fieldType) && !fieldType.IsImmutableStruct)
                            {
                                // Call $Clone
                                var cloneMethod = GetCloneMethod(fieldType);
                                var valueArgIndex = node.Arguments.Count - 1; // Last argument
                                var valueArg = node.Arguments[valueArgIndex];
                                if (IsCloneNeeded(valueArg))
                                {
                                    var clone = new AstExpression(node.SourceLocation, AstCode.Call, field.FieldType.CreateReference(cloneMethod), valueArg);
                                    node.Arguments[valueArgIndex] = clone;
                                }
                            }
                        }
                        break;
                    case AstCode.Stloc:
                        {
                            var variable = (AstVariable) node.Operand;
                            XTypeDefinition varType;
                            if (variable.Type.IsStruct(out varType))
                            {
                                if (variable.IsThis)
                                {
                                    // Call this.$CopyFrom
                                    var copyFromMethod = GetCopyFromMethod(varType);
                                    node.Code = AstCode.Call;
                                    node.Operand = variable.Type.CreateReference(copyFromMethod);
                                }
                                else
                                {
                                    if (!varType.IsImmutableStruct)
                                    {
                                        // Call $Clone
                                        var cloneMethod = GetCloneMethod(varType);
                                        var valueArgIndex = node.Arguments.Count - 1; // Last argument
                                        var valueArg = node.Arguments[valueArgIndex];
                                        if (IsCloneNeeded(valueArg))
                                        {
                                            var clone = new AstExpression(node.SourceLocation, AstCode.Call,
                                                variable.Type.CreateReference(cloneMethod), valueArg);
                                            node.Arguments[valueArgIndex] = clone;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case AstCode.Stobj:
                        {
                            var type = (XTypeReference) node.Operand;
                            XTypeDefinition typeDef;
                            if (type.IsStruct(out typeDef) && !typeDef.IsImmutableStruct)
                            {
                                // Convert to arg0.$CopyFrom(arg1)
                                var copyFromMethod = GetCopyFromMethod(typeDef);
                                node.Code = AstCode.Call;
                                node.Operand = type.CreateReference(copyFromMethod);
                            }
                        }
                        break;
                    case AstCode.Box:
                        if (node.Arguments.Count == 1)
                        {
                            var typeRef = (XTypeReference) node.Operand;
                            XTypeDefinition type;
                            if (typeRef.TryResolve(out type) && type.IsValueType && !type.IsPrimitive &&
                                !type.IsSystemNullable())
                            {
                                var ldObjExpr = node.Arguments[0];
                                if ((ldObjExpr.Code == AstCode.Ldobj) && (ldObjExpr.Arguments.Count == 1))
                                {
                                    var ldThisExpr = ldObjExpr.Arguments[0];
                                    if (ldThisExpr.MatchThis())
                                    {
                                        // box(ldobj(ldthis)) -> ldthis
                                        // TODO: think about making a clone here.
                                        node.CopyFrom(ldThisExpr);
                                    }
                                }
                            }
                        }
                        break;
                    case AstCode.Ret:
                        if (node.Arguments.Count == 1)
                        {
                            var ldObjExpr = node.Arguments[0];
                            if ((ldObjExpr.Code == AstCode.Ldobj) && (ldObjExpr.Arguments.Count == 1))
                            {
                                var typeRef = ldObjExpr.GetResultType();
                                XTypeDefinition type;
                                if (typeRef.TryResolve(out type) && type.IsValueType && !type.IsPrimitive &&
                                    !type.IsSystemNullable())
                                {
                                    var ldThisExpr = ldObjExpr.Arguments[0];
                                    if (ldThisExpr.MatchThis())
                                    {
                                        // box(ldobj(ldthis)) -> ldthis
                                        ldObjExpr.CopyFrom(ldThisExpr);
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the default constructor of the given value type.
        /// Throws an exception if not found.
        /// </summary>
        internal static XMethodDefinition GetDefaultValueCtor(XTypeDefinition valueType)
        {
            var defaultCtor = valueType.Methods.FirstOrDefault(x => x.IsConstructor && !x.IsStatic && (x.Parameters.Count == 0));
            if (defaultCtor == null)
                throw new NotImplementedException(string.Format("Value type {0} has no default ctor", valueType.FullName));
            return defaultCtor;
        }

        /// <summary>
        /// Gets the struct $Clone method of the given value type.
        /// Throws an exception if not found.
        /// </summary>
        private static XMethodDefinition GetCloneMethod(XTypeDefinition valueType)
        {
            var method = valueType.Methods.FirstOrDefault(x => !x.IsStatic && (x.Parameters.Count == 0) && (x.Name == NameConstants.Struct.CloneMethodName));
            if (method == null)
                throw new NotImplementedException(string.Format("Value type {0} has no struct clone method", valueType.FullName));
            return method;
        }

        /// <summary>
        /// Gets the struct $CopyFrom method of the given value type.
        /// Throws an exception if not found.
        /// </summary>
        private static XMethodDefinition GetCopyFromMethod(XTypeDefinition valueType)
        {
            var method = valueType.Methods.FirstOrDefault(x => !x.IsStatic && (x.Parameters.Count == 1) && (x.Name == NameConstants.Struct.CopyFromMethodName));
            if (method == null)
                throw new NotImplementedException(string.Format("Value type {0} has no struct copyFrom method", valueType.FullName));
            return method;
        }

        /// <summary>
        /// The given call is a call to a ctor.
        /// Convert it to newobj
        /// </summary>
        private static void ConvertCtorCall(AstExpression callNode, XMethodReference ctor)
        {
            // Create a new node to construct the object
            var newObjNode = new AstExpression(callNode.SourceLocation, AstCode.Newobj, null, callNode.Arguments.Skip(1).ToList());
            newObjNode.Operand = ctor;
            newObjNode.InferredType = ctor.DeclaringType;

            // Remove "this" argument
            var thisExpr = callNode.Arguments[0];
            callNode.Arguments.Clear();
            callNode.Arguments.Add(newObjNode);
            callNode.InferredType = ctor.DeclaringType;

            // Convert callNode to stX node
            switch (thisExpr.Code)
            {
                case AstCode.Ldloca:
                case AstCode.Ldloc:
                    callNode.Code = AstCode.Stloc;
                    callNode.Operand = thisExpr.Operand;
                    break;
                case AstCode.Ldflda:
                case AstCode.Ldfld:
                    callNode.Code = AstCode.Stfld;
                    callNode.Operand = thisExpr.Operand;
                    break;
                case AstCode.Ldsflda:
                case AstCode.Ldsfld:
                    callNode.Code = AstCode.Stsfld;
                    callNode.Operand = thisExpr.Operand;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unsupported opcode {0} in call to struct ctor", (int)thisExpr.Code));
            }
        }

        /// <summary>
        /// The given node is defaultvalue(struct).
        /// Convert it to newobj
        /// </summary>
        internal static void ConvertDefaultValue(AstExpression callNode, XMethodDefinition defaultCtorDef)
        {
            var type = (XTypeReference)callNode.Operand;
            var defaultCtor = type.CreateReference(defaultCtorDef);
            var typeDef = defaultCtorDef.DeclaringType;

            if (typeDef.IsImmutableStruct)
            {
                var fieldDef = typeDef.Fields.SingleOrDefault(f => f.Name == NameConstants.Struct.DefaultFieldName);
                if (fieldDef != null)
                {
                    // load the default field.
                    callNode.Arguments.Clear();
                    callNode.Code = AstCode.Ldsfld;
                    callNode.Operand = fieldDef;
                    callNode.SetType(defaultCtor.DeclaringType);
                    return;
                }
            }
            else
            {
                // Remove "this" argument
                callNode.Arguments.Clear();
                callNode.Code = AstCode.Newobj;
                callNode.Operand = defaultCtor;
                callNode.SetType(defaultCtor.DeclaringType);
            }
        }

        /// <summary>
        /// The given node is newarr(enum).
        /// Convert it to initEnumArray(newarr(enums))
        /// </summary>
        private static void ConvertNewArrEnum(AstExpression newArrNode)
        {
            var newArrClone = new AstExpression(newArrNode);
            newArrNode.Code = AstCode.InitEnumArray;
            newArrNode.Arguments.Clear();
            newArrNode.Arguments.Add(newArrClone);
            // Preserve Operand (XTypeReference == element type)
        }

        /// <summary>
        /// The given node is newarr(struct).
        /// Convert it to initStructArray(newarr(struct))
        /// </summary>
        private static void ConvertNewArrStruct(AstExpression newArrNode, XMethodDefinition defaultCtor)
        {
            var newArrClone = new AstExpression(newArrNode);
            newArrNode.Code = AstCode.InitStructArray;
            newArrNode.Arguments.Clear();
            newArrNode.Arguments.Add(newArrClone);

            var typeDef = defaultCtor.DeclaringType;

            if (typeDef.IsImmutableStruct)
            {
                var field = typeDef.Fields.SingleOrDefault(f=>f.Name == NameConstants.Struct.DefaultFieldName);
                if(field != null)
                    newArrNode.Operand = field;
                else
                    newArrNode.Operand = defaultCtor;
            }
            else
            {
                newArrNode.Operand = defaultCtor;
            }
        }

        /// <summary>
        /// Process an argument of the given call node.
        /// </summary>
        private static void ProcessArgument(AstExpression callNode, XMethodDefinition method, int argumentIndex, XModule assembly)
        {
            var argNode = callNode.Arguments[argumentIndex];
           
            // Process argument
            switch (argNode.Code)
            {
                case AstCode.Ldloca: // Parameter
                    argNode.Code = AstCode.Ldloc;
                    break;
                case AstCode.Ldflda: // Instance field
                    argNode.Code = AstCode.Ldfld;
                    break;
                case AstCode.Ldsflda: // Static field
                    argNode.Code = AstCode.Ldsfld;
                    break;
                case AstCode.Ldelema: // Array element
                    argNode.Code = AstCode.Ldelem_Any;
                    break;
                case AstCode.Ldobj: // Load object
                    if ((argumentIndex == 0) && !method.IsStatic)
                    {
                        AstVariable variable;
                        if ((argNode.Arguments.Count == 1) && (argNode.Arguments[0].Match(AstCode.Ldloc, out variable)) &&
                            variable.Type.IsByReference)
                        {
                            argNode.Code = AstCode.Ldloc;
                            argNode.Operand = variable;
                            argNode.InferredType = variable.Type.ElementType;
                            argNode.Arguments.Clear();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Process an argument of the given call node.
        /// </summary>
        private static void CloneStructArgument(AstExpression callNode, XMethodDefinition method, int argumentIndex)
        {
            // Clone structs
            var paramIndex = method.IsStatic ? argumentIndex : argumentIndex - 1;
            if ((paramIndex >= 0) && (method.Name != NameConstants.Struct.CloneMethodName) && (method.Name != NameConstants.Struct.CopyFromMethodName))
            {
                var paramType = method.Parameters[paramIndex].ParameterType;
                XTypeDefinition typeDef;
                if (paramType.IsStruct(out typeDef) && !typeDef.IsImmutableStruct) 
                    // TODO: check if this decision should be made based on the actual argument type, not the destination type.
                    //       this should be true at least for generic parameters, which might either take a ValueType or 
                    //       a reference type.
                    //       Then again, maybe the struct argument should be cloned on an "box" instruction.
                {
                    // Call $Clone
                    var argNode = callNode.Arguments[argumentIndex];
                    if (IsCloneNeeded(argNode))
                    {
                        var cloneMethod = GetCloneMethod(typeDef);
                        var clone = new AstExpression(argNode.SourceLocation, AstCode.Call, paramType.CreateReference(cloneMethod), argNode);
                        callNode.Arguments[argumentIndex] = clone;
                    }
                }
            }
        }

        /// <summary>
        /// Is is needed to clone the given struct argument?
        /// </summary>
        private static bool IsCloneNeeded(AstExpression argument)
        {
            switch (argument.Code)
            {
                case AstCode.Newobj:
                case AstCode.DefaultValue:
                    return false;
                default:
                    return true;
            }
        }
    }
}
