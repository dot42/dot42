using System;
using System.Collections.Generic;
using System.Linq;

namespace Dot42.CompilerLib.RL.Transformations
{
    public sealed class BranchReRouter
    {
        private readonly List<Instruction> branches;
        private readonly ExceptionHandler[] exceptionHandlers;

        /// <summary>
        /// Default ctor
        /// </summary>
        public BranchReRouter(MethodBody body)
        {
            List<Instruction> list = null;
            // Record instructions with branch targets
            foreach (var inst in body.Instructions)
            {
                switch (inst.Code)
                {
                    case RCode.Goto:
                    case RCode.Leave:
                    case RCode.If_eq:
                    case RCode.If_ne:
                    case RCode.If_lt:
                    case RCode.If_ge:
                    case RCode.If_gt:
                    case RCode.If_le:
                    case RCode.If_eqz:
                    case RCode.If_nez:
                    case RCode.If_ltz:
                    case RCode.If_gez:
                    case RCode.If_gtz:
                    case RCode.If_lez:
                    case RCode.Packed_switch:
                    case RCode.Sparse_switch:
                        list = list ?? new List<Instruction>();
                        list.Add(inst);
                        break;
                }
            }
            branches = list;
            exceptionHandlers = body.Exceptions.Any() ? body.Exceptions.ToArray() : null;
        }


        /// <summary>
        /// Reroute all old targets to the new targets.
        /// </summary>
        internal void Reroute(Instruction oldTarget, Instruction newTarget)
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
                    else if (ins.Code == RCode.Packed_switch)
                    {
                        var targets = (Instruction[])ins.Operand;
                        for (var j = 0; j < targets.Length; j++)
                        {
                            if (targets[j] == oldTarget)
                            {
                                targets[j] = newTarget;
                            }
                        }
                    }
                    else if (ins.Code == RCode.Sparse_switch)
                    {
                        var targetPairs = (Tuple<int, Instruction>[])ins.Operand;
                        for (var j = 0; j < targetPairs.Length; j++)
                        {
                            if ((targetPairs[j] != null) && (targetPairs[j].Item2 == oldTarget))
                            {
                                targetPairs[j] = Tuple.Create(targetPairs[j].Item1, newTarget);
                            }
                        }
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
                    if (eh.TryEnd == oldTarget) { eh.TryEnd = oldTarget.Previous; }
                    if (eh.TryStart == oldTarget) { eh.TryStart = newTarget; }
                    foreach (var c in eh.Catches)
                    {
                        if (c.Instruction == oldTarget) c.Instruction = newTarget;
                    }
                }
            }

            // Save PDB info
            /*if ((oldTarget.SourceLocation != null) && (newTarget.SourceLocation == null))
            {
                newTarget.SourceLocation = oldTarget.SourceLocation;
            }*/
        }
    }
}
