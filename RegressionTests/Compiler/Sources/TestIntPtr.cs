using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestIntPtr : TestCase
    {
        public class InnerClass
        {
            
        }

        public void test1()
        {
            var innerClass = new InnerClass();
            if (innerClass.Equals(IntPtr.Zero))
            {
                Fail();
            }
        }
    }
}
