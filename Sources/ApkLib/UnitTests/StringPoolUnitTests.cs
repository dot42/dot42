using System.IO;
using Dot42.ApkLib.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dot42.ApkLib.UnitTests
{
    [TestClass]
    public class StringPoolUnitTests
    {
        [TestMethod]
        public void LoadAndCreateTest()
        {
            var xml = new XmlTree(new MemoryStream(TestResource.AndroidManifest));
            var pool = xml.StringPool;

            var stream = new MemoryStream();
            var writer = new ResWriter(stream);
            pool.Write(writer);

            stream.Position = 0;
            var pool2 = new StringPool(new ResReader(stream));

            Assert.AreEqual(pool.Count, pool2.Count);
        }
    }
}
