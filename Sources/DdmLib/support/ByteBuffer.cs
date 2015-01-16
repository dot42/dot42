using System;

namespace Dot42.DdmLib.support
{
    public class ByteBuffer
    {
        public static ByteBuffer allocate(int capacity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns this buffer's limit.
        /// </summary>
        public int limit
        {
            get { throw new NotImplementedException();}
            set { throw new NotImplementedException();}
        }

        /// <summary>
        /// Relative get method for reading an int value.
        /// Reads the next four bytes at this buffer's current position, composing them into an int value according to the current byte order, and then increments the position by four.
        /// </summary>
        public int getInt() { throw new NotImplementedException();}

        public int getInt(int index)
        {
            throw new NotImplementedException();
        }

        public char getChar()
        {
            throw new NotImplementedException(); 
        }

        public short getShort()
        {
            throw new NotImplementedException();
        }

        public long getLong()
        {
            throw new NotImplementedException();
        }

        public void put(byte value)
        {
            throw new NotImplementedException();
        }

        public void put(byte[] value)
        {
            throw new NotImplementedException();
        }

        public void put(ByteBuffer src)
        {
            throw new NotImplementedException();
        }

        public void put(int index, byte value)
        {
            throw new NotImplementedException();
        }

        public void putChar(char value)
        {
            throw new NotImplementedException();
        }

        public void putChar(int index, char value)
        {
            throw new NotImplementedException();
        }

        public void putInt(int value)
        {
            throw new NotImplementedException();
        }

        public void putInt(int index, int value)
        {
            throw new NotImplementedException();
        }

        public int position
        {
            get { throw new NotImplementedException();}
            set { throw new NotImplementedException();}
        }

        public byte get()
        {
            throw new NotImplementedException();
        }

        public byte get(int index)
        {
            throw new NotImplementedException();
        }

        public void get(byte[] copy)
        {
            throw new NotImplementedException();
        }

        public void get(byte[] copy, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public ByteOrder order { get; set; }

        /// <summary>
        /// Returns the byte array that backs this buffer  (optional operation).
        /// </summary>
        public byte[] array()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the offset within this buffer's backing array of the first element of the buffer  (optional operation).
        /// </summary>
        /// <returns></returns>
        public int arrayOffset()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new, read-only byte buffer that shares this buffer's content.
        /// </summary>
        /// <returns></returns>
        public ByteBuffer asReadOnlyBuffer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns this buffer's capacity.
        /// </summary>
        public int capacity
        {
            get { throw new NotImplementedException(); }
        }

        public void clear()
        {
            throw new NotImplementedException();
        }

        public void compact()
        {
            throw new NotImplementedException();
        }

        public void flip()
        {
            throw new NotImplementedException();
        }

        public static ByteBuffer wrap(byte[] copy)
        {
            throw new NotImplementedException();
        }

        public static ByteBuffer wrap(byte[] copy, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public void rewind()
        {
            throw new NotImplementedException();
        }

        public ByteBuffer slice()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(ByteBuffer other)
        {
            throw new NotImplementedException();
        }
    }
}
