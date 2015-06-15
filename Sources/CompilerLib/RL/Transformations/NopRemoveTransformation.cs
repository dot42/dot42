using System.Linq;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Remove all nop's
    /// </summary>
    internal sealed class NopRemoveTransformation : IRLTransformation
    {
        public bool Transform(Dex target, MethodBody body)
        {
            bool hasChanges = false;
#if DEBUG
            //return;
#endif
            var instructions = body.Instructions;
            var hasNops = instructions.Any(x => x.Code == RCode.Nop);
            if (!hasNops)
                return false;

            var rerouter = new BranchReRouter(body);
            var i = 0;
            while (i < instructions.Count - 1)
            {
                var inst = instructions[i];
                if (inst.Code != RCode.Nop)
                {
                    i++;
                    continue;
                }
                if (body.Exceptions.Count > 0)
                {
                    if (body.Exceptions.Any(x => (x.TryEnd == inst) /*|| (x.TryStart == inst)*/))
                    {
                        i++;
                        continue;
                    }
                }

                var next = instructions[i + 1];
                rerouter.Reroute(inst, next);
                instructions.RemoveAt(i);
                hasChanges = true;
            }

            return hasChanges;
        }
    }
}
