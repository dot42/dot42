using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestConstSharing : TestCase
    {
        public void testSimpleEqual1()
        {
            int iconSize = 5 * 10;
            string tmp = "aap";

            var f1 = Foo1(iconSize, iconSize, 0, 0, 0);
            var f2 = Foo2(null, tmp, tmp, null, null);
            var f3 = Foo3(0.0f, 0.0f, 0.0f, 0.0f, 0.0f);

            var f4 = Foo1(0, 0, 0, iconSize, iconSize);
            var f5 = Foo2(null, null, null, tmp, tmp);
            var f6 = Foo3(0.0f, 0.0f, 0.0f, 0.0f, 0.0f);

            var f7 = Foo1(0, 0, iconSize, 0, iconSize);
            var f8 = Foo2(null, null, null, null, null);
            var f9 = Foo3(0.0f, 0.0f, 0.0f, 0.0f, 0.0f);

            var f10 = Foo1(0, 0, 0, 0, 0);
            var f11 = Foo2(tmp, tmp, null, null, null);
            var f12 = Foo3(0.0f, 0.0f, 0.0f, 0.0f, 0.0f);

            AssertEquals(1, f1);
            AssertEquals(2, f2);
            AssertEquals(3, f3);
        }

        private int Foo1(int a, int b, int c, int d, int e)
        {
            return 1;
        }

        private int Foo2(string a, string b, string c, string d, string e)
        {
            return 2;
        }

        private int Foo3(float a, float b, float c, float d, float e)
        {
            return 3;
        }
    }
}
