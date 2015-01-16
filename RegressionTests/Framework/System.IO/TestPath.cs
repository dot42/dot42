using System.IO;
using Junit.Framework;

namespace Dot42.Tests.System.IO
{
    public class TestPath : TestCase
    {
        public void testCombine1()
        {
            var s = Path.Combine("aap", "noot");
            AssertEquals(s, "aap/noot", s);
        }
    }
}
