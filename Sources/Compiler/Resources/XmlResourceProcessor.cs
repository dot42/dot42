using System.Xml.Linq;

namespace Dot42.Compiler.Resources
{
    /// <summary>
    /// Base class for classes that alter XML based resources before they are passed to aapt.
    /// </summary>
    internal abstract class XmlResourceProcessor
    {
        /// <summary>
        /// Process the file with the given path.
        /// </summary>
        public void Process(string path, bool save)
        {
            var doc = XDocument.Load(path);
            if (Process(doc))
            {
                if (save)
                {
                    doc.Save(path);
                }
            }
        }

        /// <summary>
        /// Process this XML resource.
        /// </summary>
        /// <param name="document">The XML document</param>
        /// <returns>True if changes were made, false otherwise</returns>
        protected abstract bool Process(XDocument document);
    }
}
