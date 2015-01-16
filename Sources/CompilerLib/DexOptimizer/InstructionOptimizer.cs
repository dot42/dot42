using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.DexOptimizer
{
    /// <summary>
    /// Replace instructions by shorter formats
    /// </summary>
    internal sealed class InstructionOptimizer : IDexTransformation
    {
        public void Transform(MethodBody body)
        {
            //Debugger.Launch();
            foreach (var ins in body.Instructions)
            {
                switch (ins.OpCode)
                {
                    case OpCodes.Const:
                    case OpCodes.Const_4:
                    case OpCodes.Const_16:
                    case OpCodes.Const_high16:
                        if (IsI4(ins.Operand) && Is4Bits(ins.Registers[0]))
                        {
                            ins.OpCode = OpCodes.Const_4;
                        }
                        else if (IsI16(ins.Operand) && Is8Bits(ins.Registers[0]))
                        {
                            ins.OpCode = OpCodes.Const_16;
                        }
                        else if (IsIHigh16(ins.Operand) && Is8Bits(ins.Registers[0]))
                        {
                            ins.OpCode = OpCodes.Const_high16;
                        }
                        else
                        {
                            ins.OpCode = OpCodes.Const;
                        }
                        break;
                    case OpCodes.Const_wide:
                    case OpCodes.Const_wide_16:
                    case OpCodes.Const_wide_32:
                    case OpCodes.Const_wide_high16:
                        {
                            // Convert int to long
                            if (ins.Operand is int)
                            {
                                ins.Operand = (long) ((int) ins.Operand);
                            }
                        }
                        break;
                    case OpCodes.Move:
                    case OpCodes.Move_16:
                    case OpCodes.Move_from16:
                        if (Is4Bits(ins.Registers[0]) && Is4Bits(ins.Registers[1]))
                        {
                            ins.OpCode = OpCodes.Move;
                        }
                        else if (Is8Bits(ins.Registers[0]))
                        {
                            ins.OpCode = OpCodes.Move_from16;
                        }
                        else
                        {
                            ins.OpCode = OpCodes.Move_16;
                        }
                        break;
                    case OpCodes.Move_wide:
                    case OpCodes.Move_wide_16:
                    case OpCodes.Move_wide_from16:
                        if (Is4Bits(ins.Registers[0]) && Is4Bits(ins.Registers[1]))
                        {
                            ins.OpCode = OpCodes.Move_wide;
                        }
                        else if (Is8Bits(ins.Registers[0]))
                        {
                            ins.OpCode = OpCodes.Move_wide_from16;
                        }
                        else
                        {
                            ins.OpCode = OpCodes.Move_wide_16;                            
                        }
                        break;
                    case OpCodes.Move_object:
                    case OpCodes.Move_object_16:
                    case OpCodes.Move_object_from16:
                        if (Is4Bits(ins.Registers[0]) && Is4Bits(ins.Registers[1]))
                        {
                            ins.OpCode = OpCodes.Move_object;
                        }
                        else if (Is8Bits(ins.Registers[0]))
                        {
                            ins.OpCode = OpCodes.Move_object_from16;
                        }
                        else
                        {
                            ins.OpCode = OpCodes.Move_object_16;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Is the given a signed int that fits in 4 bits?
        /// </summary>
        private static bool IsI4(object value)
        {
            if (!(value is int))
                return false;
            const int limit = 1 << (4 - 1);
            var x = (int)value;
            return (x >= -limit) && (x <= (limit - 1));
        }

        /// <summary>
        /// Is the given a signed int that fits in 16 bits?
        /// </summary>
        private static bool IsI16(object value)
        {
            if (!(value is int))
                return false;
            const int limit = 1 << (16 - 1);
            var x = (int)value;
            return (x >= -limit) && (x <= (limit - 1));
        }

        /// <summary>
        /// Is the given a signed int that fits in bits 32-16 and are the lower 16 bits 0?
        /// </summary>
        private static bool IsIHigh16(object value)
        {
            if (!(value is int))
                return false;
            var x = (int)value;
            return (x  & 0xFFFF) == 0;
        }

        /// <summary>
        /// Does the given register fit in 4 bits?
        /// </summary>
        private static bool Is4Bits(Register r)
        {
            return (r.Index < (1 << 4));
        }

        /// <summary>
        /// Does the given register fit in 8 bits?
        /// </summary>
        private static bool Is8Bits(Register r)
        {
            return (r.Index < (1 << 8));
        }
    }
}
