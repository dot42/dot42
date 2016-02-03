using System.IO;

namespace Dot42.Mapping
{
    /// <summary>
    /// represents a DocumentPosition within a specific Document
    /// </summary>
    public class SourceCodePosition 
    {
        public readonly Document Document;
        public readonly DocumentPosition Position;

        public SourceCodePosition(Document document, DocumentPosition position)
        {
            Document = document;
            Position = position;
        }

        /// <summary>
        /// return true if this is a compiler generated instruction
        /// with no source code attached. Debuggers should step through
        /// the instruction.
        /// </summary>
        public bool IsSpecial { get { return Position.IsSpecial; } }

        internal static SourceCodePosition Create(Document document, DocumentPosition position)
        {
            return new SourceCodePosition(document, position);
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}, {2}]", Position, Path.GetFileName(Document.Path), Path.GetDirectoryName(Document.Path));

        }
    }
}