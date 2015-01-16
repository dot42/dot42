using Luxmate.MMT.LmBusMgr.Timer;
using Dot42.Manifest;

[assembly: Application("dot42 case 670")]
[assembly: Instrumentation(Label = "dot42 case 670", FunctionalTest = true)]
[assembly: UsesLibrary("android.test.runner")]

namespace Case670
{
    public class Test : Junit.Framework.TestCase
    {
        public void test1()
        {
            var timer = new LmTimer();
            AssertNotNull(timer);
        }
    }
}
