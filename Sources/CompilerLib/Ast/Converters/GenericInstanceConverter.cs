using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.XModel;
using Dot42.FrameworkDefinitions;
using Dot42.Utility;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Add/expand generic instance type information
    /// </summary>
    internal static class GenericInstanceConverter 
    {
        enum TypeConversion
        {
            // will not change the type
            None,
            // Will change to nullable marker class if Nullable<T>
            // (note: this could be expanded to support generic marker classes as well)
            NullableTypeOf,
            // will make sure that the type is neither a marker class nor a primitive type
            EnsureRuntimeType,
        }

        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, MethodSource currentMethod, AssemblyCompiler compiler)
        {
            var typeSystem = compiler.Module.TypeSystem;

            // Expand typeof
            foreach (var node in ast.GetSelfAndChildrenRecursive<AstExpression>(x => x.Code == AstCode.TypeOf))
            {
                var type = (XTypeReference) node.Operand;
                var typeHelperType = compiler.GetDot42InternalType(InternalConstants.TypeHelperName).Resolve();
                var loadExpr = LoadTypeForGenericInstance(node.SourceLocation, currentMethod, type, typeHelperType, typeSystem, TypeConversion.None);
                node.CopyFrom(loadExpr);
            }

            // Expand instanceOf
            foreach (var node in ast.GetSelfAndChildrenRecursive<AstExpression>(x => x.Code == AstCode.SimpleInstanceOf))
            {
                var type = (XTypeReference)node.Operand;
                var gp = type as XGenericParameter;
                if (gp == null) continue;

                var typeHelperType = compiler.GetDot42InternalType(InternalConstants.TypeHelperName).Resolve();
                var loadExpr = LoadTypeForGenericInstance(node.SourceLocation, currentMethod, type, typeHelperType, typeSystem, TypeConversion.EnsureRuntimeType);
                //// both types are boxed, no need for conversion.
                var typeType = compiler.GetDot42InternalType("System", "Type").Resolve();
                var isInstanceOfType = typeType.Methods.Single(n => n.Name == "JavaIsInstance" && n.Parameters.Count == 1);
                var call = new AstExpression(node.SourceLocation, AstCode.Call, isInstanceOfType, loadExpr, node.Arguments[0]);
                node.CopyFrom(call);
            }

            // Expand newarr
            foreach (var node in ast.GetSelfAndChildrenRecursive<AstExpression>(x => x.Code == AstCode.Newarr))
            {
                var type = (XTypeReference)node.Operand;
                if (!type.IsDefinitionOrReferenceOrPrimitive())
                {
                    // Resolve type to a Class<?>
                    var typeHelperType = compiler.GetDot42InternalType(InternalConstants.TypeHelperName).Resolve();
                    // while having primitive arrays for primitive types would be nice, a lot of boxing and unboxing
                    // would be needed. only for-primitive-specialized generic classes could optimize this.
                    var ldType = LoadTypeForGenericInstance(node.SourceLocation, currentMethod, type, typeHelperType, typeSystem, TypeConversion.EnsureRuntimeType);
                    var newInstanceExpr = new AstExpression(node.SourceLocation, AstCode.ArrayNewInstance, null, ldType, node.Arguments[0]) { ExpectedType = typeSystem.Object };
                    var arrayType = new XArrayType(type);
                    var cast = new AstExpression(node.SourceLocation, AstCode.SimpleCastclass, arrayType, newInstanceExpr) { ExpectedType = arrayType };
                    node.CopyFrom(cast);
                }
            }

            // Add generic instance call arguments
            foreach (var node in ast.GetSelfAndChildrenRecursive<AstExpression>(x => x.Code.IsCall()))
            {
                var method = (XMethodReference)node.Operand;
                if (method.DeclaringType.IsArray)
                    continue;
                XMethodDefinition methodDef;
                if (!method.TryResolve(out methodDef)) 
                    continue;
                if (methodDef.HasDexNativeAttribute())
                    continue;

                if (methodDef.NeedsGenericInstanceTypeParameter)
                {
                    // Add generic instance type parameter value
                    var arg = CreateGenericInstance(node.SourceLocation, method.DeclaringType, currentMethod, compiler);
                    node.Arguments.Add(arg);
                    node.GenericInstanceArgCount++;
                }

                if (methodDef.NeedsGenericInstanceMethodParameter)
                {
                    // Add generic instance method parameter
                    var arg = CreateGenericInstance(node.SourceLocation, method, currentMethod, compiler);
                    node.Arguments.Add(arg);
                    node.GenericInstanceArgCount++;
                }
            }

            // Add generic instance Delegate arguments for static methods.
            foreach (var node in ast.GetSelfAndChildrenRecursive<AstExpression>(x => x.Code == AstCode.Delegate))
            {
                var delegateInfo = (Tuple<XTypeDefinition, XMethodReference>)node.Operand;

                var genMethodDef = delegateInfo.Item2 as IXGenericInstance;
                var genTypeDef = delegateInfo.Item2.DeclaringType as IXGenericInstance;

                // Add generic instance type parameter value, if method is static
                if (genTypeDef != null && delegateInfo.Item2.Resolve().IsStatic)
                {
                    
                    var arg = CreateGenericInstance(node.SourceLocation, delegateInfo.Item2.DeclaringType, currentMethod, compiler);
                    node.Arguments.Add(arg);
                    node.GenericInstanceArgCount++;
                }

                // add generic method type parameter value.
                if (genMethodDef != null)
                {
                    var arg = CreateGenericInstance(node.SourceLocation, delegateInfo.Item2, currentMethod, compiler);
                    node.Arguments.Add(arg);
                    node.GenericInstanceArgCount++;
                }
            }

            // Convert NewObj when needed
            foreach (var node in ast.GetSelfAndChildrenRecursive<AstExpression>(x => x.Code == AstCode.Newobj))
            {
                var ctorRef = (XMethodReference)node.Operand;
                var declaringType = ctorRef.DeclaringType;
                if (declaringType.IsArray)
                {
                    // New multi dimensional array
                    // Get element type
                    var elemType = ((XArrayType) declaringType).ElementType;
                    var typeExpr = new AstExpression(node.SourceLocation, AstCode.TypeOf, elemType);

                    // Create dimensions array
                    var intArrayType = new XArrayType(typeSystem.Int);
                    var dimArrayExpr = new AstExpression(node.SourceLocation, AstCode.InitArrayFromArguments, intArrayType, node.Arguments).SetType(intArrayType);

                    // Call java.lang.reflect.Array.newInstance(type, int[])
                    var newInstanceExpr = new AstExpression(node.SourceLocation, AstCode.ArrayNewInstance2, null, typeExpr, dimArrayExpr).SetType(typeSystem.Object);

                    // Cast to correct type
                    var cast = new AstExpression(node.SourceLocation, AstCode.SimpleCastclass, declaringType, newInstanceExpr).SetType(declaringType);

                    // Replace node
                    node.CopyFrom(cast);
                }
                else
                {
                    // Normal "new object"
                    XMethodDefinition ctorDef;
                    if (ctorRef.TryResolve(out ctorDef) && ctorDef.NeedsGenericInstanceTypeParameter)
                    {
                        // Add generic instance type parameter value
                        var arg = CreateGenericInstance(node.SourceLocation, ctorRef.DeclaringType, currentMethod, compiler);
                        node.Arguments.Add(arg);
                        node.GenericInstanceArgCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Build expression that creates an instance of GenericInstance with arguments from the given .NET generic instance.
        /// </summary>
        private static AstExpression CreateGenericInstance(ISourceLocation seqp, XReference member, MethodSource currentMethod, AssemblyCompiler compiler)
        {
            // Prepare
            var genericInstance = member as IXGenericInstance;
            if (genericInstance == null)
            {
#if DEBUG
                //Debugger.Launch();
#endif
                throw new CompilerException(string.Format("{0} is not a generic instance", member));
            }
            var count = genericInstance.GenericArguments.Count;
            var typeHelperType = compiler.GetDot42InternalType(InternalConstants.TypeHelperName).Resolve();

            // Foreach type argument
            var typeExpressions = new List<AstExpression>();
            for (var i = 0; i < count; i++)
            {
                var argType = genericInstance.GenericArguments[i];
                var typeExpr = LoadTypeForGenericInstance(seqp, currentMethod, argType, typeHelperType, compiler.Module.TypeSystem, 
                                                          TypeConversion.NullableTypeOf);
                typeExpressions.Add(typeExpr);
            }

            var elementType = compiler.Module.TypeSystem.Type;
            return new AstExpression(seqp, AstCode.InitArrayFromArguments, new XArrayType(elementType), typeExpressions) { ExpectedType = new XArrayType(elementType) };
        }

        /// <summary>
        /// Create an expression that loads the given type at runtime.
        /// </summary>
        private static AstExpression LoadTypeForGenericInstance(ISourceLocation seqp, MethodSource currentMethod, XTypeReference type, XTypeDefinition typeHelperType, XTypeSystem typeSystem, TypeConversion typeConversion, XGenericInstanceType typeGenericArguments=null)
        {
            if (type.IsArray)
            {
                // Array type
                var arrayType = (XArrayType)type;
                // Load element type
                var prefix = LoadTypeForGenericInstance(seqp, currentMethod, ((XArrayType)type).ElementType, typeHelperType, typeSystem, typeConversion);
                // Convert to array type
                if (arrayType.Dimensions.Count() == 1)
                {
                    var giCreateArray = typeHelperType.Methods.Single(x => (x.Name == "Array") && (x.Parameters.Count == 1));
                    return new AstExpression(seqp, AstCode.Call, giCreateArray, prefix) { ExpectedType = typeSystem.Type };
                }
                else
                {
                    var giCreateArray = typeHelperType.Methods.Single(x => (x.Name == "Array") && (x.Parameters.Count == 2));
                    var dimensionsExpr = new AstExpression(seqp, AstCode.Ldc_I4, arrayType.Dimensions.Count()) { ExpectedType = typeSystem.Int };
                    return new AstExpression(seqp, AstCode.Call, giCreateArray, prefix, dimensionsExpr) { ExpectedType = typeSystem.Type };
                }
            }

            var gp = type as XGenericParameter;
            if (gp != null)
            {
                AstExpression gi;
                if (gp.Owner is XTypeReference)
                {
                    // Class type parameter
                    var owner = (XTypeReference)gp.Owner;
                    if (owner.GetElementType().Resolve().HasDexImportAttribute())
                    {
                        // Imported type
                        return new AstExpression(seqp, AstCode.TypeOf, typeSystem.Object) { ExpectedType = typeSystem.Type };
                    }
                    if (currentMethod.IsClassCtor)
                    {
                        // Class ctor's cannot have type information.
                        // Return Object instead
                        DLog.Warning(DContext.CompilerCodeGenerator, "Class (static) constructor of {0} tries to use generic parameter. This will always yield Object.", currentMethod.DeclaringTypeFullName);
                        return new AstExpression(seqp, AstCode.TypeOf, typeSystem.Object) { ExpectedType = typeSystem.Type };
                    }
                    gi = currentMethod.IsStatic ?
                        LoadStaticClassGenericInstance(seqp, typeSystem) :
                        LoadInstanceClassGenericInstance(seqp, typeSystem);
                }
                else
                {
                    // Method type parameter
                    var owner = (XMethodReference)gp.Owner;
                    if (owner.GetElementMethod().Resolve().DeclaringType.HasDexImportAttribute())
                    {
                        // Imported type
                        return LoadTypeForGenericInstance(seqp, currentMethod, type.Module.TypeSystem.Object, typeHelperType, typeSystem, typeConversion);
                    }
                    gi = LoadMethodGenericInstance(seqp, typeSystem);
                }

                var indexExpr = new AstExpression(seqp, AstCode.Ldc_I4, gp.Position) { ExpectedType = typeSystem.Int };
                var loadExpr  = new AstExpression(seqp, AstCode.Ldelem_Ref, null,
                                                        gi, indexExpr);

                loadExpr.ExpectedType = typeSystem.Type;

                if (typeConversion == TypeConversion.EnsureRuntimeType)
                    return EnsureGenericRuntimeType(loadExpr, typeSystem, typeHelperType);
                else
                    return loadExpr;
            }
            
            if (type is XTypeSpecification)
            {
                // Just use the element type
                var typeSpec = (XTypeSpecification)type;
                var git = type as XGenericInstanceType;
                return LoadTypeForGenericInstance(seqp, currentMethod, typeSpec.ElementType, typeHelperType, typeSystem, typeConversion, git);
            }
            
            if (typeConversion == TypeConversion.NullableTypeOf && type.GetElementType().IsNullableT())
            {
                if (typeGenericArguments != null)
                {
                    var underlying = typeGenericArguments.GenericArguments[0];
                    var code = underlying.IsPrimitive ? AstCode.BoxedTypeOf : AstCode.NullableTypeOf;
                    return new AstExpression(seqp, code, underlying) { ExpectedType = typeSystem.Type };
                }
                // should not happen...
                throw new Exception("unable to infer generic arguments: " + currentMethod + ": " + type);
            }

            // Plain type reference or definition
            return new AstExpression(seqp, AstCode.TypeOf, type) { ExpectedType = typeSystem.Type };
        }

        /// <summary>
        /// expand the loadExpression, so that primitive types are converted to their boxed counterparts,
        /// and marker types are converted to their underlying types.
        /// </summary>
        private static AstExpression EnsureGenericRuntimeType(AstExpression loadExpr, XTypeSystem typeSystem, XTypeDefinition typeHelper)
        {
            var ensureMethod = typeHelper.Methods.Single(x => x.Name == "EnsureGenericRuntimeType");
            return new AstExpression(loadExpr.SourceLocation, AstCode.Call, ensureMethod, loadExpr)
                            .SetType(typeSystem.Type);
        }

        /// <summary>
        /// Load the GenericInstance of the current instance.
        /// The result is a temporary register.
        /// </summary>
        private static AstExpression LoadInstanceClassGenericInstance(ISourceLocation seqp, XTypeSystem typeSystem)
        {
            return new AstExpression(seqp, AstCode.LdGenericInstanceField, null) { ExpectedType = new XArrayType(typeSystem.Type) };
        }

        /// <summary>
        /// Load the GenericInstance of the current static method.
        /// The result is a register that cannot be destroyed.
        /// </summary>
        private static AstExpression LoadStaticClassGenericInstance(ISourceLocation seqp, XTypeSystem typeSystem)
        {
            return new AstExpression(seqp, AstCode.LdGenericInstanceTypeArgument, null) { ExpectedType = new XArrayType(typeSystem.Type) };
        }

        /// <summary>
        /// Load the GenericInstance of the current generic method.
        /// The result is a register that cannot be destroyed.
        /// </summary>
        private static AstExpression LoadMethodGenericInstance(ISourceLocation seqp, XTypeSystem typeSystem)
        {
            return new AstExpression(seqp, AstCode.LdGenericInstanceMethodArgument, null) { ExpectedType = new XArrayType(typeSystem.Type) };
        }
    }
}
