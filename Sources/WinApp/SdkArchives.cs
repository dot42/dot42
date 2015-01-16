using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.Gui
{
    internal class SdkArchives : IEnumerable<SdkArchive>
    {
        private readonly List<SdkArchive> archives;

        /// <summary>
        /// Default ctor
        /// </summary>
        public SdkArchives(XElement element) 
        {
            if (element != null)
            {
                archives = element.Elements(XName.Get("archive", SdkConstants.Namespace)).Select(x => new SdkArchive(x)).ToList();
            }
            else
            {
                archives = new List<SdkArchive>();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<SdkArchive> GetEnumerator()
        {
            return archives.GetEnumerator();
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
