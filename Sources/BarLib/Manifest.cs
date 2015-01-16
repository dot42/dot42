using System.Collections.Generic;
using System.IO;

namespace Dot42.BarLib
{
    /// <summary>
    /// Wrapper for META-INF/MANIFEST.MF
    /// </summary>
    public class Manifest
    {
        private readonly Dictionary<string, string> items = new Dictionary<string, string>();

        /// <summary>
        /// Load from the given stream.
        /// </summary>
        internal Manifest(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim().Length == 0)
                        continue;
                    var index = line.IndexOf(':');
                    if (index < 0)
                        continue;
                    var key = line.Substring(0, index).Trim();
                    var value = line.Substring(index + 1).Trim();
                    items[key] = value;
                }
            }
        }

        /// <summary>
        /// Gets the Package-Id
        /// </summary>
        public string PackageId { get { return Get("Package-Id"); } }

        /// <summary>
        /// Gets the Package-Name
        /// </summary>
        public string PackageName { get { return Get("Package-Name"); } }

        /// <summary>
        /// Gets the Package-Author
        /// </summary>
        public string PackageAuthor { get { return Get("Package-Author"); } }

        /// <summary>
        /// Gets the Package-Author-Id
        /// </summary>
        public string PackageAuthorId { get { return Get("Package-Author-Id"); } }

        /// <summary>
        /// Gets the Package-Type
        /// </summary>
        public string PackageType { get { return Get("Package-Type"); } }

        /// <summary>
        /// Gets the value for the given key of null if not found.
        /// </summary>
        public string Get(string key)
        {
            string value;
            if (items.TryGetValue(key, out value))
                return value;
            return null;
        }
    }
}
