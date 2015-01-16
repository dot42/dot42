using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestReferenceEquals : TestCase
    {
        public void test1()
        {
            AssertTrue(ReferenceEquals(this, this));
            AssertFalse(ReferenceEquals(this, null));
            AssertFalse(ReferenceEquals(null, this));
            AssertFalse(ReferenceEquals(this, "hello"));
        }
    }
}
