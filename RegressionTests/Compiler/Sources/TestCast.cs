using System;
using Junit.Framework;
using Android.Graphics;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestCast : TestCase
    {
        public void test1()
        {
            var obj = new Base();
            AssertEquals("Base", 0, Foo(obj));
        }

        public void test2()
        {
            var obj = new Derived1();
            AssertEquals("Derived1", 1, Foo(obj));
        }

        public void test3a()
        {
            Base obj = new Derived2();
            AssertEquals("Derived2", 22, Foo(obj));
        }

        public void test3b()
        {
            Base obj = new Derived2();
            AssertEquals("Derived2", 12, Foo((Derived2)obj));
        }
		
		public void test4a() 
		{
			AssertEquals(5, Length("Dot42"));
		}
		
		public void test4b() 
		{
			AssertEquals(-1, Length(this));
		}
		
		public void test5() 
		{
			object value = (int)123; 
			AssertTrue(value is int);
			value = (uint)123; 
			AssertTrue(value is uint);
			value = (bool)true; 
			AssertTrue(value is bool);
			value = (byte)123; 
			AssertTrue(value is byte);
			value = (sbyte)123; 
			AssertTrue(value is sbyte);
			value = (char)'a'; 
			AssertTrue(value is char);
			value = (short)12300; 
			AssertTrue(value is short);
			value = (ushort)12300; 
			AssertTrue(value is ushort);
			value = (long)123L; 
			AssertTrue(value is long);
			value = (ulong)1230000000L; 
			AssertTrue(value is ulong);
			value = (float)5.5f; 
			AssertTrue(value is float);
			value = (double)55555.5555; 
			AssertTrue(value is double);
		}


        public void test6()
        {
            object value = (int) 123;
            AssertEquals((int) value, 123);
            value = (uint) 123;
            AssertTrue(((uint) value) == 123);
            value = (bool) true;
            AssertEquals((bool) value, true);
            value = (byte) 5;
            AssertTrue(((byte) value) == 5);
            value = (sbyte) 5;
            AssertTrue(((sbyte) value) == 5);
            value = (char) 'c';
            AssertTrue(((char) value) == 'c');
            value = (short) 1230;
            AssertTrue(((short) value) == 1230);
            value = (ushort) 1230;
            AssertTrue(((ushort) value) == 1230);
            value = (long) 12300000000L;
            AssertTrue(((long) value) == 12300000000L);
            value = (ulong) 12300000000L;
            AssertTrue(((ulong) value) == 12300000000L);
            value = (float) 5.5F;
            AssertTrue(((float) value) == 5.5F);
            value = (double) 6.6;
            AssertTrue(((double) value) == 6.6);
        }

        public void test7()
        {
            var value = 20L;
            AssertTrue((byte) value == 20);
            value = 200L;
            AssertTrue((byte) value == 200);
            AssertTrue((sbyte) value != 200);
            value = -120L;
            AssertTrue((sbyte) value == -120);
            value = 32L;
            AssertTrue((char) value == ' ');
            value = 45L;
            AssertTrue((short) value == 45);
        }

        public void test8()
        {
            var value = 't';
            AssertTrue(checked((byte)value) == 0x74);
            value = '\xCD';
            AssertTrue(checked((byte)value) == 205);

            value = 't';
            AssertTrue(checked((sbyte)value) == 0x74);
            value = '\xCD';
            AssertTrue(checked((sbyte)value) != 205);

            value = 't';
            AssertTrue(checked((ushort)value) == 0x74);
            value = '\xCDCD';
            AssertTrue(checked((ushort)value) == 52685);

            value = 't';
            AssertTrue(checked((short)value) == 0x74);
            value = '\xCDCD';
            AssertTrue(checked((short)value) != 52685);

            value = 't';
            AssertTrue(checked((uint)value) == 0x74);
            value = '\xCD';
            AssertTrue(checked((uint)value) == 205);

            value = 't';
            AssertTrue(checked((int)value) == 0x74);
            value = '\xCD';
            AssertTrue(checked((int)value) == 205);

            value = 't';
            AssertTrue(checked((ulong)value) == 0x74L);
            value = '\xCD';
            AssertTrue(checked((ulong)value) == 205L);

            value = 't';
            AssertTrue(checked((long)value) == 0x74L);
            value = '\xCD';
            AssertTrue(checked((long)value) == 205L);
		}

        public void test9()
        {
            ulong number = 123;
            var castedNumber = (double)number;

            AssertEquals(123.0, castedNumber);
        } 

        public void test10()
        {
            byte b = 0x74;
            char c = (char) b;

            AssertEquals('t', c);
        }

        public void test11()
        {
            object obj = (ushort)'t';
            var chars = new char[] { (char)((ushort)obj) };

            AssertEquals(1, chars.Length);
            AssertEquals('t', chars[0]);
        }

        public void test12()
        {
            ushort ui = 6;

            pushBack((byte)ui);
        }

        public void test13()
        {
            var o = new Derived1();
            Test13Helper(o);
        }

        public void test14()
        {
            char c = 't';
            ushort us = (ushort) c;

            AssertEquals(0x74, us);
        }

        public void _test15()
        {
            char c = 't';
            ushort us = ToUInt16(c);

            AssertEquals(0x74, us);
        }

        public void _test16()
        {
            byte b = 7;
            int i = Convert.ToInt16(b);

            string s = i.ToString();
            AssertEquals("7", s);
        }

        public ushort ToUInt16(char value)
        {
            return (ushort)value;
        }

        private void Test13Helper(Base o)
        {
            var x = o as IDerived;
            if (x != null)
            {
                x.Foo();
            }
            o.Foo();
        }

        private void pushBack(ushort i)
        {
            AssertEquals(6, i);
        }

		public int Length(object x) 
		{
			var s = x as string;
			if (s != null)
				return s.Length;
			return -1;
		}

        public int Foo(Base x)
        {
            if (x is Derived2)
            {
                return ((Derived2) x).FooX();
            }
            return x.Foo();
        }

        public int Foo(Derived2 x)
        {
            return x.Foo() + 10;
        }

        public class Base
        {
            public virtual int Foo()
            {
                return 0;
            }
        }

        public interface IDerived
        {
            int Foo();
        }

        public class Derived1 : Base, IDerived
        {
            public override int Foo()
            {
                return 1;
            }
        }

        public class Derived2 : Base
        {
            public override int Foo()
            {
                return 2;
            }

            public int FooX()
            {
                return 22;
            }
        }
		
		public void testCase719()
		{		
			var x = (int)(m_Location.X + (m_Direction.X * m_MoveSpeed)); 
			var y = (int)(m_Location.Y + (m_Direction.Y * m_MoveSpeed)); 
		}
		
		private Point m_Location = new Point(0,0); 
		private Vector2D m_Direction = new Vector2D(1,1); 
		private double m_MoveSpeed = 1.0; 
		
		internal class Vector2D 
		{ 
			public double X; 
			public double Y; 

			public Vector2D(double x, double y) 
			{ 
				X = x; 
				Y = y; 
			} 
		} 
    }
}
