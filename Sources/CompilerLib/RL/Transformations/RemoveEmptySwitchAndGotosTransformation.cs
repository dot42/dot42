using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Remove all empty switches, gotos to next address, and redirect branch instructions
    /// to a goto.
    /// </summary>
    internal sealed class RemoveEmptySwitchAndGotosTransformation : IRLTransformation
    {
        public bool Transform(Dex target, MethodBody body)
        {
#if DEBUG
            //return;
#endif
            if (DoOptimization(body))
            {
                new NopRemoveTransformation().Transform(target, body);
                return true;
            }
            return false;
        }

        private bool DoOptimization(MethodBody body)
        {
            bool hasChanges = false;

            var instructions = body.Instructions;

            for (int i = 0; i < instructions.Count; ++i)
            {
                var ins = instructions[i];
                if (ins.Code == RCode.Packed_switch)
                {
                    hasChanges = OptimizePacketSwitch(ins, i) || hasChanges;
                }
                else if (ins.Code == RCode.Sparse_switch)
                {
                    hasChanges = OptimizeSparseSwitch(ins, i) || hasChanges;
                }
                else if (IsSimpleBranch(ins.Code))
                {
                    hasChanges = OptimizeSimpleBranch(ins, i) || hasChanges;
                }
            }
            return hasChanges;
        }

        private static bool OptimizeSimpleBranch(Instruction ins, int i)
        {
            // Eliminate chained and empty gotos, pull return_void
            bool hasChanges = false;
            
            var finalTarget = FindFinalGotoChainTarget(ins, (Instruction) ins.Operand);

            if (finalTarget != (Instruction) ins.Operand)
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
                ins.Operand = targets.Where(p => p != null).ToArray();
            }

            if (((Tuple<int, Instruction>[]) ins.Operand).Length == 0)
            {
                ins.ConvertToNop();
                hasChanges = true;
            }
            return hasChanges;
        }

        private static bool OptimizePacketSwitch(Instruction ins, int i)
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
}

