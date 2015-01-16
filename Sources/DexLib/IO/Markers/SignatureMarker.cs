using System.IO;
using Dot42.DexLib.Extensions;

namespace Dot42.DexLib.IO.Markers
{
    internal class SignatureMarker : Marker<byte[]>
    {
        public SignatureMarker(BinaryWriter writer) : base(writer)
        {
        }

        public override byte[] Value
        {
            set
            {
#if !DISABLE_MARKERS || !DEBUG
                foreach (var position in Positions)
                {
                    Writer.PreserveCurrentPosition(position, () => Writer.Write(value));
                }
#endif
            }
        }

        /// <summary>
        /// Reserve space
        /// </summary>
        protected override void Allocate()
        {
            Writer.Write(new byte[DexConsts.SignatureSize]);
        }
    }
}