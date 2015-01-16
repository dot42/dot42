using Junit.Framework;
using Dot42.Java.Test;

namespace JavaImportTest.Sources
{
    public class Test1 : TestCase
    {
        public void testSimpleEqual1()
        {
			var helper = new Dot42.Java.Test.Test1();
            AssertTrue(helper.Add(5, 7) == 12);
        }
    }
}
