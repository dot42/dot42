package dot42.java.test;

import junit.framework.*;

public class TestInvokeSpecial extends TestCase
{
	public void test1() 
	{
		Derived x = new Derived();
		assertTrue(x.equals(x));
	}

	public class Derived
	{
		@Override
		final public boolean equals(Object other)
		{
			return super.equals(other);
		}
	}
}
