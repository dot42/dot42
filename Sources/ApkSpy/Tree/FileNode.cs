using System;
using System.IO;

namespace Dot42.ApkSpy.Tree
{
    internal abstract class FileNode : Node
    {
        private readonly SourceFile source;
        private readonly string fileName;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected FileNode(SourceFile source, string fileName)
        {
            this.source = source;
            this.fileName = fileName;
            Text = Path.GetFileName(fileName);
        }

        /// <summary>
        /// Gets the containing source file
        /// </summary>
        public SourceFile Source { get { return source; } }

        /// <summary>
        /// Gets the name of the entry
        /// </summary>
        public string FileName { get { return fileName; } }

        /// <summary>
        /// Load the file.
        /// </summary>
        protected byte[] Load()
        {
            if (source.IsApk)
                return source.Apk.Load(FileName);
            if (source.IsJar)
                return source.Jar.GetResource(FileName);
            if (source.IsSingleFile)
                return File.ReadAllBytes(source.SingleFilePath);
            throw new NotSupportedException("Unknown source");
        }
    }
}
