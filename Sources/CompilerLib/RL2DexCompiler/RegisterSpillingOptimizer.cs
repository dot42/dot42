using Dot42.CompilerLib.RL2DexCompiler.Extensions;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    /// <summary>
    /// Replace instructions that should be spilled with other instructions to avoid spilling.
    /// </summary>
    internal sealed class RegisterSpillingOptimizer 
    {
        public static void Transform(MethodBody body)
        {
            //Debugger.Launch();
            foreach (var ins in body.Instructions)
            {
                switch (ins.OpCode)
                {
                    case OpCodes.Add_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Add_int);
                        break;
                    case OpCodes.Sub_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Sub_int);
                        break;
                    case OpCodes.Mul_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Mul_int);
                        break;
                    case OpCodes.Div_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Div_int);
                        break;
                    case OpCodes.Rem_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Rem_int);
                        break;
                    case OpCodes.And_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.And_int);
                        break;
                    case OpCodes.Or_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Or_int);
                        break;
                    case OpCodes.Xor_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Xor_int);
                        break;
                    case OpCodes.Shl_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Shl_int);
                        break;
                    case OpCodes.Shr_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Shr_int);
                        break;
                    case OpCodes.Ushr_int_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Ushr_int);
                        break;

                    case OpCodes.Add_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Add_long);
                        break;
                    case OpCodes.Sub_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Sub_long);
                        break;
                    case OpCodes.Mul_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Mul_long);
                        break;
                    case OpCodes.Div_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Div_long);
                        break;
                    case OpCodes.Rem_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Rem_long);
                        break;
                    case OpCodes.And_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.And_long);
                        break;
                    case OpCodes.Or_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Or_long);
                        break;
                    case OpCodes.Xor_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Xor_long);
                        break;
                    case OpCodes.Shl_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Shl_long);
                        break;
                    case OpCodes.Shr_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Shr_long);
                        break;
                    case OpCodes.Ushr_long_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Ushr_long);
                        break;

                    case OpCodes.Add_float_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Add_float);
                        break;
                    case OpCodes.Sub_float_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Sub_float);
                        break;
                    case OpCodes.Mul_float_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Mul_float);
                        break;
                    case OpCodes.Div_float_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Div_float);
                        break;
                    case OpCodes.Rem_float_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Rem_float);
                        break;

                    case OpCodes.Add_double_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Add_double);
                        break;
                    case OpCodes.Sub_double_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Sub_double);
                        break;
                    case OpCodes.Mul_double_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Mul_double);
                        break;
                    case OpCodes.Div_double_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Div_double);
                        break;
                    case OpCodes.Rem_double_2addr:
                        OptimizeBinOp2Addr(ins, OpCodes.Rem_double);
                        break;
                }
            }
        }

        /// <summary>
        /// See if the register of a binop/2addr instruction are both 4 bits.
        /// If not, but they are 8 bits, switch to a binop alternative.
        /// </summary>
        private static void OptimizeBinOp2Addr(Instruction ins, OpCodes binopCode)
        {
            var regA = ins.Registers[0];
            var regB = ins.Registers[1];

            if (regA.IsBits4 && regB.IsBits4)
                return;

            if (!(regA.IsBits8 && regB.IsBits8))
                return;

            // Ok, at least regA or B does not fit in 4-bits, but both fit in 8-bit.
            // Change to binop alternative.
            ins.OpCode = binopCode;
            ins.Registers.Clear();
            ins.Registers.Add(regA);
            ins.Registers.Add(regA);
            ins.Registers.Add(regB);
        }
    }
}
