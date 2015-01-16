using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Dot42.DdmLib.support
{
    internal static class Extensions
    {
        /// <summary>
        /// Decodes a String into an Integer. Accepts decimal, hexadecimal, and octal numbers given by the following grammar:
        /// DecodableString:
        /// Signopt DecimalNumeral
        /// Signopt 0x HexDigits
        /// Signopt 0X HexDigits
        /// Signopt # HexDigits
        /// Signopt 0 OctalDigits
        /// Sign:
        /// </summary>
        internal static int decode(this string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException();
            var sign = 1;
            if (value[0] == '-')
            {
                sign = -1;
                value = value.Substring(1);
            }

            if (value[0] == '#')
            {
                return sign * int.Parse(value.Substring(1), NumberStyles.HexNumber);
            }
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return sign * int.Parse(value.Substring(2), NumberStyles.HexNumber);
            }

            if ((value.Length > 1) && (value[0] == '0') && (char.IsDigit(value[1])))
            {
                // octal
                var result = 0;
                while (value.Length > 0)
                {
                    var x = (int)(value[0] - '0');
                    result = result * 8 + x;
                    value = value.Substring(1);
                }
                return result * sign;
            }

            return sign*int.Parse(value);
        }

         public static int reverseBytes(this int i)
         {
             return ((i >> 24)) |
                    ((i >> 8) & 0xFF00) |
                    ((i << 8) & 0xFF0000) |
                    ((i << 24));
         }

        internal static string toHexString(this int value)
        {
            return value.ToString("X8");
        }

        internal static string toHexString(this long value)
        {
            return value.ToString("X16");
        }

        internal static string group(this Match match, int groupNr)
        {
            return match.Groups[groupNr - 1].Value;
        }

        internal static bool matches(this Match match)
        {
            return match.Success;
        }

        internal static bool matches(this string source, string pattern)
        {
            return new Regex(pattern).IsMatch(source);
        }

        internal static byte[] getBytes(this string value, string encodingName)
        {
            var encoding = Encoding.GetEncoding(encodingName);
            return encoding.GetBytes(value);
        }

        internal static string getString(this byte[] value, string encodingName)
        {
            var encoding = Encoding.GetEncoding(encodingName);
            return encoding.GetString(value);
        }

        internal static string getString(this byte[] value, int offset, int length, string encodingName = null)
        {
            var encoding = (encodingName != null) ? Encoding.GetEncoding(encodingName) : Encoding.Default; 
            return encoding.GetString(value, offset, length);
        }

        internal static string replaceAll(this string source, string from, string to)
        {
            var result = source.Replace(from, to);
            while (result != source)
            {
                source = result;
                result = source.Replace(from, to);
            }
            return result;
        }
    }
}
