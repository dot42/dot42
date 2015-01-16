using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class Table
    {
        /// <summary>
        /// ResTable_package
        /// </summary>
        public sealed class Package : Chunk
        {
            private readonly Table table;
            private readonly int id;
            private readonly string name;
            // contains the names of the types of the Resources defined in the Package
            private readonly StringPool typeStrings;
            // contains the names (keys) of the Resources defined in the Package.
            private readonly StringPool keyStrings;
            private readonly List<TypeSpec> typeSpecs = new List<TypeSpec>();
            private Mark.Int32 typeStringsMark;
            private Mark.Int32 keyStringsMark;
            private int headerStartPosition;

            /// <summary>
            /// Creation ctor
            /// </summary>
            internal Package(Table table, int id, string name)
                : base(ChunkTypes.RES_TABLE_PACKAGE_TYPE)
            {
                this.table = table;
                this.id = id;
                this.name = name;
                keyStrings = new StringPool();
                typeStrings = new StringPool();
            }

            /// <summary>
            /// Default ctor
            /// </summary>
            internal Package(Table table, ResReader reader)
                : base(reader, ChunkTypes.RES_TABLE_PACKAGE_TYPE)
            {
                this.table = table;

                id = reader.ReadInt32();
                name = reader.ReadFixedLenghtUnicodeString(128);
                var typeStringsOffset = reader.ReadInt32();
                var lastPublicType = reader.ReadInt32();
                var keyStringsOffset = reader.ReadInt32();
                var lastPublicKey = reader.ReadInt32();

                // Record offset
                var dataOffset = reader.Position;

                // Data
                typeStrings = new StringPool(reader);
                keyStrings = new StringPool(reader);

                TypeSpec currentTypeSpec = null;
                while (reader.Position - dataOffset < DataSize)
                {
                    var chunkType = reader.PeekChunkType();
                    if (chunkType == ChunkTypes.RES_TABLE_TYPE_SPEC_TYPE)
                    {
                        currentTypeSpec = Read(reader, () => new TypeSpec(this, reader));
                        typeSpecs.Add(currentTypeSpec);
                    }
                    else if (chunkType == ChunkTypes.RES_TABLE_TYPE_TYPE)
                    {
                        if (currentTypeSpec == null)
                        {
                            throw new IOException("Invalid chunk sequence: content read before typeSpec.");
                        }
                        var parent = currentTypeSpec;
                        var type = Read(reader, () => new Type(parent, reader));
                        currentTypeSpec.Add(type);
                    }
                    else
                    {
                        throw new IOException("Unexpected chunk type (" + chunkType + ").");
                    }
                }
            }

            /// <summary>
            /// Prepare this chunk for writing
            /// </summary>
            protected internal override void PrepareForWrite()
            {
                base.PrepareForWrite();
                typeSpecs.ForEach(x => x.PrepareForWrite());
            }

            /// <summary>
            /// Write the header of this chunk.
            /// Always call the base method first.
            /// </summary>
            protected override void WriteHeader(ResWriter writer)
            {
                headerStartPosition = writer.Position;
                base.WriteHeader(writer);
                writer.WriteInt32(id);
                writer.WriteFixedLenghtUnicodeString(name, 128);
                typeStringsMark = writer.MarkInt32(); 
                writer.WriteInt32(typeStrings.Count); 
                keyStringsMark = writer.MarkInt32(); 
                writer.WriteInt32(KeyStrings.Count); 
            }

            /// <summary>
            /// Write the data of this chunk.
            /// </summary>
            protected override void WriteData(ResWriter writer)
            {
                base.WriteData(writer);
                typeStringsMark.Value = writer.Position - headerStartPosition;
                typeStrings.Write(writer);
                keyStringsMark.Value = writer.Position - headerStartPosition;
                keyStrings.Write(writer);

                // Write typeSpecs and types
                foreach (var typeSpec in typeSpecs)
                {
                    typeSpec.Write(writer);

                    foreach (var typeAndFlags in typeSpec.Types)
                    {
                        typeAndFlags.Write(writer);
                    }
                }
            }

            /// <summary>
            /// Gets the ID of this package
            /// </summary>
            public int Id { get { return id; } }

            /// <summary>
            /// Gets the name of this package
            /// </summary>
            public string Name { get { return name; } }

            /// <summary>
            /// String pool containing asset names
            /// </summary>
            public StringPool TypeStrings { get { return typeStrings; } }

            /// <summary>
            /// String pool containing resource names
            /// </summary>
            public StringPool KeyStrings { get { return keyStrings; } }

            /// <summary>
            /// Gets all type specs.
            /// </summary>
            public IEnumerable<TypeSpec> TypeSpecs
            {
                get { return typeSpecs; }
            }

            /// <summary>
            /// Search for a type spec by the given name.
            /// Create one if not found.
            /// </summary>
            public TypeSpec GetOrCreateTypeSpec(string name)
            {
                var result = typeSpecs.FirstOrDefault(x => x.Name == name);
                if (result != null)
                    return result;

                // Not found, add one
                //Debugger.Launch();
                result = new TypeSpec(this, typeSpecs.Count + 1);
                typeStrings.Add(name, -1);
                typeSpecs.Add(result);
                return result;
            }

            /// <summary>
            /// Get my parent
            /// </summary>
            public Table Table { get { return table; } }
        }
    }
}
