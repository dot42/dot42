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
    }
}
