using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// several branching related optimizations
    /// </summary>
    internal sealed class SwitchAndGotoOptimization : IRLTransformation
    {
        public bool Transform(Dex target, MethodBody body)
        {
#if DEBUG
            //return;
#endif
            return DoOptimization(body);
        }

        private bool DoOptimization(MethodBody body)
        {
            bool hasChanges = false;

            var instructions = body.Instructions;

            for (int i = 0; i < instructions.Count; ++i)
            {
                var ins = instructions[i];

                if (ins.Registers.Count > 0 && ins.Registers[0].PreventOptimization)
                    continue;

                if (ins.Code == RCode.Packed_switch)
                {
                    hasChanges = OptimizePackedSwitch(ins, i) || hasChanges;
                }
                if (ins.Code == RCode.Sparse_switch)
                {
                    hasChanges = OptimizeSparseSwitch(ins, i) || hasChanges;
                }
                if (IsSimpleBranch(ins.Code))
                {
                    hasChanges = OptimizeSimpleBranch(ins, i) || hasChanges;
                }
            }
            return hasChanges;
        }

        /// <summary>
        /// Eliminate chained and empty gotos, pull return_void
        /// </summary>
        /// <param name="ins"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static bool OptimizeSimpleBranch(Instruction ins, int i)
        {
            bool hasChanges = false;
            
            var finalTarget = FindFinalGotoChainTarget(ins, (Instruction) ins.Operand);

            if (finalTarget != (Instruction) ins.Operand && finalTarget != ins)
            {
                ins.Operand = finalTarget;
                hasChanges = true;
            }

            if (ins.Code == RCode.Goto)
            {
                var gotoTarget = (Instruction) ins.Operand;

                if (gotoTarget.Index == i + 1)
                {
                    // remove empty gotos
                    ins.ConvertToNop();
                    hasChanges = true;
                }
                else if (gotoTarget.Code == RCode.Return_void)
                {
                    // pull return_void (which can not throw an exception)
                    // TODO: check if we would do any harm by pulling return_something as well.
                    ins.Code = RCode.Return_void;
                    ins.Operand = null;
                    ins.Registers.Clear();
                    hasChanges = true;
                }
            }

            return hasChanges;
        }

        private static bool OptimizeSparseSwitch(Instruction ins, int i)
        {
            bool hasChanges = false;
            var targets = (Tuple<int, Instruction>[]) ins.Operand;

            // if if we can convert it to a packed switch
            Array.Sort(targets, new PackedSwitchComparer());

            bool repack = false;

            for (int t = 0; t < targets.Length; ++t)
            {
                var finalTarget = FindFinalGotoChainTarget(ins, targets[t].Item2);
                if (finalTarget != targets[t].Item2)
                {
                    targets[t] = Tuple.Create(targets[t].Item1, finalTarget);
                    hasChanges = true;
                }

                if (targets[t].Item2.Index == i + 1)
                {
                    targets[t] = null;
                    repack = true;
                    hasChanges = true;
                }
            }

            if (repack)
            {
                ins.Operand = targets = targets.Where(p => p != null).ToArray();
            }

            if (targets.Length == 0)
            {
                ins.ConvertToNop();
                return true;
            }
            
            // check if we can convert it to a packet switch.
            int first = targets[0].Item1;

            if (first != 0)
                return hasChanges; // the RL format only supports zero-based packed switch at the moment...

            for (int idx = 1; idx < targets.Length; ++idx)
            {
                if(targets[idx].Item1 != first + idx)
                    return hasChanges;
            }

            ins.Code = RCode.Packed_switch;
            ins.Operand = targets.Select(p => p.Item2).ToArray();

            return true;
        }

        private static bool OptimizePackedSwitch(Instruction ins, int i)
        {
            bool hasChanges = false;

            var targets = (Instruction[]) ins.Operand;

            for (int t = 0; t < targets.Length; ++t)
            {
                var finalTarget = FindFinalGotoChainTarget(ins, targets[t]);
                if (finalTarget != targets[t])
                {
                    targets[t] = finalTarget;
                    hasChanges = true;
                }
            }

            if (targets.All(p => p.Index == i + 1))
            {
                // all targets and default point to next instruction.
                ins.ConvertToNop();
                hasChanges = true;
            }
            return hasChanges;
        }


        private static Instruction FindFinalGotoChainTarget(Instruction branchInstruction, Instruction branchTarget)
        {
            HashSet<Instruction> visited = new HashSet<Instruction> { branchInstruction };

            while (branchTarget.Code == RCode.Goto)
            {
                branchTarget = (Instruction)branchTarget.Operand;

                // this happens e.g. for the generated 'setNextInstruction' instructions.
                if (visited.Contains(branchTarget))
                    break;
                visited.Add(branchTarget);

            }
            return branchTarget;
        }

        public bool IsSimpleBranch(RCode code)
        {
            switch (code)
            {
                case RCode.Goto:
                case RCode.Leave:
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
    }

    internal class PackedSwitchComparer : IComparer<Tuple<int, Instruction>>
    {
        public int Compare(Tuple<int, Instruction> x, Tuple<int, Instruction> y)
        {
            return Comparer<int>.Default.Compare(x.Item1, y.Item1);
        }
    }
}

