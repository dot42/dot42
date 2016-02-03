using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// will nop instructions that check_cast to object.
    /// </summary>
    internal class EliminateCheckCastToObject : IRLTransformation
    {
        public bool Transform(Dex target, MethodBody body)
        {
            foreach (var ins in body.Instructions)
            {
                if (ins.Code == RCode.Check_cast)
                {
                    var typeReference = (TypeReference) ins.Operand;
                    if(typeReference.Descriptor == "Ljava/lang/Object;")
                        ins.ConvertToNop();
                }
            }

            return false;
        }
    }
}
