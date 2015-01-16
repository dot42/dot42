namespace Dot42.DexLib.IO.Markers
{
    internal class SizeOffset
    {
        public SizeOffset(uint size, uint offset)
        {
            Size = size;
            Offset = offset;
        }

        public uint Size { get; set; }
        public uint Offset { get; set; }
    }
}