using System.IO;
using Dot42.DexLib.Extensions;

namespace Dot42.DexLib.IO.Markers
{
    internal sealed class UIntMarker : Marker<uint>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public UIntMarker(BinaryWriter writer) : base(writer)
        {
        }

        /// <summary>
        /// Set the value at the marked position.
        /// </summary>
        public override uint Value
        {
            set
            {
                foreach (var position in Positions)
                {
                    Writer.PreserveCurrentPosition(position, () => Writer.Write(value));
                }
            }
        }

        /// <summary>
        /// Reserve space.
        /// </summary>
        protected override void Allocate()
        {
            Writer.Write((uint) 0);
        }
    }
}