using System;
using System.Linq;
using Dot42.CompilerLib.XModel;
using Dot42.FrameworkDefinitions;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Add/expand cast / instance of and IsAssignableFrom expressions
    /// - for Arrays
    /// - for IFormattable
    /// </summary>
    internal static class CastConverter
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, MethodSource currentMethod, AssemblyCompiler compiler)
        {
            var typeSystem = compiler.Module.TypeSystem;

            foreach (var node in ast.GetSelfAndChildrenRecursive<AstExpression>())
            {
                switch (node.Code)
                {
                    case AstCode.InstanceOf:
                        ConvertInstanceOf(compiler, node, typeSystem);
                        break;
                    case AstCode.Isinst:
                        ConvertIsInst(compiler, node, typeSystem);
                        break;
                    case AstCode.Castclass:
                        ConvertCastclass(compiler, node, typeSystem);
                        break;
                    case AstCode.Call:
                        ConvertAsNativeIFormattable(node, typeSystem);
                        break;
                    case AstCode.Callvirt:
                        ConvertCallvirtIEnumerable(compiler, node, typeSystem);
                        break;
                    // TODO: this might better be handled in RLBuilder.ConvertTypeBeforeStore()
                    case AstCode.Ret:
                        ConvertRetOrStfldOrStsfld(compiler, currentMethod.Method.ReturnType, node, typeSystem);
                        break;
                    // TODO: this appears to be handled in RLBuilder.ConvertTypeBeforeStore(),
                    //       but is not. why?
                    case AstCode.Stfld:
                    case AstCode.Stsfld:
                        ConvertRetOrStfldOrStsfld(compiler, ((XFieldReference)node.Operand).FieldType, node, typeSystem);
                        break;
                }
            }
        }

        /// <summary>
        /// Convert node with code Cast.
        /// </summary>
        private static void ConvertCastclass(AssemblyCompiler compiler, AstExpression node, XTypeSystem typeSystem)
        {
            var type = (XTypeReference) node.Operand;
            if (type.IsSystemArray())
            {
                // Call cast method
                var arrayHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var castToArray = arrayHelper.Methods.First(x => x.Name == "CastToArray");
                var castToArrayExpr = new AstExpression(node.SourceLocation, AstCode.Call, castToArray, node.Arguments).SetType(type);
                node.CopyFrom(castToArrayExpr);
                return;
            }

            string castMethod = null;
            if (type.IsSystemCollectionsIEnumerable() ||
                type.IsSystemCollectionsIEnumerableT())
            {
                castMethod = "CastToEnumerable";
            }
            else if (type.IsSystemCollectionsICollection() ||
                     type.IsSystemCollectionsICollectionT())
            {
                castMethod = "CastToCollection";
            }
            else if (type.IsSystemCollectionsIList() ||
                     type.IsSystemCollectionsIListT())
            {
                castMethod = "CastToList";
            }
            else if (type.IsSystemIFormattable())
            {
                castMethod = "CastToFormattable";
            }

            if (castMethod != null)
            {
                // make sure we don't evaluate the expression twice.
                var tempVar = new AstGeneratedVariable("temp$$", null) { Type = compiler.Module.TypeSystem.Object };
                var storeTempVar = new AstExpression(node.SourceLocation, AstCode.Stloc, tempVar, node.Arguments[0]) { ExpectedType =compiler.Module.TypeSystem.Object} ;
                var loadTempVar = new AstExpression(node.SourceLocation, AstCode.Ldloc, tempVar).SetType(compiler.Module.TypeSystem.Object);

                // Call cast method
                var arrayHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var castToArray = arrayHelper.Methods.First(x => x.Name == castMethod);

                // Call "(x instanceof T) ? (T)x : asMethod(x)"

                // "instanceof x"
                var instanceofExpr = new AstExpression(node.SourceLocation, AstCode.SimpleInstanceOf, type, storeTempVar).SetType(typeSystem.Bool);

                // CastX(x)
                var castXExpr = new AstExpression(node.SourceLocation, AstCode.Call, castToArray, loadTempVar).SetType(typeSystem.Object);

                // T(x)
                var txExpr = new AstExpression(node.SourceLocation, AstCode.SimpleCastclass, type, loadTempVar).SetType(type);

                // Combine
                var conditional = new AstExpression(node.SourceLocation, AstCode.Conditional, type, instanceofExpr, txExpr, castXExpr).SetType(type);

                node.CopyFrom(conditional);
                return;
            }

            // Normal castclass
            node.Code = AstCode.SimpleCastclass;
        }

        /// <summary>
        /// Convert node with code IsInst.
        /// </summary>
        private static void ConvertIsInst(AssemblyCompiler compiler, AstExpression node, XTypeSystem typeSystem)
        {
            var type = (XTypeReference)node.Operand;
            if (type.IsSystemArray())
            {
                // Call ArrayHelper.AsArray
                var arrayHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var asArray = arrayHelper.Methods.First(x => x.Name == "AsArray");
                var asArrayExpr = new AstExpression(node.SourceLocation, AstCode.Call, asArray, node.Arguments).SetType(typeSystem.Bool);
                node.CopyFrom(asArrayExpr);
                return;
            }

            string asMethod = GetCollectionConvertMethodName(type);

            if (asMethod != null)
            {
                asMethod = "As" + asMethod;
            }
            else if (type.IsSystemIFormattable())
            {
                asMethod = "AsFormattable";
            }

            // make sure we don't evaluate the expression twice.
            var tempVar = new AstGeneratedVariable("temp$$", null) { Type = compiler.Module.TypeSystem.Object };
            var storeTempVar = new AstExpression(node.SourceLocation, AstCode.Stloc, tempVar, node.Arguments[0]) { ExpectedType = compiler.Module.TypeSystem.Object };
            var loadTempVar = new AstExpression(node.SourceLocation, AstCode.Ldloc, tempVar).SetType(compiler.Module.TypeSystem.Object);


            if (asMethod != null)
            {
                // Call "(x instanceof T) ? (T)x : asMethod(x)"
                var arrayHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var asArray = arrayHelper.Methods.First(x => x.Name == asMethod);

                // "instanceof x"
                var instanceofExpr = new AstExpression(node.SourceLocation, AstCode.SimpleInstanceOf, type, storeTempVar).SetType(typeSystem.Bool);

                // AsX(x)
                var asXExpr = new AstExpression(node.SourceLocation, AstCode.Call, asArray, loadTempVar).SetType(typeSystem.Object);

                // T(x)
                var txExpr = new AstExpression(node.SourceLocation, AstCode.SimpleCastclass, type, loadTempVar).SetType(type);

                // Combine
                var conditional = new AstExpression(node.SourceLocation, AstCode.Conditional, type, instanceofExpr, txExpr, asXExpr).SetType(type);

                node.CopyFrom(conditional);
                return;
            }

            // Normal "as": Convert to (x instanceof T) ? (T)x : null
            if(!type.IsPrimitive)
            {
                // "instanceof x"
                var instanceofExpr = new AstExpression(node.SourceLocation, AstCode.SimpleInstanceOf, type, storeTempVar).SetType(typeSystem.Bool);

                // T(x)
                var txExpr = new AstExpression(node.SourceLocation, AstCode.SimpleCastclass, type, loadTempVar).SetType(type);

                // null
                var nullExpr = new AstExpression(node.SourceLocation, AstCode.Ldnull, null).SetType(typeSystem.Object);

                // Combine
                var conditional = new AstExpression(node.SourceLocation, AstCode.Conditional, type, instanceofExpr, txExpr, nullExpr).SetType(type);

                node.CopyFrom(conditional);
                return;                
            }
            else
            {
                // treat as "x is T"
                if(!node.ExpectedType.IsBoolean())
                    throw new NotImplementedException(); // can this happen?

                node.Code = AstCode.SimpleInstanceOf;
            }
        }


        /// <summary>
        /// Convert node with code Callvirt. 
        /// 
        /// For arrays: intercepts call to IEnumerable.IEnumerable_GetEnumerator generated 
        /// by foreach statements and swaps them out to System.Array.GetEnumerator.
        /// 
        /// This call will then at a later compilation stage be replaced with the final destination.
        /// </summary>
        private static void ConvertCallvirtIEnumerable(AssemblyCompiler compiler, AstExpression node, XTypeSystem typeSystem)
        {
            var targetMethodRef = ((XMethodReference)node.Operand);
            var targetMethodDefOrRef = targetMethodRef;

            if (targetMethodDefOrRef.DeclaringType.IsSystemCollectionsIEnumerable()
                && targetMethodDefOrRef.Name == "IEnumerable_GetEnumerator"
                && node.Arguments.Count == 1)
            {
                var argument = node.Arguments[0];
                if (!argument.InferredType.IsArray)
                    return;

                // swap the call to System.Array
                var systemArray = compiler.GetDot42InternalType("System", "Array").Resolve();
                var getEnumerator = systemArray.Methods.First(x => x.Name == "GetEnumerator" && !x.IsStatic && x.Parameters.Count == 0);
                node.Operand = getEnumerator;
            }
            else if (targetMethodDefOrRef.DeclaringType.IsSystemCollectionsIEnumerableT()
                  && targetMethodDefOrRef.Name.EndsWith("_GetEnumerator")
                  && node.Arguments.Count == 1)
            {
                var argument = node.Arguments[0];
                if (!argument.InferredType.IsArray)
                    return;

                var elementType = argument.InferredType.ElementType;

                // Use As...Enumerable to convert
                var asEnumerableName = FrameworkReferences.GetAsEnumerableTMethodName(elementType);
                var compilerHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var asEnumerableMethod = compilerHelper.Methods.First(x => x.Name == asEnumerableName);
                
                var call = new AstExpression(node.SourceLocation, AstCode.Call, asEnumerableMethod, argument)
                {
                    InferredType = asEnumerableMethod.ReturnType
                };
                node.Arguments[0] = call;

                argument.ExpectedType = argument.InferredType;
            }
        }

        /// <summary>
        /// Convert  ret or store field node.
        /// 
        /// converts to IEnumerable, ICollection or IList if required.
        /// </summary>
        private static void ConvertRetOrStfldOrStsfld(AssemblyCompiler compiler, XTypeReference targetType, AstExpression node, XTypeSystem typeSystem)
        {
            var argument = node.Arguments.LastOrDefault();
            
            if (argument == null)
                return;

            if (argument.InferredType == null || !argument.InferredType.IsArray)
                return;

            var methodName = GetCollectionConvertMethodName(targetType);
            if (methodName == null) 
                return;
            
            // Call "ret asMethod(x)"
            var arrayHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
            var asArray = arrayHelper.Methods.First(x => x.Name == "As" + methodName);

            // AsX(x)
            var asXExpr = new AstExpression(node.SourceLocation, AstCode.Call, asArray, argument).SetType(typeSystem.Object);

            // replace argument.
            node.Arguments[node.Arguments.Count-1] = asXExpr;
        }

        /// <summary>
        /// Convert node with code InstanceOf.
        /// </summary>
        private static void ConvertInstanceOf(AssemblyCompiler compiler, AstExpression node, XTypeSystem typeSystem)
        {
            var type = (XTypeReference)node.Operand;
            if (type.IsSystemArray()) // "is System.Array"
            {
                // Call ArrayHelper.IsArray
                var arrayHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var isArray = arrayHelper.Methods.First(x => x.Name == "IsArray");
                var isArrayExpr = new AstExpression(node.SourceLocation, AstCode.Call, isArray, node.Arguments).SetType(typeSystem.Bool);
                node.CopyFrom(isArrayExpr);
                return;
            }

            // make sure we don't evaluate the expression twice.
            var tempVar = new AstGeneratedVariable("temp$$", null) { Type = compiler.Module.TypeSystem.Object };
            var storeTempVar = new AstExpression(node.SourceLocation, AstCode.Stloc, tempVar, node.Arguments[0]) { ExpectedType = compiler.Module.TypeSystem.Object };
            var loadTempVar = new AstExpression(node.SourceLocation, AstCode.Ldloc, tempVar).SetType(compiler.Module.TypeSystem.Object);

            if (type.IsSystemCollectionsIEnumerable() ||
                type.IsSystemCollectionsICollection() ||
                type.IsSystemCollectionsIList())
            {
                // Call "(is x) || IsArray(x)"
                var arrayHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var isArray = arrayHelper.Methods.First(x => x.Name == "IsArray" && x.Parameters.Count == 1);

                // "is" 
                var isExpr = new AstExpression(node).SetArguments(storeTempVar).SetCode(AstCode.SimpleInstanceOf);

                // Call IsArray
                var isArrayExpr = new AstExpression(node.SourceLocation, AstCode.Call, isArray, loadTempVar).SetType(typeSystem.Bool);

                // Combined
                var combined = new AstExpression(node.SourceLocation, AstCode.Or, null, isExpr, isArrayExpr).SetType(typeSystem.Bool);
                node.CopyFrom(combined);
                return;
            }

            if (type.IsSystemCollectionsIEnumerableT() ||
                type.IsSystemCollectionsICollectionT() ||
                type.IsSystemCollectionsIListT())
            {
                // TODO: implement InstanceOf with type check for array types.
                // (is that even possible here?)
            }

            if (type.IsSystemIFormattable())
            {
                // Call "(is x) || IsFormattable(x)"
                var formattable = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var isFormattable = formattable.Methods.First(x => x.Name == "IsVirtualFormattable");

                // "is" 
                var isExpr = new AstExpression(node).SetArguments(storeTempVar).SetCode(AstCode.SimpleInstanceOf);

                // Call IsFormattable
                var isFormattableExpr = new AstExpression(node.SourceLocation, AstCode.Call, isFormattable, loadTempVar).SetType(typeSystem.Bool);

                // Combined
                var combined = new AstExpression(node.SourceLocation, AstCode.Or, null, isExpr, isFormattableExpr).SetType(typeSystem.Bool);
                node.CopyFrom(combined);
                return;
            }

            // Normal instanceof
            node.Code = AstCode.SimpleInstanceOf;            
        }

        private static string GetCollectionConvertMethodName(XTypeReference targetType)
        {
            if (targetType.IsSystemCollectionsIEnumerable())
                return "Enumerable";
            if (targetType.IsSystemCollectionsIEnumerableT())
                return "EnumerableOfObject";
            if (targetType.IsSystemCollectionsICollection())
                return "Collection";
            if (targetType.IsSystemCollectionsICollectionT())
                return "CollectionOfObject";
            if (targetType.IsSystemCollectionsIList())
                return "List";
            if (targetType.IsSystemCollectionsIListT())
                return "ListOfObject";

            return null;
        }

        private static void ConvertAsNativeIFormattable(AstExpression node, XTypeSystem typeSystem)
        {
            var method = (XMethodReference)node.Operand;
            var type = method.ReturnType;

            if (method.Name == "AsNativeIFormattable"
                && method.DeclaringType.Name == InternalConstants.CompilerHelperName
                && type.FullName == "System.IFormattable")
            {
                // make sure we don't evaluate the expression twice.
                var tempVar = new AstGeneratedVariable("temp$$", null) { Type = typeSystem.Object };
                var storeTempVar = new AstExpression(node.SourceLocation, AstCode.Stloc, tempVar, node.Arguments[0]) { ExpectedType = typeSystem.Object };
                var loadTempVar = new AstExpression(node.SourceLocation, AstCode.Ldloc, tempVar).SetType(typeSystem.Object);

                // Convert to "(x instanceof T) ? (T)x : null"

                // "instanceof x"
                var instanceofExpr = new AstExpression(node.SourceLocation, AstCode.SimpleInstanceOf, type, storeTempVar).SetType(typeSystem.Bool);
                // T(x)
                var txExpr = new AstExpression(node.SourceLocation, AstCode.SimpleCastclass, type, loadTempVar).SetType(type);
                // null
                var nullExpr = new AstExpression(node.SourceLocation, AstCode.Ldnull, null).SetType(type);
                // Combine
                var conditional = new AstExpression(node.SourceLocation, AstCode.Conditional, type,
                                        instanceofExpr, txExpr, nullExpr).SetType(type);
                node.CopyFrom(conditional);
            }
        }
    }
}
