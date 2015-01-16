using System.Collections.Generic;
using System.IO;

namespace Dot42.ApkLib.Resources
{
    partial class Table
    {
        /// <summary>
        /// ResTable_type
        /// </summary>
        public new sealed class Type : Chunk
        {
            private const int NO_ENTRY = -1;

            private readonly TypeSpec parent;
            private readonly Configuration configuration;

            /// <summary>
            /// Creation ctor
            /// </summary>
            internal Type(TypeSpec parent, Configuration configuration)
                : base(ChunkTypes.RES_TABLE_TYPE_TYPE)
            {
                this.parent = parent;
                this.configuration = configuration;
            }

            /// <summary>
            /// Read ctor
            /// </summary>
            internal Type(TypeSpec parent, ResReader reader)
                : base(reader, ChunkTypes.RES_TABLE_TYPE_TYPE)
            {
                this.parent = parent;

                var id = reader.ReadByte();
                reader.Skip(3); // reserved
                if (id != parent.Id)
                {
                    throw new IOException("Type id (" + id + ") " + "doesn't match parent id (" + parent.Id + ").");
                }
                var entryCount = reader.ReadInt32();
                if (entryCount != parent.EntryCount)
                {
                    throw new IOException(string.Format("Type entry count ({0}) doesn't match parent entry count ({1})", entryCount, parent.EntryCount));
                }
                var entriesStart = reader.ReadInt32();
                configuration = new Configuration(reader);
                // Data
                var offsets = reader.ReadIntArray(entryCount);
                var dataSize = (Size - entriesStart);
                if ((dataSize % 4) != 0)
                {
                    throw new IOException("Type data size (" + dataSize + ") is not multiple of 4.");
                }
                for (var i = 0; i < entryCount; i++ )
                {
                    if (offsets[i] != NO_ENTRY)
                    {
                        var actualOffset = reader.Position;
                        var instance = EntryInstance.Read(this, reader);
                        parent.GetEntry(i).Add(instance);
                    } 
                }
            }

            /// <summary>
            /// Write the header of this chunk.
            /// Always call the base method first.
            /// </summary>
            protected override void WriteHeader(ResWriter writer)
            {
                var startPosition = writer.Position;
                base.WriteHeader(writer);
                writer.WriteByte(parent.Id);
                writer.WriteByte(0); // res0
                writer.WriteUInt16(0); // res1
                writer.WriteInt32(parent.EntryCount); // entryCount
                var entriesStartMark = writer.MarkInt32(); // entriesStart
                configuration.Write(writer);

                // Patch entriesStart
                entriesStartMark.Value = (writer.Position - startPosition) + (parent.EntryCount * 4);
            }

            /// <summary>
            /// Write the data of this chunk.
            /// </summary>
            protected override void WriteData(ResWriter writer)
            {
                // Create offset marks
                var offsetMarks = new List<Mark.Int32>();
                foreach (var entry in parent.Entries)
                {
                    var mark = writer.MarkInt32();
                    offsetMarks.Add(mark);
                    EntryInstance instance;
                    if (!entry.TryGetInstance(this, out instance))
                    {
                        mark.Value = NO_ENTRY;
                    }
                }

                // Write entries, update offset marks while we go.
                var startPosition = writer.Position;

                for (var i = 0; i < parent.EntryCount; i++)
                {
                    var entry = parent.GetEntry(i);
                    EntryInstance instance;
                    if (!entry.TryGetInstance(this, out instance))
                        continue;

                    // Update mark
                    offsetMarks[i].Value = writer.Position - startPosition;
                    instance.Write(writer);
                }
            }

            /// <summary>
            /// Get my parent
            /// </summary>
            internal TypeSpec TypeSpec { get { return parent; } }

            /// <summary>
            /// Gets the configuration in which this type can be used.
            /// </summary>
            public Configuration Configuration { get { return configuration; } }
        }
    }
}
