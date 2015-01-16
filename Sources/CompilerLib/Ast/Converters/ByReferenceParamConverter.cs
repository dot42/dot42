using System.Collections.Generic;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Ast.Optimizer;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert by reference parameters
    /// </summary>
    internal static class ByReferenceParamConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(DecompilerContext context, AstBlock ast, AssemblyCompiler compiler)
        {
            // Convert stobj/stind_ref
            foreach (var node in ast.GetExpressions(x => (x.Arguments.Count == 2) && ((x.Code == AstCode.Stobj) || (x.Code == AstCode.Stind_Ref))))
            {
                var addrNode = node.Arguments[0];
                var valueNode = node.Arguments[1];

                AstVariable variable;
                if ((addrNode.GetResultType().IsByReference) && addrNode.Match(AstCode.Ldloc, out variable))
                {
                    if (variable.IsThis && valueNode.Match(AstCode.DefaultValue))
                    {
                        // Struct init : this()
                        node.CopyFrom(new AstExpression(node.SourceLocation, AstCode.Nop, null));
                    }
                    else if (!variable.IsThis)
                    {
                        // Convert byref type to array type
                        var addrType = (XByReferenceType) addrNode.GetResultType();
                        var arrayType = new XArrayType(addrType.ElementType);
                        addrNode.ExpectedType = arrayType;
                        addrNode.InferredType = arrayType;

                        // Convert to stelem array, index, value
                        var int32Type = compiler.Module.TypeSystem.Int;
                        node.Arguments.Insert(1, new AstExpression(node.SourceLocation, AstCode.Ldc_I4, 0).SetType(int32Type));
                        node.Code = arrayType.ElementType.GetStElemCode();
                    }
                    else
                    {
                        // Convert to stloc
                    }
                }
            }

            // Convert ldobj
            var resetTypes = false;
            var processed = new HashSet<AstExpression>();
            foreach (var pair in ast.GetExpressionPairs())
            {
                var node = pair.Expression;
                if ((node.Arguments.Count == 1) && ((node.Code == AstCode.Ldobj) || (node.Code == AstCode.Ldind_Ref)))
                {
                    var parent = pair.Parent;
                    var useAsValue = true;
                    var isCallArgument = (parent != null) && (parent.Code.IsCall());
                    var isBoxArgument = (parent != null) && (parent.Match(AstCode.Box));
                    if (isCallArgument && (node.Code == AstCode.Ldobj))
                    {
                        if (IsArgByRefOrOut(node, parent))
                        {
                            useAsValue = false;
                        }
                    }

                    if (isBoxArgument)
                    {
                        var boxType = (XTypeReference) parent.Operand;
                        if (!boxType.IsGenericParameter)
                        {
                            useAsValue = false;
                        }
                    }

                    var ldlocNode = node.Arguments[0];
                    processed.Add(ldlocNode);
                    var addrNodeType = ldlocNode.GetResultType();

                    if (ldlocNode.MatchThis())
                    {
                        useAsValue = false;
                    }

                    if (useAsValue)
                    {
                        if ((addrNodeType.IsByReference) && (ldlocNode.Code == AstCode.Ldloc))
                        {
                            // Convert byref type to array type
                            var addrType = (XByReferenceType)ldlocNode.GetResultType();
                            var arrayType = new XArrayType(addrType.ElementType);
                            ldlocNode.ExpectedType = arrayType;
                            ldlocNode.InferredType = arrayType;

                            // Convert to ldelem array, index, value
                            var int32Type = compiler.Module.TypeSystem.Int;
                            node.Arguments.Insert(1, new AstExpression(node.SourceLocation, AstCode.Ldc_I4, 0).SetType(int32Type));
                            node.Code = arrayType.ElementType.GetLdElemCode();
                            node.SetType(arrayType.ElementType);
                            resetTypes = true;
                        }
                    }
                    else if (isCallArgument && (ldlocNode.Code == AstCode.Ldloc))
                    {
                        var typeRef = (XTypeReference)node.Operand;
                        XTypeDefinition typeDef;
                        if ((typeRef != null) && typeRef.TryResolve(out typeDef) && typeDef.IsValueType &&
                            !typeDef.IsPrimitive)
                        {
                            // Replace by ldloc
                            node.Code = AstCode.Ldloc;
                            node.Operand = ldlocNode.Operand;
                            node.Arguments.Clear();
                        }
                    }
                }
                else if ((node.Code == AstCode.Ldloc) && ((AstVariable)node.Operand).IsParameter && (node.GetResultType().IsByReference))
                {
                    var parent = pair.Parent;
                    if ((parent != null) && (parent.Code == AstCode.Ldobj))
                    {
                        continue;
                    }
                    var useAsValue = true;
                    var isCallArgument = (parent != null) && (parent.Code.IsCall());
                    var isBoxArgument = (parent != null) && (parent.Match(AstCode.Box));
                    if (isCallArgument)
                    {
                        if (IsArgByRefOrOut(node, parent))
                        {
                            useAsValue = false;
                        }
                    }

                    if (isBoxArgument)
                    {
                        useAsValue = false;
                    }

                    if (node.MatchThis())
                    {
                        useAsValue = false;
                    }

                    if (useAsValue)
                    {
                        // Convert byref type to array type
                        var addrType = (XByReferenceType) node.GetResultType();
                        var arrayType = new XArrayType(addrType.ElementType);
                        var clone = new AstExpression(node).SetType(arrayType);

                        // Convert to ldelem array, index, value
                        var int32Type = compiler.Module.TypeSystem.Int;
                        node.SetArguments(clone, new AstExpression(node.SourceLocation, AstCode.Ldc_I4, 0).SetType(int32Type));
                        node.Code = arrayType.ElementType.GetLdElemCode();
                        node.SetType(arrayType.ElementType);
                        resetTypes = true;
                    }
                }
            }

            if (resetTypes)
            {
                TypeAnalysis.Run(context, ast);
            }
        }

        private static bool IsArgByRefOrOut(AstExpression node, AstExpression parent)
        {
            var argIndex = parent.Arguments.IndexOf(node);
            var methodRef = (XMethodReference)parent.Operand;
            XMethodDefinition methodDef;
            if (methodRef.TryResolve(out methodDef))
            {
                if (!methodDef.IsStatic)
                {
                    if (argIndex == 0) // this
                        return false;
                    argIndex--;

                }
                var p = methodDef.Parameters[argIndex];
                return (p.Kind != XParameterKind.Input) || (p.ParameterType.IsByReference);
            }
            return false;
        }
    }
}
