using System.IO;
using Dot42.DexLib.Extensions;

namespace Dot42.DexLib.IO.Markers
{
    internal class SizeOffsetMarker : Marker<SizeOffset>
    {
        public SizeOffsetMarker(BinaryWriter writer) : base(writer)
        {
        }

        public override SizeOffset Value
        {
            set
            {
#if !DISABLE_MARKERS || !DEBUG
                foreach (uint position in Positions)
                {
                    Writer.PreserveCurrentPosition(position, () => {
                                                                 Writer.Write(value.Size);
                                                                 Writer.Write(value.Offset);
                                                             });
                }
#endif
            }
        }

        /// <summary>
        /// Reserve space
        /// </summary>
        protected override void Allocate()
        {
            Writer.Write((uint) 0);
            Writer.Write((uint) 0);
        }
    }
}