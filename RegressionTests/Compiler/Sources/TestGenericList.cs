using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestGenericList : TestCase
    {
        public void testSimple1()
        {
            var list = new Java.Util.ArrayList<string>();
            list.Add("Hello world");
        }
		
	}
}
