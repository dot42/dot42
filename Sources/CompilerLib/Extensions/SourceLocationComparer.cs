using System;
using System.Collections.Generic;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Compare sequence points such that they are ordered.
    /// </summary>
    internal static class SourceLocationComparer 
    {
        /// <summary>
        /// Default instance
        /// </summary>
        internal static readonly IComparer<ISourceLocation> Instance = new Comparer();

        /// <summary>
        /// Is sequence point x equal to sequence point y?
        /// </summary>
        public static bool IsEqual(this ISourceLocation x, ISourceLocation y)
        {
            return (Instance.Compare(x, y) == 0);
        }

        /// <summary>
        /// Compare class.
        /// </summary>
        private sealed class Comparer : IComparer<ISourceLocation>
        {
            public int Compare(ISourceLocation x, ISourceLocation y)
            {
                // Compare on null refs.
                if ((x == null) && (y == null)) return 0;
                if (x == null) return 1;
                if (y == null) return -1;

                // Compare on url.
                var docx = x.Document ?? "";
                var docy = y.Document ?? "";

                var rc = string.Compare(docx, docy, StringComparison.OrdinalIgnoreCase);
                if (rc != 0) return rc;

                // Compare on start line.
                rc = Compare(x.StartLine, y.StartLine);
                if (rc != 0) return rc;

                // Compare on start column.
                rc = Compare(x.StartColumn, y.StartColumn);
                if (rc != 0) return rc;

                // Compare on end line.
                rc = Compare(x.EndLine, y.EndLine);
                if (rc != 0) return rc;

                // Compare on end column.
                rc = Compare(x.EndColumn, y.EndColumn);
                if (rc != 0) return rc;

                // They are equal.
                return 0;
            }

            /// <summary>
            /// Compare 2 numbers.
            /// </summary>
            private static int Compare(int x, int y)
            {
                if (x < y) return -1;
                if (x > y) return 1;
                return 0;
            }
        }
    }
}
