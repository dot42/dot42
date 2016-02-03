using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestByRefParam : TestCase
    {
        internal enum MyEnum
        {
            a, b, c, d, e
        }

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
        private MyEnum enumInstance;
        private static MyEnum enumStatic;

        private static readonly object ObjectValue = new object();

        #region byte

        public void testByteLocal()
        {
            byte i = 7;
            GetByteValue(ref i);
            AssertTrue(i == 5);
            GetValue(ref i, (byte)15);
            AssertTrue(i == 15);
        }

        public void testByteArray()
        {
            var i = new byte[2] { (byte)3, (byte)7 };
            GetByteValue(ref i[1]);
            AssertTrue(i[1] == 5);
            GetValue(ref i[0], (byte)15);
            AssertTrue(i[0] == 15);
        }

        public void testByteInstance()
        {
            byteInstance = (byte) 12;
            GetByteValue(ref byteInstance);
            AssertTrue(byteInstance == 10);
            GetByteValue2(ref byteInstance);
            AssertTrue(byteInstance == 10);
            GetValue(ref byteInstance, (byte)15);
            AssertTrue(byteInstance == 15);
        }

        public void testByteStatic()
        {
            byteStatic = (byte)24;
            GetByteValue(ref byteStatic);
            AssertTrue(byteStatic == 22);
            GetByteValue2(ref byteStatic);
            AssertTrue(byteStatic == 22);
            GetValue(ref byteStatic, (byte)15);
            AssertTrue(byteStatic == 15);
        }

        private void GetByteValue(ref byte i)
        {
            i = (byte)(i - 2);
        }

        private void GetByteValue2(ref byte i)
        {
            i = (byte)(i + 5);
            GetByteValue(ref i);
            i = (byte)(i - 3);
        }

        #endregion

        #region sbyte

        public void testSByteLocal()
        {
            sbyte i = 7;
            GetSByteValue(ref i);
            AssertTrue(i == 5);
            GetValue(ref i, (sbyte)15);
            AssertTrue(i == 15);
        }

        public void testSByteArray()
        {
            var i = new sbyte[2] { (sbyte)3, (sbyte)7 };
            GetSByteValue(ref i[1]);
            AssertTrue(i[1] == 5);
            GetValue(ref i[0], (sbyte)15);
            AssertTrue(i[0] == 15);
        }

        public void testSByteInstance()
        {
            sbyteInstance = (sbyte)12;
            GetSByteValue(ref sbyteInstance);
            AssertTrue(sbyteInstance == 10);
            GetSByteValue2(ref sbyteInstance);
            AssertTrue(sbyteInstance == 10);
            GetValue(ref sbyteInstance, (sbyte)15);
            AssertTrue(sbyteInstance == 15);
        }

        public void testSByteStatic()
        {
            sbyteStatic = (sbyte)24;
            GetSByteValue(ref sbyteStatic);
            AssertTrue(sbyteStatic == 22);
            GetSByteValue2(ref sbyteStatic);
            AssertTrue(sbyteStatic == 22);
            GetValue(ref sbyteStatic, (sbyte)15);
            AssertTrue(sbyteStatic == 15);
        }

        private void GetSByteValue(ref sbyte i)
        {
            i = (sbyte)(i - 2);
        }

        private void GetSByteValue2(ref sbyte i)
        {
            i = (sbyte)(i + 5);
            GetSByteValue(ref i);
            i = (sbyte)(i - 3);
        }

        #endregion

        #region bool

        public void testBoolLocal()
        {
            bool i = true;
            GetBoolValue(ref i);
            AssertFalse(i);
            GetValue(ref i, true);
            AssertTrue(i);
        }

        public void testBoolArray()
        {
            var i = new bool[2] { true, false };
            GetBoolValue(ref i[1]);
            AssertTrue(i[1]);
            GetValue(ref i[0], true);
            AssertTrue(i[0]);
        }

        public void testBoolInstance()
        {
            boolInstance = false;
            GetBoolValue(ref boolInstance);
            AssertTrue(boolInstance);
            boolInstance = false;
            GetBoolValue2(ref boolInstance);
            AssertTrue(boolInstance);
            GetValue(ref boolInstance, true);
            AssertTrue(boolInstance);
        }

        public void testBoolStatic()
        {
            boolStatic = false;
            GetBoolValue(ref boolStatic);
            AssertTrue(boolStatic);
            boolStatic = false;
            GetBoolValue2(ref boolStatic);
            AssertTrue(boolStatic);
            GetValue(ref boolStatic, false);
            AssertTrue(!boolStatic);
        }

        private void GetBoolValue(ref bool i)
        {
            i = !i;
        }

        private void GetBoolValue2(ref bool i)
        {
            i = !i;
            GetBoolValue(ref i);
            i = !i;
        }

        #endregion

        #region char

        public void testCharLocal()
        {
            char i = '7';
            GetCharValue(ref i);
            AssertTrue(i == '5');
            GetValue(ref i, 'b');
            AssertTrue(i == 'b');
        }

        public void testCharArray()
        {
            var i = new char[2] { '3', '7' };
            GetCharValue(ref i[1]);
            AssertTrue(i[1] == '5');
            GetValue(ref i[0], 'r');
            AssertTrue(i[0] == 'r');
        }

        public void testCharInstance()
        {
            charInstance = 'd';
            GetCharValue(ref charInstance);
            AssertTrue(charInstance == 'b');
            GetCharValue2(ref charInstance);
            AssertTrue(charInstance == 'b');
            GetValue(ref charInstance, 'c');
            AssertTrue(charInstance == 'c');
        }

        public void testCharStatic()
        {
            charStatic = 'D';
            GetCharValue(ref charStatic);
            AssertTrue(charStatic == 'B');
            GetCharValue2(ref charStatic);
            AssertTrue(charStatic == 'B');
            GetValue(ref charStatic, 't');
            AssertTrue(charStatic == 't');
        }

        private void GetCharValue(ref char i)
        {
            i = (char)(i - 2);
        }

        private void GetCharValue2(ref char i)
        {
            i = (char)(i + 5);
            GetCharValue(ref i);
            i = (char)(i - 3);
        }

        #endregion

        #region short

        public void testShortLocal()
        {
            short i = (short)7;
            GetShortValue(ref i);
            AssertTrue(i == 5.0f);
            GetValue(ref i, (short)15);
            AssertTrue(i == 15);
        }

        public void testShortArray()
        {
            var i = new short[2] { (short)3, (short)7 };
            GetShortValue(ref i[1]);
            AssertTrue(i[1] == 5);
            GetValue(ref i[0], (short)15);
            AssertTrue(i[0] == 15);
        }

        public void testShortInstance()
        {
            shortInstance = (short)12;
            GetShortValue(ref shortInstance);
            AssertTrue(shortInstance == 10);
            GetShortValue2(ref shortInstance);
            AssertTrue(shortInstance == 10);
            GetValue(ref shortInstance, (short)15);
            AssertTrue(shortInstance == 15);
        }

        public void testShortStatic()
        {
            shortStatic = (short)24;
            GetShortValue(ref shortStatic);
            AssertTrue(shortStatic == 22);
            GetShortValue2(ref shortStatic);
            AssertTrue(shortStatic == 22);
            GetValue(ref shortStatic, (short)15);
            AssertTrue(shortStatic == 15);
        }

        private void GetShortValue(ref short i)
        {
            i = (short)(i - 2);
        }

        private void GetShortValue2(ref short i)
        {
            i = (short)(i + 5);
            GetShortValue(ref i);
            i = (short)(i - 3);
        }

        #endregion

        #region int

        public void testIntLocal()
        {
            int i = 7;
            GetIntValue(ref i);
            AssertTrue(i == 5);
            GetValue(ref i, (int)15);
            AssertTrue(i == 15);
        }

        public void testIntLocal2()
        {
            int i = 7;
            GetIntValue3(ref i);
            AssertTrue(i == 4);
        }

        public void testIntArray()
        {
            var i = new int[2] { 3, 7 };
            GetIntValue(ref i[1]);
            AssertTrue(i[1] == 5);
            GetValue(ref i[0], (int)15);
            AssertTrue(i[0] == 15);
        }

        public void testIntInstance()
        {
            intInstance = 12;
            GetIntValue(ref intInstance);
            AssertTrue(intInstance == 10);
            GetIntValue2(ref intInstance);
            AssertTrue(intInstance == 10);
            GetValue(ref intInstance, (int)15);
            AssertTrue(intInstance == 15);
        }

        public void testIntStatic()
        {
            intStatic = 24;
            GetIntValue(ref intStatic);
            AssertTrue(intStatic == 22);
            GetIntValue2(ref intStatic);
            AssertTrue(intStatic == 22);
            GetValue(ref intStatic, (int)1500);
            AssertTrue(intStatic == 1500);
        }

        private void GetIntValue(ref int i)
        {
            i = i - 2;
        }

        private void GetIntValue2(ref int i)
        {
            i = i + 5;
            GetIntValue(ref i);
            i = i - 3;
        }

        private void GetIntValue3(ref int i)
        {
            GetIntValue5(i);
            i = i - 3;
        }

        private void GetIntValue5(int i)
        {
            //notihng to do.
        }


        #endregion

        #region long

        public void testLongLocal()
        {
            long i = 7L;
            GetLongValue(ref i);
            AssertTrue(i == 5);
            GetValue(ref i, 15L);
            AssertTrue(i == 15);
        }

        public void testLongArray()
        {
            var i = new long[2] { 3L, 7L };
            GetLongValue(ref i[1]);
            AssertTrue(i[1] == 5);
            GetValue(ref i[0], 15L);
            AssertTrue(i[0] == 15);
        }

        public void testLongInstance()
        {
            longInstance = 12L;
            GetLongValue(ref longInstance);
            AssertTrue(longInstance == 10);
            GetLongValue2(ref longInstance);
            AssertTrue(longInstance == 10);
            GetValue(ref longInstance, 15L);
            AssertTrue(longInstance == 15L);
        }

        public void testLongStatic()
        {
            longStatic = 24L;
            GetLongValue(ref longStatic);
            AssertTrue(longStatic == 22);
            GetLongValue2(ref longStatic);
            AssertTrue(longStatic == 22);
            GetValue(ref longStatic, 1600L);
            AssertTrue(longStatic == 1600L);
        }

        private void GetLongValue(ref long i)
        {
            i = i - 2;
        }

        private void GetLongValue2(ref long i)
        {
            i = i + 5;
            GetLongValue(ref i);
            i = i - 3;
        }

        #endregion

        #region float

        public void testFloatLocal()
        {
            float i = 7.0f;
            GetFloatValue(ref i);
            AssertTrue(i == 5.0f);
            GetValue(ref i, 15.0f);
            AssertTrue(i == 15.0f);
        }

        public void testFloatArray()
        {
            var i = new float[2] { 3.0f, 7.0f };
            GetFloatValue(ref i[1]);
            AssertTrue(i[1] == 5.0f);
            GetValue(ref i[0], 15.0f);
            AssertTrue(i[0] == 15.0f);
        }

        public void testFloatInstance()
        {
            floatInstance = 12.0f;
            GetFloatValue(ref floatInstance);
            AssertTrue(floatInstance == 10.0f);
            GetFloatValue2(ref floatInstance);
            AssertTrue(floatInstance == 10.0f);
            GetValue(ref floatInstance, 15.0f);
            AssertTrue(floatInstance == 15.0f);
        }

        public void testFloatStatic()
        {
            floatStatic = 24.0f;
            GetFloatValue(ref floatStatic);
            AssertTrue(floatStatic == 22.0f);
            GetFloatValue2(ref floatStatic);
            AssertTrue(floatStatic == 22.0f);
            GetValue(ref floatStatic, 15.0f);
            AssertTrue(floatStatic == 15.0f);
        }

        private void GetFloatValue(ref float i)
        {
            i = i - 2.0f;
        }

        private void GetFloatValue2(ref float i)
        {
            i = i + 5.0f;
            GetFloatValue(ref i);
            i = i - 3.0f;
        }

        #endregion

        #region double

        public void testDoubleLocal()
        {
            double i = 7.0;
            GetDoubleValue(ref i);
            AssertTrue(i == 5.0);
            GetValue(ref i, 15.0);
            AssertTrue(i == 15.0);
        }

        public void testDoubleArray()
        {
            var i = new double[2] { 3.0, 7.0 };
            GetDoubleValue(ref i[1]);
            AssertTrue(i[1] == 5.0);
            GetValue(ref i[0], 15.0);
            AssertTrue(i[0] == 15.0);
        }

        public void testDoubleInstance()
        {
            doubleInstance = 12.0;
            GetDoubleValue(ref doubleInstance);
            AssertTrue(doubleInstance == 10.0);
            GetDoubleValue2(ref doubleInstance);
            AssertTrue(doubleInstance == 10.0);
            GetValue(ref doubleInstance, 15.0);
            AssertTrue(doubleInstance == 15.0);
        }

        public void testDoubleStatic()
        {
            doubleStatic = 24.0;
            GetDoubleValue(ref doubleStatic);
            AssertTrue(doubleStatic == 22.0);
            GetDoubleValue2(ref doubleStatic);
            AssertTrue(doubleStatic == 22.0);
            GetValue(ref doubleStatic, 15.0);
            AssertTrue(doubleStatic == 15.0);
        }

        private void GetDoubleValue(ref double i)
        {
            i = i - 2.0;
        }

        private void GetDoubleValue2(ref double i)
        {
            i = i + 5.0;
            GetDoubleValue(ref i);
            i = i - 3.0;
        }

        #endregion

        #region object

        public void testObjectLocal()
        {
            object i = null;
            GetObjectValue(ref i);
            AssertTrue(i == ObjectValue);
            GetValue(ref i, ObjectValue);
            AssertTrue(i == ObjectValue);
        }

        public void testObjectArray()
        {
            var i = new object[2] { null, null };
            GetObjectValue(ref i[1]);
            AssertTrue(i[1] == ObjectValue);
            GetValue(ref i[0], ObjectValue);
            AssertTrue(i[0] == ObjectValue);
        }

        public void testObjectInstance()
        {
            objectInstance = null;
            GetObjectValue(ref objectInstance);
            AssertTrue(objectInstance == ObjectValue);
            GetObjectValue2(ref objectInstance);
            AssertTrue(objectInstance == ObjectValue);
            GetValue(ref objectInstance, ObjectValue);
            AssertTrue(objectInstance == ObjectValue);
        }

        public void testObjectStatic()
        {
            objectStatic = null;
            GetObjectValue(ref objectStatic);
            AssertTrue(objectStatic == ObjectValue);
            GetObjectValue2(ref objectStatic);
            AssertTrue(objectStatic == ObjectValue);
            GetValue(ref objectStatic, ObjectValue);
            AssertTrue(objectStatic == ObjectValue);
        }

        private void GetObjectValue(ref object i)
        {
            i = ObjectValue;
        }

        private void GetObjectValue2(ref object i)
        {
            i = null;
            GetObjectValue(ref i);
        }

        #endregion

        #region string

        public void testStringLocal()
        {
            string i = null;
            GetStringValue(ref i);
            AssertTrue(i == "test");
            GetValue(ref i, "gentest");
            AssertTrue(i == "gentest");
        }

        public void testStringArray()
        {
            var i = new string[2] { "aap", "boot"};
            GetStringValue(ref i[1]);
            AssertTrue(i[1] == "test");
            GetValue(ref i[0], "gentest");
            AssertTrue(i[0] == "gentest");
        }

        public void testStringInstance()
        {
            stringInstance = "app";
            GetStringValue(ref stringInstance);
            AssertTrue(stringInstance == "test");
            GetStringValue2(ref stringInstance);
            AssertTrue(stringInstance == "test2");
            GetValue(ref stringInstance, "gentest");
            AssertTrue(stringInstance == "gentest");
        }

        public void testStringStatic()
        {
            stringStatic = "aap";
            GetStringValue(ref stringStatic);
            AssertTrue(stringStatic == "test");
            GetStringValue2(ref stringStatic);
            AssertTrue(stringStatic == "test2");
            GetValue(ref stringStatic, "gentest");
            AssertTrue(stringStatic == "gentest");
        }

        private void GetStringValue(ref string i)
        {
            i = "test";
        }

        private void GetStringValue2(ref string i)
        {
            i = null;
            GetStringValue(ref i);
            i = i + "2";
        }

        #endregion

        #region enum

        public void testEnumLocal()
        {
            MyEnum i = MyEnum.b;
            GetEnumValue(ref i);
            AssertTrue(i == MyEnum.c);
            GetValue(ref i, MyEnum.d);
            AssertTrue(i == MyEnum.d);
        }

        public void testEnumArray()
        {
            var i = new MyEnum[2] { MyEnum.a, MyEnum.b };
            GetEnumValue(ref i[1]);
            AssertTrue(i[1] == MyEnum.c);
            GetValue(ref i[0], MyEnum.d);
            AssertTrue(i[0] == MyEnum.d);
        }

        public void testEnumInstance()
        {
            enumInstance = MyEnum.b;
            GetEnumValue(ref enumInstance);
            AssertTrue(enumInstance == MyEnum.c);
            GetEnumValue2(ref enumInstance);
            AssertTrue(enumInstance == MyEnum.e);
            GetValue(ref enumInstance, MyEnum.d);
            AssertTrue(enumInstance == MyEnum.d);
        }

        public void testEnumStatic()
        {
            enumStatic = MyEnum.b;
            GetEnumValue(ref enumStatic);
            AssertTrue(enumStatic == MyEnum.c);
            GetEnumValue2(ref enumStatic);
            AssertTrue(enumStatic == MyEnum.e);
            GetValue(ref enumStatic, MyEnum.d);
            AssertTrue(enumStatic == MyEnum.d);
        }

        private void GetEnumValue(ref MyEnum i)
        {
            i = MyEnum.c;
        }

        private void GetEnumValue2(ref MyEnum i)
        {
            GetEnumValue(ref i);
            i += 2;
        }

        #endregion

        #region Generic

        private void GetValue<T>(ref T i, T value)
        {
            i = value;
        }

        private void GetValue2<T>(ref T i, T value)
        {
            i = default(T);
            GetValue(ref i, value);
        }

        #endregion

        public void testDuplicate()
        {
            var duplicate = new Duplicate();

            var array = new int[] {11};
            int i = 7;

            duplicate.Foo(array);
            AssertSame(42, array[0]);

            duplicate.Foo(ref i);
            AssertSame(24, i);
        }

        public class Duplicate
        {
            public void Foo(int[] array)
            {
                array[0] = 42;
            }

            public void Foo(ref int i)
            {
                i = 24;
            }
        }
		
		public void testStruct1()
		{
			var s = new MyStruct();
			TestStruct1Helper(ref s);
			AssertSame(42, s.Field);
		}
		
		private void TestStruct1Helper(ref MyStruct s)
		{
			s.Field = 42;
		}
		
		public class MyStruct
		{
			public int Field;
		}
		
		public void testGenericConstraint1() 
		{
			var impl = new MyInterfaceImpl();
			TestGenericConstraint1Helper(ref impl);
		}
		
		private void TestGenericConstraint1Helper<T>(ref T x)
			where T : IMyInterface
		{
			x.Foo();
		}
		
		public interface IMyInterface
		{
			void Foo();
		}
		
		public class MyInterfaceImpl : IMyInterface 
		{
			public void Foo()  { }
		}
    }
}
