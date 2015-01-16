using System.Collections.Generic;
using System.Linq;

namespace Dot42.DexLib.Instructions
{
    public sealed class BranchReRouter
    {
        private readonly MethodBody body;
        private readonly List<Instruction> branches;
        private readonly ExceptionHandler[] exceptionHandlers;

        /// <summary>
        /// Default ctor
        /// </summary>
        public BranchReRouter(MethodBody body)
        {
            this.body = body;
            List<Instruction> list = null;
            // Record instructions with branch targets
            foreach (var inst in body.Instructions)
            {
                if (inst.OpCode.IsBranch())
                {
                    list = list ?? new List<Instruction>();
                    list.Add(inst);
                }
            }
            branches = list;
            exceptionHandlers = body.Exceptions.Any() ? body.Exceptions.ToArray() : null;
        }


        /// <summary>
        /// Reroute all old targets to the new targets.
        /// </summary>
        public void Reroute(Instruction oldTarget, Instruction newTarget)
        {
            if (branches != null)
            {
                var count = branches.Count;
                for (var i = 0; i < count; i++)
                {
                    var ins = branches[i];
                    if (ins.Operand == oldTarget)
                    {
                        ins.Operand = newTarget;
                    }
                    else if (ins.OpCode == OpCodes.Packed_switch || ins.OpCode== OpCodes.Sparse_switch)
                    {
                        var targets = (ISwitchData)ins.Operand;
                        targets.ReplaceTarget(oldTarget, newTarget);
                    }
                }
            }

            if (exceptionHandlers != null)
            {
                var count = exceptionHandlers.Length;
                for (var i = 0; i < count; i++)
                {
                    var eh = exceptionHandlers[i];
                    if (eh.CatchAll == oldTarget) { eh.CatchAll = newTarget; }
                    if (eh.TryEnd == oldTarget) { eh.TryEnd = GetPrevious(oldTarget); }
                    if (eh.TryStart == oldTarget) { eh.TryStart = newTarget; }
                    foreach (var c in eh.Catches)
                    {
                        if (c.Instruction == oldTarget) c.Instruction = newTarget;
                    }
                }
            }

            // Save PDB info
            /*if ((oldTarget.SequencePoint != null) && (newTarget.SequencePoint == null))
            {
                newTarget.SequencePoint = oldTarget.SequencePoint;
            }*/
        }

        /// <summary>
        /// Gets the instruction directly before the given instruction.
        /// </summary>
        private Instruction GetPrevious(Instruction x)
        {
            var instructions = body.Instructions;
            var index = instructions.IndexOf(x);
            return instructions[index - 1];
        }
    }
}
