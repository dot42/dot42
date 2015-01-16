using System.Collections.Generic;
using System.Text;

namespace Dot42.DexLib.Instructions
{
    public class Instruction 
    {
        public Instruction()
        {
            Registers = new List<Register>();
        }

        public Instruction(OpCodes opcode, params Register[] registers)
            : this(opcode, null, registers)
        {
        }

        public Instruction(OpCodes opcode)
            : this(opcode, null, (Register[])null)
        {
        }

        public Instruction(OpCodes opcode, object operand)
            : this(opcode, operand, null)
        {
        }

        public Instruction(OpCodes opcode, object operand, params Register[] registers) : this()
        {
            OpCode = opcode;
            Operand = operand;

            if (registers != null)
                Registers = new List<Register>(registers);
        }

        public OpCodes OpCode { get; set; }
        public int Offset { get; set; }
        public List<Register> Registers { get; set; }
        public object Operand { get; set; }

        /// <summary>
        /// Source code reference
        /// </summary>
        public object SequencePoint { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(OpCodesNames.GetName(OpCode));
            for (int i = 0; i < Registers.Count; i++)
            {
                var r = Registers[i];
                builder.AppendFormat(" {0}", r);
            }
            builder.Append(" ");
            if (Operand is Instruction)
            {
                builder.AppendFormat("-> D_{0:X4}", (Operand as Instruction).Offset);
            }
            else if (Operand is string)
            {
                builder.Append(string.Concat("\"", Operand, "\""));
            }
            else if (Operand == null)
            {
                builder.Append("<null>");
            }
            else
            {
                builder.AppendFormat("[{0}] {1}", Operand.GetType().Name, Operand);
            }

            return builder.ToString();
        }
    }
}