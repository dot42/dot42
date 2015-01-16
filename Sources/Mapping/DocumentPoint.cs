using System;
using System.Diagnostics;

namespace Dot42.Mapping
{
    /// <summary>
    /// Line+columm position within a document
    /// </summary>
    [DebuggerDisplay("{Line},{Column}")]
    public struct DocumentPoint : IComparable<DocumentPoint>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public DocumentPoint(int startLine, int startCol)
        {
            Line = startLine;
            Column = startCol;
        }

        public readonly int Line;
        public readonly int Column;

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(DocumentPoint other)
        {
            if (Line < other.Line) return -1;
            if (Line > other.Line) return 1;

            if (Column < other.Column) return -1;
            if (Column > other.Column) return 1;

            return 0;
        }

        /// <summary>
        /// Is there an intersection between the given range my location?
        /// </summary>
        public bool IsContainedIn(int startLine, int startCol, int endLine, int endCol)
        {
            if ((Line < startLine) || (Line > endLine)) return false;
            if ((Line == startLine) && (Column < startCol)) return false;
            if ((Line == endLine) && (Column > endCol)) return false;
            return true;
        }

        public static bool operator <(DocumentPoint a, DocumentPoint b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator <=(DocumentPoint a, DocumentPoint b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator >(DocumentPoint a, DocumentPoint b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator >=(DocumentPoint a, DocumentPoint b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator ==(DocumentPoint a, DocumentPoint b)
        {
            return a.CompareTo(b) == 0;
        }

        public static bool operator !=(DocumentPoint a, DocumentPoint b)
        {
            return a.CompareTo(b) != 0;
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", Line, Column);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}
