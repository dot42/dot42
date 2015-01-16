using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestSwitch : TestCase
    {
        public void testSimple1()
        {
            var j = 3;
			switch (j) 
			{
			case 1:
				AssertTrue(false);
				break;
			case 2:
				AssertEquals(3, 4);
				break;
			case 3:
				AssertTrue(true);
				break;
			case 4:
				AssertEquals(30, 4);
				break;
			}
        }
    }
}
