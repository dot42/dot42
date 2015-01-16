using System.Text;

namespace Dot42.DexLib.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.AppendFormat("{0:x2}", b);
            }
            return builder.ToString();
        }

        public static bool Match(this byte[] array, byte[] item, int offset)
        {
            for (int i = 0; i < item.Length; i++)
            {
                if (i >= array.Length || (array[i + offset] != item[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static int IndexOf(this byte[] array, byte[] item)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (Match(array, item, i))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}