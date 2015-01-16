namespace Dot42.CompilerLib.Java2Ast
{
    /// <summary>
    /// Converts stack-based bytecode to variable-based bytecode by calculating use-define chains
    /// </summary>
    public sealed partial class AstBuilder
    {
        internal enum Category
        {
            Unknown,
            Category1, // 32-bit types
            Category2, // long/double
        }
    }
}
