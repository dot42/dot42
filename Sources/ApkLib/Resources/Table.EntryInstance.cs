using System;

namespace Dot42.ApkLib.Resources
{
    partial class Table
    {
        /// <summary>
        /// ResTable_entry
        /// </summary>
        public abstract class EntryInstance
        {
            [Flags]
            public enum EntryFlags
            {
                None = 0x0000,
                // If set, this is a complex entry, holding a set of name/value
                // mappings.  It is followed by an array of ResTable_map structures.
                Complex = 0x0001,
                // If set, this resource has been declared public, so libraries
                // are allowed to reference it.
                Public = 0x0002,
            }

            private readonly Type parent;
            private readonly EntryFlags flags;
            private readonly string key;

            /// <summary>
            /// Read an entry and return it.
            /// </summary>
            internal static EntryInstance Read(Type parent, ResReader reader)
            {
                var position = reader.Position;
                var size = reader.ReadUInt16();
                var flags = (EntryFlags) reader.ReadUInt16();
                reader.Position = position;

                if ((flags & EntryFlags.Complex) != 0)
                    return new ComplexEntryInstance(parent, reader);
                return new SimpleEntryInstance(parent, reader);
            }

            /// <summary>
            /// Read ctor
            /// </summary>
            protected EntryInstance(Type parent, ResReader reader)
            {
                this.parent = parent;

                // header
                var size = reader.ReadUInt16();
                flags = (EntryFlags) reader.ReadUInt16();
                key = StringPoolRef.Read(reader, parent.TypeSpec.Package.KeyStrings);
            }

            /// <summary>
            /// Create ctor
            /// </summary>
            protected EntryInstance(Type parent, EntryFlags flags, string key)
            {
                this.parent = parent;
                this.flags = flags;
                this.key = key;
            }

            /// <summary>
            /// Gets my flags
            /// </summary>
            public EntryFlags Flags
            {
                get { return flags; }
            }

            /// <summary>
            /// Is this a complex entry?
            /// If so, it is followed by an array of ReTable_map structures
            /// If not, it is followed by a Res_value structure
            /// </summary>
            public bool IsComplex
            {
                get { return ((flags & EntryFlags.Complex) != 0); }
            }

            /// <summary>
            /// If set, libraries are allowed to reference it.
            /// </summary>
            public bool IsPublic
            {
                get { return ((flags & EntryFlags.Public) != 0); }
            }

            /// <summary>
            /// Gets my key
            /// </summary>
            public string Key
            {
                get { return key; }
            }

            /// <summary>
            /// Gets my parent
            /// </summary>
            public Type Type
            {
                get { return parent; }
            }

            /// <summary>
            /// Write this entry.
            /// </summary>
            internal virtual void Write(ResWriter writer)
            {
                writer.WriteUInt16(IsComplex ? 16 : 8);
                writer.WriteUInt16((int) flags);
                StringPoolRef.Write(writer, parent.TypeSpec.Package.KeyStrings, key);
            }

            /// <summary>
            /// Prepare this chunk for writing
            /// </summary>
            internal virtual void PrepareForWrite()
            {
                StringPoolRef.Prepare(parent.TypeSpec.Package.KeyStrings, key);
            }
        }
    }
}
