using System.Linq;

using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestLambda : TestCase
    {
        public void testLambdaSingleOrDefault()
        {
            var someItems = new []{ "aap", "noot", "mies"};

            var x = someItems.SingleOrDefault(s => s == "noot");

            AssertNotNull(x);
            AssertEquals(x, "noot");
        }

        public void testLambdaSelect()
        {
            var someItems = new[] { "aap", "noot", "mies" };

            var x = someItems.Select(s => s == "noot").ToArray();

            AssertNotNull(x);
            AssertEquals(x.Length, 3);
            AssertFalse(x[0]);
            AssertTrue(x[1]);
            AssertFalse(x[2]);
        }
    }
}
