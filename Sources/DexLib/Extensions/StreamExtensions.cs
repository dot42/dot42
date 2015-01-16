using System.IO;

namespace Dot42.DexLib.Extensions
{
    public static class StreamExtensions
    {
        // Note: already implemented in .NET 4.0
        public static void CopyTo(this Stream source, Stream destination)
        {
            int num;
            var buffer = new byte[0x1000];
            while ((num = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, num);
            }
        }
    }
}