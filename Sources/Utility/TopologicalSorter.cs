using System;
using System.Collections.Generic;
using System.Linq;

namespace Dot42.Utility
{ /* Taken from (great thanks)
 * http://stackoverflow.com/questions/1982592/topological-sorting-using-linq
 */

    public interface IPartialComparer<T>
    {
        int? PartialCompare(T x, T y);
    }

    internal class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            return Object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }

    public class TopologicalSorter
    {
        public IEnumerable<TElement> TopologicalSort<TElement>(
            IEnumerable<TElement> elements,
            IPartialComparer<TElement> comparer
            )
        {
            var search = new DepthFirstSearch<TElement, TElement>(
                elements,
                element => element,
                comparer
                );
            return search.VisitAll();
        }

        public IEnumerable<TElement> TopologicalSort<TElement, TKey>(
            IEnumerable<TElement> elements,
            Func<TElement, TKey> selector, IPartialComparer<TKey> comparer
            )
        {
            var search = new DepthFirstSearch<TElement, TKey>(
                elements,
                selector,
                comparer
                );
            return search.VisitAll();
        }

        #region Nested type: DepthFirstSearch

        private class DepthFirstSearch<TElement, TKey>
        {
            private readonly IPartialComparer<TKey> _comparer;
            private readonly IEnumerable<TElement> _elements;
            private readonly Dictionary<TElement, TKey> _keys;
            private readonly Func<TElement, TKey> _selector;
            private readonly List<TElement> _sorted;
            private readonly HashSet<TElement> _visited;

            public DepthFirstSearch(
                IEnumerable<TElement> elements,
                Func<TElement, TKey> selector,
                IPartialComparer<TKey> comparer
                )
            {
                _elements = elements;
                _selector = selector;
                _comparer = comparer;
                var referenceComparer = new ReferenceEqualityComparer<TElement>();
                _visited = new HashSet<TElement>(referenceComparer);
                _keys = elements.ToDictionary(
                    e => e,
                    e => _selector(e),
                    referenceComparer
                    );
                _sorted = new List<TElement>();
            }

            public IEnumerable<TElement> VisitAll()
            {
                foreach (var element in _elements)
                {
                    Visit(element);
                }
                return _sorted;
            }

            private void Visit(TElement element)
            {
                if (!_visited.Contains(element))
                {
                    _visited.Add(element);
                    var predecessors = _elements.Where(
                        e => _comparer.PartialCompare(_keys[e], _keys[element]) < 0
                        );
                    foreach (var e in predecessors)
                    {
                        Visit(e);
                    }
                    _sorted.Add(element);
                }
            }
        }

        #endregion
    }
}