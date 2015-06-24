using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    internal static class RLTransformations
    {
        private static readonly IRLTransformation[] optimizations1 =
        {
            new InvokeTypeTransformation(), 
            new ConstPropagationTransformation(), 
            new ShareConstTransformation(),
        };
        
        private static readonly IRLTransformation[] incrementalOptimizations = 
        {
            new SwitchAndGotoOptimization(), 
            new EliminateDeadCodeTransformation(), 
            new PredictableBranchOptimizer(), 
            new EliminateDeadRegistersOptimizer(), 
        };

        private static readonly IRLTransformation[] optimizations2 = {
            new ShareRegistersTransformation(), 
            new NopRemoveTransformation(), 
            new FlattenExceptionsTransformation(), 
            // Last
            new InitializeRegistersTransformation(),
        };

        /// <summary>
        /// Transform the given body towards Dex compilation.
        /// </summary>
        internal static void Transform(Dex target, MethodBody body)
        {
            bool hasChanges = true;

            foreach (var transformation in optimizations1)
                transformation.Transform(target, body);

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

            foreach (var transformation in optimizations2)
            {
                transformation.Transform(target, body);
            }            
        }
    }
}
