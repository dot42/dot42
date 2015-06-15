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
                    var targets = (Instruction[]) ins.Operand;

                    if (targets.All(p => p.Index == i + 1))
                    {
                        // all targets and default point to next instruction.
                        instructions[i].ConvertToNop();
                        hasChanges = true;
                    }
                }

                // Eliminate chained and empty gotos, pull return_void
                if (IsSimpleBranch(ins.Code))
                {
                    var gotoTarget = (Instruction) ins.Operand;

                    HashSet<Instruction> visited = new HashSet<Instruction> {ins};

                    while (gotoTarget.Code == RCode.Goto)
                    {
                        gotoTarget = (Instruction) gotoTarget.Operand;

                        // this happens e.g. for the generated 'setNextInstruction' instructions.
                        if (visited.Contains(gotoTarget))
                            break;
                        visited.Add(gotoTarget);

                        ins.Operand = gotoTarget;
                        hasChanges = true;
                    }

                    if (ins.Code == RCode.Goto)
                    {
                        // pull return_void (which can not throw an exception)
                        // TODO: check if we would do any harm by pulling return_something as well.
                        if (gotoTarget.Code == RCode.Return_void)
                        {
                            ins.Code = RCode.Return_void;
                            ins.Operand = null;
                            ins.Registers.Clear();
                            hasChanges = true;
                        }
                        // remove empty gotos
                        else if (gotoTarget.Index == i + 1)
                        {
                            ins.ConvertToNop();
                            hasChanges = true;
                        }
                    }
                }
            }
            return hasChanges;
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

