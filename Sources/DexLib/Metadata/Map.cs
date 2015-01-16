using System.Collections.Generic;

namespace Dot42.DexLib.Metadata
{
    /// <summary>
    /// Map of <see cref="MapItem"/> indexed by type code.
    /// </summary>
    internal sealed class Map : Dictionary<TypeCodes, MapItem>
    {
        /// <summary>
        /// Add an item for the given type, size and offset.
        /// </summary>
        internal void Add(TypeCodes type, uint size, uint offset)
        {
            Add(type, new MapItem(type, size, offset));
        }
    }
}