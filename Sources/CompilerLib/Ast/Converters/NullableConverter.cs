using System;
using System.Linq;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert call(Nullable.ctor, ldloca|ldflda|lfsflda, value)
    /// </summary>
    internal static class NullableConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, AssemblyCompiler compiler)
        {
            foreach (var node in ast.GetExpressions(AstCode.Call))
            {
                var method = node.Operand as XMethodReference;
                if ((method != null) && method.DeclaringType.GetElementType().IsNullableT())
                {
                    var git = (XGenericInstanceType)method.DeclaringType;
                    var type = git.GenericArguments[0];
                    if ((node.Arguments.Count == 2) && (method.Name == ".ctor"))
                    {
                        var target = node.Arguments[0];
                        var value = node.Arguments[1];

                        if (type.IsPrimitive)
                        {
                            switch (target.Code)
                            {
                                case AstCode.Ldloca:
                                case AstCode.Ldflda:
                                case AstCode.Ldsflda:
                                    ConvertPrimitiveCtor(node, method, type, target, value);
                                    break;
                            }
                        }
                        else 
                        {
                            switch (target.Code)
                            {
                                case AstCode.Ldloca:
                                case AstCode.Ldflda:
                                case AstCode.Ldsflda:
                                    ConvertOtherCtor(node, type, target, value);
                                    break;
                            }                            
                        }
                    }
                    else if ((node.Arguments.Count == 1) && (method.Name == "get_HasValue"))
                    {
                        var target = node.Arguments[0];
                        switch (target.Code)
                        {
                            case AstCode.Ldloca:
                            case AstCode.Ldflda:
                            case AstCode.Ldsflda:
                            case AstCode.Ldloc:
                            case AstCode.Ldfld:
                            case AstCode.Ldsfld:
                                ConvertHasValue(node, type, target);
                                break;
                        }
                    }
                    else if ((node.Arguments.Count == 1) && (method.Name == "get_Value"))
                    {
                        if (type.IsPrimitive)
                        {
                            var target = node.Arguments[0];
                            switch (target.Code)
                            {
                                case AstCode.Ldloca:
                                case AstCode.Ldflda:
                                case AstCode.Ldsflda:
                                case AstCode.Ldloc:
                                case AstCode.Ldfld:
                                case AstCode.Ldsfld:
                                    ConvertPrimitiveGetValue(node, method, type, target, compiler.Module);
                                    break;
                            }
                        }
                        else 
                        {
                            var target = node.Arguments[0];
                            switch (target.Code)
                            {
                                case AstCode.Ldloca:
                                case AstCode.Ldflda:
                                case AstCode.Ldsflda:
                                case AstCode.Ldloc:
                                case AstCode.Ldfld:
                                case AstCode.Ldsfld:
                                    ConvertOtherGetValue(node, method, type, target, compiler.Module);
                                    break;
                            }
                        }
                    }
                    else if (method.Name == "GetValueOrDefault" && (node.Arguments.Count == 1 || node.Arguments.Count == 2))
                        
                    {
                        var target = node.Arguments[0];
                        switch (target.Code)
                        {
                            case AstCode.Ldloca:
                            case AstCode.Ldflda:
                            case AstCode.Ldsflda:
                            case AstCode.Ldloc:
                            case AstCode.Ldfld:
                            case AstCode.Ldsfld:
                            {
                               ConvertGetValueOrDefault(node, method, type, target, compiler.Module);
                            }
                            break;
                        }
                    }
                    else if ((node.Arguments.Count == 1) && (method.Name == "get_RawValue"))
                    {
                        var target = node.Arguments[0];
                        if (type.IsPrimitive)
                        {
                            switch (target.Code)
                            {
                                case AstCode.Ldloca:
                                case AstCode.Ldflda:
                                case AstCode.Ldsflda:
                                    ConvertPrimitiveGetRawValue(node, type, target);
                                    break;
                            }
                        }
                        else 
                        {
                            switch (target.Code)
                            {
                                case AstCode.Ldloca:
                                case AstCode.Ldflda:
                                case AstCode.Ldsflda:
                                    ConvertOtherGetRawValue(node, type, target);
                                    break;
                            }
                        }
                    }
                    else if (type.IsPrimitive || type.IsEnum())
                    {
                        // Convert ld_a to ld
                        foreach (var target in node.Arguments)
                        {
                            switch (target.Code)
                            {
                                case AstCode.Ldloca:
                                case AstCode.Ldflda:
                                case AstCode.Ldsflda:
                                    ConvertLoad(target);
                                    break;
                            }
                        }
                    }
                }
            }

            foreach (var node in ast.GetExpressions(AstCode.Callvirt))
            {
                var method = node.Operand as XMethodReference;
                if ((method != null) && method.DeclaringType.IsSystemObject()
                    && method.Name == "ToString" && method.Parameters.Count == 0
                    && node.Arguments.Count == 1 && !method.Resolve().IsStatic)
                {
                    var type = node.Arguments[0].InferredType;
                    if (type != null && type.GetElementType().IsNullableT())
                    {
                        // redirect to Nullable.ToStringChecked() implementation
                        node.Operand = compiler.GetDot42InternalType("System", "Nullable").Resolve()
                            .Methods.First(m => m.Name == "ToStringChecked" && m.IsStatic && m.Parameters.Count == 1);
                        node.Code = AstCode.Call;
                    }
                }
            }

            foreach (var node in ast.GetExpressions(AstCode.Newobj))
            {
                var method = node.Operand as XMethodReference;
                if ((method != null) && method.DeclaringType.GetElementType().IsNullableT())
                {
                    var git = (XGenericInstanceType)method.DeclaringType;
                    var type = git.GenericArguments[0];
                    if ((node.Arguments.Count == 1) && (method.Name == ".ctor"))
                    {
                        var value = node.Arguments[0];

                        if (type.IsPrimitive)
                        {
                            ConvertPrimitiveNewObj(node, method, type, value);
                        }
                        else
                        {
                            ConvertOtherNewObj(node, method, type, value);
                        }
                    }                    
                }                
            }

            foreach (var node in ast.GetExpressions(AstCode.TypeOf))
            {
                var typeRef = node.Operand as XTypeReference;
                if (typeRef == null || !typeRef.IsGenericInstance)
                    continue;
                if (!typeRef.GetElementType().IsSystemNullable())
                    continue;
                var git = (XGenericInstanceType)typeRef;

                if (git.GenericArguments.Count != 1) // should not happen, but don't bother.
                    continue;

                var type = git.GenericArguments[0];

                node.Operand = type;
                node.Code = type.IsPrimitive ? AstCode.BoxedTypeOf : AstCode.NullableTypeOf;
            }
        }

        /// <summary>
        /// Convert a nullable ctor into a convert function.
        /// </summary>
        private static void ConvertPrimitiveCtor(AstExpression node, XMethodReference ctor, XTypeReference type, AstExpression target, AstExpression value)
        {
            // Clear node
            node.Arguments.Clear();
            node.InferredType = ctor.DeclaringType;
            node.ExpectedType = ctor.DeclaringType;

            switch (target.Code)
            {
                case AstCode.Ldloca:
                    node.Code = AstCode.Stloc;
                    node.Operand = target.Operand;
                    node.Arguments.Add(new AstExpression(node.SourceLocation, AstCode.Box, type, value));
                    break;

                case AstCode.Ldflda:
                    node.Code = AstCode.Stfld;
                    node.Operand = target.Operand;
                    node.Arguments.Add(new AstExpression(node.SourceLocation, AstCode.Box, type, value));
                    break;

                case AstCode.Ldsflda:
                    node.Code = AstCode.Stsfld;
                    node.Operand = target.Operand;
                    node.Arguments.Add(new AstExpression(node.SourceLocation, AstCode.Box, type, value));
                    break;
            }
        }

        /// <summary>
        /// Convert a nullable ctor into a convert function.
        /// </summary>
        private static void ConvertOtherCtor(AstExpression node, XTypeReference type, AstExpression target, AstExpression value)
        {
            // Clear node
            node.Arguments.Clear();
            node.InferredType = type;
            node.ExpectedType = type;

            switch (target.Code)
            {
                case AstCode.Ldloca:
                    node.Code = AstCode.Stloc;
                    node.Operand = target.Operand;
                    node.Arguments.Add(value);
                    break;

                case AstCode.Ldflda:
                    node.Code = AstCode.Stfld;
                    node.Operand = target.Operand;
                    node.Arguments.Add(value);
                    break;

                case AstCode.Ldsflda:
                    node.Code = AstCode.Stsfld;
                    node.Operand = target.Operand;
                    node.Arguments.Add(value);
                    break;
            }
            ConvertLoadArguments(node, type);
        }

        /// <summary>
        /// Convert a nullable(PrimitiveT) ctor into a convert function.
        /// </summary>
        private static void ConvertPrimitiveNewObj(AstExpression node, XMethodReference ctor, XTypeReference type, AstExpression value)
        {
            // Clear node
            node.Arguments.Clear();
            node.InferredType = ctor.DeclaringType;
            node.ExpectedType = ctor.DeclaringType;

            node.Code = AstCode.Box;
            node.Operand = type;
            node.Arguments.Add(value);
        }

        /// <summary>
        /// Convert a nullable(T) ctor into a convert function.
        /// </summary>
        private static void ConvertOtherNewObj(AstExpression node, XMethodReference ctor, XTypeReference type, AstExpression value)
        {
            // Clear node
            node.CopyFrom(value);
            /*node.Arguments.Clear();
            node.InferredType = ctor.DeclaringType;
            node.ExpectedType = ctor.DeclaringType;

            node.Code = AstCode.Box;
            node.Operand = type;
            node.Arguments.Add(value);*/
        }

        /// <summary>
        /// Convert a nullable .HasValue into.
        /// </summary>
        private static void ConvertHasValue(AstExpression node, XTypeReference type, AstExpression target)
        {
            // Clear node
            var originalArgs = node.Arguments.ToList();
            node.Code = AstCode.CIsNotNull;
            node.Operand = null;
            node.Arguments.Clear();

            AddLoadArgument(node, target, originalArgs[0]);
        }

        /// <summary>
        /// Convert a nullable .Value into.
        /// </summary>
        private static void ConvertPrimitiveGetValue(AstExpression node, XMethodReference ilMethod, XTypeReference type, AstExpression target, XModule data)
        {
            // Clear node
            var originalArgs = node.Arguments.ToList();
            var getValueRef = new XMethodReference.Simple("GetValue", false, ilMethod.ReturnType, ilMethod.DeclaringType, new[] { data.TypeSystem.Object, data.TypeSystem.Bool }, null);
            node.Operand = getValueRef;
            node.Arguments.Clear();
            node.InferredType = type;
            node.ExpectedType = type;

            AddLoadArgument(node, target, originalArgs[0]);
            node.Arguments.Add(new AstExpression(node.SourceLocation, AstCode.Ldc_I4, 0) { InferredType = data.TypeSystem.Bool });
        }

        /// <summary>
        /// Convert a nullable .Value into.
        /// </summary>
        private static void ConvertOtherGetValue(AstExpression node, XMethodReference ilMethod, XTypeReference type, AstExpression target, XModule data)
        {
            // Clear node
            var originalArgs = node.Arguments.ToList();
            var getValueRef = new XMethodReference.Simple("GetValue", false, ilMethod.ReturnType, ilMethod.DeclaringType, new[] { data.TypeSystem.Object, data.TypeSystem.Bool }, null);
            node.Operand = getValueRef;
            node.Arguments.Clear();
            node.InferredType = type;
            node.ExpectedType = type;

            AddLoadArgument(node, target, originalArgs[0]);
            node.Arguments.Add(new AstExpression(node.SourceLocation, AstCode.Ldc_I4, 0) { InferredType = data.TypeSystem.Bool });
        }

        /// <summary>
        /// Convert a nullable .GetValueOrDefault()
        /// </summary>
        private static void ConvertGetValueOrDefault(AstExpression node, XMethodReference ilMethod, XTypeReference type, AstExpression target, XModule data)
        {
            // Clear node
            var originalArgs = node.Arguments.ToList();
            XMethodReference.Simple getValueRef;

            // note: there used to be some special code handling enums and structs
            //       that didn't work and that i (olaf) did not understand. 
            //       TODO: check if my changes have undesired performance and/or
            //       validity implications.

            if (originalArgs.Count == 1 && type.IsPrimitive)
            {
                getValueRef = new XMethodReference.Simple("GetValue", false, ilMethod.ReturnType,
                    ilMethod.DeclaringType, new[] {data.TypeSystem.Object, data.TypeSystem.Bool}, null);
                node.Operand = getValueRef;
                node.Arguments.Clear();
                node.InferredType = type;
                node.ExpectedType = type;

                AddLoadArgument(node, target, originalArgs[0]);
                node.Arguments.Add(new AstExpression(node.SourceLocation, AstCode.Ldc_I4, 1)
                {
                    InferredType = data.TypeSystem.Bool
                });
                return;
            }
            if (originalArgs.Count == 1 && !type.IsEnum())
            {
                // this might not be the most efficient implementation, but honestly
                // i don't know how to delay the creation of a value type.
                var ctor = GetDefaultValueCtor(type.Resolve());
                var newObj = new AstExpression(node.SourceLocation, AstCode.Newobj, ctor)
                                                   .SetType(type);
                originalArgs.Add(newObj);
            }
            else if (originalArgs.Count == 1)
            {
                // for enums we just load the default value before calling GetValueOrDefault(object, T)
                originalArgs.Add(new AstExpression(node.SourceLocation, AstCode.DefaultValue, type)
                                                    .SetType(type));
            }
            
            
            getValueRef = new XMethodReference.Simple("GetValueOrDefault", false, ilMethod.ReturnType,
                                    ilMethod.DeclaringType, new[] { data.TypeSystem.Object, ilMethod.ReturnType }, null);
            node.Operand = getValueRef;
            node.Arguments.Clear();
            node.InferredType = type;
            node.ExpectedType = type;

            AddLoadArgument(node, target, originalArgs[0]);
            node.Arguments.Add(originalArgs[1]);
        }

        /// <summary>
        /// Convert a nullable .RawValue into.
        /// </summary>
        private static void ConvertPrimitiveGetRawValue(AstExpression node, XTypeReference type, AstExpression target)
        {
            // Clear node
            var originalArgs = node.Arguments.ToList();
            node.Code = AstCode.Unbox;
            node.Operand = type;
            node.Arguments.Clear();
            node.InferredType = type;
            node.ExpectedType = type;

            AddLoadArgument(node, target, originalArgs[0]);
        }

        /// <summary>
        /// Convert a nullable .RawValue into.
        /// </summary>
        private static void ConvertOtherGetRawValue(AstExpression node, XTypeReference type, AstExpression target)
        {
            var valueArg = node.Arguments[0];
            CopyEnumValueOf(node, type, valueArg);
        }

        /// <summary>
        /// Convert the given target expression to an argument added to the given node.
        /// </summary>
        private static void AddLoadArgument(AstExpression node, AstExpression target, AstExpression originalLoad)
        {
            switch (target.Code)
            {
                case AstCode.Ldloc:
                case AstCode.Ldloca:
                    var variable = (AstVariable)target.Operand;
                    node.Arguments.Add(new AstExpression(node.SourceLocation, AstCode.Ldloc, variable) { InferredType = variable.Type });
                    break;

                case AstCode.Ldfld:
                case AstCode.Ldflda:
                    {
                        var field = (XFieldReference)target.Operand;
                        node.Arguments.Add(new AstExpression(node.SourceLocation, AstCode.Ldfld, field, originalLoad.Arguments[0]) { InferredType = field.FieldType });
                    }
                    break;

                case AstCode.Ldsfld:
                case AstCode.Ldsflda:
                    {
                        var field = (XFieldReference)target.Operand;
                        node.Arguments.Add(new AstExpression(node.SourceLocation, AstCode.Ldsfld, target.Operand) { InferredType = field.FieldType });
                    }
                    break;
            }
        }

        /// <summary>
        /// Convert the load opcode of the arguments of the given node.
        /// </summary>
        private static void ConvertLoadArguments(AstExpression node, XTypeReference type)
        {
            foreach (var target in node.Arguments)
            {
                switch (target.Code)
                {
                    case AstCode.Ldloca:
                    case AstCode.Ldflda:
                    case AstCode.Ldsflda:
                        ConvertLoad(target);
                        break;
                }
            }
        }

        /// <summary>
        /// Convert load address instruction in a nullable .Foo into a load object.
        /// </summary>
        private static void ConvertLoad(AstExpression target)
        {
            switch (target.Code)
            {
                case AstCode.Ldloca:
                    target.Code = AstCode.Ldloc;
                    break;

                case AstCode.Ldflda:
                    target.Code = AstCode.Ldfld;
                    break;

                case AstCode.Ldsflda:
                    target.Code = AstCode.Ldsfld;
                    break;
            }
        }

        /// <summary>
        /// Clone the properties of valueArg into node.
        /// </summary>
        private static void CopyEnumValueOf(AstExpression node, XTypeReference type, AstExpression valueArg)
        {
            // Clear node
            node.Arguments.Clear();
            node.InferredType = type;
            node.ExpectedType = type;
            switch (valueArg.Code)
            {
                case AstCode.Ldloca:
                    node.Code = AstCode.Ldloc;
                    break;
                case AstCode.Ldflda:
                    node.Code = AstCode.Ldfld;
                    break;
                case AstCode.Ldsflda:
                    node.Code = AstCode.Ldsfld;
                    break;
                default:
                    node.Code = valueArg.Code;
                    break;
            }
            node.Operand = valueArg.Operand;
            node.Arguments.AddRange(valueArg.Arguments);
            ConvertLoadArguments(node, type);
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
    }
}
