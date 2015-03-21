using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.XModel;
using Dot42.FrameworkDefinitions;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert ldc.x (enum)
    /// </summary>
    internal static class EnumConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, AssemblyCompiler compiler)
        {
            foreach (var node in ast.GetExpressions().Reverse())
            {
                if ((node.Code == AstCode.Ldc_I4) || (node.Code == AstCode.Ldc_I8))
                {
                    var typeRef = node.InferredType;
                    XTypeDefinition typeDef;
                    if ((typeRef != null) && typeRef.TryResolve(out typeDef) && typeDef.IsEnum)
                    {
                        // Convert to ld-enum-value
                        XFieldDefinition field;
                        if (typeDef.TryGetEnumConstField(node.Operand, out field))
                        {
                            // Convert to ldsfld
                            node.Code = AstCode.Ldsfld;
                            node.Operand = field;
                            node.InferredType = typeDef;
                            node.ExpectedType = typeDef;
                        }
                        else
                        {
                            // Call Enum.GetValue(enumType, value)
                            var module = compiler.Module;
                            var valueType = (node.Code == AstCode.Ldc_I8)
                                                ? module.TypeSystem.Long
                                                : module.TypeSystem.Int;
                            var value = new AstExpression(node) {InferredType = valueType, ExpectedType = valueType};
                            node.Code = (node.Code == AstCode.Ldc_I8) ? AstCode.Long_to_enum : AstCode.Int_to_enum;
                            node.Operand = null;
                            node.InferredType = typeDef;
                            node.ExpectedType = typeDef;
                            node.Arguments.Clear();
                            node.Arguments.Add(value);
                        }
                    }
                }
                else if (node.Code.IsBinaryOperation())
                {
                    var typeRef = node.InferredType;
                    XTypeDefinition typeDef;
                    if ((typeRef != null) && typeRef.TryResolve(out typeDef) && typeDef.IsEnum)
                    {
                        ConvertBinOp(node, typeDef, compiler);
                        continue; // Avoid recursion
                    }
                }
                else if (node.Code.IsIntegerOnlyCompare())
                {
                    var typeRef = node.Arguments[0].InferredType;
                    XTypeDefinition typeDef;
                    if ((typeRef != null) && typeRef.TryResolve(out typeDef) && typeDef.IsEnum)
                    {
                        ConvertICmpArgument(node, 0, typeDef, compiler);
                    }
                    typeRef = node.Arguments[1].InferredType;
                    if ((typeRef != null) && typeRef.TryResolve(out typeDef) && typeDef.IsEnum)
                    {
                        ConvertICmpArgument(node, 1, typeDef, compiler);
                    }
                }
                else if ((node.Code == AstCode.Switch) || (node.Code == AstCode.LookupSwitch))
                {
                    var typeRef = node.Arguments[0].InferredType;
                    XTypeDefinition typeDef;
                    if ((typeRef != null) && typeRef.TryResolve(out typeDef) && typeDef.IsEnum)
                    {
                        var isWide = typeDef.GetEnumUnderlyingType().IsWide();
                        var typeSystem = compiler.Module.TypeSystem;
                        var enumValue = new AstExpression(node.Arguments[0]);
                        var numericValue = new AstExpression(node.SourceLocation,
                                                             isWide ? AstCode.Enum_to_long : AstCode.Enum_to_int, null);
                        numericValue.InferredType = isWide ? typeSystem.Long : typeSystem.Int;
                        numericValue.Arguments.Add(enumValue);
                        node.Arguments.Clear();
                        node.Arguments.Add(numericValue);
                    }
                }
                else if (node.Code == AstCode.Brfalse)
                {
                    // brfalse(enum-expr) is used as "enum-expr == 0-valued-enum-value"
                    var typeRef = node.Arguments[0].InferredType;
                    XTypeDefinition typeDef;
                    if ((typeRef != null) && typeRef.TryResolve(out typeDef) && typeDef.IsEnum)
                    {
                        var isWide = typeDef.GetEnumUnderlyingType().IsWide();
                        var typeSystem = compiler.Module.TypeSystem;
                        var enumValue = new AstExpression(node.Arguments[0]);
                        var numericValue = new AstExpression(node.SourceLocation,
                                                             isWide ? AstCode.Enum_to_long : AstCode.Enum_to_int, null);
                        numericValue.InferredType = isWide ? typeSystem.Long : typeSystem.Int;
                        numericValue.Arguments.Add(enumValue);
                        node.Arguments.Clear();
                        node.Arguments.Add(numericValue);
                    }
                }

                // Note: no else here
                {
                    // Need to value type?
                    var inferredType = node.InferredType;
                    var expectedType = node.ExpectedType;
                    if ((inferredType != null) && (expectedType != null) && !inferredType.IsSame(expectedType))
                    {
                        // don't convert if either one is the abstract base class.
                        if (inferredType.IsInternalEnum() || expectedType.IsInternalEnum())
                            continue;

                        XTypeDefinition expectedTypeDef;
                        if (expectedType.TryResolve(out expectedTypeDef) && expectedTypeDef.IsEnum)
                        {
                            // Enum expected, non-enum or different enum found
                            var underlyingType = expectedTypeDef.GetEnumUnderlyingType();
                            var isWide = underlyingType.IsWide();

                            // If inferred type is another enum, convert to primitive first
                            if (inferredType.IsEnum())
                            {
                                var toPrimitive = new AstExpression(node) { ExpectedType = null };
                                node.Code = isWide ? AstCode.Enum_to_long : AstCode.Enum_to_int;
                                node.SetArguments(toPrimitive);
                                node.Operand = null;
                                node.SetType(underlyingType);
                            }
                            else if (inferredType.IsPrimitive)
                            {
                                // Convert float/double to int/long before converting to enum
                                if (inferredType.IsDouble() || inferredType.IsFloat())
                                {
                                    var code = isWide ? AstCode.Conv_I8 : AstCode.Conv_I4;
                                    var clone = new AstExpression(node) { ExpectedType = null };
                                    node.Code = code;
                                    node.SetArguments(clone);
                                    node.Operand = null;
                                    node.SetType(underlyingType);                                    
                                }
                            }

                            var actualValue = new AstExpression(node) {ExpectedType = null};
                            node.Code = isWide ? AstCode.Long_to_enum : AstCode.Int_to_enum;
                            node.Operand = null;
                            node.SetArguments(actualValue);
                            node.SetType(expectedTypeDef);
                        }
                        else
                        {
                            XTypeDefinition inferredTypeDef;
                            if (inferredType.TryResolve(out inferredTypeDef) && inferredTypeDef.IsEnum)
                            {
                                // Enum found, non-enum or different enum expected
                                var underlyingType = inferredTypeDef.GetEnumUnderlyingType();
                                var isWide = underlyingType.IsWide();

                                var actualValue = new AstExpression(node) {ExpectedType = null};
                                node.Code = isWide ? AstCode.Enum_to_long : AstCode.Enum_to_int;
                                node.Operand = null;
                                node.SetArguments(actualValue);
                                node.SetType(underlyingType);
                            }
                        }
                    }
                }
            }

            // Convert value of switch node
            foreach (var node in ast.GetSelfAndChildrenRecursive<AstSwitch>())
            {
                var typeRef = node.Condition.InferredType;
                XTypeDefinition typeDef;
                if ((typeRef != null) && typeRef.TryResolve(out typeDef) && typeDef.IsEnum)
                {
                    var isWide = typeDef.GetEnumUnderlyingType().IsWide();
                    var typeSystem = compiler.Module.TypeSystem;
                    var toValue = new AstExpression(node.Condition.SourceLocation, isWide ? AstCode.Enum_to_long : AstCode.Enum_to_int, null);
                    toValue.InferredType = isWide ? typeSystem.Long : typeSystem.Int;
                    toValue.Arguments.Add(node.Condition);
                    node.Condition = toValue;
                }                
            }
        }

        /// <summary>
        /// Convert a binary operation (Add, Sub, Mul, Div, Rem, And, Or, Xor) resulting in an enum.
        /// </summary>
        private static void ConvertBinOp(AstExpression node, XTypeDefinition enumType, AssemblyCompiler compiler)
        {
            Debug.Assert(node.Arguments.Count == 2);
            var isWide = enumType.GetEnumUnderlyingType().IsWide();
            var module = compiler.Module;            
            var retType = isWide ? module.TypeSystem.Long : module.TypeSystem.Int;

            // Convert arguments
            ConvertNumericOpArgument(node, 0, enumType, isWide, retType);
            ConvertNumericOpArgument(node, 1, enumType, isWide, retType);

            // Convert return value
            if ((node.ExpectedType == null) || (!node.ExpectedType.IsPrimitive))
            {
                ConvertNumericToEnum(node, enumType, retType);
            }
        }

        /// <summary>
        /// Convert a integer compare operation (bge, bgt, ble, blt) with enum argument.
        /// </summary>
        private static void ConvertICmpArgument(AstExpression node, int argIndex, XTypeDefinition enumType, AssemblyCompiler compiler)
        {
            Debug.Assert(node.Arguments.Count == 2);
            var isWide = enumType.GetEnumUnderlyingType().IsWide();
            var module = compiler.Module;
            var retType = isWide ? module.TypeSystem.Long : module.TypeSystem.Int;

            // Convert arguments
            ConvertNumericOpArgument(node, argIndex, enumType, isWide, retType);
        }

        /// <summary>
        /// Convert an argument or a numeric operation to int/long.
        /// </summary>
        private static void ConvertNumericOpArgument(AstExpression node, int argumentIndex, XTypeDefinition enumType, bool isWide, XTypeReference enumNumericType)
        {
            var module = enumType.Module;
            var argument = node.Arguments[argumentIndex];
            switch (argument.Code)
            {
                case AstCode.Ldc_I4:
                    // Keep numeric value
                    argument.SetType(module.TypeSystem.Int);
                    break;
                case AstCode.Ldc_I8:
                    // Keep numeric value
                    argument.SetType(module.TypeSystem.Long);
                    break;
                default:
                    // Convert enum to numeric
                    var numericValue = new AstExpression(argument);
                    argument.SetCode(isWide ? AstCode.Enum_to_long : AstCode.Enum_to_int)
                            .SetType(enumNumericType)
                            .SetArguments(numericValue);
                    break;
            }
        }

        /// <summary>
        /// Convert the given node that holds a numeric value and convert it to it's enum instance.
        /// </summary>
        private static void ConvertNumericToEnum(AstExpression node, XTypeDefinition enumType, XTypeReference enumNumericType)
        {
            // Call Enum.GetValue(enumType, value)
            var enumValue = new AstExpression(node);
            node.SetCode(enumNumericType.IsWide() ? AstCode.Long_to_enum : AstCode.Int_to_enum).SetArguments(enumValue).SetType(enumType);
            node.Operand = null;
        }
    }
}
