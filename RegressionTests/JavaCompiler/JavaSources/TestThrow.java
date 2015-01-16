package dot42.java.test;

import junit.framework.*;

public class TestThrow extends TestCase
{
	public void testSimple1()
	{
		boolean ok = false;
		try
		{
			throw new Exception("Test");
		}
		catch (Exception ex) 
		{
			ok = true;
		}
		assertTrue(ok);
	}
}
