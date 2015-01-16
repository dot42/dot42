using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.DexOptimizer
{
    internal static class DexOptimizer
    {
        private static readonly IDexTransformation[] optimizations = new IDexTransformation[] {
            // Last
            //new InitializeRegistersTransformation(), 
            new InstructionOptimizer(),
            new NopRemoveOptimizer(), 
            new BranchOptimizer(), 
        };

        /// <summary>
        /// Optimize the given body
        /// </summary>
        internal static void Optimize(MethodBody body)
        {
            foreach (var transformation in optimizations)
            {
                transformation.Transform(body);
            }            
        }
    }
}
