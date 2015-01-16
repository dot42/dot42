using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestOutParam : TestCase
    {
        private byte byteInstance;
        private static byte byteStatic;
        private sbyte sbyteInstance;
        private static sbyte sbyteStatic;
        private bool boolInstance;
        private static bool boolStatic;
        private char charInstance;
        private static char charStatic;
        private short shortInstance;
        private static short shortStatic;
        private int intInstance;
        private static int intStatic;
        private uint uintInstance;
        private static uint uintStatic;
        private float floatInstance;
        private static float floatStatic;
        private long longInstance;
        private static long longStatic;
        private double doubleInstance;
        private static double doubleStatic;
        private object objectInstance;
        private static object objectStatic;
        private string stringInstance;
        private static string stringStatic;

        private static readonly object ObjectValue = new object();

        #region byte

        public void testByteLocal()
        {
            byte i;
            GetByteValue(out i);
            AssertTrue(i == 5);
            GetByteValue2(out i);
            AssertTrue(i == 5);
        }

        public void testByteArray()
        {
            var i = new byte[2];
            GetByteValue(out i[1]);
            AssertTrue(i[1] == 5);
            GetByteValue2(out i[0]);
            AssertTrue(i[0] == 5);
        }

        public void testByteParam()
        {
            byte i;
            i = GetByteParam((byte) 55);
            AssertTrue(i == 5);
        }

        public void testByteInstance()
        {
            GetByteValue(out byteInstance);
            AssertTrue(byteInstance == 5);
            GetByteValue2(out byteInstance);
            AssertTrue(byteInstance == 5);
        }

        public void testByteStatic()
        {
            GetByteValue(out byteStatic);
            AssertTrue(byteStatic == 5);
            GetByteValue2(out byteStatic);
            AssertTrue(byteStatic == 5);
        }

        public void testBytesLocal()
        {
            byte[] bytesLocal;
            GetBytesValue(out bytesLocal);
            AssertTrue(bytesLocal.Length == 3);
            AssertTrue(bytesLocal[0] == 1);
            AssertTrue(bytesLocal[1] == 2);
            AssertTrue(bytesLocal[2] == 3);
        }


        private byte GetByteParam(byte i)
        {
            GetByteValue(out i);
            return i;
        }

        private void GetByteValue(out byte i)
        {
            i = (byte) 5;
        }

        private void GetByteValue2(out byte i)
        {
            GetByteValue(out i);
        }

        private void GetBytesValue(out byte[] bytes)
        {
            bytes = new byte[]{1,2,3};
        }

        #endregion // byte

        #region sbyte

        public void testSByteLocal()
        {
            sbyte i;
            GetSByteValue(out i);
            AssertTrue(i == 5);
            GetSByteValue2(out i);
            AssertTrue(i == 5);
        }

        public void testSByteArray()
        {
            var i = new sbyte[2];
            GetSByteValue(out i[1]);
            AssertTrue(i[1] == 5);
            GetSByteValue2(out i[0]);
            AssertTrue(i[0] == 5);
        }

        public void testSByteParam()
        {
            sbyte i;
            i = GetSByteParam((sbyte)55);
            AssertTrue(i == 5);
        }

        public void testSByteInstance()
        {
            GetSByteValue(out sbyteInstance);
            AssertTrue(sbyteInstance == 5);
            GetSByteValue2(out sbyteInstance);
            AssertTrue(sbyteInstance == 5);
        }

        public void testSByteStatic()
        {
            GetSByteValue(out sbyteStatic);
            AssertTrue(sbyteStatic == 5);
            GetSByteValue2(out sbyteStatic);
            AssertTrue(sbyteStatic == 5);
        }

        private sbyte GetSByteParam(sbyte i)
        {
            GetSByteValue(out i);
            return i;
        }

        private void GetSByteValue(out sbyte i)
        {
            i = (sbyte)5;
        }

        private void GetSByteValue2(out sbyte i)
        {
            GetSByteValue(out i);
        }

        #endregion // byte

        #region bool

        public void testBoolLocal()
        {
            bool i;
            GetBoolValue(out i);
            AssertTrue(i);
            GetBoolValue2(out i);
            AssertTrue(i);
        }

        public void testBoolArray()
        {
            var i = new bool[2];
            GetBoolValue(out i[1]);
            AssertTrue(i[1]);
            GetBoolValue2(out i[0]);
            AssertTrue(i[0]);
        }

        public void testBoolParam()
        {
            bool i;
            i = GetBoolParam(false);
            AssertTrue(i);
        }

        public void testBoolInstance()
        {
            GetBoolValue(out boolInstance);
            AssertTrue(boolInstance);
            GetBoolValue2(out boolInstance);
            AssertTrue(boolInstance);
        }

        public void testBoolStatic()
        {
            GetBoolValue(out boolStatic);
            AssertTrue(boolStatic);
            GetBoolValue2(out boolStatic);
            AssertTrue(boolStatic);
        }

        private bool GetBoolParam(bool i)
        {
            GetBoolValue(out i);
            return i;
        }

        private void GetBoolValue(out bool i)
        {
            i = true;
        }

        private void GetBoolValue2(out bool i)
        {
            GetBoolValue(out i);
        }

        #endregion // bool

        #region char

        public void testCharLocal()
        {
            char i;
            GetCharValue(out i);
            AssertTrue(i == 't');
            GetCharValue2(out i);
            AssertTrue(i == 't');
        }

        public void testCharArray()
        {
            var i = new char[2];
            GetCharValue(out i[1]);
            AssertTrue(i[1] == 't');
            GetCharValue2(out i[0]);
            AssertTrue(i[0] == 't');
        }

        public void testCharParam()
        {
            char i;
            i = GetCharParam('d');
            AssertTrue(i == 't');
        }

        public void testCharInstance()
        {
            GetCharValue(out charInstance);
            AssertTrue(charInstance == 't');
            GetCharValue2(out charInstance);
            AssertTrue(charInstance == 't');
        }

        public void testCharStatic()
        {
            GetCharValue(out charStatic);
            AssertTrue(charStatic == 't');
            GetCharValue2(out charStatic);
            AssertTrue(charStatic == 't');
        }

        private char GetCharParam(char i)
        {
            GetCharValue(out i);
            return i;
        }

        private void GetCharValue(out char i)
        {
            i = 't';
        }

        private void GetCharValue2(out char i)
        {
            GetCharValue(out i);
        }

        #endregion // char

        #region short

        public void testShortLocal()
        {
            short i;
            GetShortValue(out i);
            AssertTrue(i == 5);
            GetShortValue2(out i);
            AssertTrue(i == 5);
        }

        public void testShortArray()
        {
            var i = new short[2];
            GetShortValue(out i[1]);
            AssertTrue(i[1] == 5);
            GetShortValue2(out i[0]);
            AssertTrue(i[0] == 5);
        }

        public void testShortParam()
        {
            short i;
            i = GetShortParam((short)55);
            AssertTrue(i == 5);
        }

        public void testShortInstance()
        {
            GetShortValue(out shortInstance);
            AssertTrue(shortInstance == 5);
            GetShortValue2(out shortInstance);
            AssertTrue(shortInstance == 5);
        }

        public void testShortStatic()
        {
            GetShortValue(out shortStatic);
            AssertTrue(shortStatic == 5);
            GetShortValue2(out shortStatic);
            AssertTrue(shortStatic == 5);
        }

        private short GetShortParam(short i)
        {
            GetShortValue(out i);
            return i;
        }

        private void GetShortValue(out short i)
        {
            i = (short) 5;
        }

        private void GetShortValue2(out short i)
        {
            GetShortValue(out i);
        }

        #endregion // short

        #region int

        public void testIntLocal()
        {
            int i;
            GetIntValue(out i);
            AssertTrue(i == 5);
            GetIntValue2(out i);
            AssertTrue(i == 5);
        }

        public void testIntArray()
        {
            int[] i = new int[2];
            GetIntValue(out i[1]);
            AssertTrue(i[1] == 5);
            GetIntValue2(out i[0]);
            AssertTrue(i[0] == 5);
        }

        public void testIntParam()
        {
            int i;
            i = GetIntParam(55);
            AssertTrue(i == 5);
        }

        public void testIntInstance()
        {
            GetIntValue(out intInstance);
            AssertTrue(intInstance == 5);
            GetIntValue2(out intInstance);
            AssertTrue(intInstance == 5);
        }

        public void testIntStatic()
        {
            GetIntValue(out intStatic);
            AssertTrue(intStatic == 5);
            GetIntValue2(out intStatic);
            AssertTrue(intStatic == 5);
        }

        private int GetIntParam(int i)
        {
            GetIntValue(out i);
            return i;
        }

        private void GetIntValue(out int i)
        {
            i = 5;
        }

        private void GetIntValue2(out int i)
        {
            GetIntValue(out i);
        }

        #endregion // int

        #region uint

        public void testUIntLocal()
        {
            uint i;
            GetUIntValue(out i);
            AssertTrue(i == 5);
            GetUIntValue2(out i);
            AssertTrue(i == 5);
        }

        public void testUIntArray()
        {
            uint[] i = new uint[2];
            GetUIntValue(out i[1]);
            AssertTrue(i[1] == 5);
            GetUIntValue2(out i[0]);
            AssertTrue(i[0] == 5);
        }

        public void testUIntParam()
        {
            uint i;
            i = GetUIntParam(55);
            AssertTrue(i == 5);
        }

        public void testUIntValue2()
        {
            uint i;
            GetUIntValue2(42, out i);
            AssertTrue(i == 42);
        }

        public void testUIntInstance()
        {
            GetUIntValue(out uintInstance);
            AssertTrue(uintInstance == 5);
            GetUIntValue2(out uintInstance);
            AssertTrue(uintInstance == 5);
        }

        public void testUIntStatic()
        {
            GetUIntValue(out uintStatic);
            AssertTrue(uintStatic == 5);
            GetUIntValue2(out uintStatic);
            AssertTrue(uintStatic == 5);
        }

        private uint GetUIntParam(uint i)
        {
            GetUIntValue(out i);
            return i;
        }

        private void GetUIntValue(out uint i)
        {
            i = 5;
        }

        private void GetUIntValue2(uint d, out uint i)
        {
            i = d;
        }

        private void GetUIntValue2(out uint i)
        {
            GetUIntValue(out i);
        }

        #endregion // uint

        #region long

        public void testLongLocal()
        {
            long i;
            GetLongValue(out i);
            AssertTrue(i == 5);
            GetLongValue2(out i);
            AssertTrue(i == 5);
        }

        public void testLongArray()
        {
            var i = new long[2];
            GetLongValue(out i[1]);
            AssertTrue(i[1] == 5);
            GetLongValue2(out i[0]);
            AssertTrue(i[0] == 5);
        }

        public void testLongParam()
        {
            long i;
            i = GetLongParam(55);
            AssertTrue(i == 5);
        }

        public void testLongInstance()
        {
            GetLongValue(out longInstance);
            AssertTrue(longInstance == 5);
            GetLongValue2(out longInstance);
            AssertTrue(longInstance == 5);
        }

        public void testLongStatic()
        {
            GetLongValue(out longStatic);
            AssertTrue(longStatic == 5);
            GetLongValue2(out longStatic);
            AssertTrue(longStatic == 5);
        }

        private long GetLongParam(long i)
        {
            GetLongValue(out i);
            return i;
        }

        private void GetLongValue(out long i)
        {
            i = 5;
        }

        private void GetLongValue2(out long i)
        {
            GetLongValue(out i);
        }

        #endregion // long

        #region float

        public void testFloatLocal()
        {
            float i;
            GetFloatValue(out i);
            AssertTrue(i == 5.0f);
            GetFloatValue2(out i);
            AssertTrue(i == 5.0f);
        }

        public void testFloatArray()
        {
            var i = new float[2];
            GetFloatValue(out i[1]);
            AssertTrue(i[1] == 5.0f);
            GetFloatValue2(out i[0]);
            AssertTrue(i[0] == 5.0f);
        }

        public void testFloatParam()
        {
            float i;
            i = GetFloatParam(55.0f);
            AssertTrue(i == 5.0f);
        }

        public void testFloatInstance()
        {
            GetFloatValue(out floatInstance);
            AssertTrue(floatInstance == 5.0f);
            GetFloatValue2(out floatInstance);
            AssertTrue(floatInstance == 5.0f);
        }

        public void testFloatStatic()
        {
            GetFloatValue(out floatStatic);
            AssertTrue(floatStatic == 5.0f);
            GetFloatValue2(out floatStatic);
            AssertTrue(floatStatic == 5.0f);
        }

        private float GetFloatParam(float i)
        {
            GetFloatValue(out i);
            return i;
        }

        private void GetFloatValue(out float i)
        {
            i = 5.0f;
        }

        private void GetFloatValue2(out float i)
        {
            GetFloatValue(out i);
        }

        #endregion // float

        #region double

        public void testDoubleLocal()
        {
            double i;
            GetDoubleValue(out i);
            AssertTrue(i == 5.0);
            GetDoubleValue2(out i);
            AssertTrue(i == 5.0);
        }

        public void testDoubleArray()
        {
            var i = new double[2];
            GetDoubleValue(out i[1]);
            AssertTrue(i[1] == 5.0);
            GetDoubleValue2(out i[0]);
            AssertTrue(i[0] == 5.0);
        }

        public void testDoubleParam()
        {
            double i;
            i = GetDoubleParam(55.0);
            AssertTrue(i == 5.0);
        }

        public void testDoubleInstance()
        {
            GetDoubleValue(out doubleInstance);
            AssertTrue(doubleInstance == 5.0);
            GetDoubleValue2(out doubleInstance);
            AssertTrue(doubleInstance == 5.0);
        }

        public void testDoubleStatic()
        {
            GetDoubleValue(out doubleStatic);
            AssertTrue(doubleStatic == 5.0);
            GetDoubleValue2(out doubleStatic);
            AssertTrue(doubleStatic == 5.0);
        }

        private double GetDoubleParam(double i)
        {
            GetDoubleValue(out i);
            return i;
        }

        private void GetDoubleValue(out double i)
        {
            i = 5.0;
        }

        private void GetDoubleValue2(out double i)
        {
            GetDoubleValue(out i);
        }

        public void testDoubleOut1()
        {
            double result;
            if (GetLiter(out result))
            {
                AssertEquals(12.5, result);
            }
            else
            {
                Fail();
            }
        }

        private bool GetLiter(out double xLiter)
        {
            return double.TryParse("12,5".Replace(',', '.'), out xLiter) &&
            !double.IsNaN(xLiter) && !double.IsInfinity(xLiter);
        } 

        #endregion // double

        #region object

        public void testObjectLocal()
        {
            object i;
            GetObjectValue(out i);
            AssertTrue(i == ObjectValue);
            GetObjectValue2(out i);
            AssertTrue(i == ObjectValue);
        }

        public void testObjectArray()
        {
            var i = new object[2];
            GetObjectValue(out i[1]);
            AssertTrue(i[1] == ObjectValue);
            GetObjectValue2(out i[0]);
            AssertTrue(i[0] == ObjectValue);
        }

        public void testObjectParam()
        {
            object i;
            i = GetObjectParam(null);
            AssertTrue(i == ObjectValue);
        }

        public void testObjectInstance()
        {
            GetObjectValue(out objectInstance);
            AssertTrue(objectInstance == ObjectValue);
            GetObjectValue2(out objectInstance);
            AssertTrue(objectInstance == ObjectValue);
        }

        public void testObjectStatic()
        {
            GetObjectValue(out objectStatic);
            AssertTrue(objectStatic == ObjectValue);
            GetObjectValue2(out objectStatic);
            AssertTrue(objectStatic == ObjectValue);
        }

        private object GetObjectParam(object i)
        {
            GetObjectValue(out i);
            return i;
        }

        private void GetObjectValue(out object i)
        {
            i = ObjectValue;
        }

        private void GetObjectValue2(out object i)
        {
            GetObjectValue(out i);
        }

        #endregion // object

        #region string

        public void testStringLocal()
        {
            string i;
            GetStringValue(out i);
            AssertTrue(i == "test");
            GetStringValue2(out i);
            AssertTrue(i == "test");
        }

        public void testStringArray()
        {
            var i = new string[2];
            GetStringValue(out i[1]);
            AssertTrue(i[1] == "test");
            GetStringValue2(out i[0]);
            AssertTrue(i[0] == "test");
        }

        public void testStringParam()
        {
            string i;
            i = GetStringParam(null);
            AssertTrue(i == "test");
        }

        public void testStringInstance()
        {
            GetStringValue(out stringInstance);
            AssertTrue(stringInstance == "test");
            GetStringValue2(out stringInstance);
            AssertTrue(stringInstance == "test");
        }

        public void testStringStatic()
        {
            GetStringValue(out stringStatic);
            AssertTrue(stringStatic == "test");
            GetStringValue2(out stringStatic);
            AssertTrue(stringStatic == "test");
        }

        private string GetStringParam(string i)
        {
            GetStringValue(out i);
            return i;
        }

        private void GetStringValue(out string i)
        {
            i = "test";
        }

        private void GetStringValue2(out string i)
        {
            GetStringValue(out i);
        }

        #endregion // string

        #region misc

        public void test1()
        {
            object i;
            var result = GetObjectValueNull(out i);
            AssertNull(i);
            AssertFalse(result);
        }

        private bool GetObjectValueNull(out object i)
        {
            i = null;
            return i != null;
        }

        public void _test2()
        {
            var innerClass = new InnerClass<object>();

            object i;
            var result = innerClass.GetValueNull(out i);
            AssertNull(i);
            AssertFalse(result);
        }

        internal class InnerClass<T> where T : class
        {
            public bool GetValueNull(out T param)
            {
                param = null;
                return param != null;
            }

        }

        public void test3()
        {
            OutParamEnum o;
            SetEnumValue(out o);
            AssertEquals(OutParamEnum.b, o);
        }

        private void SetEnumValue(out OutParamEnum e)
        {
            e = OutParamEnum.b;
        }

        private enum OutParamEnum
        {
            a,
            b,
            c
        }

        public void _test4()
        {
            OutParamStruct s;
            SetEnumValue(out s);
            AssertEquals(10, s.A);
        }

        private void SetEnumValue(out OutParamStruct s)
        {
            s.A = 10;
        }

        private struct OutParamStruct
        {
            public int A;
        }

        #endregion
    }
}
