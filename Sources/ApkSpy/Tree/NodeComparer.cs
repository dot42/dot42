using System.Collections;

namespace Dot42.ApkSpy.Tree
{
    internal class NodeComparer : IComparer
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>. 
        /// </returns>
        /// <param name="x">The first object to compare. </param><param name="y">The second object to compare. </param><exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception><filterpriority>2</filterpriority>
        public int Compare(object ox, object oy)
        {
            var x = (Node)ox;
            var y = (Node)oy;

            var xIsDir = x is DirNode;
            var yIsDir = y is DirNode;

            if (xIsDir && !yIsDir)
                return -1;
            if (!xIsDir && yIsDir)
                return 1;

            return System.String.CompareOrdinal(x.Text, y.Text);
        }
    }
}
