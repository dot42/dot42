using System.Collections.Generic;
using System.Linq;

namespace Dot42.JvmClassLib.Attributes
{
    /// <summary>
    /// Representation of LineNumberTable attribute.
    /// </summary>
    public sealed class LineNumberTableAttribute : Attribute
    {
        internal const string AttributeName = "LineNumberTable";

        private readonly List<LineNumber> lineNumbers = new List<LineNumber>();
        private bool sorted;

        /// <summary>
        /// Gets the name of this attribute
        /// </summary>
        public override string Name
        {
            get { return AttributeName; }
        }

        /// <summary>
        /// Gets all line number entries.
        /// </summary>
        public IEnumerable<LineNumber> LineNumbers { get { return lineNumbers; } }

        /// <summary>
        /// Gets the line number matching the given offset.
        /// </summary>
        public int GetLineNumber(int offset)
        {
            if (!sorted)
            {
                lineNumbers.Sort();
                sorted = true;
            }
            var entry = lineNumbers.LastOrDefault(x => x.StartPC <= offset);
            return (entry != null) ? entry.Number : 0;
        }

        /// <summary>
        /// Add the given line number to the list
        /// </summary>
        internal void Add(LineNumber lineNumber)
        {
            lineNumbers.Add(lineNumber);
            sorted = false;
        }
    }
}
