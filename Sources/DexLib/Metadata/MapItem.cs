using System.Diagnostics;

namespace Dot42.DexLib.Metadata
{
    /// <summary>
    /// Entry of the map_list.
    /// </summary>
    [DebuggerDisplay("{@Type}, {@Offset}, {@Size}")]
    internal sealed class MapItem
    {
        internal MapItem(TypeCodes type, uint size, uint offset)
        {
            Type = type;
            Size = size;
            Offset = offset;
        }

        public TypeCodes Type { get; private set; }
        public uint Size { get; private set; }
        public uint Offset { get; private set; }
    }
}