using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    internal static class RLTransformations
    {
        private static readonly IRLTransformation[] optimizations = {
            new InvokeTypeTransformation(), 
            new ConstPropagationTransformation(), 
            new ShareConstTransformation(),
            new ShareRegistersTransformation(), 
            //new ConstPropagationTransformation(), 
            //new ShareConstTransformation(),
            new NopRemoveTransformation(), 
            new FlattenExceptionsTransformation(), 
            // Last
            new InitializeRegistersTransformation(),
        };

        private static readonly IRLTransformation[] incrementalOptimizations = 
        {
            new SwitchAndGotoOptimization(), 
            new EliminateDeadCodeTransformation(), 
            new PredictableBranchOptimizer(), 
            new EliminateDeadRegistersOptimizer(), 
        };

        /// <summary>
        /// Transform the given body towards Dex compilation.
        /// </summary>
        internal static void Transform(Dex target, MethodBody body)
        {
            bool hasChanges = true;

            var noopRemove = new NopRemoveTransformation();
            noopRemove.Transform(target, body);

            while (hasChanges)
            {
                hasChanges = false;
                foreach (var transformation in incrementalOptimizations)
                {
                    bool changed = transformation.Transform(target, body);
                    if (changed) noopRemove.Transform(target, body);
                        
                    hasChanges =  changed || hasChanges;
                }
            }

            foreach (var transformation in optimizations)
            {
                transformation.Transform(target, body);
            }            
        }
    }
}
