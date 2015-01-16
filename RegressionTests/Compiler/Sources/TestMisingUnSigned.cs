using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestMisingUnSigned : TestCase
    {
        public void testByteSByte1()
        {
            string x;
            x = Foo((byte)5, (byte)6);
            AssertEquals("byte-byte", x);
            x = Foo((sbyte)5, (byte)6);
            AssertEquals("sbyte-byte", x);
            x = Foo((byte)5, (sbyte)6);
            AssertEquals("byte-sbyte", x);
            x = Foo((sbyte)5, (sbyte)6);
            AssertEquals("sbyte-sbyte", x);
        }

        public void testShortUShort1()
        {
            string x;
            x = Foo((short)5, (short)6);
            AssertEquals("short-short", x);
            x = Foo((ushort)5, (short)6);
            AssertEquals("ushort-short", x);
            x = Foo((short)5, (ushort)6);
            AssertEquals("short-ushort", x);
            x = Foo((ushort)5, (ushort)6);
            AssertEquals("ushort-ushort", x);
        }

        public void testIntUInt1()
        {
            string x;
            x = Foo((int)5, (int)6);
            AssertEquals("int-int", x);
            x = Foo((uint)5, (int)6);
            AssertEquals("uint-int", x);
            x = Foo((int)5, (uint)6);
            AssertEquals("int-uint", x);
            x = Foo((uint)5, (uint)6);
            AssertEquals("uint-uint", x);
        }

        public void testLongULong()
        {
            string x;
            x = Foo((long)5, (long)6);
            AssertEquals("long-long", x);
            x = Foo((ulong)5, (long)6);
            AssertEquals("ulong-long", x);
            x = Foo((long)5, (ulong)6);
            AssertEquals("long-ulong", x);
            x = Foo((ulong)5, (ulong)6);
            AssertEquals("ulong-ulong", x);
        }

        private string Foo(byte a, byte b)
        {
            return "byte-byte";
        }

        private string Foo(byte a, sbyte b)
        {
            return "byte-sbyte";
        }

        private string Foo(sbyte a, byte b)
        {
            return "sbyte-byte";
        }

        private string Foo(sbyte a, sbyte b)
        {
            return "sbyte-sbyte";
        }

        private string Foo(short a, short b)
        {
            return "short-short";
        }

        private string Foo(ushort a, short b)
        {
            return "ushort-short";
        }

        private string Foo(short a, ushort b)
        {
            return "short-ushort";
        }

        private string Foo(ushort a, ushort b)
        {
            return "ushort-ushort";
        }

        private string Foo(int a, int b)
        {
            return "int-int";
        }

        private string Foo(uint a, int b)
        {
            return "uint-int";
        }

        private string Foo(int a, uint b)
        {
            return "int-uint";
        }

        private string Foo(uint a, uint b)
        {
            return "uint-uint";
        }

        private string Foo(long a, long b)
        {
            return "long-long";
        }

        private string Foo(ulong a, long b)
        {
            return "ulong-long";
        }

        private string Foo(long a, ulong b)
        {
            return "long-ulong";
        }

        private string Foo(ulong a, ulong b)
        {
            return "ulong-ulong";
        }

    }
}
