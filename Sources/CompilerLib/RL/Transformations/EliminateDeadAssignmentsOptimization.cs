using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// remove assignments to registers directly before ret instructions, if the 
    /// register is not used as a return value.
    /// 
    /// The main motivation here is to allow elimination of the
    /// "<>t__doFinallyBodies" variable in async methods. Only 
    /// with the elimination of these will the dex2oat verifier accept
    /// lock's in async methods.
    /// </summary>
    internal sealed class EliminateDeadAssignmentsOptimization : IRLTransformation
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

                if (!ins.Code.IsReturn())
                    continue;

                // go backward as long as we encounter assignments or nops.

                for (int pos = i - 1; pos >= 0; --pos)
                {
                    var check = instructions[i - 1];
                    if (check.Code == RCode.Nop)
                        continue;
                    if (!check.Code.IsMove() && !check.Code.IsConst())
                        break;
                    bool allowOptimize = check.Registers.Count > 0 && !check.Registers[0].PreventOptimization;
                    if (!allowOptimize)
                        break;
                    var isAssignmentToReturnValue = ins.Registers.Count > 0 &&
                                                    ins.Registers[0] == check.Registers[0];
                    if (isAssignmentToReturnValue)
                        break;

                    // eliminate the assignment
                    check.ConvertToNop();
                    hasChanges = true;
                }
            }
            return hasChanges;
        }
    }

}

