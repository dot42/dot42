using System;

using NUnit.Framework;

namespace Case11
{
    [TestFixture]
    public class BitConverterTest
    {
        private byte[] byteArray;
        private byte[] floatArray;
        private byte[] doubleArray;
        private byte[] charArray;

        [SetUp]
        public void SetupTest()
        {
            this.byteArray = new byte[]   { 0x00, 0x01, 0x00, 0x00,   0x00, 0x02, 0x01, 0x00,   0x00, 0x03, 0x04, 0x00 };
            this.floatArray = new byte[]  { 0x00, 0x00, 0x80, 0x3f,   0xdb, 0x0f, 0x49, 0x40 };
            this.doubleArray = new byte[] { 0x00, 0x00, 0x00, 0x00,   0x00, 0x00, 0xf0, 0x3f,   0xf1, 0xd4, 0xc8, 0x53,   0xfb, 0x21, 0x09, 0x40 };
            this.charArray = new byte[] { 0x50, 0x00, 0x4C, 0x00 };
        }

        /// <summary>
        /// Run the code and check if it throws the expected exception.
        /// </summary>
        /// <remarks>
        /// Testing for an exception this way instead of an attribute, gives us the
        /// possibility to test for several exceptions in one test method.
        /// </remarks>
        /// <param name="exception">The expected type of the exception.</param>
        /// <param name="codeToTest">The code to call.</param>
        /// <returns>True if the expected exception was caught, false otherwise.</returns>
        private void TestForException(Type exception, Action codeToTest)
        {
            bool noExceptionCaught = false;
            try
            {
                codeToTest();
                noExceptionCaught = true;
            }
            catch (Exception e)
            {
                if (!e.GetType().Equals(exception))
                {
                    Assert.Fail(string.Format("Caught {1} instead of the expected {0}. ", exception.Name, e.GetType().Name));
                }
            }

            if (noExceptionCaught)
            {
                Assert.Fail(string.Format("Expected {0} not caught.", exception.Name));
            }
        }

        [Test]
        public void TestEmptyBuffer()
        {
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToBoolean(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToChar(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToInt16(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToInt32(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToInt64(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToUInt16(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToUInt32(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToUInt64(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToSingle(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToDouble(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToString(null); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToString(null, 0); });
            this.TestForException(typeof(ArgumentNullException), delegate { BitConverter.ToString(null, 0, 1); });
        }

        [Test]
        public void TestWrongStartIndex()
        {
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToBoolean(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToChar(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToInt16(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToInt32(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToInt64(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToUInt16(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToUInt32(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToUInt64(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToSingle(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToDouble(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToString(this.charArray, 4); });
            this.TestForException(typeof(ArgumentOutOfRangeException), delegate { BitConverter.ToString(this.charArray, 4, 1); });
        }

        [Test]
        public void TestWrongLength()
        {
            this.TestForException(typeof(ArgumentException), delegate { BitConverter.ToInt16(this.charArray, 3); });
            this.TestForException(typeof(ArgumentException), delegate { BitConverter.ToInt32(this.charArray, 3); });
            this.TestForException(typeof(ArgumentException), delegate { BitConverter.ToInt64(this.charArray, 3); });
            this.TestForException(typeof(ArgumentException), delegate { BitConverter.ToUInt16(this.charArray, 3); });
            this.TestForException(typeof(ArgumentException), delegate { BitConverter.ToUInt32(this.charArray, 3); });
            this.TestForException(typeof(ArgumentException), delegate { BitConverter.ToUInt64(this.charArray, 3); });
            this.TestForException(typeof(ArgumentException), delegate { BitConverter.ToSingle(this.charArray, 3); });
            this.TestForException(typeof(ArgumentException), delegate { BitConverter.ToDouble(this.charArray, 3); });
            this.TestForException(typeof(ArgumentException), delegate { BitConverter.ToString(this.charArray, 3, 2); });
        }

        [Test]
        public void TestLittleEndian()
        {
            Assert.IsTrue(BitConverter.IsLittleEndian);
        }

        [Test]
        public void TestToBool()
        {
            Assert.AssertEquals(false, BitConverter.ToBoolean(this.byteArray, 0));
            Assert.AssertEquals(true, BitConverter.ToBoolean(this.byteArray, 1));
            Assert.AssertEquals(true, BitConverter.ToBoolean(this.byteArray, 6));
            Assert.AssertEquals(false, BitConverter.ToBoolean(this.byteArray, 7));
        }

        [Test]
        public void TestToInt16()
        {
            Assert.AssertEquals((short)0x0100, BitConverter.ToInt16(this.byteArray, 0));
            Assert.AssertEquals((short)0x0001, BitConverter.ToInt16(this.byteArray, 1));
            Assert.AssertEquals((short)0x0000, BitConverter.ToInt16(this.byteArray, 2));
            Assert.AssertEquals((short)0x0000, BitConverter.ToInt16(this.byteArray, 3));
            Assert.AssertEquals((short)0x0200, BitConverter.ToInt16(this.byteArray, 4));
            Assert.AssertEquals((short)0x0102, BitConverter.ToInt16(this.byteArray, 5));
            Assert.AssertEquals((short)0x0001, BitConverter.ToInt16(this.byteArray, 6));
            Assert.AssertEquals((short)0x0000, BitConverter.ToInt16(this.byteArray, 7));
            Assert.AssertEquals((short)0x0300, BitConverter.ToInt16(this.byteArray, 8));
            Assert.AssertEquals((short)0x0403, BitConverter.ToInt16(this.byteArray, 9));
            Assert.AssertEquals((short)0x0004, BitConverter.ToInt16(this.byteArray, 10));
        }

        [Test]
        public void TestToInt32()
        {
            Assert.AssertEquals(0x00000100, BitConverter.ToInt32(this.byteArray, 0));
            Assert.AssertEquals(0x00000001, BitConverter.ToInt32(this.byteArray, 1));
            Assert.AssertEquals(0x02000000, BitConverter.ToInt32(this.byteArray, 2));
            Assert.AssertEquals(0x01020000, BitConverter.ToInt32(this.byteArray, 3));
            Assert.AssertEquals(0x00010200, BitConverter.ToInt32(this.byteArray, 4));
            Assert.AssertEquals(0x00000102, BitConverter.ToInt32(this.byteArray, 5));
            Assert.AssertEquals(0x03000001, BitConverter.ToInt32(this.byteArray, 6));
            Assert.AssertEquals(0x04030000, BitConverter.ToInt32(this.byteArray, 7));
            Assert.AssertEquals(0x00040300, BitConverter.ToInt32(this.byteArray, 8));
        }

        [Test]
        public void TestToInt64()
        {
            Assert.AssertEquals(0x0001020000000100L, BitConverter.ToInt64(this.byteArray, 0));
            Assert.AssertEquals(0x0000010200000001L, BitConverter.ToInt64(this.byteArray, 1));
            Assert.AssertEquals(0x0300000102000000L, BitConverter.ToInt64(this.byteArray, 2));
            Assert.AssertEquals(0x0403000001020000L, BitConverter.ToInt64(this.byteArray, 3));
            Assert.AssertEquals(0x0004030000010200L, BitConverter.ToInt64(this.byteArray, 4));
        }

        [Test]
        public void TestToUInt16()
        {
            Assert.AssertEquals((short)0x0100, BitConverter.ToUInt16(this.byteArray, 0));
            Assert.AssertEquals((short)0x0001, BitConverter.ToUInt16(this.byteArray, 1));
            Assert.AssertEquals((short)0x0000, BitConverter.ToUInt16(this.byteArray, 2));
            Assert.AssertEquals((short)0x0000, BitConverter.ToUInt16(this.byteArray, 3));
            Assert.AssertEquals((short)0x0200, BitConverter.ToUInt16(this.byteArray, 4));
            Assert.AssertEquals((short)0x0102, BitConverter.ToUInt16(this.byteArray, 5));
            Assert.AssertEquals((short)0x0001, BitConverter.ToUInt16(this.byteArray, 6));
            Assert.AssertEquals((short)0x0000, BitConverter.ToUInt16(this.byteArray, 7));
            Assert.AssertEquals((short)0x0300, BitConverter.ToUInt16(this.byteArray, 8));
            Assert.AssertEquals((short)0x0403, BitConverter.ToUInt16(this.byteArray, 9));
            Assert.AssertEquals((short)0x0004, BitConverter.ToUInt16(this.byteArray, 10));
        }

        [Test]
        public void TestToUInt32()
        {
            Assert.AssertEquals(0x00000100, BitConverter.ToUInt32(this.byteArray, 0));
            Assert.AssertEquals(0x00000001, BitConverter.ToUInt32(this.byteArray, 1));
            Assert.AssertEquals(0x02000000, BitConverter.ToUInt32(this.byteArray, 2));
            Assert.AssertEquals(0x01020000, BitConverter.ToUInt32(this.byteArray, 3));
            Assert.AssertEquals(0x00010200, BitConverter.ToUInt32(this.byteArray, 4));
            Assert.AssertEquals(0x00000102, BitConverter.ToUInt32(this.byteArray, 5));
            Assert.AssertEquals(0x03000001, BitConverter.ToUInt32(this.byteArray, 6));
            Assert.AssertEquals(0x04030000, BitConverter.ToUInt32(this.byteArray, 7));
            Assert.AssertEquals(0x00040300, BitConverter.ToUInt32(this.byteArray, 8));
        }

        [Test]
        public void TestToUInt64()
        {
            Assert.AssertEquals(0x0001020000000100L, BitConverter.ToUInt64(this.byteArray, 0));
            Assert.AssertEquals(0x0000010200000001L, BitConverter.ToUInt64(this.byteArray, 1));
            Assert.AssertEquals(0x0300000102000000L, BitConverter.ToUInt64(this.byteArray, 2));
            Assert.AssertEquals(0x0403000001020000L, BitConverter.ToUInt64(this.byteArray, 3));
            Assert.AssertEquals(0x0004030000010200L, BitConverter.ToUInt64(this.byteArray, 4));
        }

        [Test]
        public void TestToSingle()
        {
            Assert.AssertEquals(1f, BitConverter.ToSingle(this.floatArray, 0));
            Assert.AssertEquals(3.14159265f, BitConverter.ToSingle(this.floatArray, 4));
        }

        [Test]
        public void TestToDouble()
        {
            Assert.AssertEquals(1.0, BitConverter.ToDouble(this.doubleArray, 0));
            Assert.AssertEquals(3.14159265, BitConverter.ToDouble(this.doubleArray, 8));
        }

        [Test]
        public void TestToChar()
        {
            Assert.AssertEquals('P', BitConverter.ToChar(this.charArray, 0));
            Assert.AssertEquals('L', BitConverter.ToChar(this.charArray, 2));
        }

        [Test]
        public void TestToString()
        {
            Assert.AssertEquals("F1-D4-C8-53", BitConverter.ToString(this.doubleArray, 8, 4));
        }

        [Test]
        public void TestGetBytes()
        {
            // Test GetBytes(char c).
            byte[] b = BitConverter.GetBytes('P');
            Assert.AreEqual(2, b.Length);
            Assert.AreEqual((byte)0x50, b[0]);
            Assert.AreEqual((byte)0x00, b[1]);
            b = BitConverter.GetBytes('L');
            Assert.AreEqual(2, b.Length);
            Assert.AreEqual((byte)0x4C, b[0]);
            Assert.AreEqual((byte)0x00, b[1]);

            // Test GetBytes(boolean b).
            b = BitConverter.GetBytes(false);
            Assert.AreEqual(1, b.Length);
            Assert.AreEqual((byte)0, b[0]);
            b = BitConverter.GetBytes(true);
            Assert.AreEqual(1, b.Length);
            Assert.AreEqual((byte)1, b[0]);

            // Test GetBytes(int16 i).
            b = BitConverter.GetBytes((short)0x0102);
            Assert.AreEqual(2, b.Length);
            Assert.AreEqual((byte)0x02, b[0]);
            Assert.AreEqual((byte)0x01, b[1]);
            b = BitConverter.GetBytes((short)0x0203);
            Assert.AreEqual(2, b.Length);
            Assert.AreEqual((byte)0x03, b[0]);
            Assert.AreEqual((byte)0x02, b[1]);

            // Test GetBytes(int i).
            b = BitConverter.GetBytes(0x01020304);
            Assert.AreEqual(4, b.Length);
            Assert.AreEqual((byte)0x04, b[0]);
            Assert.AreEqual((byte)0x03, b[1]);
            Assert.AreEqual((byte)0x02, b[2]);
            Assert.AreEqual((byte)0x01, b[3]);

            // Test GetBytes(int64 i).
            b = BitConverter.GetBytes(0x0102030405060708L);
            Assert.AreEqual(8, b.Length);
            Assert.AreEqual((byte)0x08, b[0]);
            Assert.AreEqual((byte)0x07, b[1]);
            Assert.AreEqual((byte)0x06, b[2]);
            Assert.AreEqual((byte)0x05, b[3]);
            Assert.AreEqual((byte)0x04, b[4]);
            Assert.AreEqual((byte)0x03, b[5]);
            Assert.AreEqual((byte)0x02, b[6]);
            Assert.AreEqual((byte)0x01, b[7]);

            // Test GetBytes(float f).
            b = BitConverter.GetBytes(3.14159265f);
            Assert.AreEqual(4, b.Length);
            Assert.AreEqual((byte)0xdb, b[0]);
            Assert.AreEqual((byte)0x0f, b[1]);
            Assert.AreEqual((byte)0x49, b[2]);
            Assert.AreEqual((byte)0x40, b[3]);

            // Test GetBytes(double d).
            b = BitConverter.GetBytes(3.14159265);
            Assert.AreEqual(8, b.Length);
            Assert.AreEqual((byte)0xf1, b[0]);
            Assert.AreEqual((byte)0xd4, b[1]);
            Assert.AreEqual((byte)0xc8, b[2]);
            Assert.AreEqual((byte)0x53, b[3]);
            Assert.AreEqual((byte)0xfb, b[4]);
            Assert.AreEqual((byte)0x21, b[5]);
            Assert.AreEqual((byte)0x09, b[6]);
            Assert.AreEqual((byte)0x40, b[7]);
        }

        [Test]
        public void TestDoubleToInt64Bits()
        {
            long l = BitConverter.DoubleToInt64Bits(3.14159265);
            Assert.AreEqual(0x400921fb53c8d4f1L, l);
        }

        [Test]
        public void TestInt64BitsToDouble()
        {
            double d = BitConverter.Int64BitsToDouble(0x400921fb53c8d4f1L);
            Assert.AreEqual(3.14159265, d);
        }
    }
}
