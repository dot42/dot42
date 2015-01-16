using Dot42.JvmClassLib.Attributes;
using Dot42.JvmClassLib.Bytecode;

namespace Dot42.CompilerLib.Java2Ast
{
    internal class SourceLocation : ISourceLocation
    {
        private readonly CodeAttribute code;
        private readonly Instruction instruction;

        /// <summary>
        /// Default ctor
        /// </summary>
        public SourceLocation(CodeAttribute code, Instruction instruction)
        {
            this.code = code;
            this.instruction = instruction;
        }

        public string Document
        {
            get { return code.SourceFile ?? "?"; }
        }
        public int StartLine
        {
            get { return 0; }
        }
        public int StartColumn { get { return 0; } }
        public int EndLine { get { return StartLine; } }
        public int EndColumn { get { return int.MaxValue; } }
        public bool IsSpecial { get { return (StartLine < 0); } }
    }
}
