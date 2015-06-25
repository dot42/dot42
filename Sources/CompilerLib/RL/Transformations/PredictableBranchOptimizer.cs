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
            var cfg = new ControlFlowGraph(body);
            return OptimizeBranches(cfg.ToList());
        }

        private bool OptimizeBranches(List<BasicBlock> basicBlocks)
        {
            bool hasChanges = false;

            foreach (var bb in basicBlocks)
            {
                var ins = bb.Exit;
                if (ins.Code.IsComparisonBranch() && ins.Next.Code == RCode.Goto)
                    hasChanges = OptimizeBranchFollowedByGoto(ins, basicBlocks) || hasChanges;

                // Eliminate Predictable branches
                if (ins.Code.IsComparisonBranch() || ins.Code == RCode.Goto)
                {
                    hasChanges = OptimizePredictableBranch(ins, basicBlocks) || hasChanges;
                }
            }
            return hasChanges;
        }

        private static bool OptimizeBranchFollowedByGoto(Instruction ins, List<BasicBlock> basicBlocks)
        {
            // Eliminate Branches immediately followed by a goto
            // If they jump just after the goto, and we are the only
            // instruction reaching the goto.
            
            var branchTarget = (Instruction) ins.Operand;
            if (branchTarget != ins.Next.NextOrDefault) 
                return false;

            var gotoBlock = basicBlocks.Single(b => b.Entry == ins.Next);

            // if there are other instructions reaching the goto, don't do anything.
            if (gotoBlock.EntryBlocks.Count() != 1) 
                return false;

            // invert the comparison and eliminate the goto
            ins.Code = ReverseComparison(ins.Code);
            ins.Operand = ins.Next.Operand;
            ins.Next.ConvertToNop();
            return true;
        }

        /// <summary>
        /// This will eliminate or shortcut: 
        ///     (1) zero-comparisons to a just set const
        ///     (2) branch to a branch when the second branch zero-compares to a 
        ///         just set const before the first branch.
        //      (3) branch to a const immediatly followed by a zero-comparison
        //          to that same const.
        /// </summary>
        private bool OptimizePredictableBranch(Instruction ins, List<BasicBlock> blocks)
        {
            // (1) zero-comparisons to a just set const
            if (ins.Index > 0 && IsComparisonWithZero(ins.Code) 
                && ins.Previous.Code == RCode.Const && IsSameRegister(ins.Registers, ins.Previous.Registers))
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

            // (2)  branch to a branch when the second branch zero-compares to a 
            //      just set const before the first branch.
            var target = (Instruction)ins.Operand;
            if (ins.Index > 0 && IsComparisonWithZero(target.Code) 
                && ins.Previous.Code == RCode.Const && IsSameRegister(target.Registers, ins.Previous.Registers))
            {
                if (WillTakeBranch(target.Code, Convert.ToInt32(ins.Previous.Operand)))
                    ins.Operand = target.Operand;
                else
                    ins.Operand = target.Next;
                return true;
            }

            // (3) branch to a const immediatly followed by a zero-comparison to that same const.
            //     We can only shortcut if the register is not read again after the branch.
            if (target.Code == RCode.Const && IsComparisonWithZero(target.Next.Code)
                && IsSameRegister(target.Registers, target.Next.Registers))
            {
                var secondBranch = target.Next;
                var secondBlock = blocks.First(b => b.Exit == secondBranch);

                var visited = new HashSet<BasicBlock>{ secondBlock }; // we know the second block contains only two instructions. 
                if (!IsRegisterReadAgain(target.Registers[0], secondBlock.ExitBlocks, visited))
                {
                    if (WillTakeBranch(target.Next.Code, Convert.ToInt32(target.Operand)))
                        ins.Operand = target.Next.Operand;
                    else
                        ins.Operand = target.Next.Next;
                    return true;
                }
            }

            return false;
        }

        private bool IsRegisterReadAgain(Register register, IEnumerable<BasicBlock> blocks, HashSet<BasicBlock> visited)
        {
            foreach (var block in blocks)
            {
                if(visited.Contains(block))
                    continue;
                visited.Add(block);

                foreach (var ins in block.Instructions)
                {
                    if (register.IsSourceIn(ins))
                        return true;
                }
                if (IsRegisterReadAgain(register, block.ExitBlocks, visited))
                    return true;
            }
            return false;
        }

        private bool IsSameRegister(RegisterList r1, RegisterList r2)
        {
            return r1.Count == 1 && r2.Count == 1 && r1[0].Index == r2[0].Index;
        }

        private static RCode ReverseComparison(RCode code)
        {
            switch (code)
            {
                case RCode.If_eqz:
                    return RCode.If_nez;
                case RCode.If_nez:
                    return RCode.If_eqz;
                case RCode.If_gez:
                    return RCode.If_ltz;
                case RCode.If_gtz:
                    return RCode.If_lez;
                case RCode.If_lez:
                    return RCode.If_gtz;
                case RCode.If_ltz:
                    return RCode.If_gez;
                case RCode.If_eq:
                    return RCode.If_ne;
                case RCode.If_ne:
                    return RCode.If_eq;
                case RCode.If_ge:
                    return RCode.If_lt;
                case RCode.If_gt:
                    return RCode.If_le;
                case RCode.If_le:
                    return RCode.If_gt;
                case RCode.If_lt:
                    return RCode.If_ge;
                default:
                    throw new InvalidOperationException();
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
