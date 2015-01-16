using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestMultiDimensionalArray : TestCase
    {
        public void testBool1()
        {
            var arr = new bool[2,5,6];
            arr[0, 0, 0] = true;
            arr[1, 4, 2] = true;
            AssertEquals(2, arr.Length);
            AssertEquals(arr[0, 0, 0], true);
            AssertEquals(arr[1, 0, 0], false);
            AssertEquals(arr[1, 4, 2], true);
        }

        public void testSByte1()
        {
            var arr = new sbyte[2, 5, 6];
            arr[0, 0, 0] = 4;
            arr[1, 4, 2] = 44;
            AssertEquals(2, arr.Length);
            AssertEquals(arr[0, 0, 0], (sbyte)4);
            AssertEquals(arr[1, 0, 0], (sbyte)0);
            AssertEquals(arr[1, 4, 2], (sbyte)44);
        }

        public void testByte1()
        {
            var arr = new byte[2, 5, 6];
            arr[0, 0, 0] = 4;
            arr[1, 4, 2] = 44;
            AssertEquals(2, arr.Length);
            AssertEquals(arr[0, 0, 0], (byte)4);
            AssertEquals(arr[1, 0, 0], (byte)0);
            AssertEquals(arr[1, 4, 2], (byte)44);
        }

        public void testChar1()
        {
            var arr = new char[1, 5];
            arr[0, 0] = 'a';
            arr[0, 4] = 'd';
            AssertEquals(1, arr.Length);
            AssertEquals(arr[0, 0], 'a');
            AssertEquals(arr[0, 4], 'd');
        }

        public void testShort1()
        {
            var arr = new short[7, 5, 6, 5];
            arr[0, 0, 0, 2] = 4;
            arr[1, 4, 2, 0] = 44;
            AssertEquals(7, arr.Length);
            AssertEquals(arr[0, 0, 0, 2], (short)4);
            AssertEquals(arr[1, 0, 0, 3], (short)0);
            AssertEquals(arr[1, 4, 2, 0], (short)44);
        }

        public void testInt1()
        {
            var arr = new int[7, 5];
            arr[0, 0] = 4;
            arr[1, 4] = 44;
            AssertEquals(7, arr.Length);
            AssertEquals(arr[0, 0], 4);
            AssertEquals(arr[1, 0], 0);
            AssertEquals(arr[1, 4], 44);
        }

        public void testLong1()
        {
            var arr = new long[3, 5];
            arr[0, 0] = 4L;
            arr[1, 4] = 44L;
            AssertEquals(3, arr.Length);
            AssertEquals(arr[0, 0], 4L);
            AssertEquals(arr[1, 0], 0L);
            AssertEquals(arr[1, 4], 44L);
        }

        public void testFloat1()
        {
            var arr = new float[3, 5];
            arr[0, 0] = 4.0F;
            arr[1, 4] = 44.0F;
            AssertEquals(3, arr.Length);
            AssertEquals(arr[0, 0], 4.0F, 0.0001F);
            AssertEquals(arr[1, 0], 0.0F, 0.0001F);
            AssertEquals(arr[1, 4], 44.0F, 0.0001F);
        }

        public void testDouble1()
        {
            var arr = new double[3, 5];
            arr[0, 0] = 4.0;
            arr[1, 4] = 44.0;
            AssertEquals(3, arr.Length);
            AssertEquals(arr[0, 0], 4.0, 0.0001);
            AssertEquals(arr[1, 0], 0.0, 0.0001);
            AssertEquals(arr[1, 4], 44.0, 0.0001);
        }

        public void testString1()
        {
            var arr = new string[2, 5, 6];
            arr[0, 0, 0] = "aap";
            arr[1, 4, 2] = "noot";
            AssertEquals(2, arr.Length);
            AssertEquals(arr[0, 0, 0], "aap");
            AssertEquals(arr[1, 0, 0], null);
            AssertEquals(arr[1, 4, 2], "noot");
        }


    }
}
