package dot42.java.test;

import junit.framework.*;

public class TestCast extends TestCase
{
	public void test1()
	{
		Base obj = new Base();
		assertEquals("Base", 0, Foo(obj));
	}

	public void test2()
	{
		Derived1 obj = new Derived1();
		assertEquals("Derived1", 1, Foo(obj));
	}

	public void test3a()
	{
		Base obj = new Derived2();
		assertEquals("Derived2", 22, Foo(obj));
	}

	public void test3b()
	{
		Base obj = new Derived2();
		assertEquals("Derived2", 12, Foo((Derived2)obj));
	}
	
	public void test4a() 
	{
		assertEquals(5, Length("Dot42"));
	}
	
	public void test4b() 
	{
		assertEquals(-1, Length(this));
	}
	
	public int Length(Object x) 
	{
		String s = (x instanceof String) ? ((String)x) : null;
		if (s != null)
			return s.length();
		return -1;
	}

	public int Foo(Base x)
	{
		if (x instanceof Derived2)
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
		public int Foo()
		{
			return 0;
		}
	}

	public class Derived1 extends Base
	{
		@Override
		public int Foo()
		{
			return 1;
		}
	}

	public class Derived2 extends Base
	{
		@Override
		public int Foo()
		{
			return 2;
		}

		public int FooX()
		{
			return 22;
		}
	}
}
