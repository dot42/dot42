package dot42.java.test;

import junit.framework.*;

public class TestNewT extends TestCase
{
	public void testNewMyClass()
	{
		Object x = new MyClass();
		assertTrue(x instanceof MyClass);
	}

	public void testNewMyClass2()
	{
		MyClass x = new MyClass2();
		assertTrue(x instanceof MyClass);
		assertTrue(x instanceof MyClass2);
	}

	public class MyClass
	{            
	}

	public class MyClass2 extends MyClass
	{
	}
}
