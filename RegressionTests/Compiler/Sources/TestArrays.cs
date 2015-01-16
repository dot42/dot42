using Java.Util;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestArrays : TestCase
    {
        public void testBoolean()
        {
            int count = 8;
            var buf = new bool[count];

            Arrays.Fill(buf, true);

            var newBuf = Arrays.CopyOf<bool>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == true);
            AssertTrue(newBuf[8] == false);
        }

        public void testByte1()
        {
            int count = 8;
            var buf = new byte[count];

            Arrays.Fill(buf, 128);

            var newBuf = Arrays.CopyOf<byte>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 128);
            AssertTrue(newBuf[8] == 0);
        }

        public void testSByte1()
        {
            int count = 8;
            var buf = new sbyte[count];

            Arrays.Fill(buf, 127);

            var newBuf = Arrays.CopyOf<sbyte>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 127);
            AssertTrue(newBuf[8] == 0);
        }

        public void testShort1()
        {
            int count = 8;
            var buf = new short[count];

            Arrays.Fill(buf, 1270);

            var newBuf = Arrays.CopyOf<short>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 1270);
            AssertTrue(newBuf[8] == 0);
        }

        public void testUShort1()
        {
            int count = 8;
            var buf = new ushort[count];

            Arrays.Fill(buf, 1270);

            var newBuf = Arrays.CopyOf<ushort>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 1270);
            AssertTrue(newBuf[8] == 0);
        }

        public void testInt1()
        {
            int count = 8;
            var buf = new int[count];

            Arrays.Fill(buf, 1270);

            var newBuf = Arrays.CopyOf<int>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 1270);
            AssertTrue(newBuf[8] == 0);
        }

        public void testUInt1()
        {
            int count = 8;
            var buf = new uint[count];

            Arrays.Fill(buf, 1270);

            var newBuf = Arrays.CopyOf<uint>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 1270);
            AssertTrue(newBuf[8] == 0);
        }

        public void testLong1()
        {
            int count = 8;
            var buf = new long[count];

            Arrays.Fill(buf, 1270);

            var newBuf = Arrays.CopyOf<long>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 1270);
            AssertTrue(newBuf[8] == 0);
        }

        public void testULong1()
        {
            int count = 8;
            var buf = new ulong[count];

            Arrays.Fill(buf, 1270);

            var newBuf = Arrays.CopyOf<ulong>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 1270);
            AssertTrue(newBuf[8] == 0);
        }

        public void testFloat1()
        {
            int count = 8;
            var buf = new float[count];

            Arrays.Fill(buf, 1270.0f);

            var newBuf = Arrays.CopyOf<float>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 1270.0f);
            AssertTrue(newBuf[8] == 0.0f);
        }

        public void testDouble1()
        {
            int count = 8;
            var buf = new double[count];

            Arrays.Fill(buf, 1270.0);

            var newBuf = Arrays.CopyOf<double>(buf, count+1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 1270.0);
            AssertTrue(newBuf[8] == 0.0);
        }

        public void testChar1()
        {
            int count = 8;
            var buf = new char[count];

            Arrays.Fill(buf, 't');

            var newBuf = Arrays.CopyOf<char>(buf, count + 1);

            AssertTrue(buf.Length == 8);
            AssertTrue(newBuf.Length == 9);

            AssertTrue(newBuf[0] == 't');
            AssertTrue(newBuf[8] == 0);
        }

        public void testChar2()
        {
            var targetLocal = new char[8];
            Arrays.Fill(targetLocal, 't');
            
            ushort index = 3;
            targetLocal[index] = 'u';

            AssertTrue(targetLocal.Length == 8);
            AssertTrue(targetLocal[0] == 't');
            AssertTrue(targetLocal[index] == 'u');
        }

        char[] targetInstance = new char[8];
        public void testChar3()
        {
            Arrays.Fill(targetInstance, 't');

            ushort index = 3;
            targetInstance[index] = 'u';

            AssertTrue(targetInstance.Length == 8);
            AssertTrue(targetInstance[0] == 't');
            AssertTrue(targetInstance[index] == 'u');
        }

        static char[] targetStatic = new char[8];
        public void testChar4()
        {
            Arrays.Fill(targetStatic, 't');

            ushort index = 3;
            targetStatic[index] = 'u';

            AssertTrue(targetStatic.Length == 8);
            AssertTrue(targetStatic[0] == 't');
            AssertTrue(targetStatic[index] == 'u');
        }

        public void testChar5()
        {
            for (ushort code = 0; code < 8; code++)
            {
                char c = (char)code;
                targetStatic[code] = c;
            }

            AssertTrue(targetStatic.Length == 8);
            AssertTrue(targetStatic[0] == 0);
            AssertTrue(targetStatic[2] == 2);
            AssertTrue(targetStatic[4] == 4);
            AssertTrue(targetStatic[7] == 7);
        }

        byte[,] tk = new byte[4, 4];
        private static readonly byte[] S = {99};

        public void _test2()
        {
            tk[0, 0] ^= S[tk[1, 1]];
        }

        public void test3()
        {
            var array = new int[]{ 1,2,3,4,5};
            Resize(ref array, 10);

            AssertEquals(10, array.Length);
            AssertEquals(1, array[0]);
            AssertEquals(5, array[4]);
            AssertEquals(0, array[5]);
            AssertEquals(0, array[9]);
        }
       
        public void test4()
        {
            var array = new string[]{ "aap","noot","mies" };
            Resize(ref array, 5);

            AssertEquals(5, array.Length);
            AssertEquals("aap", array[0]);
            AssertEquals("noot", array[1]);
            AssertEquals("mies", array[2]);
            AssertEquals(null, array[3]);
            AssertEquals(null, array[4]);
        }
       
        public void test5()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };

            var typeArray = array.GetType();
            AssertNotNull(typeArray);
            AssertTrue(typeArray.IsArray);

            var typeElem = typeArray.GetElementType();
            AssertNotNull(typeElem);
            AssertFalse(typeElem.IsArray);

            AssertTrue("typeElem NOT primitive", typeElem.IsPrimitive);
            AssertEquals("int", typeElem.FullName); //Expected: System.Int32
            AssertTrue("typeElem NOT typeof int", typeElem == typeof (int));
        }

        public void test6()
        {
            var array = new short[1,1];
            var b = (byte) 42;

            var s = (short) 42;
            s = (short)b; //without this line, it's working...

            array[0, 0] = s;
            var result = array[0, 0];

            AssertEquals((short)42, result);
        }

        public static void Resize<T>(ref T[] array, int newSize)
        {
            array = Arrays.CopyOf<T>(array, newSize);
        }
       
        public static void Resize(ref int[] array, int newSize)
        {
            array = Arrays.CopyOf(array, newSize);
        }
    }
}
