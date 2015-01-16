using System.IO;
using System.Xml.Linq;
using Dot42.ApkLib.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dot42.ApkLib.UnitTests
{
    [TestClass]
    public class XmlTests
    {
        [TestMethod]
        public void LoadXmlTest()
        {
            var xml = new XmlTree(new MemoryStream(TestResource.AndroidManifest));

            var doc = xml.AsXml();
        }

        [TestMethod]
        public void LoadAndCreateXmlTest()
        {
            var xml = new XmlTree(new MemoryStream(TestResource.AndroidManifest));
            var doc = xml.AsXml();

            var xml2 = new XmlTree(doc);
            var stream = new MemoryStream();
            var writer = new ResWriter(stream);
            xml2.Write(writer);

            stream.Position = 0;
            var xml3 = new XmlTree(stream);
            var doc3 = xml3.AsXml();

            Assert.AreEqual(doc.ToString(), doc3.ToString());
        }

        [TestMethod]
        public void CreateXmlTreeTest()
        {
            var doc = new XDocument(
                new XElement("root"));
            var xml = new XmlTree(doc);
            var stream = new MemoryStream();
            var writer = new ResWriter(stream);
            xml.Write(writer);
        }

    }
}
