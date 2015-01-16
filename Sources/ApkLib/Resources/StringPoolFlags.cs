namespace Dot42.ApkLib.Resources
{
    public enum StringPoolFlags
    {
        // If set, the string index is sorted by the string values (based
        // on strcmp16()).
        SORTED_FLAG = 1 << 0,

        // String pool is encoded in UTF-8
        UTF8_FLAG = 1 << 8
    }
}
