using System.Linq;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.DexOptimizer
{
    /// <summary>
    /// Remove all nop's
    /// </summary>
    internal sealed class NopRemoveOptimizer : IDexTransformation
    {
        public void Transform(MethodBody body)
        {
#if DEBUG
            //return;
#endif
            var instructions = body.Instructions;
            var hasNops = instructions.Any(x => x.OpCode == OpCodes.Nop);
            if (!hasNops)
                return;

            var rerouter = new BranchReRouter(body);
            var i = 0;
            while (i < instructions.Count - 1)
            {
                var inst = instructions[i];
                if (inst.OpCode != OpCodes.Nop)
                {
                    i++;
                    continue;
                }

                if (body.Exceptions.Any(x => (x.TryEnd == inst) /*|| (x.TryStart == inst)*/))
                {
                    i++;
                    continue;
                }

                var next = instructions[i + 1];

                // TODO: prevent removal of nop if the next instruction branches to this nop,
                //       (i.e. a spin loop); or, better, handle this case in the RL to dex compiler.

                rerouter.Reroute(inst, next);
                instructions.RemoveAt(i);
            }
        }
    }
}
