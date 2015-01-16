using System;
using System.Collections.Generic;
using System.Text;

namespace Dot42.DexLib.Instructions
{
    public class DebugInstruction
    {
        private readonly DebugOpCodes opCode;
        private readonly List<object> operands;

        public DebugInstruction(DebugOpCodes opCode, params object[] operands)
            : this(opCode, (IEnumerable<object>)operands)
        {
        }

        public DebugInstruction(DebugOpCodes opCode, IEnumerable<object> operands)
        {
            this.opCode = opCode;
            this.operands = new List<object>(operands);
        }

        public DebugOpCodes OpCode
        {
            get { return opCode; }
        }

        public List<object> Operands
        {
            get { return operands; }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var opcodeNum = (int) OpCode;
            if ((opcodeNum >= 0x0A) && (opcodeNum <= 0xFF))
            {
                builder.AppendFormat("0x{0:X2} ", opcodeNum);
                var adjustedOpcode = opcodeNum - 0x0A;
                var line = -4 + (adjustedOpcode % 15);
                var address = (adjustedOpcode / 15);
                builder.AppendFormat("line += {0}, address += {1}", line, address);
            }
            else
            {
                builder.Append(OpCode.ToString());
                for (int i = 0; i < Operands.Count; i++)
                {
                    builder.Append(" ");
                    builder.Append(Operands[i]);
                }
            }
            return builder.ToString();
        }

        public void UpdateLineAndAddress(ref int lineNumber, ref int address)
        {
            var opcodeNum = (int)OpCode;
            if ((opcodeNum >= 0x0A) && (opcodeNum <= 0xFF))
            {
                var adjustedOpcode = opcodeNum - 0x0A;
                var lineDelta = -4 + (adjustedOpcode % 15);
                var addressDelta = (adjustedOpcode / 15);

                lineNumber += lineDelta;
                address += addressDelta;
            }
            else
            {
                switch (OpCode)
                {
                    case DebugOpCodes.AdvanceLine:
                        lineNumber += Convert.ToInt32(Operands[0]);
                        break;
                    case DebugOpCodes.AdvancePc:
                        address += Convert.ToInt32(Operands[0]);
                        break;
                }
            }            
        }

        public bool IsLowLevel
        {
            get
            {
                var opcodeNum = (int)OpCode;
                if ((opcodeNum >= 0x0A) && (opcodeNum <= 0xFF)) return true;
                switch (OpCode)
                {
                    case DebugOpCodes.AdvanceLine:
                    case DebugOpCodes.AdvancePc:
                        return true;
                    default:
                        return false;
                }

            }
        }
    }
}