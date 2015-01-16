using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestEnumSpecial : TestCase
    {
        [Flags]
        public enum MyFlagsEnum
        {
            V1 = 0x01,
            V2 = 0x02,

            V1and2 = V1 | V2,
            V1_x = V1 | 0x55,
            V2copy = V2
        };

        [Flags]
        public enum MyUIntEnum: uint
        {
            V1 = 0x01,
            V2 = 0x02,
            V3 = 0X10
        };

        [Flags]
        public enum MyFlagsEnum2 // Make sure this is the same as MyFlagsEnum
        {
            V1 = 0x01,
            V2 = 0x02,

            V1and2 = V1 | V2,
            V1_x = V1 | 0x55,
            V2copy = V2
        };
		
		public enum ZeroStartingEnum
		{
			Zero,
			One
		}

        private MyFlagsEnum instanceNotInitialized;
        private static MyFlagsEnum staticNotInitialized;
		private ZeroStartingEnum instanceZeroStartingEnum;

        public void testFlags()
        {
            var x = MyFlagsEnum.V1;
            AssertTrue((x | MyFlagsEnum.V2) == MyFlagsEnum.V1and2);
            AssertEquals(3, (int)MyFlagsEnum.V1and2);
            AssertEquals(0x55, (int)MyFlagsEnum.V1_x);
            x = MyFlagsEnum.V2;
            AssertTrue(x == MyFlagsEnum.V2copy);
        }

        public void testCopy()
        {
            AssertEquals(MyFlagsEnum.V2, MyFlagsEnum.V2copy);
            AssertTrue(ReferenceEquals(MyFlagsEnum.V2, MyFlagsEnum.V2copy));
        }

        public void testSwitch1()
        {
            var x = MyFlagsEnum.V1;
            string result;
            switch (x)
            {
                case MyFlagsEnum.V1:
                    result = "v1";
                    break;
                case MyFlagsEnum.V2:
                    result = "v2";
                    break;
                default:
                    result = "def";
                    break;
            }
            AssertEquals("v1", result);
        }

        public void testSwitch2()
        {
            string result = GetResult(MyFlagsEnum.V2);
            AssertEquals("v2", result);
        }

        private string GetResult(MyFlagsEnum x)
        {
            string result;
            switch (x)
            {
                case MyFlagsEnum.V1:
                    result = "v1";
                    break;
                case MyFlagsEnum.V2:
                    result = "v2";
                    break;
                default:
                    result = "def";
                    break;
            }
            return result;
        }

        public void testNotInitialized()
        {
            var tmp = new TestEnumSpecial();
            AssertEquals(tmp.instanceNotInitialized, (MyFlagsEnum)0);
            AssertNotNull(tmp.instanceNotInitialized);
            AssertTrue(tmp.instanceNotInitialized == default(MyFlagsEnum));
            AssertTrue(staticNotInitialized == default(MyFlagsEnum));
        }

        public void testReturn()
        {
            var tmp = ReturnFromStatic();
            AssertEquals(tmp, MyFlagsEnum.V2);

            tmp = ReturnFromInstance();
            AssertEquals(tmp, MyFlagsEnum.V1);
        }

        public void testCastIntToEnum()
        {
            var tmp = (MyFlagsEnum)ReturnInt();
            AssertEquals(tmp, MyFlagsEnum.V2);
        }

        public static MyFlagsEnum ReturnFromStatic()
        {
            return MyFlagsEnum.V2;
        }

        public MyFlagsEnum ReturnFromInstance()
        {
            return MyFlagsEnum.V1;
        }

        public int ReturnInt()
        {
            return 2;
        }

        public void testCastToOtherEnum()
        {
            var tmp = (MyFlagsEnum2)ReturnFromStatic();
            AssertEquals(tmp, MyFlagsEnum2.V2);
        }

        public void testCastEnumToInt()
        {
            var tmp = (int)ReturnFromInstance();
            AssertEquals(tmp, 0x01);
        }

        public void testEnumInClass1()
        {
            var tmp = new TestClass1(MyFlagsEnum.V2);
        }

        private class TestClass1
        {
            private MyFlagsEnum myEnum;

            public TestClass1(MyFlagsEnum myEnum)
            {
                this.myEnum = myEnum;
            }
        }

        public void testEnumInClass2()
        {
            var tmp = new TestClass2(MyFlagsEnum.V2);
        }

        private class TestClass2
        {
            public MyFlagsEnum MyEnum { get; private set; }

            public TestClass2(MyFlagsEnum myEnum)
            {
                MyEnum = myEnum;
            }
        }

        public void testEnumInStruct1()
        {
            var tmp = new TestStruct1(MyFlagsEnum.V2);
        }

        private struct TestStruct1
        {
            private MyFlagsEnum myEnum;

            public TestStruct1(MyFlagsEnum myEnum)
            {
                this.myEnum = myEnum;
            }
        }

        //ignore for now
        public void testEnumInStruct2()
        {
            var tmp = new TestStruct2(MyFlagsEnum.V2);
        }

        private struct TestStruct2
        {
            public MyFlagsEnum MyEnum { get; private set; }

            public TestStruct2(MyFlagsEnum myEnum)
                : this()
            {
                MyEnum = myEnum;
            }
        }

        public void testCase701_A()
        {
            var x = new Case701_A();
            x._SendLmBusMsg(new Case701_A.LmBusMsg());
        }

        private class Case701_A
        {
            private const int LMWIN_HEAD_LEN = 20;
            private const int LMWIN_DATA_LM_LEN = 21;
            private const int TEL_POS = 1;
            private const int LM_TEL_POS_BUS_CMD = 1;

            public enum LmBusCmd //: byte 
            {
                RESET = 0x00,
                POLL_P_NR = 0x01,
            }

            public class LmBusMsg
            {
                protected LmBusCmd _Cmd;
                public LmBusCmd Cmd { get { return _Cmd; } }
            }

            internal void _SendLmBusMsg(LmBusMsg msg)
            {
                byte[] buf = new byte[LMWIN_HEAD_LEN + LMWIN_DATA_LM_LEN]; // 41 

                //int nCmd = (int)msg.Cmd; 
                byte nCmd = (byte)msg.Cmd;

                buf[TEL_POS + LM_TEL_POS_BUS_CMD] = (byte)nCmd; // (byte)msg.Cmd; 

            }
        }

        public void testCase701_B()
        {
            var x = Case701_B.GetAddressType(0x80);
            AssertEquals(Case701_B.AddressType.RG, x);
        }

        private class Case701_B
        {
            public enum AddressType //: byte 
            {
                Unknown = 0x01,

                R = 0x00,
                RB = 0x40,
                RG = 0x80,
                PNR = 0xc0
            }


            public static AddressType GetAddressType(byte cmd) // byte is the key.
            {
                return (AddressType)(cmd & 0xc0);
            }
        }
		
		 public void testCase_CaseUnnamed_XXX_A()
		 {
				var x = new CaseUnnamed.CaseXXX_A(CaseUnnamed.AddressType.R);
				byte[] buf = new byte[5];
				int xx = x.SetAddressFromBytes(buf, 0, false);
		 }
 
		private class CaseUnnamed 
		{
		  public enum AddressType //: byte
			   {
					 Unknown = 0x01,
		 
					 R = 0x00,
					 RB = 0x40,
					 RG = 0x80,
					 PNR = 0xc0
			   }
		 
 
             public class CaseXXX_A
             {
                    public AddressType AddrType;
 
                    public CaseXXX_A(AddressType addrtype)
                    {
                           AddrType = addrtype;
                    }
 
                    public int SetAddressFromBytes(byte[] srcbytes, int srcindex, bool maskR)
                    {
                           switch (AddrType)
                           {
                                  case AddressType.R:
                                        srcindex += 1;
                                        break;
 
                                  case AddressType.RB:
                                        srcindex += 2;
                                        break;
 
                                  case AddressType.RG:
                                        srcindex += 2;
                                        break;
 
                                  case AddressType.PNR:
                                        srcindex += 5;
                                        break;
                           }
 
                           return srcindex;
                    }
             }
 		}
		
		public void testZero() 
		{
			SetZero();
			if (instanceZeroStartingEnum != ZeroStartingEnum.Zero) 
				throw new Exception("Must be zero");
			SetOne();
			if (instanceZeroStartingEnum == ZeroStartingEnum.Zero) 
				throw new Exception("Must not be Zero");
		}
		
		public void SetZero() 
		{
			instanceZeroStartingEnum = ZeroStartingEnum.Zero;
		}
		
		public void SetOne() 
		{
			instanceZeroStartingEnum = ZeroStartingEnum.One;
		}

        public void testEnumBitAnd1()
        {
            MyUIntEnum enumUInt = MyUIntEnum.V1;
            var result = ((enumUInt & MyUIntEnum.V2) == 0);
            AssertTrue(result);
        }

        public void testEnumBitAnd2()
        {
            MyUIntEnum enumUInt = MyUIntEnum.V1;
            var result = doUInt(ref enumUInt);
            AssertTrue(result);
        }

        public void testEnumBitAnd3()
        {
            MyUIntEnum enumUInt = MyUIntEnum.V1;
            var result = doUInt2(ref enumUInt);
            AssertTrue(result);
        }

        public void testEnumBitOr1()
        {
            MyUIntEnum enumUInt = MyUIntEnum.V1;
            enumUInt |= MyUIntEnum.V2;
            AssertTrue((int)enumUInt == (int)MyUIntEnum.V1 + (int)MyUIntEnum.V2);
            AssertTrue((int)enumUInt == 3);
        }

        public void testEnumBitOr2()
        {
            MyUIntEnum enumUInt = MyUIntEnum.V1;
            var result = doOr(enumUInt);
            AssertTrue(result == 3);
        }

        public bool doUInt(ref MyUIntEnum myEnum)
        {
            return ((myEnum & MyUIntEnum.V2) == 0);
        }

        public bool doUInt2(ref MyUIntEnum myEnum)
        {
            return (((uint)myEnum & (uint)MyUIntEnum.V2) == 0);
        }

        public int doOr(MyUIntEnum enumUInt)
        {
            enumUInt |= MyUIntEnum.V2;

            return (int) enumUInt;
        }

    }
}
