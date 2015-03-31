using System.Linq;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Remove all empty switches and gotos to next address.
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
                else if (ins.Code == RCode.Goto)
                {
                    var gotoTarget = (Instruction) ins.Operand;

                    // eliminate chained gotos.
                    while (gotoTarget.Code == RCode.Goto)
                    {
                        gotoTarget = (Instruction) gotoTarget.Operand;
                        ins.Operand = gotoTarget;
                    }

                    // pull return_void (which can not throw an exception)
                    if (gotoTarget.Code == RCode.Return_void)
                    {
                        ins.Code = RCode.Return_void;
                        ins.Operand = null;
                        ins.Registers.Clear();
                    }
                    else if (gotoTarget.Index == i + 1)
                    {
                        ins.ConvertToNop();
                    }
                }
            }
        }
    }
}

