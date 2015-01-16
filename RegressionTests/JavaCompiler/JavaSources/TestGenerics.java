package dot42.java.test;

import junit.framework.*;

public class TestGenerics extends TestCase
{
	public void test1() 
	{
		Derived x = new Derived();
		assertTrue(x.equals(x));
	}
	
	public abstract class BaseClass<T>
	{
		public abstract void Foo(T arg);
	}

	public class Derived extends BaseClass<String>
	{
		@Override
		public void Foo(String arg) 
		{
		}
	}
}
