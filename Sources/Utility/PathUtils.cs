using System.IO;

namespace Dot42.Utility
{
    public static class PathUtils
    {
        /// <summary>
        /// Add a folder separator to the given path when needed.
        /// </summary>
        public static string AddDirSeparator(this string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            var sep = Path.DirectorySeparatorChar;
            if (path[path.Length - 1] != sep)
                return path + sep;
            return path;
        }

        /// <summary>
        /// Make the given path relative to the given parent.
        /// </summary>
        public static string MakeRelativeTo(this string path, string parent)
        {
            path = Path.GetFullPath(path);
            parent = AddDirSeparator(Path.GetFullPath(parent));

            if (path.StartsWith(parent))
                return path.Substring(parent.Length);
            return path;
        }
    }
}
