using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.ApkLib.Manifest
{
    /// <summary>
    /// Wrap an intent-filter in an android manifest.
    /// </summary>
    public class IntentFilter
    {
        private readonly XElement element;

        /// <summary>
        /// Default ctor
        /// </summary>
        public IntentFilter(XElement element)
        {
            this.element = element;
        }

        /// <summary>
        /// Gets all action's
        /// </summary>
        public IEnumerable<Action> Actions
        {
            get { return element.Elements("action").Select(x => new Action(x)); }
        }

        /// <summary>
        /// Gets all category's
        /// </summary>
        public IEnumerable<Category> Categories
        {
            get { return element.Elements("category").Select(x => new Category(x)); }
        }
    }
}
