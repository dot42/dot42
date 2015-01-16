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
        private static void TransformTempToVariable(MethodBody body, List<BasicBlock> basicBlocks)
        {
            body.Instructions.UpdateIndexCache();
            var allTempRegisterRanges = CollectTempRegisterUsageRanges(basicBlocks).Values.ToList();

            foreach (var iterator in basicBlocks)
            {
                var block = iterator;
                // Collect all register ranges that fit completly within this block and are used exactly 2 times
                var registerRanges = allTempRegisterRanges.Where(x => (x.InstructionCount == 2) && x.ContainsExactlyOne(block)).ToList();

                foreach (var range in registerRanges)
                {
                    var firstIns = range.Range.First;
                    var lastIns = range.Range.Last;
                    if (!lastIns.Code.IsMove())
                        continue;
                    var reg = range.Register;
                    if (lastIns.Registers[1] != reg)
                        continue;
                    var varReg = lastIns.Registers[0];
                    if (varReg.Category != RCategory.Variable)
                        continue;

                    // Replace
                    firstIns.ReplaceRegisterWith(reg, varReg);
                    lastIns.ConvertToNop();
                }
            }
        }
    }
}
