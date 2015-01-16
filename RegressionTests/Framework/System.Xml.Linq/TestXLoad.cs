using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Junit.Framework;
using JFile = Java.Io.File;

namespace Dot42.Tests.System.Xml.Linq
{
    public class TestXLoad : TestCase
    {
        public void test1()
        {
            var path = JFile.CreateTempFile("dot42.TestXLoad-1", "txt");
            File.WriteAllText(path, "<root></root>");
            var doc = XDocument.Load(path);
            path.Delete();
            AssertNotNull(doc);
            AssertNotNull(doc.Root);
        }

        public void test2()
        {
            var path = JFile.CreateTempFile("dot42.TestXLoad-2", "txt");
            File.WriteAllText(path, "<root></root>");
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var doc = XDocument.Load(stream);
                AssertNotNull(doc);
                AssertNotNull(doc.Root);
            }
            path.Delete();
        }

        public void test3()
        {
            var path = JFile.CreateTempFile("dot42.TestXLoad-3", "txt");
            File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"utf-8\"?><Version xmlns=\"urn:Dot42:TodoApi:1.0\">v1_0</Version>");
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var doc = XDocument.Load(stream);
                AssertNotNull("doc should not be null", doc);
                AssertNotNull("doc.Root should not be null", doc.Root);
                AssertTrue("doc.Root.Name.LocalName != 'Version'", doc.Root.Name.LocalName == "Version");
                AssertTrue("doc.Root.Value != 'v1_0' but:" + doc.Root.Value, doc.Root.Value == "v1_0");
            }
            path.Delete();
        }

        public void test4()
        {
            const string xmlDocument = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Version xmlns=\"urn:Dot42:TodoApi:1.0\">v1_0</Version>";
            var bytes = Encoding.UTF8.GetBytes(xmlDocument);

            using (var stream = new MemoryStream(bytes))
            {
                var doc = XDocument.Load(stream);
                AssertNotNull(doc);
                AssertNotNull(doc.Root);
                AssertTrue(doc.Root.Name.LocalName == "Version");
                AssertTrue(doc.Root.Value == "v1_0");
            }
        }
    }
}
