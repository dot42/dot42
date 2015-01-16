using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.ApkLib.Manifest
{
    /// <summary>
    /// Wrap an action in an android manifest.
    /// </summary>
    public class Action
    {
        private readonly XElement element;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Action(XElement element)
        {
            this.element = element;
        }

        /// <summary>
        /// Gets the name of this activity.
        /// </summary>
        public string Name { get { return element.GetAttribute(XName.Get("name", AndroidConstants.AndroidNamespace)); } }

        /// <summary>
        /// Is this action "android.intent.action.MAIN" ?
        /// </summary>
        public bool IsMain
        {
            get { return (Name == "android.intent.action.MAIN"); }
        }
    }
}
