using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.ApkLib.Manifest
{
    /// <summary>
    /// Wrap an uses-permission element in an android manifest.
    /// </summary>
    public class UsesPermission
    {
        private readonly XElement element;

        /// <summary>
        /// Default ctor
        /// </summary>
        public UsesPermission(XElement element)
        {
            this.element = element;
        }

        /// <summary>
        /// Gets the name of this activity.
        /// </summary>
        public string Name { get { return element.GetAttribute(XName.Get("name", AndroidConstants.AndroidNamespace)); } }
    }
}
