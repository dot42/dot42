using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Extensions
{
    public partial class AstExtensions
    {
        /// <summary>
        /// Is the given catch block a catch all?
        /// </summary>
        public static bool IsCatchAll(this AstTryCatchBlock.CatchBlock catchBlock)
        {
            return catchBlock.ExceptionType.IsSystemObject();
        }
    }
}
