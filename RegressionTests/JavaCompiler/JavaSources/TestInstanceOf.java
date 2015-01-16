package dot42.java.test;

import junit.framework.*;

public class TestInstanceOf extends TestCase
{
	public void test1()
	{
		Object obj = "hello";
		assertTrue("obj is string", IsString(obj));
	}

	public void test2()
	{
		Object obj = new TestInstanceOf();
		assertFalse("obj is not string", IsString(obj));
	}

	public boolean IsString(Object x)
	{
		return x instanceof String;
	}

	public void test4()
	{
		Derived1 obj = new Derived1();
		assertEquals("Derived1", 1, Foo(obj));
	}

	public void test5a()
	{
		IBase obj = new Derived2();
		assertEquals("Derived2", 22, Foo(obj));
	}

	public void test5b()
	{
		IBase obj = new Derived2();
		assertEquals("Derived2", 12, Foo((Derived2)obj));
	}

	public int Foo(IBase x)
	{
		if (x instanceof Derived2)
		{
			return ((Derived2)x).FooX();
		}
		return x.Foo();
	}

	public int Foo(Derived2 x)
	{
		return x.Foo() + 10;
	}

	public interface IBase
	{
		int Foo();
	}

	public class Derived1 implements IBase
	{
		public int Foo()
		{
			return 1;
		}
	}

	public class Derived2 implements IBase
	{
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
