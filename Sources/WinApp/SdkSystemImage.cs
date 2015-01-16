using System.Xml.Linq;

namespace Dot42.Gui
{
    internal class SdkSystemImage : SdkElement
    {
        private readonly SdkArchives archives;

        /// <summary>
        /// Default ctor
        /// </summary>
        public SdkSystemImage(XElement element) : base(element)
        {
            archives = new SdkArchives(element.Element(XName.Get("archives", SdkConstants.Namespace)));
        }

        /// <summary>
        /// Gets a human readable description
        /// </summary>
        public string Description { get { return GetElementValue("description"); } }

        /// <summary>
        /// Gets a ABI type
        /// </summary>
        public string Abi { get { return GetElementValue("abi"); } }

        /// <summary>
        /// Gets a supported API level
        /// </summary>
        public int ApiLevel { get { return GetElementIntValue("api-level", -1); } }

        /// <summary>
        /// Gets the archives for this image
        /// </summary>
        public SdkArchives Archives { get { return archives; } }
    }
}
