using System;
using System.Globalization;
using NUnit.Framework;

namespace Case6
{
    /// <summary>
    /// Class to test the String.Format method. We try to be as close to the original implementation as possible.
    /// 
    /// For numbers:
    /// Tested parameters are:
    /// C D E F N P R X
    /// C (as in Currency) is a special case. See http://speakman.net.nz/blog/2013/10/21/android-currency-localisation-hell/ for more info.
    /// X (as in heX) is also special, because it handles only long integers, giving problems using negative integers.
    /// In the test <see cref="TestHex"/> you can see the workaround for these cases.
    /// Not tested, because we know they will fail:
    /// G (behaves like R).
    /// </summary>
    [TestFixture]
    public class StringFormatTest
    {
        /// <summary>
        /// Setup the test. Nothing to setup for this test.
        /// </summary>
        [SetUp]
        public void SetupTest()
        {
            
        }

        /// <summary>
        /// Cleanup the test. Nothing to cleanup for this test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
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
        public void TestPositiveIntegers()
        {
            Assert.AreEqual("1013", String.Format(CultureInfo.InvariantCulture, "{0}", 1013));
            Assert.AreEqual("1013", String.Format(CultureInfo.InvariantCulture, "{0:D}", 1013));
            Assert.AreEqual("1013", String.Format(CultureInfo.InvariantCulture, "{0:D3}", 1013));
            Assert.AreEqual("1013", String.Format(CultureInfo.InvariantCulture, "{0:D4}", 1013));
            Assert.AreEqual("01013", String.Format(CultureInfo.InvariantCulture, "{0:D5}", 1013));
            Assert.AreEqual("001013", String.Format(CultureInfo.InvariantCulture, "{0:D6}", 1013));
        }

        [Test]
        public void TestNegativeIntegers()
        {
            Assert.AreEqual("-1013", String.Format(CultureInfo.InvariantCulture, "{0}", -1013));
            Assert.AreEqual("-1013", String.Format(CultureInfo.InvariantCulture, "{0:D}", -1013));
            Assert.AreEqual("-1013", String.Format(CultureInfo.InvariantCulture, "{0:D3}", -1013));
            Assert.AreEqual("-1013", String.Format(CultureInfo.InvariantCulture, "{0:D4}", -1013));
            Assert.AreEqual("-01013", String.Format(CultureInfo.InvariantCulture, "{0:D5}", -1013));
            Assert.AreEqual("-001013", String.Format(CultureInfo.InvariantCulture, "{0:D6}", -1013));
        }

        [Test]
        public void TestPositiveLong()
        {
            Assert.AreEqual("1013", String.Format(CultureInfo.InvariantCulture, "{0}", 1013L));
            Assert.AreEqual("1013", String.Format(CultureInfo.InvariantCulture, "{0:D}", 1013L));
            Assert.AreEqual("1013", String.Format(CultureInfo.InvariantCulture, "{0:D3}", 1013L));
            Assert.AreEqual("1013", String.Format(CultureInfo.InvariantCulture, "{0:D4}", 1013L));
            Assert.AreEqual("01013", String.Format(CultureInfo.InvariantCulture, "{0:D5}", 1013L));
            Assert.AreEqual("001013", String.Format(CultureInfo.InvariantCulture, "{0:D6}", 1013L));
        }

        [Test]
        public void TestNegativeLong()
        {
            Assert.AreEqual("-1013", String.Format(CultureInfo.InvariantCulture, "{0}", -1013L));
            Assert.AreEqual("-1013", String.Format(CultureInfo.InvariantCulture, "{0:D}", -1013L));
            Assert.AreEqual("-1013", String.Format(CultureInfo.InvariantCulture, "{0:D3}", -1013L));
            Assert.AreEqual("-1013", String.Format(CultureInfo.InvariantCulture, "{0:D4}", -1013L));
            Assert.AreEqual("-01013", String.Format(CultureInfo.InvariantCulture, "{0:D5}", -1013L));
            Assert.AreEqual("-001013", String.Format(CultureInfo.InvariantCulture, "{0:D6}", -1013L));
        }

        [Test]
        public void TestPositiveFloat()
        {
            // Because the float is converted to double, it changes value a little bit.
            // This shows at the least significant digits.
            Assert.AreEqual("3141.59252929688", String.Format(CultureInfo.InvariantCulture, "{0}", 3141.592529F));
            Assert.AreEqual("3,141.59", String.Format(CultureInfo.InvariantCulture, "{0:N}", 3141.592529F));
            Assert.AreEqual("3,141.593", String.Format(CultureInfo.InvariantCulture, "{0:N3}", 3141.592529F));
            Assert.AreEqual("3,141.5925", String.Format(CultureInfo.InvariantCulture, "{0:N4}", 3141.592529F));
            Assert.AreEqual("3,141.59253", String.Format(CultureInfo.InvariantCulture, "{0:N5}", 3141.592529F));
            Assert.AreEqual("3,141.592529", String.Format(CultureInfo.InvariantCulture, "{0:N6}", 3141.592529F));
            Assert.AreEqual("3,141,592", String.Format(CultureInfo.InvariantCulture, "{0:N0}", 3141592F));
            Assert.AreEqual("3,141,592.00", String.Format(CultureInfo.InvariantCulture, "{0:N}", 3141592F));

            Assert.AreEqual("3141.59", String.Format(CultureInfo.InvariantCulture, "{0:F}", 3141.592529F));
            Assert.AreEqual("3141.593", String.Format(CultureInfo.InvariantCulture, "{0:F3}", 3141.592529F));
            Assert.AreEqual("3141.5925", String.Format(CultureInfo.InvariantCulture, "{0:F4}", 3141.592529F));
            Assert.AreEqual("3141.59253", String.Format(CultureInfo.InvariantCulture, "{0:F5}", 3141.592529F));
            Assert.AreEqual("3141.592529", String.Format(CultureInfo.InvariantCulture, "{0:F6}", 3141.592529F));
            Assert.AreEqual("3141592", String.Format(CultureInfo.InvariantCulture, "{0:F0}", 3141592F));
            Assert.AreEqual("3141592.00", String.Format(CultureInfo.InvariantCulture, "{0:F}", 3141592F));

            Assert.AreEqual("3.141593E+003", String.Format(CultureInfo.InvariantCulture, "{0:E}", 3141.592529F));
            Assert.AreEqual("3.142E+004", String.Format(CultureInfo.InvariantCulture, "{0:E3}", 31415.92529F));
            Assert.AreEqual("3.1416E+005", String.Format(CultureInfo.InvariantCulture, "{0:E4}", 314159.2529F));
            Assert.AreEqual("3.14159E+006", String.Format(CultureInfo.InvariantCulture, "{0:E5}", 3141592.529F));
            Assert.AreEqual("3.141593E+003", String.Format(CultureInfo.InvariantCulture, "{0:E6}", 3141.592529F));
            Assert.AreEqual("3E+006", String.Format(CultureInfo.InvariantCulture, "{0:E0}", 3141592F));
            Assert.AreEqual("3.141592E+006", String.Format(CultureInfo.InvariantCulture, "{0:E}", 3141592F));

            Assert.AreEqual("3.141592E-003", String.Format(CultureInfo.InvariantCulture, "{0:E}", 0.003141592529F));
            Assert.AreEqual("3.142E-004", String.Format(CultureInfo.InvariantCulture, "{0:E3}", 0.0003141592529F));
            Assert.AreEqual("3.1416E-005", String.Format(CultureInfo.InvariantCulture, "{0:E4}", 0.00003141592529F));
            Assert.AreEqual("3.14159E-006", String.Format(CultureInfo.InvariantCulture, "{0:E5}", 0.000003141592529F));
            Assert.AreEqual("3.141592E-003", String.Format(CultureInfo.InvariantCulture, "{0:E6}", 0.003141592529F));
            Assert.AreEqual("3E-003", String.Format(CultureInfo.InvariantCulture, "{0:E0}", 0.003141592529F));
        }

        [Test]
        public void TestNegativeFloat()
        {
            // Because the float is converted to double, it changes value a little bit.
            // This shows at the least significant digits.
            Assert.AreEqual("-3141.59252929688", String.Format(CultureInfo.InvariantCulture, "{0}", -3141.592529F));
            Assert.AreEqual("-3,141.59", String.Format(CultureInfo.InvariantCulture, "{0:N}", -3141.592529F));
            Assert.AreEqual("-3,141.593", String.Format(CultureInfo.InvariantCulture, "{0:N3}", -3141.592529F));
            Assert.AreEqual("-3,141.5925", String.Format(CultureInfo.InvariantCulture, "{0:N4}", -3141.592529F));
            Assert.AreEqual("-3,141.59253", String.Format(CultureInfo.InvariantCulture, "{0:N5}", -3141.592529F));
            Assert.AreEqual("-3,141.592529", String.Format(CultureInfo.InvariantCulture, "{0:N6}", -3141.592529F));
            Assert.AreEqual("-3,141,592", String.Format(CultureInfo.InvariantCulture, "{0:N0}", -3141592F));
            Assert.AreEqual("-3,141,592.00", String.Format(CultureInfo.InvariantCulture, "{0:N}", -3141592F));

            Assert.AreEqual("-3141.59", String.Format(CultureInfo.InvariantCulture, "{0:F}", -3141.592529F));
            Assert.AreEqual("-3141.593", String.Format(CultureInfo.InvariantCulture, "{0:F3}", -3141.592529F));
            Assert.AreEqual("-3141.5925", String.Format(CultureInfo.InvariantCulture, "{0:F4}", -3141.592529F));
            Assert.AreEqual("-3141.59253", String.Format(CultureInfo.InvariantCulture, "{0:F5}", -3141.592529F));
            Assert.AreEqual("-3141.592529", String.Format(CultureInfo.InvariantCulture, "{0:F6}", -3141.592529F));
            Assert.AreEqual("-3141592", String.Format(CultureInfo.InvariantCulture, "{0:F0}", -3141592F));
            Assert.AreEqual("-3141592.00", String.Format(CultureInfo.InvariantCulture, "{0:F}", -3141592F));

            Assert.AreEqual("-3.141593E+003", String.Format(CultureInfo.InvariantCulture, "{0:E}", -3141.592529F));
            Assert.AreEqual("-3.142E+004", String.Format(CultureInfo.InvariantCulture, "{0:E3}", -31415.92529F));
            Assert.AreEqual("-3.1416E+005", String.Format(CultureInfo.InvariantCulture, "{0:E4}", -314159.2529F));
            Assert.AreEqual("-3.14159E+006", String.Format(CultureInfo.InvariantCulture, "{0:E5}", -3141592.529F));
            Assert.AreEqual("-3.141593E+003", String.Format(CultureInfo.InvariantCulture, "{0:E6}", -3141.592529F));
            Assert.AreEqual("-3E+006", String.Format(CultureInfo.InvariantCulture, "{0:E0}", -3141592F));
            Assert.AreEqual("-3.141592E+006", String.Format(CultureInfo.InvariantCulture, "{0:E}", -3141592F));

            Assert.AreEqual("-3.141592E-003", String.Format(CultureInfo.InvariantCulture, "{0:E}", -0.003141592529F));
            Assert.AreEqual("-3.142E-004", String.Format(CultureInfo.InvariantCulture, "{0:E3}", -0.0003141592529F));
            Assert.AreEqual("-3.1416E-005", String.Format(CultureInfo.InvariantCulture, "{0:E4}", -0.00003141592529F));
            Assert.AreEqual("-3.14159E-006", String.Format(CultureInfo.InvariantCulture, "{0:E5}", -0.000003141592529F));
            Assert.AreEqual("-3.141592E-003", String.Format(CultureInfo.InvariantCulture, "{0:E6}", -0.003141592529F));
            Assert.AreEqual("-3E-003", String.Format(CultureInfo.InvariantCulture, "{0:E0}", -0.003141592529F));
        }

        [Test]
        public void TestPositiveDouble()
        {
            Assert.AreEqual("3141.592529", String.Format(CultureInfo.InvariantCulture, "{0}", 3141.592529));
            Assert.AreEqual("3,141.59", String.Format(CultureInfo.InvariantCulture, "{0:N}", 3141.592529));
            Assert.AreEqual("3,141.593", String.Format(CultureInfo.InvariantCulture, "{0:N3}", 3141.592529));
            Assert.AreEqual("3,141.5925", String.Format(CultureInfo.InvariantCulture, "{0:N4}", 3141.592529));
            Assert.AreEqual("3,141.59253", String.Format(CultureInfo.InvariantCulture, "{0:N5}", 3141.592529));
            Assert.AreEqual("3,141.592529", String.Format(CultureInfo.InvariantCulture, "{0:N6}", 3141.592529));
            Assert.AreEqual("3,141,592,529", String.Format(CultureInfo.InvariantCulture, "{0:N0}", 3141592529.0));
            Assert.AreEqual("3,141,592,529.00", String.Format(CultureInfo.InvariantCulture, "{0:N}", 3141592529.0));

            Assert.AreEqual("3141.59", String.Format(CultureInfo.InvariantCulture, "{0:F}", 3141.592529));
            Assert.AreEqual("3141.593", String.Format(CultureInfo.InvariantCulture, "{0:F3}", 3141.592529));
            Assert.AreEqual("3141.5925", String.Format(CultureInfo.InvariantCulture, "{0:F4}", 3141.592529));
            Assert.AreEqual("3141.59253", String.Format(CultureInfo.InvariantCulture, "{0:F5}", 3141.592529));
            Assert.AreEqual("3141.592529", String.Format(CultureInfo.InvariantCulture, "{0:F6}", 3141.592529));
            Assert.AreEqual("3141592529", String.Format(CultureInfo.InvariantCulture, "{0:F0}", 3141592529.0));
            Assert.AreEqual("3141592529.00", String.Format(CultureInfo.InvariantCulture, "{0:F}", 3141592529.0));
            Assert.AreEqual("3141592529", String.Format(CultureInfo.InvariantCulture, "{0}", 3141592529.0));
        }

        [Test]
        public void TestNegativeDouble()
        {
            Assert.AreEqual("-3141.592529", String.Format(CultureInfo.InvariantCulture, "{0}", -3141.592529));
            Assert.AreEqual("-3,141.59", String.Format(CultureInfo.InvariantCulture, "{0:N}", -3141.592529));
            Assert.AreEqual("-3,141.593", String.Format(CultureInfo.InvariantCulture, "{0:N3}", -3141.592529));
            Assert.AreEqual("-3,141.5925", String.Format(CultureInfo.InvariantCulture, "{0:N4}", -3141.592529));
            Assert.AreEqual("-3,141.59253", String.Format(CultureInfo.InvariantCulture, "{0:N5}", -3141.592529));
            Assert.AreEqual("-3,141.592529", String.Format(CultureInfo.InvariantCulture, "{0:N6}", -3141.592529));
            Assert.AreEqual("-3,141,592,529", String.Format(CultureInfo.InvariantCulture, "{0:N0}", -3141592529.0));
            Assert.AreEqual("-3,141,592,529.00", String.Format(CultureInfo.InvariantCulture, "{0:N}", -3141592529.0));

            Assert.AreEqual("-3141.59", String.Format(CultureInfo.InvariantCulture, "{0:F}", -3141.592529));
            Assert.AreEqual("-3141.593", String.Format(CultureInfo.InvariantCulture, "{0:F3}", -3141.592529));
            Assert.AreEqual("-3141.5925", String.Format(CultureInfo.InvariantCulture, "{0:F4}", -3141.592529));
            Assert.AreEqual("-3141.59253", String.Format(CultureInfo.InvariantCulture, "{0:F5}", -3141.592529));
            Assert.AreEqual("-3141.592529", String.Format(CultureInfo.InvariantCulture, "{0:F6}", -3141.592529));
            Assert.AreEqual("-3141592529", String.Format(CultureInfo.InvariantCulture, "{0:F0}", -3141592529.0));
            Assert.AreEqual("-3141592529.00", String.Format(CultureInfo.InvariantCulture, "{0:F}", -3141592529.0));
            Assert.AreEqual("-3141592529", String.Format(CultureInfo.InvariantCulture, "{0}", -3141592529.0));
        }

        [Test]
        public void TestCurrency()
        {
            Assert.AreEqual("$3,141.59", String.Format(CultureInfo.InvariantCulture, "{0:C}", 3141.592529));
            Assert.AreEqual("$3,141.59", String.Format(CultureInfo.InvariantCulture, "{0:C}", 3141.592529));
            Assert.AreEqual("$3,141.593", String.Format(CultureInfo.InvariantCulture, "{0:C3}", 3141.592529));
            Assert.AreEqual("$3,141.5925", String.Format(CultureInfo.InvariantCulture, "{0:C4}", 3141.592529));
            Assert.AreEqual("$3,141.59253", String.Format(CultureInfo.InvariantCulture, "{0:C5}", 3141.592529));
            Assert.AreEqual("$3,141.592529", String.Format(CultureInfo.InvariantCulture, "{0:C6}", 3141.592529));
            Assert.AreEqual("$3,141,592,529", String.Format(CultureInfo.InvariantCulture, "{0:C0}", 3141592529.0));
            Assert.AreEqual("$3,141,592,529.00", String.Format(CultureInfo.InvariantCulture, "{0:C}", 3141592529.0));

            Assert.AreEqual("-$3,141.59", String.Format(CultureInfo.InvariantCulture, "{0:C}", -3141.592529));
            Assert.AreEqual("-$3,141.593", String.Format(CultureInfo.InvariantCulture, "{0:C3}", -3141.592529));
            Assert.AreEqual("-$3,141.5925", String.Format(CultureInfo.InvariantCulture, "{0:C4}", -3141.592529));
            Assert.AreEqual("-$3,141.59253", String.Format(CultureInfo.InvariantCulture, "{0:C5}", -3141.592529));
            Assert.AreEqual("-$3,141.592529", String.Format(CultureInfo.InvariantCulture, "{0:C6}", -3141.592529));
            Assert.AreEqual("-$3,141,592,529", String.Format(CultureInfo.InvariantCulture, "{0:C0}", -3141592529.0));
            Assert.AreEqual("-$3,141,592,529.00", String.Format(CultureInfo.InvariantCulture, "{0:C}", -3141592529.0));
        }

        [Test]
        public void TestPercentage()
        {
            Assert.AreEqual("31.42 %", String.Format(CultureInfo.InvariantCulture, "{0:P}", 0.314159265));
            Assert.AreEqual("31.416 %", String.Format(CultureInfo.InvariantCulture, "{0:P3}", 0.314159265));
            Assert.AreEqual("314.1593 %", String.Format(CultureInfo.InvariantCulture, "{0:P4}", 3.14159265));
            Assert.AreEqual("3141.59265 %", String.Format(CultureInfo.InvariantCulture, "{0:P5}", 31.4159265));
            Assert.AreEqual("31415.926500 %", String.Format(CultureInfo.InvariantCulture, "{0:P6}", 314.159265));
            Assert.AreEqual("3141593 %", String.Format(CultureInfo.InvariantCulture, "{0:P0}", 31415.9265));
            Assert.AreEqual("3141592.65 %", String.Format(CultureInfo.InvariantCulture, "{0:P}", 31415.9265));

            Assert.AreEqual("-31.42 %", String.Format(CultureInfo.InvariantCulture, "{0:P}", -0.314159265));
            Assert.AreEqual("-31.416 %", String.Format(CultureInfo.InvariantCulture, "{0:P3}", -0.314159265));
            Assert.AreEqual("-314.1593 %", String.Format(CultureInfo.InvariantCulture, "{0:P4}", -3.14159265));
            Assert.AreEqual("-3141.59265 %", String.Format(CultureInfo.InvariantCulture, "{0:P5}", -31.4159265));
            Assert.AreEqual("-31415.926500 %", String.Format(CultureInfo.InvariantCulture, "{0:P6}", -314.159265));
            Assert.AreEqual("-3141593 %", String.Format(CultureInfo.InvariantCulture, "{0:P0}", -31415.9265));
            Assert.AreEqual("-3141592.65 %", String.Format(CultureInfo.InvariantCulture, "{0:P}", -31415.9265));
        }

        [Test]
        public void TestHex()
        {
            Assert.AreEqual("12ABCDEF", String.Format(CultureInfo.InvariantCulture, "{0:X}", 0x12ABCDEF));
            Assert.AreEqual("12ABCDEF", String.Format(CultureInfo.InvariantCulture, "{0:X0}", 0x12ABCDEF));
            Assert.AreEqual("12ABCDEF", String.Format(CultureInfo.InvariantCulture, "{0:X7}", 0x12ABCDEF));
            Assert.AreEqual("12ABCDEF", String.Format(CultureInfo.InvariantCulture, "{0:X8}", 0x12ABCDEF));
            Assert.AreEqual("012ABCDEF", String.Format(CultureInfo.InvariantCulture, "{0:X9}", 0x12ABCDEF));
            Assert.AreEqual("0012ABCDEF", String.Format(CultureInfo.InvariantCulture, "{0:X10}", 0x12ABCDEF));

            Assert.AreEqual("ED543211", String.Format(CultureInfo.InvariantCulture, "{0:X}", (long)unchecked((uint)-0x12ABCDEF)));
            Assert.AreEqual("ED543211", String.Format(CultureInfo.InvariantCulture, "{0:X0}", (long)unchecked((uint)-0x12ABCDEF)));
            Assert.AreEqual("ED543211", String.Format(CultureInfo.InvariantCulture, "{0:X7}", (long)unchecked((uint)-0x12ABCDEF)));
            Assert.AreEqual("ED543211", String.Format(CultureInfo.InvariantCulture, "{0:X8}", (long)unchecked((uint)-0x12ABCDEF)));
            Assert.AreEqual("0ED543211", String.Format(CultureInfo.InvariantCulture, "{0:X9}", (long)unchecked((uint)-0x12ABCDEF)));
            Assert.AreEqual("00ED543211", String.Format(CultureInfo.InvariantCulture, "{0:X10}", (long)unchecked((uint)-0x12ABCDEF)));

            Assert.AreEqual("12abcdef", String.Format(CultureInfo.InvariantCulture, "{0:x}", 0x12ABCDEF));
            Assert.AreEqual("12abcdef", String.Format(CultureInfo.InvariantCulture, "{0:x0}", 0x12ABCDEF));
            Assert.AreEqual("12abcdef", String.Format(CultureInfo.InvariantCulture, "{0:x7}", 0x12ABCDEF));
            Assert.AreEqual("12abcdef", String.Format(CultureInfo.InvariantCulture, "{0:x8}", 0x12ABCDEF));
            Assert.AreEqual("012abcdef", String.Format(CultureInfo.InvariantCulture, "{0:x9}", 0x12ABCDEF));
            Assert.AreEqual("0012abcdef", String.Format(CultureInfo.InvariantCulture, "{0:x10}", 0x12ABCDEF));

            Assert.AreEqual("ed543211", String.Format(CultureInfo.InvariantCulture, "{0:x}", (long)unchecked((uint)-0x12ABCDEF)));
            Assert.AreEqual("ed543211", String.Format(CultureInfo.InvariantCulture, "{0:x0}", (long)unchecked((uint)-0x12ABCDEF)));
            Assert.AreEqual("ed543211", String.Format(CultureInfo.InvariantCulture, "{0:x7}", (long)unchecked((uint)-0x12ABCDEF)));
            Assert.AreEqual("ed543211", String.Format(CultureInfo.InvariantCulture, "{0:x8}", (long)unchecked((uint)-0x12ABCDEF)));
            Assert.AreEqual("0ed543211", String.Format(CultureInfo.InvariantCulture, "{0:x9}", (long)unchecked((uint)-0x12ABCDEF)));
            Assert.AreEqual("00ed543211", String.Format(CultureInfo.InvariantCulture, "{0:x10}", (long)unchecked((uint)-0x12ABCDEF)));
        }
    }
}
