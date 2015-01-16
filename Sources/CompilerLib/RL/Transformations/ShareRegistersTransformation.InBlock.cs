using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Share temporary registers are much as possible.
    /// </summary>
    partial class ShareRegistersTransformation
    {
        /// <summary>
        /// Transform the given body.
        /// </summary>
        private static void TransformInBlock(MethodBody body, List<BasicBlock> basicBlocks)
        {
            body.Instructions.UpdateIndexCache();
            var allTempRegisterRanges = CollectTempRegisterUsageRanges(basicBlocks).Values.ToList();

            foreach (var iterator in basicBlocks)
            {
                var block = iterator;
                // Collect all register ranges that fit completly within this block
                var registerRanges = allTempRegisterRanges.Where(x => x.ContainsExactlyOne(block)).ToList();

                // For each register, look for all other range that does not overlap.
                while (registerRanges.Count > 1)
                {
                    var initial = registerRanges[0];
                    var initialR = initial.Register;
                    for (var i = 1/* skip initial*/; i < registerRanges.Count; i++)
                    {
                        var current = registerRanges[i];
                        var currentR = current.Register;
                        if ((currentR.Type == initialR.Type) && (currentR.IsKeepWithNext == initialR.IsKeepWithNext))
                        {
                            // We can share with respect to type, now check overlapping ranges
                            if (!initial.Range.IntersectsWith(current.Range))
                            {
                                // There is no overlap, we can share the register
                                // Share register with this range
                                current.ReplaceRegisterWith(currentR, initialR, body);
                                registerRanges.RemoveAt(i);
                                i--; // Make sure we use the same 'i' after the i++ in the for loop

                                // Extend initial range
                                initial.Range = initial.Range.Extend(current.Range);
                            }
                        }
                    }

                    // Now remove the initial range
                    registerRanges.RemoveAt(0);
                }
            }
        }
    }
}
