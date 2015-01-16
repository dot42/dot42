package dot42.java.test;

import junit.framework.*;

public class TestSynchronized extends TestCase
{
	public void test1()
	{
		try 
		{
			TestNotify();
		} 
		catch (Exception ex)
		{
			fail("Error in notify");
		}
	}
	
	public void test2()
	{
		try 
		{
			TestStaticNotify();
		} 
		catch (Exception ex)
		{
			fail("Error in static notify");
		}
	}
	
	private synchronized void TestNotify() 
	{
		this.notify();
	}
	
	private static synchronized void TestStaticNotify() 
	{
		TestSynchronized.class.notify();
	}
}
