using System;
using System.Collections.Generic;
using System.Linq;

namespace Dot42.Utility
{
    /// <summary>
    /// Collection extension methods
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Is the given list null of does it have a count of 0?
        /// </summary>
        public static bool IsEmpty<T>(this IList<T> list)
        {
            return (list == null) || (list.Count == 0);
        }

        /// <summary>
        /// Is the given array null of does it have a count of 0?
        /// </summary>
        public static bool IsEmpty<T>(this T[] array)
        {
            return (array == null) || (array.Length == 0);
        }

        /// <summary>
        /// Remove a range out of the given list and return that range as a new list.
        /// </summary>
        public static List<T> CutRange<T>(this List<T> list, int start, int count)
        {
            var ret = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                ret.Add(list[start + i]);
            }
            list.RemoveRange(start, count);
            return ret;
        }

        /// <summary>
        /// Create an array that is the union of all members in a, with b added if b is not an element of a.
        /// </summary>
        public static T[] Union<T>(this T[] a, T b)
        {
            if (a.Length == 0)
                return new[] { b };
            if (Array.IndexOf(a, b) >= 0)
                return a;
            var res = new T[a.Length + 1];
            Array.Copy(a, 0, res, 0, a.Length);
            res[res.Length - 1] = b;
            return res;
        }

        /// <summary>
        /// Create an array that is the union of all members in a, with all members of b that are not already in a.
        /// </summary>
        public static T[] Union<T>(this T[] a, T[] b)
        {
            if (a == b)
                return a;
            if (a.Length == 0)
                return b;
            if (b.Length == 0)
                return a;
            if (a.Length == 1)
            {
                if (b.Length == 1)
                    return a[0].Equals(b[0]) ? a : new[] { a[0], b[0] };
                return b.Union(a[0]);
            }
            if (b.Length == 1)
                return a.Union(b[0]);
            return Enumerable.Union(a, b).ToArray();
        }

        /// <summary>
        /// Invoke the given action for each element in values.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var v in values)
                action(v);
        }

        /// <summary>
        /// Invoke the given action for each element in values.
        /// </summary>
        public static void ForEachWithExceptionMessage<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var v in values)
            {
                try
                {
                    action(v);
                }
                catch (AggregateException ex)
                {
                    throw new Exception("Error while handling " + v + ": " + ex.Flatten().InnerException.Message, ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while handling " + v + ": " + ex.Message, ex);
                }
            }
        }

    }
}
