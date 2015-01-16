namespace Dot42.JvmClassLib.Bytecode
{
    public class Instruction
    {
        private readonly Code opcode;
        private readonly int offset;
        private readonly object operand2;

        public Instruction(int offset, Code opcode, object operand, object operand2)
        {
            this.offset = offset;
            this.opcode = opcode;
            Operand = operand;
            this.operand2 = operand2;
        }

        /// <summary>
        /// Byte offset from start of the code
        /// </summary>
        public int Offset
        {
            get { return offset; }
        }

        /// <summary>
        /// Opcode
        /// </summary>
        public Code Opcode
        {
            get { return opcode; }
        }

        /// <summary>
        /// Operand
        /// </summary>
        public object Operand { get; set; }

        /// <summary>
        /// Second operand
        /// </summary>
        public object Operand2
        {
            get { return operand2; }
        }

        /// <summary>
        /// Location of this instruction in source code
        /// </summary>
        public int LineNumber { get; internal set; }
    }
}
