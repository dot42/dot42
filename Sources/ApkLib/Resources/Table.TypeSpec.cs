using System.Collections.Generic;
using System.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class Table
    {
        /// <summary>
        /// ResTable_typeSpec
        /// </summary>
        public sealed class TypeSpec : Chunk
        {
            private readonly Package parent;
            private readonly int id;
            private readonly List<Type> types = new List<Type>();
            private readonly List<Entry> entries = new List<Entry>(); 

            /// <summary>
            /// Reader ctor
            /// </summary>
            internal TypeSpec(Package parent, int id)
                : base(ChunkTypes.RES_TABLE_TYPE_SPEC_TYPE)
            {
                this.parent = parent;
                this.id = id;
            }

            /// <summary>
            /// Reader ctor
            /// </summary>
            internal TypeSpec(Package parent, ResReader reader)
                : base(reader, ChunkTypes.RES_TABLE_TYPE_SPEC_TYPE)
            {
                this.parent = parent;

                id = reader.ReadByte();
                reader.Skip(3); // reserved
                var entryCount = reader.ReadInt32();
                var flags = reader.ReadIntArray(entryCount).ToList();
                entries.AddRange(flags.Select(x => new Entry(this, x)));
            }

            /// <summary>
            /// The type identifier this chunk is holding.  Type IDs start
            /// at 1 (corresponding to the value of the type bits in a
            /// resource identifier).  0 is invalid.
            /// </summary>
            public int Id { get { return id; } }

            /// <summary>
            /// Gets the name of this type.
            /// </summary>
            public string Name { get { return parent.TypeStrings[id - 1]; } }

            /// <summary>
            /// Gets my parent
            /// </summary>
            internal Package Package { get { return parent; } }

            /// <summary>
            /// Gets all types
            /// </summary>
            public IEnumerable<Type> Types
            {
                get { return types; }
            }

            /// <summary>
            /// Gets all entries
            /// </summary>
            public IEnumerable<Entry> Entries
            {
                get { return entries; }
            }

            /// <summary>
            /// Gets the number of entries
            /// </summary>
            public int EntryCount { get { return entries.Count; } }

            /// <summary>
            /// Gets the entry at the given index
            /// </summary>
            internal Entry GetEntry(int index)
            {
                return entries[index];
            }

            /// <summary>
            /// Add a type that has been read.
            /// </summary>
            internal void Add(Type type)
            {
                types.Add(type);
            }
            
            /// <summary>
            /// Search for a type by the given configuration.
            /// Create one if not found.
            /// </summary>
            public Type GetOrCreateType(Configuration configuration)
            {
                var result = types.FirstOrDefault(x => x.Configuration.Equals(configuration));
                if (result != null)
                    return result;

                result = new Type(this, configuration);
                Add(result);
                return result;
            }

            /// <summary>
            /// Add a new entry
            /// </summary>
            public Entry AddEntry()
            {
                var result = new Entry(this, 0);
                entries.Add(result);
                return result;
            }

            /// <summary>
            /// Prepare this chunk for writing
            /// </summary>
            protected internal override void PrepareForWrite()
            {
                base.PrepareForWrite();
                types.ForEach(x => x.PrepareForWrite());
                entries.ForEach(x => x.PrepareForWrite());
            }

            /// <summary>
            /// Write the header of this chunk.
            /// Always call the base method first.
            /// </summary>
            protected override void WriteHeader(ResWriter writer)
            {
                base.WriteHeader(writer);
                writer.WriteByte(id);
                writer.WriteByte(0); // res0
                writer.WriteUInt16(0); // res1
                writer.WriteInt32(entries.Count); // entryCount
            }

            /// <summary>
            /// Write the data of this chunk.
            /// </summary>
            protected override void WriteData(ResWriter writer)
            {
                base.WriteData(writer);
                var flags = entries.Select(x => x.Flags).ToArray();
                writer.WriteIntArray(flags.ToArray());
            }
        }
    }
}
