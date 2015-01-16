using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.Mapping
{
    /// <summary>
    /// Base class for register mapping entries.
    /// </summary>
    public class RegisterEntryList<T> : IEnumerable<T> 
        where T : RegisterEntry
    {
        private readonly string elementName;
        private readonly List<T> list = new List<T>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public RegisterEntryList(string elementName)
        {
            this.elementName = elementName;
        }

        /// <summary>
        /// XML ctor
        /// </summary>
        internal RegisterEntryList(XElement e, string elementName, Func<XElement, T> buider)
        {
            this.elementName = elementName;
            list.AddRange(e.Elements(elementName).Select(buider));
        }

        /// <summary>
        /// Convert to XML element
        /// </summary>
        internal void AddTo(XElement e)
        {
            e.Add(list.Select(x => x.ToXml(this.elementName)));
        }

        /// <summary>
        /// Gets the number of elements
        /// </summary>
        public int Count { get { return list.Count; } }

        /// <summary>
        /// Add the given element
        /// </summary>
        public void Add(T element)
        {
            list.Add(element);
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
