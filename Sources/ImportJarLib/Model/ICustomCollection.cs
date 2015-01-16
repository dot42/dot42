using System.Collections.Generic;

namespace Dot42.ImportJarLib.Model
{
    public interface ICustomCollection<T> : ICollection<T>
    {
        /// <summary>
        /// Insert a given item at the given index.
        /// </summary>
        void Insert(int index, T item);
    }
}
