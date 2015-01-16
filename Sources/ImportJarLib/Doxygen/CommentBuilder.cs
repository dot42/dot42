using System.IO;

namespace Dot42.ImportJarLib.Doxygen
{
    internal class CommentBuilder
    {
        private CommentSection summary;
        private CommentSection example;
        private CommentSection returns;
        private CommentSection remarks;
        private CommentSection javaName;

        /// <summary>
        /// Gets the summary part.
        /// </summary>
        public CommentSection Summary
        {
            get { return summary ?? (summary = new CommentSection("summary")); }
        }

        /// <summary>
        /// Gets the example part.
        /// </summary>
        public CommentSection Example
        {
            get { return example ?? (example = new CommentSection("example")); }
        }

        /// <summary>
        /// Gets the returns part.
        /// </summary>
        public CommentSection Returns
        {
            get { return returns ?? (returns = new CommentSection("returns")); }
        }

        /// <summary>
        /// Gets the remarks part.
        /// </summary>
        public CommentSection Remarks
        {
            get { return remarks ?? (remarks = new CommentSection("remarks")); }
        }

        /// <summary>
        /// Gets the java name part.
        /// </summary>
        public CommentSection JavaName
        {
            get { return javaName ?? (javaName = new CommentSection("java-name")); }
        }

        /// <summary>
        /// Write this section to the given writer.
        /// </summary>
        public void WriteTo(TextWriter writer, string indent)
        {
            if (summary != null) summary.WriteTo(writer, indent);
            if (returns != null) returns.WriteTo(writer, indent);
            if (example != null) example.WriteTo(writer, indent);
            if (remarks != null) remarks.WriteTo(writer, indent);
            if (javaName != null) javaName.WriteTo(writer, indent);
        }
    }
}
