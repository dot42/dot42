using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Dot42.Utility
{
    public static class JarReferenceHash
    {
        /// <summary>
        /// Compute a hash of a the content of a file with the given path.
        /// KEEP THIS METHOD THE SAME BECAUSE THE BUILD RELIES ON IT
        /// </summary>
        public static string ComputeJarReferenceHash(string path)
        {
            var sha = SHA1.Create();
            using (var stream = File.OpenRead(path))
            {
                var hash = sha.ComputeHash(stream);
                return string.Join("", hash.Select(x => ConvByte((~x) & 0xFF)));
            }
        }

        public static string ComputeJarReferenceHash(byte[] jarData)
        {
            var sha = SHA1.Create();
            var hash = sha.ComputeHash(jarData);
            return string.Join("", hash.Select(x => ConvByte((~x) & 0xFF)));
        }
        
        private static string ConvByte(int value)
        {
            return value.ToString("x2");
        }

    }
}
