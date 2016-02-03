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
            var cfg = new ControlFlowGraph2(body);
            return OptimizeBranches(cfg);
        }

        private bool OptimizeBranches(ControlFlowGraph2 cfg)
        {
            bool hasChanges = false;

            foreach (var bb in cfg.BasicBlocks)
            {
                var ins = bb.Exit;

                if (ins.Registers.Count > 0 && ins.Registers[0].PreventOptimization)
                    continue;

                if (IsComparisonToRegister(ins.Code))
                    hasChanges = OptimizeComparisonToConstZero(ins, bb) || hasChanges;

                if (ins.Code.IsComparisonBranch() && ins.Next.Code == RCode.Goto)
                    hasChanges = OptimizeBranchFollowedByGoto(ins, cfg) || hasChanges;

                // Eliminate Predictable branches
                if (ins.Code.IsComparisonBranch() || ins.Code == RCode.Goto)
                {
                    hasChanges = OptimizePredictableBranch(ins, bb, cfg) || hasChanges;
                }
            }
            return hasChanges;
        }

        /// <summary>
        /// replaces comparisons where one operand has just been set to a const zero.
        /// asumes that instruction is a comparison instruction with two registers.
        /// </summary>
        private bool OptimizeComparisonToConstZero(Instruction ins, BasicBlock bb)
        {
            var r1 = ins.Registers[0];
            var r2 = ins.Registers[1];
            
            bool? r1IsZero = null, r2IsZero = null;

            if (r1.Category == RCategory.Argument || r1.PreventOptimization)
                r1IsZero = false;
            if (r2.Category == RCategory.Argument || r2.PreventOptimization)
                r2IsZero = false;

            for (var prev = ins.PreviousOrDefault; prev != null && prev.Index >= bb.Entry.Index; prev=prev.PreviousOrDefault)
            {
                if(r1IsZero.HasValue && r2IsZero.HasValue)
                    break;
                if ((r1IsZero.HasValue && r1IsZero.Value) || (r2IsZero.HasValue && r2IsZero.Value))
                    break;

                if (r1IsZero == null)
                {
                    if (r1.IsDestinationIn(prev))
                    {
                        r1IsZero = prev.Code == RCode.Const && Convert.ToInt32(prev.Operand) == 0;
                        continue;
                    }
                }
                if (r2IsZero == null)
                {
                    if (r2.IsDestinationIn(prev))
                    {
                        r2IsZero = prev.Code == RCode.Const && Convert.ToInt32(prev.Operand) == 0;
                        continue;
                    }
                }
            }

            if (r2IsZero.HasValue && r2IsZero.Value)
            {
                ins.Code = ToComparisonWithZero(ins.Code);
                ins.Registers.Clear();
                ins.Registers.Add(r1);
                return true;
            }

            if (r1IsZero.HasValue && r1IsZero.Value)
            {
                // swap the registers before converting to zero-comparison.
                ins.Code = ToComparisonWithZero(SwapComparisonRegisters(ins.Code));
           

                ins.Registers.Clear();
                ins.Registers.Add(r2);
                return true;
            }
            

            return false;
        }

        private static bool OptimizeBranchFollowedByGoto(Instruction ins, ControlFlowGraph2 cfg)
        {
            // Redirect branches immediately followed by a goto
            // If they jump just after the goto, and we are the only
            // instruction reaching the goto.
            
            var branchTarget = (Instruction) ins.Operand;
            if (branchTarget != ins.Next.NextOrDefault) 
                return false;

            var gotoBlock = cfg.GetBlockFromEntry(ins.Next);

            // if there are other instructions reaching the goto, don't do anything.
            if (gotoBlock.EntryBlocks.Count() != 1) 
                return false;

            // invert the comparison and eliminate the goto
            ins.Code = ReverseComparison(ins.Code);
            cfg.RerouteBranch(ins, (Instruction)ins.Next.Operand);
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
        private bool OptimizePredictableBranch(Instruction ins, BasicBlock block, ControlFlowGraph2 cfg)
        {
            // (1) zero-comparisons to a just set const
            if (ins != block.Entry 
                && IsComparisonWithZero(ins.Code) && ins.Previous.Code == RCode.Const 
                && IsSameRegister(ins.Registers, ins.Previous.Registers))
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
            if (ins != block.Entry 
                && IsComparisonWithZero(target.Code) && ins.Previous.Code == RCode.Const 
                && IsSameRegister(target.Registers, ins.Previous.Registers))
            {
                if (WillTakeBranch(target.Code, Convert.ToInt32(ins.Previous.Operand)))
                    cfg.RerouteBranch(ins, (Instruction)target.Operand);
                else
                    cfg.RerouteBranch(ins, target.Next);
                return true;
            }

            // (3) branch to a const immediatly followed by a zero-comparison to that same const.
            //     We can only shortcut if the register is not read again after the branch.
            if (target.Code == RCode.Const && IsComparisonWithZero(target.Next.Code)
                && IsSameRegister(target.Registers, target.Next.Registers))
            {
                var secondBranch = target.Next;
                var secondBlock = cfg.GetBlockFromExit(secondBranch);

                var visited = new HashSet<BasicBlock> {secondBlock};
                    // we know the second block contains only two instructions. 
                if (!IsRegisterReadAgain(target.Registers[0], secondBlock.ExitBlocks, visited))
                {
                    if (WillTakeBranch(secondBranch.Code, Convert.ToInt32(target.Operand)))
                        cfg.RerouteBranch(ins, (Instruction)target.Next.Operand);
                    else
                        cfg.RerouteBranch(ins, target.Next.Next);
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
                    if (register.IsDestinationIn(ins))
                        goto nextBlock;
                }
                if (IsRegisterReadAgain(register, block.ExitBlocks, visited))
                    return true;
            nextBlock:;
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

        private static RCode SwapComparisonRegisters(RCode code)
        {
            switch (code)
            {
                case RCode.If_eq:
                    return RCode.If_eq;
                case RCode.If_ne:
                    return RCode.If_ne;
                case RCode.If_ge:
                    return RCode.If_le;
                case RCode.If_gt:
                    return RCode.If_lt;
                case RCode.If_le:
                    return RCode.If_ge;
                case RCode.If_lt:
                    return RCode.If_gt;
                default:
                    throw new InvalidOperationException();
            }
        }

        private RCode ToComparisonWithZero(RCode code)
        {
            switch (code)
            {
                case RCode.If_eq:
                    return RCode.If_eqz;
                case RCode.If_ge:
                    return RCode.If_gez;
                case RCode.If_gt:
                    return RCode.If_gtz;
                case RCode.If_le:
                    return RCode.If_lez;
                case RCode.If_lt:
                    return RCode.If_ltz;
                case RCode.If_ne:
                    return RCode.If_nez;
                default:
                    throw new InvalidOperationException();
            }
        }


        public static bool IsComparisonWithZero(RCode code)
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

        public bool IsComparisonToRegister(RCode code)
        {
            return code.IsComparisonBranch() && !IsComparisonWithZero(code);
        }

        public static bool WillTakeBranch(RCode code, int comparand)
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
