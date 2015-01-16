using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Math;

namespace Dot42.BarDeployLib
{
    internal static class PwCrypto
    {
        public const int ALGORITHM_SHA1_V1 = 0;
        public const int ALGORITHM_SHA1_V2 = 1;
        public const int ALGORITHM_SHA512_V2 = 2;
        private static readonly char[] HEX_CHARS = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private static readonly Encoding encoding = Encoding.UTF8;

        public static String hashAndEncode(String password, int hashalg, String salt, int icount, String challenge)
        {
            var bibytes = new BigInteger(salt, 16).ToByteArray();
            var saltbytes = new byte[8];

            int max = Math.Min(bibytes.Length, saltbytes.Length);
            for (int i = 1; i <= max; i++)
            {
                saltbytes[(8 - i)] = bibytes[(bibytes.Length - i)];
            }

            return hashAndEncode(encoding.GetBytes(password), hashalg, saltbytes, icount, encoding.GetBytes(challenge));
        }

        public static String hashAndEncode(byte[] password, int hashalg, byte[] salt, int icount, byte[] challenge)
        {
            byte[] hash;
            if (hashalg == 0)
            {
                hash = sha1(password);
            }
            else
            {
                hash = HashPasswordV2(hashalg, salt, icount, password);
            }

            if (challenge.Length > 0)
            {
                var ms = new MemoryStream();
                ms.Write(challenge, 0, challenge.Length);
                ms.Write(hash, 0, hash.Length);

                if (hashalg == 0)
                    hash = sha1(ms.ToArray());
                else
                {
                    hash = HashPasswordV2(hashalg, salt, icount, ms.ToArray());
                }
            }
            return hexEncode(hash);
        }

        private static string hexEncode(byte[] buffer)
        {
            var sb = new StringBuilder();

            foreach (byte entry in buffer)
            {
                sb.Append(HEX_CHARS[((0xF0 & entry) >> 4)]);
                sb.Append(HEX_CHARS[(0xF & entry)]);
            }

            return sb.ToString();
        }

        private static byte[] HashPasswordV2(int hashAlgo, byte[] salt, int iterationCount, byte[] password)
        {
            int count = 0;

            byte[] hashedData = password;
            do
            {
                var buf = new MemoryStream();
                buf.Write(BitConverter.GetBytes(count), 0, 4);
                Debug.Assert(BitConverter.IsLittleEndian);
                //buf.order(ByteOrder.LITTLE_ENDIAN);
                //buf.putInt(count);
                buf.Write(salt, 0, salt.Length);
                buf.Write(hashedData, 0, hashedData.Length);

                if (hashAlgo == 1)
                    hashedData = sha1(buf.ToArray());
                else if (hashAlgo == 2)
                {
                    hashedData = sha512(buf.ToArray());
                }

                count++;
            } while (count < iterationCount);
            return hashedData;
        }

        private static byte[] sha1(byte[] plaintext)
        {
            var md = SHA1Managed.Create();
            return md.ComputeHash(plaintext);
        }

        private static byte[] sha512(byte[] plaintext)
        {
            var md = SHA512Managed.Create();
            return md.ComputeHash(plaintext);
        }

    }
}
