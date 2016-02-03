using System.Collections.Generic;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Share temporary registers are much as possible.
    /// </summary>
    internal partial class ShareRegistersTransformation : IRLTransformation
    {
        /// <summary>
        /// Transform the given body.
        /// </summary>
        public bool Transform(Dex target, MethodBody body)
        {
            // Find all basic blocks
            var basicBlocks = BasicBlock.Find(body);

            // Replace temp with variable registers with possible.
            TransformTempToVariable(body, basicBlocks);

            // Share registers in block only
            TransformInBlock(body, basicBlocks);

            // Share register across blocks
            TransformCrossBlock(body, basicBlocks);

            return false;
        }

        /// <summary>
        /// Collect the usage range of each temp register used in the given method body.
        /// </summary>
        private static Dictionary<Register, RegisterRange> CollectTempRegisterUsageRanges(IEnumerable<BasicBlock> blocks)
        {
            var ranges = new Dictionary<Register, RegisterRange>();

            foreach (var block in blocks)
            {
                foreach (var inst in block.Instructions)
                {
                    foreach (var reg in inst.Registers)
                    {
                        if (reg.Category == RCategory.Temp)
                        {
                            RegisterRange range;
                            if (ranges.TryGetValue(reg, out range))
                            {
                                // Extend range
                                range.Range = new InstructionRange(range.Range.First, inst);
                                range.InstructionCount++;
                            }
                            else
                            {
                                // Create range
                                range = new RegisterRange(reg, new InstructionRange(inst, inst)) { InstructionCount = 1 };
                                ranges.Add(reg, range);
                            }
                            range.AddBlock(block);
                        }
                    }
                }
            }
            return ranges;
        }
    }
}
