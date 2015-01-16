using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.ApkLib.Manifest
{
    /// <summary>
    /// Wrap an activity in an android manifest.
    /// </summary>
    public class Activity
    {
        private readonly XElement element;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Activity(XElement element)
        {
            this.element = element;
        }

        /// <summary>
        /// Gets the name of this activity.
        /// </summary>
        public string Name { get { return element.GetAttribute(XName.Get("name", AndroidConstants.AndroidNamespace)); } }

        /// <summary>
        /// Gets the icon of this activity.
        /// </summary>
        public string Icon { get { return element.GetAttribute(XName.Get("icon", AndroidConstants.AndroidNamespace)); } }

        /// <summary>
        /// Gets all intent-filter's
        /// </summary>
        public IEnumerable<IntentFilter> IntentFilters
        {
            get { return element.Elements("intent-filter").Select(x => new IntentFilter(x)); }
        }
    }
}
