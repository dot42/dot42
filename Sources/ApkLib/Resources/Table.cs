using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dot42.ApkLib.Resources
{
    /// <summary>
    /// ResTable_header
    /// </summary>
    public partial class Table : Chunk
    {
        private readonly StringPool strings;
        private readonly List<Package> packages = new List<Package>();

        /// <summary>
        /// Creation ctor
        /// </summary>
        public Table(string packageName)
            : base(ChunkTypes.RES_TABLE_TYPE)
        {
            strings = new StringPool();
            packages.Add(new Package(this, 0x7f, packageName));
        }

        /// <summary>
        /// Stream ctor
        /// </summary>
        public Table(Stream stream)
            : this(new ResReader(stream, false))
        {            
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal Table(ResReader reader)
            : base(reader, ChunkTypes.RES_TABLE_TYPE)
        {
            var packageCount = reader.ReadInt32();
            strings = new StringPool(reader);
            for (var i = 0; i < packageCount; i++)
            {
                packages.Add(new Package(this, reader));
            }
        }

        /// <summary>
        /// Write the header of this chunk.
        /// Always call the base method first.
        /// </summary>
        protected override void WriteHeader(ResWriter writer)
        {
            base.WriteHeader(writer);
            writer.WriteInt32(packages.Count); // packageCount
        }

        /// <summary>
        /// Prepare this chunk for writing
        /// </summary>
        protected internal override void PrepareForWrite()
        {
            base.PrepareForWrite();
            packages.ForEach(x => x.PrepareForWrite());
        }

        /// <summary>
        /// Write the data of this chunk.
        /// </summary>
        protected override void WriteData(ResWriter writer)
        {
            base.WriteData(writer);
            strings.Write(writer);
            packages.ForEach(x => x.Write(writer));
        }

        /// <summary>
        /// Gets my string pool
        /// </summary>
        public StringPool Strings { get { return strings; } }

        /// <summary>
        /// Gets my packages
        /// </summary>
        public List<Package> Packages { get { return packages; } }

        /// <summary>
        /// Gets an id that belongs to the given resource id.
        /// </summary>
        public string GetResourceIdentifier(int resid)
        {
            var packageId = (resid >> 24);
            var typeSpecId = (resid >> 16) & 0xFF;
            var entryIndex = (resid & 0xFFFF);

            var package = packages.FirstOrDefault(x => x.Id == packageId);
            if (package == null)
                return null;
            var typeSpec = package.TypeSpecs.FirstOrDefault(x => x.Id == typeSpecId);
            if (typeSpec == null)
                return null;
            var entry = typeSpec.GetEntry(entryIndex);
            if (entry == null)
                return null;
            return entry.Key;
        }
    }
}
