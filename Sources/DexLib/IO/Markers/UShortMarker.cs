using System.IO;
using Dot42.DexLib.Extensions;

namespace Dot42.DexLib.IO.Markers
{
    internal class UShortMarker : Marker<ushort>
    {
        public UShortMarker(BinaryWriter writer) : base(writer)
        {
        }

        public override ushort Value
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
        /// Reserve space.
        /// </summary>
        protected override void Allocate()
        {
            Writer.Write((ushort) 0);
        }
    }
}