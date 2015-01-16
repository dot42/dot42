using System;
using System.Collections.Generic;

namespace Dot42.Utility
{
    /// <summary>
    /// Enumerable methods optimized for lists.
    /// </summary>
    public static class FastEnumerable
    {
        /// <summary>
        /// Gets the last element in the list for which the given predicate returns true.
        /// </summary>
        public static T LastOrDefault<T>(this IList<T> list, Func<T, bool> predicate)
        {
            var index = list.Count - 1;
            while (index >= 0)
            {
                var current = list[index--];
                if (predicate(current))
                    return current;
            }
            return default(T);
        }
    }
}
