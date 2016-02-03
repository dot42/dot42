using System.Collections;
using System.Collections.Generic;

namespace Dot42.Utility
{
    /// <summary>
    /// Speeds up IndexOf and Contains Operations of the wrapped list by keeping 
    /// a dictionary of indices of the contained values. Does not allow duplicate
    /// values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IndexLookupList<T> : IList<T>
    {
        private readonly IList<T> _list;
        
        // use an array, so that we can quickly update values in the dictionary.
        private readonly Dictionary<T, int[]> _indices;

        public IndexLookupList()
        {
            _list = new List<T>();
            _indices = new Dictionary<T, int[]>();
        }

        public IndexLookupList(IList<T> list)
        {
            _list = list;
            _indices = new Dictionary<T, int[]>(list.Count);

            for(int i = 0; i < list.Count; ++i)
                _indices.Add(list[i], new [] {i});
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }

        public void Add(T item)
        {
            _indices.Add(item, new [] { _list.Count });
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
            _indices.Clear();
        }

        public bool Contains(T item)
        {
            return _indices.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            int[] idxA;
            if (!_indices.TryGetValue(item, out idxA))
                return false;

            int idx = idxA[0];

            RemoveAt(idx);
            return true;
        }

        public int Count
        {
            get { return _list.Count; } 
        }

        public bool IsReadOnly
        {
            get { return _list.IsReadOnly; } 
        }

        public int IndexOf(T item)
        {
            int[] idxA;
            if (!_indices.TryGetValue(item, out idxA))
                return -1;
            return idxA[0];
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);

            foreach (var valA in _indices.Values)
            {
                int val = valA[0];
                if (val >= index)
                    valA[0] = val + 1;
            }

            _indices[item] = new[] { index };
        }

        public void RemoveAt(int index)
        {
            T item = _list[index];

            _list.RemoveAt(index);
            _indices.Remove(item);

            foreach (var valA in _indices.Values)
            {
                int val = valA[0];
                if (val > index)
                    valA[0] = val - 1;
            }
        }

        public T this[int index]
        {
            get { return _list[index]; }
            set
            {
                T old = _list[index];
                _indices.Remove(old);
                _list[index] = value;
                _indices[value] = new [] {index};
            }
        }
    }
}
