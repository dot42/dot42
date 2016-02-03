using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestEnum : TestCase
    {
		private enum EnumSByte : sbyte { V1, V2 };
		private enum EnumByte : byte { V1, V2, V3 };
		private enum EnumShort : short { V1, V2, V3 };
		private enum EnumUShort : ushort { V1, V2, V3 };
		private enum EnumInt : int { V1, V2, V3 };
		private enum EnumUInt : uint { V1, V2, V3 };
        private enum EnumUInt2 : uint { V1, V2, V3 = 0xFF000000 };
		private enum EnumLong : long { V1, V2, V3 };
		private enum EnumULong : ulong { V1, V2, V3 };
		
		private EnumSByte instanceEnumSByte;
		private static EnumSByte staticEnumSByte;

        private EnumSByte? instanceEnumSByteN;
        private static EnumSByte? staticEnumSByteN;

        private EnumByte instanceEnumByte;
		private static EnumByte staticEnumByte;

        private EnumByte? instanceEnumByteN;
        private static EnumByte? staticEnumByteN;

        private EnumShort instanceEnumShort;
		private static EnumShort staticEnumShort;

        private EnumShort? instanceEnumShortN;
        private static EnumShort? staticEnumShortN;

        private EnumUShort instanceEnumUShort;
		private static EnumUShort staticEnumUShort;

        private EnumUShort? instanceEnumUShortN;
        private static EnumUShort? staticEnumUShortN;

        private EnumInt instanceEnumInt;
		private static EnumInt staticEnumInt;

        private EnumInt? instanceEnumIntN;
        private static EnumInt? staticEnumIntN;

        private EnumUInt instanceEnumUInt;
		private static EnumUInt staticEnumUInt;

        private EnumUInt? instanceEnumUIntN;
        private static EnumUInt? staticEnumUIntN;

        private EnumLong instanceEnumLong;
		private static EnumLong staticEnumLong;

        private EnumLong? instanceEnumLongN;
        private static EnumLong? staticEnumLongN;

        private EnumULong instanceEnumULong;
		private static EnumULong staticEnumULong;

        private EnumULong? instanceEnumULongN;
        private static EnumULong? staticEnumULongN;

        private enum E { Val1, Val2 };
        private enum TwoFields { Aap, Noot }

        public void testEnumSByte1()
        {
			instanceEnumSByte = EnumSByte.V1;
			staticEnumSByte = EnumSByte.V2;
			var local = EnumSByte.V2;
			
            AssertTrue(instanceEnumSByte == EnumSByte.V1);
            AssertTrue(staticEnumSByte == EnumSByte.V2);
            AssertTrue(local == EnumSByte.V2);

			var arr = new EnumSByte[1];
			arr[0] = EnumSByte.V2;
            AssertTrue(arr[0] == EnumSByte.V2);
        }

        public void testEnumSByte2()
        {
            EnumSByte? nullable;
            nullable = null;

            AssertFalse(nullable.HasValue);

            nullable = EnumSByte.V2;
            AssertTrue(nullable.HasValue);
            AssertTrue(nullable.Value == EnumSByte.V2);
            AssertTrue(nullable == EnumSByte.V2);

            instanceEnumSByteN = null;
            AssertFalse(instanceEnumSByteN.HasValue);

            instanceEnumSByteN = EnumSByte.V2;
            AssertTrue(instanceEnumSByteN.HasValue);
            AssertTrue(instanceEnumSByteN.Value == EnumSByte.V2);
            AssertTrue(instanceEnumSByteN == EnumSByte.V2);

            staticEnumSByteN = null;
            AssertFalse(staticEnumSByteN.HasValue);

            staticEnumSByteN = EnumSByte.V2;
            AssertTrue(staticEnumSByteN.HasValue);
            AssertTrue(staticEnumSByteN.Value == EnumSByte.V2);
            AssertTrue(staticEnumSByteN == EnumSByte.V2);
        }

        public void testEnumByte1()
        {
			instanceEnumByte = EnumByte.V3;
			staticEnumByte = EnumByte.V2;
			var local = EnumByte.V2;
			
            AssertTrue(instanceEnumByte == EnumByte.V3);
            AssertTrue(staticEnumByte == EnumByte.V2);
            AssertTrue(local == EnumByte.V2);

			var arr = new EnumByte[1];
			arr[0] = EnumByte.V2;
            AssertTrue(arr[0] == EnumByte.V2);
        }

        public void testEnumByte2()
        {
            EnumByte? nullable;
            nullable = null;

            AssertFalse(nullable.HasValue);

            nullable = EnumByte.V2;
            AssertTrue(nullable.HasValue);
            AssertTrue(nullable.Value == EnumByte.V2);
            AssertTrue(nullable == EnumByte.V2);

            instanceEnumByteN = null;
            AssertFalse(instanceEnumByteN.HasValue);

            instanceEnumByteN = EnumByte.V2;
            AssertTrue(instanceEnumByteN.HasValue);
            AssertTrue(instanceEnumByteN.Value == EnumByte.V2);
            AssertTrue(instanceEnumByteN == EnumByte.V2);

            staticEnumByteN = null;
            AssertFalse(staticEnumByteN.HasValue);

            staticEnumByteN = EnumByte.V2;
            AssertTrue(staticEnumByteN.HasValue);
            AssertTrue(staticEnumByteN.Value == EnumByte.V2);
            AssertTrue(staticEnumByteN == EnumByte.V2);
        }
	
        public void testEnumShort1()
        {
			instanceEnumShort = EnumShort.V3;
			staticEnumShort = EnumShort.V2;
			var local = EnumShort.V2;
			
            AssertTrue(instanceEnumShort == EnumShort.V3);
            AssertTrue(staticEnumShort == EnumShort.V2);
            AssertTrue(local == EnumShort.V2);

			var arr = new EnumShort[1];
			arr[0] = EnumShort.V2;
            AssertTrue(arr[0] == EnumShort.V2);
        }

        public void testEnumShort2()
        {
            EnumShort? nullable;
            nullable = null;

            AssertFalse(nullable.HasValue);

            nullable = EnumShort.V2;
            AssertTrue(nullable.HasValue);
            AssertTrue(nullable.Value == EnumShort.V2);
            AssertTrue(nullable == EnumShort.V2);

            instanceEnumShortN = null;
            AssertFalse(instanceEnumShortN.HasValue);

            instanceEnumShortN = EnumShort.V2;
            AssertTrue(instanceEnumShortN.HasValue);
            AssertTrue(instanceEnumShortN.Value == EnumShort.V2);
            AssertTrue(instanceEnumShortN == EnumShort.V2);

            staticEnumShortN = null;
            AssertFalse(staticEnumShortN.HasValue);

            staticEnumShortN = EnumShort.V2;
            AssertTrue(staticEnumShortN.HasValue);
            AssertTrue(staticEnumShortN.Value == EnumShort.V2);
            AssertTrue(staticEnumShortN == EnumShort.V2);
        }
	
        public void testEnumUShort1()
        {
			instanceEnumUShort = EnumUShort.V3;
			staticEnumUShort = EnumUShort.V2;
			var local = EnumUShort.V2;
			
            AssertTrue(instanceEnumUShort == EnumUShort.V3);
            AssertTrue(staticEnumUShort == EnumUShort.V2);
            AssertTrue(local == EnumUShort.V2);

			var arr = new EnumUShort[1];
			arr[0] = EnumUShort.V2;
            AssertTrue(arr[0] == EnumUShort.V2);
        }

        public void testEnumUShort2()
        {
            EnumUShort? nullable;
            nullable = null;

            AssertFalse(nullable.HasValue);

            nullable = EnumUShort.V2;
            AssertTrue(nullable.HasValue);
            AssertTrue(nullable.Value == EnumUShort.V2);
            AssertTrue(nullable == EnumUShort.V2);

            instanceEnumUShortN = null;
            AssertFalse(instanceEnumUShortN.HasValue);

            instanceEnumUShortN = EnumUShort.V2;
            AssertTrue(instanceEnumUShortN.HasValue);
            AssertTrue(instanceEnumUShortN.Value == EnumUShort.V2);
            AssertTrue(instanceEnumUShortN == EnumUShort.V2);

            staticEnumUShortN = null;
            AssertFalse(staticEnumUShortN.HasValue);

            staticEnumUShortN = EnumUShort.V2;
            AssertTrue(staticEnumUShortN.HasValue);
            AssertTrue(staticEnumUShortN.Value == EnumUShort.V2);
            AssertTrue(staticEnumUShortN == EnumUShort.V2);
        }
	
        public void testEnumInt1()
        {
			instanceEnumInt = EnumInt.V3;
			staticEnumInt = EnumInt.V2;
			var local = EnumInt.V2;
			
            AssertTrue(instanceEnumInt == EnumInt.V3);
            AssertTrue(staticEnumInt == EnumInt.V2);
            AssertTrue(local == EnumInt.V2);

			var arr = new EnumInt[1];
			arr[0] = EnumInt.V2;
            AssertTrue(arr[0] == EnumInt.V2);
        }

        public void testEnumInt2()
        {
            EnumInt? nullable;
            nullable = null;

            AssertFalse(nullable.HasValue);
           
            nullable = EnumInt.V2;
            AssertTrue(nullable.HasValue);
            AssertTrue(nullable.Value == EnumInt.V2);
            AssertTrue(nullable == EnumInt.V2);

            instanceEnumIntN = null;
            AssertFalse(instanceEnumIntN.HasValue);

            instanceEnumIntN = EnumInt.V2;
            AssertTrue(instanceEnumIntN.HasValue);
            AssertTrue(instanceEnumIntN.Value == EnumInt.V2);
            AssertTrue(instanceEnumIntN == EnumInt.V2);

            staticEnumIntN = null;
            AssertFalse(staticEnumIntN.HasValue);

            staticEnumIntN = EnumInt.V2;
            AssertTrue(staticEnumIntN.HasValue);
            AssertTrue(staticEnumIntN.Value == EnumInt.V2);
            AssertTrue(staticEnumIntN == EnumInt.V2);
        }
	
        public void testEnumUInt1()
        {
			instanceEnumUInt = EnumUInt.V3;
			staticEnumUInt = EnumUInt.V2;
			var local = EnumUInt.V2;
			
            AssertTrue(instanceEnumUInt == EnumUInt.V3);
            AssertTrue(staticEnumUInt == EnumUInt.V2);
            AssertTrue(local == EnumUInt.V2);

			var arr = new EnumUInt[1];
			arr[0] = EnumUInt.V2;
            AssertTrue(arr[0] == EnumUInt.V2);
        }

        public void testEnumUInt2()
        {
            EnumUInt? nullable;
            nullable = null;

            AssertFalse(nullable.HasValue);

            nullable = EnumUInt.V2;
            AssertTrue(nullable.HasValue);
            AssertTrue(nullable.Value == EnumUInt.V2);
            AssertTrue(nullable == EnumUInt.V2);

            instanceEnumUIntN = null;
            AssertFalse(instanceEnumUIntN.HasValue);

            instanceEnumUIntN = EnumUInt.V2;
            AssertTrue(instanceEnumUIntN.HasValue);
            AssertTrue(instanceEnumUIntN.Value == EnumUInt.V2);
            AssertTrue(instanceEnumUIntN == EnumUInt.V2);

            staticEnumUIntN = null;
            AssertFalse(staticEnumUIntN.HasValue);

            staticEnumUIntN = EnumUInt.V2;
            AssertTrue(staticEnumUIntN.HasValue);
            AssertTrue(staticEnumUIntN.Value == EnumUInt.V2);
            AssertTrue(staticEnumUIntN == EnumUInt.V2);
        }

        public void testEnumUIntLargeNumber()
        {
            var local = EnumUInt2.V2;
            AssertTrue(local == EnumUInt2.V2);
        }

        public void testEnumUIntLargeNumber1()
        {
            var local = EnumUInt2.V3;
            AssertTrue(local == EnumUInt2.V3);
        }

        public void testEnumUIntLargeNumber2()
        {
            var local = EnumUInt2.V3;
            
            if ((local & EnumUInt2.V3) != EnumUInt2.V3)
            {
                Fail();
            }
        }
	
        public void testEnumLong1()
        {
			instanceEnumLong = EnumLong.V3;
			staticEnumLong = EnumLong.V2;
			var local = EnumLong.V2;
			
            AssertTrue(instanceEnumLong == EnumLong.V3);
            AssertTrue(staticEnumLong == EnumLong.V2);
            AssertTrue(local == EnumLong.V2);

			var arr = new EnumLong[1];
			arr[0] = EnumLong.V2;
            AssertTrue(arr[0] == EnumLong.V2);
        }

        public void testEnumLong2()
        {
            EnumLong? nullable;
            nullable = null;

            AssertFalse(nullable.HasValue);

            nullable = EnumLong.V2;
            AssertTrue(nullable.HasValue);
            AssertTrue(nullable.Value == EnumLong.V2);
            AssertTrue(nullable == EnumLong.V2);

            instanceEnumLongN = null;
            AssertFalse(instanceEnumLongN.HasValue);

            instanceEnumLongN = EnumLong.V2;
            AssertTrue(instanceEnumLongN.HasValue);
            AssertTrue(instanceEnumLongN.Value == EnumLong.V2);
            AssertTrue(instanceEnumLongN == EnumLong.V2);

            staticEnumLongN = null;
            AssertFalse(staticEnumLongN.HasValue);

            staticEnumLongN = EnumLong.V2;
            AssertTrue(staticEnumLongN.HasValue);
            AssertTrue(staticEnumLongN.Value == EnumLong.V2);
            AssertTrue(staticEnumLongN == EnumLong.V2);
        }
	
        public void testEnumULong1()
        {
			instanceEnumULong = EnumULong.V3;
			staticEnumULong = EnumULong.V2;
			var local = EnumULong.V2;
			
            AssertTrue(instanceEnumULong == EnumULong.V3);
            AssertTrue(staticEnumULong == EnumULong.V2);
            AssertTrue(local == EnumULong.V2);

			var arr = new EnumULong[1];
			arr[0] = EnumULong.V2;
            AssertTrue(arr[0] == EnumULong.V2);
        }

        public void testEnumULong2()
        {
            EnumULong? nullable;
            nullable = null;

            AssertFalse(nullable.HasValue);

            nullable = EnumULong.V2;
            AssertTrue(nullable.HasValue);
            AssertTrue(nullable.Value == EnumULong.V2);
            AssertTrue(nullable == EnumULong.V2);

            instanceEnumULongN = null;
            AssertFalse(instanceEnumULongN.HasValue);

            instanceEnumULongN = EnumULong.V2;
            AssertTrue(instanceEnumULongN.HasValue);
            AssertTrue(instanceEnumULongN.Value == EnumULong.V2);
            AssertTrue(instanceEnumULongN == EnumULong.V2);

            staticEnumULongN = null;
            AssertFalse(staticEnumULongN.HasValue);

            staticEnumULongN = EnumULong.V2;
            AssertTrue(staticEnumULongN.HasValue);
            AssertTrue(staticEnumULongN.Value == EnumULong.V2);
            AssertTrue(staticEnumULongN == EnumULong.V2);
        }

        public void testEnumCast1()
        {
            long value = 1;
            EnumInt enumInt = (EnumInt) value;

            AssertTrue(enumInt == EnumInt.V2);
        }

        public void testEnumCast2()
        {
            int value = 1;
            EnumLong enumLong = (EnumLong)value;

            AssertTrue(enumLong == EnumLong.V2);
        }

        public void testEnumCast3()
        {
            long value = 1;
            int castedValue = (int)value;
            EnumInt enumInt = (EnumInt)castedValue;

            AssertTrue(enumInt == EnumInt.V2);
        }

        public void testEnumCast4()
        {
            long value = 1;
            EnumLong enumLong = (EnumLong)value;

            AssertTrue(enumLong == EnumLong.V2);
        }

        public void testEnumAsObject1()
        {
            var mc = new MyClass1();
            var result = mc.Foo(EnumInt.V2);

            AssertTrue(result == (int)EnumInt.V2);
        }

        public class MyClass1
        {
            public int Foo(object obj)
            {
                return (int) obj;
            }
        }

        public void testEnumCast5()
        {
            double value = 1.0;
            EnumLong enumLong = (EnumLong)value;

            AssertTrue(enumLong == EnumLong.V2);
        }

        public void testEnumCast6()
        {
            EnumInt enumInt = (EnumInt)GetValue();

            AssertTrue(enumInt == EnumInt.V2);
        }

        public void testEnumCast7()
        {
            EnumLong enumLong = (EnumLong)GetValue();

            AssertTrue(enumLong == EnumLong.V2);
        }

        public void testEnumCast8a()
        {
            object o = (int) EnumInt.V2;
            var result = (EnumInt) o;

            AssertTrue(result == EnumInt.V2);
        }

        public void testEnumCast8b()
        {
            var result = ReturnEnum((int)EnumInt.V2);
            AssertTrue(result == EnumInt.V2);
        }

        public void testEnumCast9a()
        {
            object o = EnumInt.V2;
            var result = (int)o;

            AssertTrue(result == (int)EnumInt.V2);
        }

        public void testEnumCast9b()
        {
            var result = ReturnInt(EnumInt.V2);
            AssertTrue(result == (int)EnumInt.V2);
        }

        public void testCallStaticMethodWithEnum()
        {
            AssertEquals("Noot", ClassEnumStaticTest.CalledMethod(TwoFields.Noot));
            AssertEquals("1", ClassEnumStaticTest.CalledMethodD(TwoFields.Noot));
        }

        public void testCallMethodWithEnum()
        {
            AssertEquals("Noot", MethodWithSystemEnumParameter(TwoFields.Noot));
        }

        private string MethodWithSystemEnumParameter(Enum e)
        {
            return e.ToString();
        }

        public void testEnumGetType()
        {
            Assert.AssertEquals(typeof(E), E.Val1.GetType());
        }

        public void testIsEnum()
        {
            Assert.AssertTrue(typeof(E).IsEnum);
            Assert.AssertTrue(E.Val1.GetType().IsEnum);
        }

        public void testRetrieveEnumValuesThroughReflection()
        {
            Assert.AssertEquals(2, GetValues(typeof(E)).Count);
        }

        public void testEnumToObjectConversion()
        {
            new EnumConversionFromVariableTests().ConvertToObject();
            new EnumConversionFromVariableTests().SetInObjectArray();
            new EnumConversionFromVariableTests().CallVarArg();
        }

        public void testEnumToSbyteConversion()
        {
            new EnumConversionFromVariableTests().TestCallSByteMethod();
            new EnumConversionFromVariableTests().TestCallSByteMethod1();
        }

        public void testEnumByReference()
        {
            new EnumConversionFromVariableTests().TestCallByReference();
        }

        public static IList<object> GetValues(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Type '" + enumType.Name + "' is not an enum.");

            List<object> values = new List<object>();

            var fields = enumType.GetFields();

            foreach (FieldInfo field in fields)
            {
                if (!field.IsLiteral)
                    continue;
                Debug.WriteLine("retrieving value of field {0}", field.Name);
                object value = field.GetValue(enumType);
                values.Add(value);
            }

            return values;
        }

        static class ClassEnumStaticTest
        {
            public static string CalledMethod(Enum e)
            {
                return e.ToString();
            }

            public static string CalledMethodD(Enum e)
            {
                return e.ToString("D");
            }

        }

        class EnumConversionFromVariableTests
        {
            public void ConvertToObject()
            {
                var obj = TwoFields.Noot;
                AssertEquals(TwoFields.Noot, obj);
            }

            public void SetInObjectArray()
            {
                object[] objs = new object[1];
                TwoFields twoFields = TwoFields.Noot;
                objs[0] = twoFields;
                AssertEquals(TwoFields.Noot, objs[0]);
            }

            public void CallVarArg()
            {
                TwoFields twoFields = TwoFields.Noot;
                AssertEquals(TwoFields.Noot, WithVarArg(twoFields));
                AssertTrue(VarArgsIsNoot(twoFields));
            }


            public void TestCallByReference()
            {
                TwoFields twoFields = TwoFields.Noot;
                AssertTrue(EnumReferenceIsNoot(ref twoFields));
            }
            public void TestCallSByteMethod()
            {
                EnumSByte e = EnumSByte.V1;
                SByteMethod((sbyte)e);
            }

            public void TestCallSByteMethod1()
            {
                int i = 0x44;
                SByteMethod((sbyte)i);
            }

            private void SByteMethod(sbyte b)
            {
            }

            public static object WithVarArg(params object[] args)
            {
                return args[0];
            }

            public static bool VarArgsIsNoot(params object[] args)
            {
                return TwoFields.Noot == (TwoFields)args[0];
            }

            public static bool EnumReferenceIsNoot(ref TwoFields val)
            {
                return TwoFields.Noot == val;
            }

        }

        private EnumInt ReturnEnum(object o)
        {
            return (EnumInt)o;
        }

        private int ReturnInt(object o)
        {
            return (int)o;
        }

        public double GetValue()
        {
            return 1.0;
        }
    }
}
