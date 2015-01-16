using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.ApkLib.Manifest
{
    /// <summary>
    /// Wrap a category in an android manifest.
    /// </summary>
    public class Category
    {
        private readonly XElement element;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Category(XElement element)
        {
            this.element = element;
        }

        /// <summary>
        /// Gets the name of this activity.
        /// </summary>
        public string Name { get { return element.GetAttribute(XName.Get("name", AndroidConstants.AndroidNamespace)); } }

        /// <summary>
        /// Is this category "android.intent.category.LAUNCHER" ?
        /// </summary>
        public bool IsLauncher
        {
            get { return (Name == "android.intent.category.LAUNCHER"); }
        }
    }
}
