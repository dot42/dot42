using System;
using System.Diagnostics;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.Mapping
{
    /// <summary>
    /// Code position within a document
    /// </summary>
    [DebuggerDisplay("{Start}-{End} {TypeId}:{MethodId}:{MethodOffset}")]
    public sealed class DocumentPosition : IComparable<DocumentPosition>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public DocumentPosition(int startLine, int startCol, int endLine, int endCol, int typeId, int methodId, int methodOffset)
        {
            Start = new DocumentPoint(startLine, startCol);
            End = new DocumentPoint(endLine, endCol);
            TypeId = typeId;
            MethodId = methodId;
            MethodOffset = methodOffset;
        }

        /// <summary>
        /// XML ctor
        /// </summary>
        public DocumentPosition(XElement element)
        {
            var startLine = int.Parse(element.GetAttribute("sl"));
            var startColumn = int.Parse(element.GetAttribute("sc"));
            var endLine = int.Parse(element.GetAttribute("el") ?? startLine.ToString());
            var endColumn = int.Parse(element.GetAttribute("ec") ?? startColumn.ToString());
            Start = new DocumentPoint(startLine, startColumn);
            End = new DocumentPoint(endLine, endColumn);

            TypeId = int.Parse(element.GetAttribute("ti"));
            MethodId = int.Parse(element.GetAttribute("mi"));
            MethodOffset = int.Parse(element.GetAttribute("mo"));
        }

        /// <summary>
        /// Convert to XML element
        /// </summary>
        internal XElement ToXml(string elementName)
        {
            var element = new XElement(elementName,
                                new XAttribute("sl", Start.Line.ToString()),
                                new XAttribute("sc", Start.Column.ToString()));
            if (Start.Line != End.Line)
                element.Add(new XAttribute("el", End.Line.ToString()));
            if (Start.Column != End.Column)
                element.Add(new XAttribute("ec", End.Column.ToString()));
            element.Add(new XAttribute("ti", TypeId.ToString()));
            element.Add(new XAttribute("mi", MethodId.ToString()));
            element.Add(new XAttribute("mo", MethodOffset.ToString()));
            return element;
        }

        public DocumentPoint Start { get; set; }
        public DocumentPoint End { get; set; }
        public int TypeId { get; set; }
        public int MethodId { get; set; }
        public int MethodOffset { get; set; }
        public bool IsReturn { get; set; } // Not persisted, used to avoid removing mapping for return address

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(DocumentPosition other)
        {
            if (Start.Line < other.Start.Line) return -1;
            if (Start.Line > other.Start.Line) return 1;

            if (Start.Column < other.Start.Column) return -1;
            if (Start.Column > other.Start.Column) return 1;

            if (End.Line < other.End.Line) return -1;
            if (End.Line > other.End.Line) return 1;

            if (End.Column < other.End.Column) return -1;
            if (End.Column > other.End.Column) return 1;

            if (MethodOffset < other.MethodOffset) return -1;
            if (MethodOffset > other.MethodOffset) return 1;

            if (MethodId < other.MethodId) return -1;
            if (MethodId > other.MethodId) return 1;

            if (TypeId < other.TypeId) return -1;
            if (TypeId > other.TypeId) return 1;

            return 0;
        }

        /// <summary>
        /// Does this document position contain the given line+column?
        /// </summary>
        public bool Contains(int line, int col)
        {
            if (line < Start.Line) return false;
            if (line > End.Line) return false;

            if ((line == Start.Line) && (col < Start.Column)) return false;
            if ((line == End.Line) && (col > End.Column)) return false;

            return true;
        }

        /// <summary>
        /// Is there an intersection between the given range and my range?
        /// </summary>
        public bool Intersects(int startLine, int startCol, int endLine, int endCol)
        {
            var start = new DocumentPoint(startLine, startCol);
            var end = new DocumentPoint(endLine, endCol);

            if (this.Start > end) return false;
            if (this.End < start) return false;

            return true;
        }

        /// <summary>
        /// Is this position equal to the other position except for the method offset?
        /// </summary>
        public bool EqualExceptOffset(DocumentPosition other)
        {
            return (Start == other.Start) &&
                   (End == other.End) &&
                   (TypeId == other.TypeId) &&
                   (MethodId == other.MethodId);
        }

        /// <summary>
        /// return True if this is a compiler generated instruction
        /// with no sourc code attached. Debugger should step through
        /// the instruction.
        /// </summary>
        public bool IsSpecial
        {
            get { return Start.Line == 0xfeefee; }
        }
    }
}
