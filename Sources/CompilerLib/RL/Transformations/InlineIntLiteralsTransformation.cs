using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;
using java.io;

namespace Dot42.CompilerLib.RL.Transformations
{
    class InlineIntLiteralsTransformation : IRLTransformation
    {
        public bool Transform(Dex target, MethodBody body)
        {
            bool hasChanges = false;

            var registerUsage = new RegisterUsageMap2(body);

            hasChanges = InlineIntConstsIntoBinOp(registerUsage);

            return hasChanges;
        }

        private bool InlineIntConstsIntoBinOp(RegisterUsageMap2 registerUsage)
        {
            bool hasChanges = false;

            foreach (var usage in registerUsage.BasicUsages)
            {
                if (usage.Register.Type != RType.Value)
                    continue;
                if (usage.Writes.Count != 1 || usage.Reads.Count != 1)
                    continue;

                var write = usage.Writes[0];
                var binOp = usage.Reads[0];

                if (write.Block != binOp.Block)
                    continue;

                var writeIns = write.Instruction;
                if (writeIns.Code != RCode.Const)
                    continue;

                int minValue, maxValue;
                // test next instruction
                if (!IsOptimizableBinOp(binOp.Instruction.Code, out minValue, out maxValue))
                    continue;

                // check range
                int value = Convert.ToInt32(writeIns.Operand);
                if (value < minValue || value > maxValue)
                    continue;

                var constReg = writeIns.Registers[0];
                var binOpIns = binOp.Instruction;
                if (binOpIns.Registers.Last() != constReg)
                    continue;

                // we found: a short const followed by a int-binary operation
                //           in the same block. The only use of the const 
                //           is as the last register of the binary operation.
                bool fits8Bits = value >= sbyte.MinValue && value <= sbyte.MaxValue;
                binOpIns.Code = ToIntLit(binOpIns.Code, fits8Bits);

                var r0 = binOpIns.Registers[0];
                var r1 = binOpIns.Registers.Count == 2 ? binOpIns.Registers[0] : binOpIns.Registers[1];

                binOpIns.Registers.Clear();
                binOpIns.Registers.Add(r0);
                binOpIns.Registers.Add(r1);
                binOpIns.Operand = value;

                usage.Clear();
                writeIns.ConvertToNop();

                hasChanges = true;
            }

            return hasChanges;
        }



        private bool IsOptimizableBinOp(RCode code, out int minValue, out int maxValue)
        {
            switch (code)
            {
                case RCode.Add_int:
                case RCode.Add_int_2addr:
                case RCode.Mul_int:
                case RCode.Mul_int_2addr:
                case RCode.Div_int:
                case RCode.Div_int_2addr:
                case RCode.Rem_int:
                case RCode.Rem_int_2addr:
                case RCode.And_int:
                case RCode.And_int_2addr:
                case RCode.Or_int:
                case RCode.Or_int_2addr:
                case RCode.Xor_int:
                case RCode.Xor_int_2addr:
                    minValue = short.MinValue;
                    maxValue = short.MaxValue;
                    return true;
                case RCode.Shl_int:
                case RCode.Shl_int_2addr:
                case RCode.Shr_int:
                case RCode.Shr_int_2addr:
                case RCode.Ushr_int:
                case RCode.Ushr_int_2addr:
                    minValue = sbyte.MinValue;
                    maxValue = sbyte.MaxValue;
                    return true;
                default:
                    minValue = int.MaxValue;
                    maxValue = int.MinValue;
                    return false;
            }
        }
        private RCode ToIntLit(RCode code, bool use8BitLit)
        {
            switch (code)
            {
                case RCode.Add_int:
                case RCode.Add_int_2addr:
                    return use8BitLit ? RCode.Add_int_lit8 : RCode.Add_int_lit;
                case RCode.Mul_int:
                case RCode.Mul_int_2addr:
                    return use8BitLit ? RCode.Mul_int_lit8 : RCode.Mul_int_lit;
                case RCode.Div_int:
                case RCode.Div_int_2addr:
                    return use8BitLit ? RCode.Div_int_lit8 : RCode.Div_int_lit;
                case RCode.Rem_int:
                case RCode.Rem_int_2addr:
                    return use8BitLit ? RCode.Rem_int_lit8 : RCode.Rem_int_lit;
                case RCode.And_int:
                case RCode.And_int_2addr:
                    return use8BitLit ? RCode.And_int_lit8 : RCode.And_int_lit;
                case RCode.Or_int:
                case RCode.Or_int_2addr:
                    return use8BitLit ? RCode.Or_int_lit8 : RCode.Or_int_lit;
                case RCode.Xor_int:
                case RCode.Xor_int_2addr:
                    return use8BitLit ? RCode.Xor_int_lit8 : RCode.Xor_int_lit;

                case RCode.Shl_int:
                case RCode.Shl_int_2addr:
                    return RCode.Shl_int_lit8;
                case RCode.Shr_int:
                case RCode.Shr_int_2addr:
                    return RCode.Shr_int_lit8;
                case RCode.Ushr_int:
                case RCode.Ushr_int_2addr:
                    return RCode.Ushr_int_lit8;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
