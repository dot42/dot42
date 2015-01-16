namespace Dot42.ApkLib.Resources
{
    /// <summary>
    /// Res_value
    /// </summary>
    internal static class StringPoolRef
    {
        /// <summary>
        /// Reading ctor
        /// </summary>
        internal static string Read(ResReader reader, StringPool pool)
        {
            var index = reader.ReadInt32();
            return (index < 0) ? null : pool[index];
        }

        /// <summary>
        /// Make sure string is added to pool
        /// </summary>
        internal static void Prepare(StringPool pool, string value, int resourceId = -1)
        {
            if (value != null)
            {
                pool.Add(value, resourceId);
            }
        }

        /// <summary>
        /// Write helper
        /// </summary>
        internal static void Write(ResWriter writer, StringPool pool, string value, int resourceId = -1)
        {
            var index = (value == null) ? -1 : pool.Get(value, resourceId);
            writer.WriteInt32(index);
        }
    }
}
