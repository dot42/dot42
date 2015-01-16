using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.Gui
{
    /// <summary>
    /// Wrap an SDK repository descriptor.
    /// </summary>
    internal class SdkRepository
    {
        private readonly XDocument doc;

        /// <summary>
        /// Default ctor
        /// </summary>
        public SdkRepository()
        {
            doc = XDocument.Load(SdkConstants.RepositoryUrl);
        }

        /// <summary>
        /// Gets all system images
        /// </summary>
        public IEnumerable<SdkSystemImage> SystemImages
        {
            get { return doc.Root.Elements(XName.Get(SdkConstants.KeySystemImage, SdkConstants.Namespace)).Select(x => new SdkSystemImage(x)); }
        }
    }
}
