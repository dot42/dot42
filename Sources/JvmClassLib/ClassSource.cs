using System.IO;

namespace Dot42.JvmClassLib
{
    public class ClassSource
    {
        public readonly string FileName;

        /// <summary>
        /// Might be null if filename points to a file on disk.
        /// </summary>
        public readonly byte[] JarData;

        /// <summary>
        /// if not null, the precalculated hash of the jar stream.
        /// </summary>
        public readonly string JarStreamHash;

        /// <summary>
        /// If true, FileName points to an actual file on disk. If false,
        /// the JarStream should be used instead.
        /// </summary>
        public bool IsDiskFile { get { return JarStreamHash == null; } }

        public ClassSource(string fileName, byte[] jarData = null, string jarStreamHash = null)
        {
            FileName = fileName;
            JarData = jarData;
            JarStreamHash = jarStreamHash;
        }
    }
}