using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Share temporary registers are much as possible.
    /// </summary>
    /// 
    /// TODO Fixed for wide registers
    partial class ShareRegistersTransformation 
    {
        /// <summary>
        /// Transform the given body.
        /// </summary>
        private static void TransformCrossBlock(MethodBody body, ICollection<BasicBlock> basicBlocks)
        {
            // Find all basic blocks
            if (basicBlocks.Count <= 1)
                return; // No need for this transformation

            var availableShortRegisters = new List<Register>(); // List of register available from previous blocks
            var availableWideRegisters = new List<Register>(); // List of register available from previous blocks
            var allTempRegisterRanges = CollectTempRegisterUsageRanges(basicBlocks).Values.Where(x => !x.Register.IsKeepWithNext).ToList();

            foreach (var iterator in basicBlocks)
            {
                var block = iterator;
                // Collect all register ranges that fit completly within this block
                var registerRanges = allTempRegisterRanges.Where(x => x.IsRegisterAssignedInFirstInstruction && block.Contains(x.Range)).ToList();

                // For each register, look for an available to use instead
                foreach (var range in registerRanges)
                {
                    // Validate the range
                    if (!block.ContainsEntireRange(range.Range))
                    {
                        throw new CompilerException("Usage range falls outside block.");
                    }

                    var r = range.Register;
                    var availableList = r.IsKeepWithNext ? availableWideRegisters : availableShortRegisters;
                    var replacement = availableList.FirstOrDefault(x => r.Type == x.Type);
                    if (replacement != null)
                    {
                        // We can replace r with the replacement we found.
                        range.ReplaceRegisterWith(r, replacement, body);
                        // Replacement is no longer available within this block
                        availableList.Remove(replacement);
                    }
                }

                // Make all registers available to following blocks
                foreach (var range in registerRanges)
                {
                    var r = range.Register;
                    var availableList = r.IsKeepWithNext ? availableWideRegisters : availableShortRegisters;
                    if (!availableList.Contains(r))
                        availableList.Add(r);
                }
            }
        }
    }
}
