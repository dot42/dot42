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

        /// <summary>
        /// Transform the given body towards Dex compilation.
        /// </summary>
        internal static void Transform(Dex target, MethodBody body)
        {
            foreach (var transformation in optimizations)
            {
                transformation.Transform(target, body);
            }            
        }
    }
}
