using System.Collections.Generic;
using System.IO;

namespace Dot42.DexLib.IO.Markers
{
    internal abstract class Marker<T>
    {
        protected readonly List<uint> Positions = new List<uint>();
        protected readonly BinaryWriter Writer;

        protected Marker(BinaryWriter writer)
        {
            Writer = writer;
            CloneMarker();
        }

        public abstract T Value { set; }

        /// <summary>
        /// Gets the first entries of the <see cref="Positions"/> list.
        /// </summary>
        internal uint FirstPosition
        {
            get { return Positions[0]; }
        }

        protected abstract void Allocate();

        public void CloneMarker()
        {
            var position = (uint) Writer.BaseStream.Position;
            Positions.Add(position);
            Allocate();
        }
    }
}