using System.Collections.Generic;
using System.IO;

namespace Dot42.ApkLib.Resources
{
    /// <summary>
    /// ResTable_map
    /// </summary>
    public sealed class ResourceMap : Chunk
    {
        private readonly List<int> resourceIds = new List<int>();

        /// <summary>
        /// Creation ctor
        /// </summary>
        internal ResourceMap()
            : base(ChunkTypes.RES_XML_RESOURCE_MAP_TYPE)
        {
            
        }

        /// <summary>
        /// Reading ctor
        /// </summary>
        internal ResourceMap(ResReader reader)
            : base(reader, ChunkTypes.RES_XML_RESOURCE_MAP_TYPE)
        {
            if (Size < 8 || (Size % 4) != 0)
            {
                throw new IOException("Invalid resource ids size (" + Size + ").");
            }
            resourceIds.AddRange(reader.ReadIntArray(Size / 4 - 2));
        }

        /// <summary>
        /// Write the data of this chunk.
        /// </summary>
        protected override void WriteData(ResWriter writer)
        {
            base.WriteData(writer);
            writer.WriteIntArray(resourceIds.ToArray());
        }

        /// <summary>
        /// Set an ID at the given index
        /// </summary>
        internal void Set(int index, int id)
        {
            while (index >= resourceIds.Count)
            {
                resourceIds.Add(0);
            }
            resourceIds[index] = id;
        }

        /// <summary>
        /// Remove all entries
        /// </summary>
        public void Clear()
        {
            resourceIds.Clear();
        }
    }
}
