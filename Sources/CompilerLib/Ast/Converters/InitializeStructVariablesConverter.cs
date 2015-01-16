using System.Linq;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Make sure all struct variables are initialized
    /// </summary>
    internal static class InitializeStructVariablesConverter
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstBlock ast)
        {
            var variableExpressions = ast.GetExpressions().Where(x => x.Operand is AstVariable).ToList();
            var allVariables = variableExpressions.Select(x => (AstVariable)x.Operand).Distinct().ToList();
            var allStructVariables = allVariables.Where(x => x.Type.IsStruct()).ToList();

            var index = 0;
            foreach (var iterator in allStructVariables)
            {
                var variable = iterator;
                if (variable.IsThis || variable.IsParameter)
                    continue;
                if (variableExpressions.Any(x => (x.Operand == variable) && (x.Code == AstCode.Stloc)))
                    continue;

                // Get struct type
                var structType = variable.Type.Resolve();
                var defaultCtorDef = StructCallConverter.GetDefaultValueCtor(structType);
                var defaultCtor = variable.Type.CreateReference(defaultCtorDef);

                // Variable is not initialized
                var newObjExpr = new AstExpression(ast.SourceLocation, AstCode.Newobj, defaultCtor).SetType(variable.Type);
                var initExpr = new AstExpression(ast.SourceLocation, AstCode.Stloc, variable, newObjExpr).SetType(variable.Type);
                ast.Body.Insert(index++, initExpr);
            }           
        }
    }
}
