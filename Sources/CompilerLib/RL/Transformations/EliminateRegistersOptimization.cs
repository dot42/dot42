using System;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Eliminate temporary registers are much as possible.
    /// </summary>
    internal class EliminateRegistersOptimization : IRLTransformation
    {
        public bool Transform(Dex target, MethodBody body)
        {
            bool hasChanges = false;

            var graph = new ControlFlowGraph2(body);
            var registerUsage = new RegisterUsageMap2(graph);

            hasChanges = EliminateRegisterAssigments(registerUsage);
            
            return hasChanges;
        }

        /// <summary>
        ///  eliminate registers that
        ///  (1) are only assigned to.
        ///  (2) are assigned exacly once directly followed by the only read, which moves them
        ///      to another register, if in same block.
        ///  (3) are 'const' exacly once and then only used in branch-with-comparison-to-zero operations. 
        ///      we remove the branch and the assignment.
        ///  (4) Are assigned from another register excactly once and not written otherwise, 
        ///      and were the source register is never changed during the usage of that register.
        ///      We also make sure that the other register is never check-casted as this could
        ///      mean we are modifying the dex-visible type.
        /// </summary>
        private static bool EliminateRegisterAssigments(RegisterUsageMap2 registerUsage)
        {
            bool hasChanges = false;

            foreach (var usage in registerUsage.BasicUsages)
            {
                var reg = usage.Register;
                if ((!reg.IsTemp && reg.Category != RCategory.TempVariable) 
                    || reg.KeepWith == RFlags.KeepWithPrev
                    || reg.PreventOptimization)
                    continue;

                // (1) quick check: write only registers.
                if (usage.Reads.Count == 0)
                {
                    foreach (var ins in usage.Writes)
                        ins.Instruction.ConvertToNop();
                    continue;
                }

                // (2) fairly simple.
                if (usage.Writes.Count == 1 && usage.Reads.Count == 1
                    && usage.Writes[0].Block == usage.Reads[0].Block
                    && usage.Writes[0].Instruction.Next == usage.Reads[0].Instruction
                    && usage.Reads[0].Instruction.Code.IsMove())
                {
                    var replUsage = registerUsage.GetBasicUsage(usage.Reads[0].Instruction.Registers[0]);
                    if (replUsage.Register.KeepWith != usage.Register.KeepWith)
                    {
                        // this might happend for Android Extensions methods, were registers are 
                        // incorrectly allocated. These allocations seem to be difficult to fix.
                        // Best would be to give up the concept of two connected registers in RL 
                        // altogether, and only introduce that complexity during dex conversion.
                        continue;
                    }
                    registerUsage.ReplaceRegister(usage, replUsage);
                    continue;
                }

                // (3) still simple.
                if (usage.Writes.Count == 1 && usage.Writes[0].Instruction.Code.IsConst()
                 && usage.Reads.All(r=>PredictableBranchOptimizer.IsComparisonWithZero(r.Instruction.Code)))
                {
                    var writeInsBlock = usage.Writes[0];
                    int operand = Convert.ToInt32(writeInsBlock.Instruction.Operand);
                    foreach (var insBlock in usage.Reads.ToArray())
                    {
                        usage.Remove(insBlock);

                        var ins = insBlock.Instruction;
                        bool willTakeBranch = PredictableBranchOptimizer.WillTakeBranch(ins.Code, operand);

                        if (willTakeBranch)
                        {
                            ins.Code = RCode.Goto;
                            ins.Registers.Clear();
                        }
                        else
                        {
                            ins.ConvertToNop();
                        }
                        
                    }
                    
                    usage.Remove(writeInsBlock);
                    writeInsBlock.Instruction.ConvertToNop();
                    continue;
                }

                // (4) more complicated.
                if (usage.Writes.Count != 1 || usage.MovesFromOtherRegisters.Count != 1)
                    continue;

                var move = usage.MovesFromOtherRegisters[0];
                var sourceReg = move.Instruction.Registers[1];

                if (sourceReg.KeepWith != reg.KeepWith) // see above.
                    continue;

                var sourceUsage = registerUsage.GetBasicUsage(sourceReg);

                if (sourceUsage.CheckCastsAndNewInstance.Count > 0)
                {
                    // I would think this is only be neccessary when we are in a try block,
                    // with a finally block. Most probably the dalvik verifier does not 
                    // correctly understand our finally routing code.
                    if (registerUsage.Body.Exceptions.Any())
                        continue;
                }

                foreach (var targetIns in usage.Reads)
                foreach (var sourceIns in sourceUsage.Writes)
                {
                    if (targetIns == sourceIns)
                    {
                        // if a register is read and assigned in the same instruction,
                        // the read is still valid.
                        continue;
                    }

                    if (registerUsage.Graph.IsReachable(targetIns, sourceIns, move))
                        goto unableToReplace;
                }

                if (reg.KeepWith == RFlags.KeepWithNext)
                {
                    var nextRegUsage = registerUsage.GetBasicUsage(registerUsage.Body.GetNext(reg));
                    var nextSourceUsage = registerUsage.GetBasicUsage(registerUsage.Body.GetNext(sourceReg));
                    if (nextRegUsage.Writes.Any() || nextSourceUsage.Writes.Any())
                    {
                        // I don't think this can happen, but better be on the safe side.
                        goto unableToReplace;
                    }
                }

                // replace the register usage.
                registerUsage.ReplaceRegister(usage, sourceUsage);
                hasChanges = true;

                unableToReplace:
                ;
            }
            return hasChanges;
        }
    }
}
