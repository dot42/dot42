using System.Collections.Generic;
using Dot42.ApkLib.Manifest;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    internal static class RLTransformations
    {
        private static readonly IRLTransformation[] optimizations1 =
        {
            new InvokeTypeTransformation(),
            new EliminateCheckCastToObject(), 
            new ConstPropagationTransformation(), 
            new ShareConstTransformation(),
        };
        
        private static readonly IRLTransformation[] incrementalOptimizations = 
        {
            new EliminateRegistersOptimization(), 
            new SwitchAndGotoOptimization(), 
            new EliminateDeadCodeTransformation(), 
            new PredictableBranchOptimizer(), 
        };

        private static readonly IRLTransformation[] optimizations2 = {
            new ShareRegistersTransformation(),             
            new NopRemoveTransformation(), 
            new FlattenExceptionsTransformation(), 
            // Last
            new InitializeRegistersTransformation(),
        };

        internal static string lastStep;

        /// <summary>
        /// Transform the given body towards Dex compilation. 
        /// <returns>
        /// Returns the name of the last applied tranformation, or null on full processing.
        /// </returns>
        /// </summary>
        internal static string Transform(Dex target, MethodBody body, int stopAfterSteps = int.MaxValue)
        {
            if (stopAfterSteps == 0) return "(no proccessing)";
            int stepCount = 0;

            foreach (var applied in GetTransformations(target, body))
            {
                if (++stepCount > stopAfterSteps)
                {
                    return applied.GetType().Name;
                }
            }

            return null;
        }

        private static IEnumerable<IRLTransformation> GetTransformations(Dex target, MethodBody body)
        {
            foreach (var transformation in optimizations1)
            {
                transformation.Transform(target, body);
                yield return transformation;
            }

            var noopRemove = new NopRemoveTransformation();
            noopRemove.Transform(target, body);
            yield return noopRemove;

            bool hasChanges = true;


            while (hasChanges)
            {
                hasChanges = false;
                foreach (var transformation in incrementalOptimizations)
                {
                    bool changed = transformation.Transform(target, body);
                    if (changed) noopRemove.Transform(target, body);

                    hasChanges = changed || hasChanges;

                    yield return transformation;
                }
            }

            foreach (var transformation in optimizations2)
            {
                transformation.Transform(target, body);
                yield return transformation;
            }    
        }
    }
}
