using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Junit.Framework;

namespace Dot42.Tests.System.Xml.Linq
{
    public class TestXParse : TestCase
    {
        public void test1()
        {
            var doc = XDocument.Parse("<root></root>");
            AssertNotNull(doc);
            AssertNotNull(doc.Root);
        }

        public void test2()
        {
            var doc = XDocument.Parse("<root x=\"aap\"></root>");
            AssertNotNull(doc);
            AssertNotNull(doc.Root);
            var a = doc.Root.Attributes().First();
            AssertNotNull(a);
            AssertEquals("x", a.Name.ToString());
            AssertNotNull(doc.Root.Attribute("x"));
            AssertEquals("aap", doc.Root.Attribute("x").Value);
        }
    }
}
