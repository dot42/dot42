using System.Threading;
using System.Xml.Linq;
using Junit.Framework;

namespace Dot42.Tests.System.Xml.Linq
{
    public class TestXDocument : TestCase
    {
        public void testNew1()
        {
            var doc = new XDocument();
            AssertNotNull(doc);
        }

        public void testNew2()
        {
            var doc = new XDocument(new XElement("root"));
            AssertNotNull(doc.Root);
        }
    }
}
