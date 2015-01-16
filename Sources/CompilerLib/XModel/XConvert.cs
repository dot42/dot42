using System;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Android targeting Convert replacement
    /// </summary>
    internal static class XConvert
    {
        /// <summary>
        /// Convert a value to 8-bit integer.
        /// </summary>
        internal static sbyte ToByte(object x)
        {
            return unchecked((sbyte)Convert.ToByte(Convert.ToInt64(x) & 0xFFL));
        }

        /// <summary>
        /// Convert a value to 16-bit integer.
        /// </summary>
        internal static char ToChar(object x)
        {
            return Convert.ToChar(Convert.ToInt64(x) & 0xFFFFL); 
        }

        /// <summary>
        /// Convert a value to 16-bit integer.
        /// </summary>
        internal static short ToShort(object x)
        {
            return unchecked((short)Convert.ToUInt16(Convert.ToInt64(x) & 0xFFFFL));
        }

        /// <summary>
        /// Convert a value to 32-bit integer.
        /// </summary>
        internal static int ToInt(object x)
        {
            return unchecked((int)Convert.ToUInt32(Convert.ToInt64(x) & 0xFFFFFFFFL));
        }

        /// <summary>
        /// Convert a value to 64-bit integer.
        /// </summary>
        internal static long ToLong(object x)
        {
            if (x is ulong)
            {
                return unchecked((long)Convert.ToUInt64(x));
            }
            return Convert.ToInt64(x);
        }

        /// <summary>
        /// Change type of the given constant.
        /// </summary>
        public static object ChangeType(object constant, Type valueType)
        {
            if ((valueType == typeof (int)) || (valueType == typeof (uint)))
                return ToInt(constant);
            if ((valueType == typeof(long)) || (valueType == typeof(ulong)))
                return ToLong(constant);
            return Convert.ChangeType(constant, valueType);
        }
    }
}
