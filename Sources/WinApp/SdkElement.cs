using System.Xml.Linq;

namespace Dot42.Gui
{
    internal abstract class SdkElement
    {
        private readonly XElement element;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected SdkElement(XElement element)
        {
            this.element = element;
        }

        /// <summary>
        /// Gets the value of a child element.
        /// </summary>
        protected string GetElementValue(string localName)
        {
            if (element == null)
                return null;
            var child = element.Element(XName.Get(localName, SdkConstants.Namespace));
            return (child != null) ? child.Value : null;
        }

        /// <summary>
        /// Gets the value of a child element.
        /// </summary>
        protected int GetElementIntValue(string localName, int defaultValue)
        {
            var sValue = GetElementValue(localName);
            if (string.IsNullOrEmpty(sValue))
                return defaultValue;
            int value;
            return int.TryParse(sValue, out value) ? value : defaultValue;
        }
    }
}
