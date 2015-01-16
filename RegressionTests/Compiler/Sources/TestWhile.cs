using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestWhile : TestCase
    {

        public void test1()
        {
            int a = 2;
	        int b = 2;

            int c, d, result;

            while ((a > 1) && (b > 1))
            {
                c = 3;
                d = 1;

                while (d < b)
                {
                    result = AddAll(a, b, c, d);
                    AssertEquals(8, result);

                    b = 0;
                }
            }
        }

        public void test2()
        {
            int a = 2;
            int b = 2;

            int c, d, result;

            MyClass myClass1 = new MyClass();
            MyClass myClass2 = new MyClass();

            while ((a > 1) && (b > 1))
            {
                c = 3;
                d = 1;

                while (d < b)
                {
                    result = AddAll(myClass1, a, b, c, d, myClass2);

                    AssertEquals(8, result);

                    b = 0;
                }
            }
        }

        private int AddAll(int a, int b, int c, int d)
        {
            return a + b + c + d;
        }

        private static int AddAll(MyClass myClass1, int a, int b, int c, int d )
        {
            return a + b + c + d;
        }

        private static int AddAll(MyClass myClass1, int a, int b, int c, int d, MyClass myClass2)
        {
            return a + b + c + d;
        }

        internal sealed class MyClass
        {
        }

    }
}
