using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    public class PredictableBranchOptimizer : IRLTransformation
    {
        public bool Transform(Dex target, MethodBody body)
        {
            var basicBlocks = BasicBlock.Find(body);
            return OptimizePredictableBranches(basicBlocks);
        }

        private bool OptimizePredictableBranches(List<BasicBlock> basicBlocks)
        {
            bool hasChanges = false;

            foreach (var bb in basicBlocks)
            {
                var instructions = bb.Instructions.ToList();

                for (int i = 1 /*start at one*/; i < instructions.Count; ++i)
                {
                    var ins = instructions[i];

                    if (IsComparisonBranch(ins.Code))
                    {
                        hasChanges = OptimizePredictableBranch(ins) || hasChanges;
                    }
                }
            }
            return hasChanges;
        }

        /// <summary>
        /// This will eliminate: 
        ///     (1) zero-comparisons to a just set const
        ///     (2) chained branches when the second branch zero-compares to a 
        ///         just set const before the first branch.
        /// </summary>
        private bool OptimizePredictableBranch(Instruction ins)
        {
            // (1) zero-comparisons to a just set const
            if (IsComparisonWithZero(ins.Code) && ins.Previous.Code == RCode.Const)
            {
                if (WillTakeBranch(ins.Code, Convert.ToInt32(ins.Previous.Operand)))
                {
                    ins.Registers.Clear();
                    ins.Code = RCode.Goto;
                    return true;
                }
                else
                {
                    ins.ConvertToNop();
                    return true;
                }
            }

            // (2) chained branches 
            var target = (Instruction)ins.Operand;
            if (!IsComparisonWithZero(target.Code))
                return false;
            if (ins.Previous.Code != RCode.Const)
                return false;
            if (target.Registers[0].Index != ins.Previous.Registers[0].Index)
                return false;

            if (WillTakeBranch(target.Code, Convert.ToInt32(ins.Previous.Operand)))
                ins.Operand = target.Operand;
            else
                ins.Operand = target.Next;

            return true;
        }

        public bool IsComparisonBranch(RCode code)
        {
            switch (code)
            {
                case RCode.If_eq:
                case RCode.If_eqz:
                case RCode.If_ge:
                case RCode.If_gez:
                case RCode.If_gt:
                case RCode.If_gtz:
                case RCode.If_le:
                case RCode.If_lez:
                case RCode.If_lt:
                case RCode.If_ltz:
                case RCode.If_ne:
                case RCode.If_nez:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsComparisonWithZero(RCode code)
        {
            switch (code)
            {
                case RCode.If_eqz:
                case RCode.If_gez:
                case RCode.If_gtz:
                case RCode.If_lez:
                case RCode.If_ltz:
                case RCode.If_nez:
                    return true;
                default:
                    return false;
            }
        }

        private bool WillTakeBranch(RCode code, int comparand)
        {
            switch (code)
            {
                case RCode.If_eqz:
                    return comparand == 0;
                case RCode.If_gez:
                    return comparand >= 0;
                case RCode.If_gtz:
                    return comparand > 0;
                case RCode.If_lez:
                    return comparand <= 0;
                case RCode.If_ltz:
                    return comparand < 0;
                case RCode.If_nez:
                    return comparand != 0;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
