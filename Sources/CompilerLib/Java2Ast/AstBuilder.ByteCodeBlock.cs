namespace Dot42.CompilerLib.Java2Ast
{
    /// <summary>
    /// Converts stack-based bytecode to variable-based bytecode by calculating use-define chains
    /// </summary>
    public sealed partial class AstBuilder
    {
        /// <summary>
        /// Block of bytecodes
        /// </summary>
        internal sealed class ByteCodeBlock
        {
            private readonly ByteCode first;
            private readonly ByteCode last;

            public ByteCodeBlock(ByteCode first, ByteCode last)
            {
                this.first = first;
                this.last = last;
            }

            /// <summary>
            /// First bytecode in this block
            /// </summary>
            public ByteCode First
            {
                get { return first; }
            }

            /// <summary>
            /// Last bytecode in this block
            /// </summary>
            public ByteCode Last
            {
                get { return last; }
            }
        }
    }
}