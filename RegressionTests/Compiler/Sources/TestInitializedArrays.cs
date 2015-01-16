using Android.Graphics;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestInitializedArrays : TestCase
    {
        public void testByte1()
        {
            var s = new byte[] { 0, 5, 11, 0xFF, 0x8 };
            AssertNotNull(s);
            AssertEquals(s[0], (byte)0);
            AssertEquals(s[1], (byte)5);
            AssertEquals(s[2], (byte)11);
            AssertEquals(s[3], (byte)0xFF);
            AssertEquals(s[4], (byte)0x8);
        }

        public void testSByte1()
        {
            var s = new sbyte[] { 0, 5, 11, 127, -128 };
            AssertNotNull(s);
            AssertEquals(s[0], (sbyte)0);
            AssertEquals(s[1], (sbyte)5);
            AssertEquals(s[2], (sbyte)11);
            AssertEquals(s[3], (sbyte)127);
            AssertEquals(s[4], (sbyte)-128);
        }

        public void testChar()
        {
            var s = new char[] { 'g', 'b', (char)0xFF };
            AssertNotNull(s);
            AssertEquals(s[0], 'g');
            AssertEquals(s[1], 'b');
            AssertEquals(s[2], 0xFF);
        }

        public void testShort()
        {
            var s = new short[] { 0, 23, -300, 1600, 22 };
            AssertNotNull(s);
            AssertEquals(s[0], (short)0);
            AssertEquals(s[1], (short)23);
            AssertEquals(s[2], (short)-300);
            AssertEquals(s[3], (short)1600);
            AssertEquals(s[4], (short)22);
        }

        public void testInt()
        {
            var s = new int[] { 0, 23, -30000, 1600, 27334235 };
            AssertNotNull(s);
            AssertEquals(s[0], 0);
            AssertEquals(s[1], 23);
            AssertEquals(s[2], -30000);
            AssertEquals(s[3], 1600);
            AssertEquals(s[4], 27334235);
        }

        public void testFloat()
        {
            var s = new float[] { 0.0f, 23f, -30000.5f, 1600.7f, 27334235.2f };
            AssertNotNull(s);
            AssertEquals(s[0], 0.0f);
            AssertEquals(s[1], 23f);
            AssertEquals(s[2], -30000.5f);
            AssertEquals(s[3], 1600.7f);
            AssertEquals(s[4], 27334235.2f);
        }

        public void testLong()
        {
            var s = new long[] { 0, 23, -300004477887522L, 27334235375625483L };
            AssertNotNull(s);
            AssertEquals(s[0], 0L);
            AssertEquals(s[1], 23L);
            AssertEquals(s[2], -300004477887522L);
            AssertEquals(s[3], 27334235375625483L);
        }

        public void testDouble()
        {
            var s = new double[] { 0.0d, 23d, -3000321364768924600.5d, 1600.7d, 273342327556925872625.2d };
            AssertNotNull(s);
            AssertEquals(s[0], 0.0d, 0.0001d);
            AssertEquals(s[1], 23d); // Converted to object on purpose!
            AssertEquals(s[2], -3000321364768924600.5d, 0.0001d);
            AssertEquals(s[3], 1600.7d, 0.0001d);
            AssertEquals(s[4], 273342327556925872625.2d, 0.0001d);
        }

        public void testString1()
        {
            var s = new[] { "aap", "noot", "mies" };
            AssertNotNull(s);
            AssertEquals(s[0], "aap");
            AssertEquals(s[1], "noot");
            AssertEquals(s[2], "mies");
        }

        public void testMyObject()
        {
            var s = new[] { new MyObject("aap"), new MyObject("noot"), new MyObject("mies") };
            AssertNotNull(s);
            AssertEquals(s[0].S, "aap");
            AssertEquals(s[1].S, "noot");
            AssertEquals(s[2].S, "mies");
        }

        public void testMyEnum()
        {
            var s = new[] { MyEnum.Aap, MyEnum.Mies, MyEnum.Mies, MyEnum.Noot, MyEnum.Aap, MyEnum.Noot };
            AssertNotNull(s);
            AssertEquals(s[0], MyEnum.Aap);
            AssertEquals(s[1], MyEnum.Mies);
            AssertEquals(s[2], MyEnum.Mies);
            AssertEquals(s[3], MyEnum.Noot);
            AssertEquals(s[4], MyEnum.Aap);
            AssertEquals(s[5], MyEnum.Noot);
        }

        public void testJavaEnum()
        {
            var s = new[] { Paint.Join.BEVEL, Paint.Join.MITER, Paint.Join.BEVEL, Paint.Join.ROUND, Paint.Join.MITER };
            AssertNotNull(s);
            AssertEquals(s[0], Paint.Join.BEVEL);
            AssertEquals(s[1], Paint.Join.MITER);
            AssertEquals(s[2], Paint.Join.BEVEL);
            AssertEquals(s[3], Paint.Join.ROUND);
            AssertEquals(s[4], Paint.Join.MITER);
        }

        private class MyObject
        {
            public readonly string S;

            public MyObject(string s)
            {
                S = s;
            }
        }

        internal enum MyEnum
        {
            Aap,
            Noot,
            Mies
        }
    }
}
