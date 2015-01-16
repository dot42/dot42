using System.Threading;
using System.Xml.Linq;
using Junit.Framework;

namespace Dot42.Tests.System.Xml.Linq
{
    public class TestXName : TestCase
    {
        public void test1()
        {
            var name = XName.Get("local");
            AssertEquals("local", name.ToString());
        }

        public void test2()
        {
            var name = XName.Get("local", "myuri");
            AssertEquals("{myuri}local", name.ToString());
        }

        public void test3()
        {
            var name = XName.Get("{uri}test3");
            AssertEquals("{uri}test3", name.ToString());
            AssertEquals("uri", name.NamespaceName);
        }

        public void testShare1()
        {
            var name1 = XName.Get("share1");
            var name2 = XName.Get("share1");
            AssertTrue(ReferenceEquals(name1, name2));
        }

        public void testShare2()
        {
            var name1 = XName.Get("share1", "uri");
            var name2 = XName.Get("share1", "uri");
            AssertTrue(ReferenceEquals(name1, name2));
        }
    }
}
