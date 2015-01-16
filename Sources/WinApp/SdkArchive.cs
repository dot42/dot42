using System.Xml.Linq;

namespace Dot42.Gui
{
    internal class SdkArchive : SdkElement
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public SdkArchive(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets the local url
        /// </summary>
        public string LocalUrl { get { return GetElementValue("url"); } }

        /// <summary>
        /// Gets the full url.
        /// </summary>
        public string Url { get { return SdkConstants.DownloadRoot + LocalUrl; } }
    }
}
