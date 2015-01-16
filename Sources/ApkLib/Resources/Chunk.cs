using System;
using System.Diagnostics;
using System.IO;

namespace Dot42.ApkLib.Resources
{
    /// <summary>
    /// Base class for data chunk's in resources files.
    /// </summary>
    public abstract class Chunk
    {
        private readonly ChunkTypes type;
        private int headerSize;
        private int size;

        /// <summary>
        /// Writer ctor
        /// </summary>
        protected Chunk(ChunkTypes type)
        {
            this.type = type;
        }

        /// <summary>
        /// Reader ctor
        /// </summary>
        protected Chunk(ResReader reader)
        {
            type = (ChunkTypes)reader.ReadUInt16();
            headerSize = reader.ReadUInt16();
            size = reader.ReadInt32();
        }

        /// <summary>
        /// Reader ctor
        /// </summary>
        protected Chunk(ResReader reader, ChunkTypes expectedType)
            : this(reader)
        {
            if (type != expectedType)
            {
                throw new IOException(string.Format("Expected chunk of type 0x{0:X}, read 0x{1:X}.", (int)expectedType, (int)type));
            }
        }

        /// <summary>
        /// Gets the total size of this chunk.
        /// This includes the header plus any data.
        /// </summary>
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// Gets the size of the chunk header in bytes
        /// </summary>
        public int HeaderSize
        {
            get { return headerSize; }
        }

        /// <summary>
        /// Gets the total size of the data part of this chunk.
        /// This is calculated at Size - HeaderSize.
        /// </summary>
        public int DataSize
        {
            get { return Size - HeaderSize; }
        }

        /// <summary>
        /// Gets the type of this chunk
        /// </summary>
        public ChunkTypes Type
        {
            get { return type; }
        }

        /// <summary>
        /// Write this chunk.
        /// </summary>
        public void Write(ResWriter writer)
        {
            // Prepare
            PrepareForWrite();

            // First pass
            var position = writer.Stream.Position;
            WriteHeader(writer);
            headerSize = (int) (writer.Stream.Position - position);
            WriteData(writer);
            var endPosition = writer.Stream.Position;
            size = (int)(endPosition - position);

            if ((size & 0x03) != 0)
            {
                
            }

            // Update header
            writer.Stream.Position = position + 2;
            writer.WriteUInt16(headerSize);
            writer.WriteInt32(size);
            writer.Stream.Position = endPosition;
        }

        /// <summary>
        /// Prepare this chunk for writing
        /// </summary>
        protected internal virtual void PrepareForWrite()
        {
            // Override me
        }

        /// <summary>
        /// Write the header of this chunk.
        /// Always call the base method first.
        /// </summary>
        protected virtual void WriteHeader(ResWriter writer)
        {
            writer.WriteUInt16((int) type);
            writer.WriteUInt16(0); // Header size will be updated later
            writer.WriteInt32(0); // Size will be updated later
        }

        /// <summary>
        /// Write the data of this chunk.
        /// </summary>
        protected virtual void WriteData(ResWriter writer)
        {
            // Override me
        }

        /// <summary>
        /// Helper used to read chunks and check the actual size read.
        /// </summary>
        internal static T Read<T>(ResReader reader, Func<T> readAction)
            where T : Chunk
        {
            var startPosition = reader.Position;
            var result = readAction();
            var endPosition = reader.Position;
            var size = endPosition - startPosition;
            if (size != result.Size)
            {
#if DEBUG
                Debugger.Launch();
#endif
                throw new ArgumentException("Size mismatch");
            }
            return result;
        }
    }
}
