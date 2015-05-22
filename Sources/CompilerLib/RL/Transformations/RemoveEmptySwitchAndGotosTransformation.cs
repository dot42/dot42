using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Remove all empty switches, gotos to next address, and redirect branch instructions
    /// to a goto.
    /// </summary>
    internal sealed class RemoveEmptySwitchAndGotosTransformation : IRLTransformation
    {
        public void Transform(Dex target, MethodBody body)
        {
#if DEBUG
            //return;
#endif
            var instructions = body.Instructions;

            for(int i = 0 ; i < instructions.Count; ++i)
            {
                var ins = instructions[i];
                if (ins.Code == RCode.Packed_switch)
                {
                    var targets = (Instruction[]) ins.Operand;

                    if (targets.All(p => p.Index == i + 1))
                    {
                        // all targets and default point to next instruction.
                        instructions[i].ConvertToNop();
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
                        if (visited.Contains(ins))
                            break;

                        ins.Operand = gotoTarget;
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
                        }
                        // remove empty gotos
                        else if (gotoTarget.Index == i + 1)
                        {
                            ins.ConvertToNop();
                        }
                    }
                }
            }
        }

        public bool IsSimpleBranch(RCode code)
        {
            switch (code)
            {
                case RCode.Goto:
                //case RCode.Leave: // what is this?
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

