using System;
using System.Collections;
using System.Collections.Generic;

namespace Dot42.ImportJarLib.Doxygen
{
    public class DocMemberList<T, TOwner> : IEnumerable<T>
        where T : DocMember<TOwner>
        where TOwner : DocMember
    {
        private readonly List<T> list = new List<T>();
        private readonly TOwner owner;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DocMemberList(TOwner owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Add the given item.
        /// </summary>
        public void Add(T item)
        {
            if (item.DeclaringClass != null)
                throw new ArgumentException("Item is already a member of a collection");
            item.DeclaringClass = owner;
            list.Add(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
