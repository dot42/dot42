using System;
using System.Text;

namespace Dot42.ApkLib.Resources
{
    partial class Table
    {
        /// <summary>
        /// ResTable_config
        /// </summary>
        public sealed class Configuration : IEquatable<Configuration>
        {
            private readonly byte[] data;
            private const bool UseBigEndian = false;

            /// <summary>
            /// Creation ctor
            /// </summary>
            internal Configuration()
            {
                data = new byte[36 - 4];
            }

            /// <summary>
            /// Read ctor
            /// </summary>
            internal Configuration(ResReader reader)
            {
                var size = reader.ReadInt32();
                data = reader.ReadByteArray(size - 4);
            }

            /// <summary>
            /// Save to given writer
            /// </summary>
            internal void Write(ResWriter writer)
            {
                writer.WriteInt32(data.Length + 4);
                writer.WriteByteArray(data);
            }

            /// <summary>
            /// Mobile country code (from SIM).  0 means "any".
            /// </summary>
            public int MobileCountryCode
            {
                get { return GetInt16(0); }
                set { SetInt16(0, value); }
            }

            /// <summary>
            /// Mobile network code (from SIM).  0 means "any".
            /// </summary>
            public int MobileNetworkCode
            {
                get { return GetInt16(2); }
                set { SetInt16(2, value); }
            }

            /// <summary>
            /// Language (empty means "any"), en, fr etc.
            /// </summary>
            public string Language
            {
                get { return Get8BitString(4, 2); }
                set { Set8BitString(4, 2, value ?? string.Empty); }
            }

            /// <summary>
            /// Country (empty means "any"), US, CA etc.
            /// </summary>
            public string Country
            {
                get { return Get8BitString(6, 2); }
                set { Set8BitString(6, 2, value ?? string.Empty); }
            }

            /// <summary>
            /// Screen orientation
            /// </summary>
            public Orientation Orientation
            {
                get { return (Orientation) data[8]; }
                set { data[8] = (byte) value; }
            }

            /// <summary>
            /// Touchscreen type
            /// </summary>
            public Touchscreen Touchscreen
            {
                get { return (Touchscreen) data[9]; }
                set { data[0] = (byte) value; }
            }

            /// <summary>
            /// Screen density
            /// </summary>
            public Density Density
            {
                get { return (Density) GetInt16(10); }
                set { SetInt16(10, (int)value); }
            }

            /// <summary>
            /// Type of keyboard
            /// </summary>
            public Keyboard Keyboard
            {
                get { return (Keyboard) data[12]; }
                set { data[12] = (byte) value; }
            }

            /// <summary>
            /// Type of navigation
            /// </summary>
            public Navigation Navigation
            {
                get { return (Navigation) data[13]; }
                set { data[13] = (byte) value; }
            }

            // TODO input flags

            /// <summary>
            /// Screen width: 0 == any
            /// </summary>
            public int ScreenWidth
            {
                get { return GetInt16(16); }
                set { SetInt16(16, value); }
            }

            /// <summary>
            /// Screen height: 0 == any
            /// </summary>
            public int ScreenHeight
            {
                get { return GetInt16(18); }
                set { SetInt16(18, value); }
            }

            /// <summary>
            /// Target sdk version; 0 == any
            /// </summary>
            public int SdkVersion
            {
                get { return GetInt16(20); }
                set { SetInt16(20, value); }
            }

            /// <summary>
            /// For now minorVersion must always be 0!!!  Its meaning is currently undefined.
            /// </summary>
            public int MinorVersion
            {
                get { return GetInt16(22); }
            }

            /// <summary>
            /// Read a 16-bit unsigned int from the given offset.
            /// </summary>
            private int GetInt16(int offset)
            {
                return ResReader.DecodeInt16(data, offset, UseBigEndian);
            }

            /// <summary>
            /// Read a 32-bit unsigned int from the given offset.
            /// </summary>
            private int GetInt32(int offset)
            {
                return ResReader.DecodeInt32(data, offset, UseBigEndian);
            }

            /// <summary>
            /// Read a 8-bit character range from the given offset.
            /// </summary>
            private string Get8BitString(int offset, int maxLength)
            {
                var length = 0;
                while ((length < maxLength) && (data[offset + length] != 0))
                    length++;
                return Encoding.ASCII.GetString(data, offset, length);
            }

            /// <summary>
            /// Read a 16-bit unsigned int from the given offset.
            /// </summary>
            private void SetInt16(int offset, int value)
            {
                ResWriter.EncodeInt16(data, offset, value, UseBigEndian);
            }

            /// <summary>
            /// Read a 32-bit unsigned int from the given offset.
            /// </summary>
            private void SetInt32(int offset, int value)
            {
                ResWriter.EncodeInt32(data, offset, value, UseBigEndian);
            }

            /// <summary>
            /// Write a 8-bit character range from the given offset.
            /// </summary>
            private void Set8BitString(int offset, int maxLength, string value)
            {
                for (var i = 0; i < maxLength; i++)
                {
                    data[offset + i] = (i < value.Length) ? (byte)value[i] : (byte)0;
                }
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            public bool Equals(Configuration other)
            {
                var length = data.Length;
                var otherLength = (other != null) ? other.data.Length : 0;
                var otherData = (other != null) ? other.data : null;

                if (length != otherLength)
                    return false;
                for (var i = 0; i < length; i++)
                {
                    if (data[i] != otherData[i])
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Is this equal to other?
            /// </summary>
            public override bool Equals(object other)
            {
                return Equals(other as Configuration);
            }

            /// <summary>
            /// Get hash code
            /// </summary>
            public override int GetHashCode()
            {
                return (data != null ? data.GetHashCode() : 0);
            }

            /// <summary>
            /// Are all settings set to Any/Default?
            /// </summary>
            public bool IsAny
            {
                get
                {
                    if (MobileCountryCode != 0) return false;
                    if (MobileNetworkCode != 0) return false;

                    if (!string.IsNullOrEmpty(Language) || !string.IsNullOrEmpty(Country))
                        return false;

                    if (Orientation != Orientation.ANY) return false;
                    if (Touchscreen != Touchscreen.ANY) return false;
                    if (Density != Density.DEFAULT) return false;

                    if (Keyboard != Keyboard.ANY) return false;
                    if (Navigation != Navigation.ANY) return false;

                    if ((ScreenWidth != 0) || (ScreenHeight != 0))
                        return false;

                    if ((SdkVersion != 0) || (MinorVersion != 0))
                        return false;

                    return true;
                }
            }

            /// <summary>
            /// Convert to string
            /// </summary>
            public override string ToString()
            {
                if (IsAny)
                    return "Any";
                var sb = new StringBuilder();
                //sb.AppendFormat("size={0} ", data.Length + 4);
                if (MobileCountryCode != 0) sb.AppendFormat("MCC: {0}, ", MobileCountryCode);
                if (MobileNetworkCode != 0) sb.AppendFormat("MNC: {0}, ", MobileNetworkCode);

                if (!string.IsNullOrEmpty(Language) || !string.IsNullOrEmpty(Country))
                    sb.AppendFormat("Locale: {0}-{1}, ", Language, Country);

                if (Orientation != Orientation.ANY) sb.AppendFormat("Orientation: {0}, ", Orientation);
                if (Touchscreen != Touchscreen.ANY) sb.AppendFormat("Touchscreen: {0}, ", Touchscreen);
                if (Density != Density.DEFAULT) sb.AppendFormat("Density: {0}, ", Density);

                if (Keyboard != Keyboard.ANY) sb.AppendFormat("Keyboard: {0}, ", Keyboard);
                if (Navigation != Navigation.ANY) sb.AppendFormat("Navigation: {0}, ", Navigation);
                
                if ((ScreenWidth != 0) || (ScreenHeight != 0))
                    sb.AppendFormat("Screen size: {0}x{1}, ", ScreenWidth, ScreenHeight);
    
                if ((SdkVersion != 0) || (MinorVersion != 0))
                    sb.AppendFormat("SDK: {0}.{1}, ", SdkVersion, MinorVersion);

                //sb.AppendFormat("RAW: {0} ", string.Join(" ", data.Select(x => x.ToString("X2")).ToArray()));

                return sb.ToString();
            }

            /// <summary>
            /// Parse the filename for a valid configuration
            /// </summary>
            public static Configuration ParseFromFilename(string getFileNameWithoutExtension)
            {
                var config = new Configuration();
                // TODO do actual parsing 
                return config;
            }
        }
    }
}
