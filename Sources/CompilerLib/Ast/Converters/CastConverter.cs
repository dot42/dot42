using System.Linq;
using Dot42.CompilerLib.XModel;
using Dot42.FrameworkDefinitions;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Add/expand cast expressions
    /// </summary>
    internal static class CastConverter
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, MethodSource currentMethod, AssemblyCompiler compiler)
        {
            var typeSystem = compiler.Module.TypeSystem;

            // Expand instanceof
            foreach (var node in ast.GetSelfAndChildrenRecursive<AstExpression>(x => x.Code == AstCode.InstanceOf))
            {
                ConvertInstanceOf(compiler, node, typeSystem);
            }

            // Expand isinst
            foreach (var node in ast.GetSelfAndChildrenRecursive<AstExpression>(x => x.Code == AstCode.Isinst))
            {
                ConvertIsInst(compiler, node, typeSystem);
            }

            // Expand Castclass
            foreach (var node in ast.GetSelfAndChildrenRecursive<AstExpression>(x => x.Code == AstCode.Castclass))
            {
                ConvertCastclass(compiler, node, typeSystem);
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
            if (type.IsSystemCollectionsIEnumerable())
            {
                castMethod = "CastToEnumerable";
            }
            else if (type.IsSystemCollectionsICollection())
            {
                castMethod = "CastToCollection";
            }
            else if (type.IsSystemCollectionsIList())
            {
                castMethod = "CastToList";
            }
            else if (type.IsSystemIFormattable())
            {
                castMethod = "CastToFormattable";
            }

            if (castMethod != null)
            {
                // Call cast method
                var arrayHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var castToArray = arrayHelper.Methods.First(x => x.Name == castMethod);

                // Call "(x instanceof T) ? (T)x : asMethod(x)"

                // "instanceof x"
                var instanceofExpr = new AstExpression(node.SourceLocation, AstCode.SimpleInstanceOf, type, node.Arguments[0]).SetType(typeSystem.Bool);

                // CastX(x)
                var castXExpr = new AstExpression(node.SourceLocation, AstCode.Call, castToArray, node.Arguments[0]).SetType(typeSystem.Object);

                // T(x)
                var txExpr = new AstExpression(node.SourceLocation, AstCode.SimpleCastclass, type, node.Arguments[0]).SetType(type);

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

            string asMethod = null;
            if (type.IsSystemCollectionsIEnumerable())
            {
                asMethod = "AsEnumerable";
            }
            else if (type.IsSystemCollectionsICollection())
            {
                asMethod = "AsCollection";
            }
            else if (type.IsSystemCollectionsIList())
            {
                asMethod = "AsList";
            }
            else if (type.IsSystemIFormattable())
            {
                asMethod = "AsFormattable";
            }

            if (asMethod != null)
            {
                // Call "(x instanceof T) ? (T)x : asMethod(x)"
                var arrayHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var asArray = arrayHelper.Methods.First(x => x.Name == asMethod);

                // "instanceof x"
                var instanceofExpr = new AstExpression(node.SourceLocation, AstCode.SimpleInstanceOf, type, node.Arguments[0]).SetType(typeSystem.Bool);

                // AsX(x)
                var asXExpr = new AstExpression(node.SourceLocation, AstCode.Call, asArray, node.Arguments[0]).SetType(typeSystem.Object);

                // T(x)
                var txExpr = new AstExpression(node.SourceLocation, AstCode.SimpleCastclass, type, node.Arguments[0]).SetType(type);

                // Combine
                var conditional = new AstExpression(node.SourceLocation, AstCode.Conditional, type, instanceofExpr, txExpr, asXExpr).SetType(type);

                node.CopyFrom(conditional);
                return;
            }

            // Normal "as": Convert to (x instanceof T) ? T(x) : null
            {
                // "instanceof x"
                var instanceofExpr = new AstExpression(node.SourceLocation, AstCode.SimpleInstanceOf, type, node.Arguments[0]).SetType(typeSystem.Bool);

                // T(x)
                var txExpr = new AstExpression(node.SourceLocation, AstCode.SimpleCastclass, type, node.Arguments[0]).SetType(type);

                // null
                var nullExpr = new AstExpression(node.SourceLocation, AstCode.Ldnull, null).SetType(typeSystem.Object);

                // Combine
                var conditional = new AstExpression(node.SourceLocation, AstCode.Conditional, type, instanceofExpr, txExpr, nullExpr).SetType(type);

                node.CopyFrom(conditional);
                return;                
            }
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

            if (type.IsSystemCollectionsIEnumerable() || type.IsSystemCollectionsICollection() ||
                type.IsSystemCollectionsIList())
            {
                // Call "(is x) || IsArray(x)"
                var arrayHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var isArray = arrayHelper.Methods.First(x => x.Name == "IsArray");

                // "is" 
                var isExpr = new AstExpression(node).SetCode(AstCode.SimpleInstanceOf);

                // Call IsArray
                var isArrayExpr = new AstExpression(node.SourceLocation, AstCode.Call, isArray, node.Arguments).SetType(typeSystem.Bool);

                // Combined
                var combined = new AstExpression(node.SourceLocation, AstCode.Or, null, isExpr, isArrayExpr).SetType(typeSystem.Bool);
                node.CopyFrom(combined);
                return;
            }
            if (type.IsSystemIFormattable())
            {
                // Call "(is x) || IsFormattable(x)"
                var formattable = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var isFormattable = formattable.Methods.First(x => x.Name == "IsVirtualFormattable");

                // "is" 
                var isExpr = new AstExpression(node).SetCode(AstCode.SimpleInstanceOf);

                // Call IsFormattable
                var isFormattableExpr = new AstExpression(node.SourceLocation, AstCode.Call, isFormattable, node.Arguments).SetType(typeSystem.Bool);

                // Combined
                var combined = new AstExpression(node.SourceLocation, AstCode.Or, null, isExpr, isFormattableExpr).SetType(typeSystem.Bool);
                node.CopyFrom(combined);
                return;
            }

            // Normal instanceof
            node.Code = AstCode.SimpleInstanceOf;            
        }
    }
}
