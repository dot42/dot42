package dot42.java.test;

import junit.framework.*;

public class TestTryFinally extends TestCase
{
	private int value;
	
	private void Foo(int x) 
	{
		value = x;
	}
	
	public void test1()
	{
		try
		{
			Foo(0);
		}
		finally
		{
			Foo(25);
		}
		assertEquals(value, 25);
	}
}
