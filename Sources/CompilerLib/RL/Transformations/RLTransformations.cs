using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    internal static class RLTransformations
    {
        private static readonly IRLTransformation[] optimizations = new IRLTransformation[] {
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

        private static readonly IRLTransformation[] incrementalOptimizations = new IRLTransformation[]
        {
            new RemoveEmptySwitchAndGotosTransformation(), 
            new EliminateDeadCodeTransformation(), 
        };

        /// <summary>
        /// Transform the given body towards Dex compilation.
        /// </summary>
        internal static void Transform(Dex target, MethodBody body)
        {
            bool hasChanges = true;
            while (hasChanges)
            {
                hasChanges = false;
                foreach (var transformation in incrementalOptimizations)
                {
                    hasChanges = transformation.Transform(target, body) || hasChanges;
                }   
            }

            foreach (var transformation in optimizations)
            {
                transformation.Transform(target, body);
            }            
        }
    }
}
