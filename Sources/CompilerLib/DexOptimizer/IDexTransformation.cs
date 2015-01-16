using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.DexOptimizer
{
    internal interface IDexTransformation
    {
        /// <summary>
        /// Transform the given body.
        /// </summary>
        void Transform(MethodBody body);
    }
}
