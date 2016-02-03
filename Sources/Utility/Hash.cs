using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dot42.Utility
{
    public static class Hash
    {
        /// <summary>
        /// Create a hash of the given strings.
        /// </summary>
        public static string Create(params string[] content)
        {
            var sb = new StringBuilder();
            foreach (var x in content)
            {
                sb.Append(x ?? "<null>");
            }
            var hash = SHA1.Create();
            var hashed = hash.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return string.Join("", hashed.Select(x => x.ToString("X2")));
        }


        public static string HashFileMD5(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return HashFileMD5(fs);
            }
        }

        public static string HashFileMD5(FileStream stream)
        {
            StringBuilder sb = new StringBuilder();

            if( stream != null )
            {
                stream.Seek( 0, SeekOrigin.Begin );

                var hashProvider = MD5.Create();

                byte[] hash = hashProvider.ComputeHash(stream);
                foreach( byte b in hash )
                    sb.Append( b.ToString( "x2" ) );

                stream.Seek( 0, SeekOrigin.Begin );
            }

            return sb.ToString();
        }
    }
}
