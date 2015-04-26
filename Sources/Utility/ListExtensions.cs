using System;
using System.Collections.Generic;

namespace NinjaTools.Collections
{
    public static class ListExtensions
    {
        private static int BinarySearch<T>(this IList<T> list, T value)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            var comp = Comparer<T>.Default;

            if (list.Count == 0) return -1;
            if (comp.Compare(list[list.Count-1], value) < 0) // der letzte wert ist noch kleiner: kein Wert passt.
                return -1;

            int lo = 0, hi = list.Count - 1;
            
            while (lo < hi)
            {
                int m = (hi + lo) / 2;  // this might overflow; be careful.
                if (comp.Compare(list[m], value) < 0) lo = m + 1;
                else hi = m - 1;
            }
            if (comp.Compare(list[lo], value) < 0) lo++;
            return lo;
        }

        private static int BinarySearch<T,E>(this IList<T> list, E value, Func<T,E> elementSelector, Comparer<E> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if(comparer == null)
                comparer = Comparer<E>.Default;

            E eValue = value;

            if (list.Count == 0) return -1;
            if (comparer.Compare(elementSelector(list[list.Count - 1]), eValue) < 0) // der letzte wert ist noch kleiner: kein Wert passt.
                return -1;

            int lo = 0, hi = list.Count - 1;

            while (lo < hi)
            {
                int m = (hi + lo) / 2;  // this might overflow; be careful.
                if (comparer.Compare(elementSelector(list[m]), eValue) < 0) lo = m + 1;
                else hi = m - 1;
            }
            if (comparer.Compare(elementSelector(list[lo]), eValue) < 0) lo++;
            return lo;
        }
        
        public static int FindFirstIndexGreaterThanOrEqualTo<T> (this IList<T> sortedList, T key)
        {
            return BinarySearch(sortedList, key);
        }

        public static int FindFirstIndexGreaterThanOrEqualTo<T, E>(this IList<T> sortedList, E key, Func<T,E> elementSelector)
        {
            return BinarySearch(sortedList, key, elementSelector);
        }

        public static int FindLastIndexSmallerThanOrEqualTo<T, E>(this IList<T> sortedList, E key, Func<T, E> elementSelector)
        {
            var comparer = Comparer<E>.Default;
            int idx = BinarySearch(sortedList, key, elementSelector, comparer);

            if (idx == -1)
            {
                // last index smaller than key: take last index
                return sortedList.Count - 1;
            }

            // equals: done.
            if (comparer.Compare(elementSelector(sortedList[idx]), key) == 0)
                return idx;
            // subtract one to get the smaller-than element [or none, if idx is zero]
            return idx - 1;
        }


        public static int FindFirstIndexGreaterThanOrEqualTo<T>(this List<T> sortedList, T key)
        {
            return BinarySearch(sortedList, key);
        }

        public static IEnumerable<T> FastReverse<T>(this IList<T> items)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                yield return items[i];
            }
        }
    }
}
