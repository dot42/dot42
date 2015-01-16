using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Extensions
{
    /// <summary>
    /// AstExpression extensions
    /// </summary>
    public partial class AstExtensions
    {
        /// <summary>
        /// Is the type of the given expression System.Boolean?
        /// </summary>
        public static bool IsBoolean(this AstExpression expression)
        {
            var type = expression.GetResultType();
            return (type != null) && (type.IsBoolean());
        }

        /// <summary>
        /// Is the type of the given expression System.Char?
        /// </summary>
        public static bool IsChar(this AstExpression expression)
        {
            var type = expression.GetResultType();
            return (type != null) && (type.IsChar());
        }

        /// <summary>
        /// Is the type of the given expression System.Int32?
        /// </summary>
        public static bool IsInt32(this AstExpression expression)
        {
            var type = expression.GetResultType();
            return (type != null) && (type.IsInt32());
        }

        /// <summary>
        /// Is the type of the given expression System.Int64?
        /// </summary>
        public static bool IsInt64(this AstExpression expression)
        {
            var type = expression.GetResultType();
            return (type != null) && (type.IsInt64());
        }

        /// <summary>
        /// Is the type of the given expression System.Single?
        /// </summary>
        public static bool IsFloat(this AstExpression expression)
        {
            var type = expression.GetResultType();
            return (type != null) && (type.IsFloat());
        }

        /// <summary>
        /// Is the type of the given expression System.Double?
        /// </summary>
        public static bool IsDouble(this AstExpression expression)
        {
            var type = expression.GetResultType();
            return (type != null) && (type.IsDouble());
        }

        /// <summary>
        /// Is the type of the given expression Int64, UInt64 or Double?
        /// </summary>
        public static bool IsWide(this AstExpression expression)
        {
            var type = expression.GetResultType();
            return (type != null) && (type.IsWide());
        }

        /// <summary>
        /// Gets the type resulting from the given expression.
        /// </summary>
        public static XTypeReference GetResultType(this AstExpression expr)
        {
            var type = expr.InferredType;
            type = type ?? expr.ExpectedType;
            return type ?? expr.Arguments[0].GetResultType();
        }
    }
}
