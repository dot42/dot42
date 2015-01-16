namespace Dot42.ApkLib.Resources
{
    internal static class Mark
    {
        /// <summary>
        /// Mark a position in a writer so we can update that location later.
        /// </summary>
        internal abstract class AbstractMark<T>
        {
            private readonly long position;
            private readonly ResWriter writer;

            /// <summary>
            /// Default ctor
            /// </summary>
            protected AbstractMark(ResWriter writer)
            {
                this.writer = writer;
                position = writer.Stream.Position;
                Write(writer, default(T));
            }

            /// <summary>
            /// Set the value in the marked position
            /// </summary>
            public T Value
            {
                set
                {
                    var current = writer.Stream.Position;
                    writer.Stream.Position = position;
                    Write(writer, value);
                    writer.Stream.Position = current;
                }
            }

            /// <summary>
            /// Write the value to the given writer
            /// </summary>
            protected abstract void Write(ResWriter writer, T value);
        }

        /// <summary>
        /// 16-bit unsigned in mark.
        /// </summary>
        internal sealed class UInt16 : AbstractMark<int>
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            public UInt16(ResWriter writer)
                : base(writer)
            {
            }

            /// <summary>
            /// Write the value to the given writer
            /// </summary>
            protected override void Write(ResWriter writer, int value)
            {
                writer.WriteUInt16(value);
            }
        }

        /// <summary>
        /// 32-bit in mark.
        /// </summary>
        internal sealed class Int32 : AbstractMark<int>
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            public Int32(ResWriter writer)
                : base(writer)
            {
            }

            /// <summary>
            /// Write the value to the given writer
            /// </summary>
            protected override void Write(ResWriter writer, int value)
            {
                writer.WriteInt32(value);
            }
        }
    }
}