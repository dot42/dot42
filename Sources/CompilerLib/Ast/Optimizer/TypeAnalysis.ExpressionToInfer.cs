using System.Collections.Generic;

namespace Dot42.CompilerLib.Ast.Optimizer
{
    /// <summary>
    /// Assigns C# types to IL expressions.
    /// </summary>
    /// <remarks>
    /// Types are inferred in a bidirectional manner:
    /// The expected type flows from the outside to the inside, the actual inferred type flows from the inside to the outside.
    /// </remarks>
    partial class TypeAnalysis
    {
        sealed class ExpressionToInfer
        {
            public readonly AstExpression Expression;
            public bool Done;

            /// <summary>
            /// Set for assignment expressions that should wait until the variable type is available
            /// from the context where the variable is used.
            /// </summary>
            public AstVariable DependsOnSingleLoad;

            /// <summary>
            /// The list variables that are read by this expression.
            /// </summary>
            public readonly List<AstVariable> Dependencies = new List<AstVariable>();

            /// <summary>
            /// Default ctor
            /// </summary>
            public ExpressionToInfer(AstExpression expression)
            {
                Expression = expression;
            }

            public override string ToString()
            {
                return (Done ? "[Done] " : "") + Expression;
            }

        }
    }
}
