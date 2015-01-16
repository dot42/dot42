namespace Dot42.DexLib.IO
{
    internal static class DexConsts
    {
        internal const uint Endian = 0x12345678;
        internal const uint ReverseEndian = 0x78563412;
        internal const uint NoIndex = 0xffffffff;
        internal const char InnerClassMarker = '$';
        internal const int SignatureSize = 20;
        internal const int HeaderSize = 0x70;
        internal static readonly byte[] FileMagic = {0x64, 0x65, 0x78, 0x0a, 0x30, 0x33, 0x35, 0x00};
    }
}