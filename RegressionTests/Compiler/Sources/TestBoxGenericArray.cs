using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestBoxGenericArray : TestCase
    {
        public void testInt()
        {
            var g = new Generic<int>();
            var arr = new int[5];
            g.CopyTo(arr, 42);
            AssertEquals(arr[2], 42);
        }

        private class Generic<T>
        {
            public void CopyTo(T[] dest, T value)
            {
                for (var i = 0; i < dest.Length; i++)
                {
                    dest[i] = value;
                }
            }
        }
    }
}
