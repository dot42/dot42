namespace Dot42.ApkLib.Resources
{
    /// <summary>
    /// ResTable_ref
    /// </summary>
    internal static class TableRef
    {
        /**
 *  This is a reference to a unique entry (a ResTable_entry structure)
 *  in a resource table.  The value is structured as: 0xpptteeee,
 *  where pp is the package index, tt is the type index in that
 *  package, and eeee is the entry index in that type.  The package
 *  and type values start at 1 for the first item, to help catch cases
 *  where they have not been supplied.
 */

        /// <summary>
        /// Reading ctor
        /// </summary>
        internal static int Read(ResReader reader)
        {
            return reader.ReadInt32();
        }

        /// <summary>
        /// Write helper
        /// </summary>
        internal static void Write(ResWriter writer, int value)
        {
            writer.WriteInt32(value);
        }
    }
}
