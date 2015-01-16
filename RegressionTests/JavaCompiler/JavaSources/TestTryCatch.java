package dot42.java.test;

import junit.framework.*;

public class TestTryCatch extends TestCase
{
	private void Foo(int x)
	{
		
	}

	public void testX()
	{
		
	}
	
	public void testTryFinally()
	{
		try
		{
			Foo(0);
		}
		finally
		{
			Foo(25);
		}
	}

	public void testTryCatch()
	{
		try
		{
			Foo(0);
		}
		catch (Exception ex)
		{
			Foo(1);
		}
	}

	public void testTryCatchFinally()
	{
		try
		{
			Foo(0);
		}
		catch (Exception ex)
		{
			Foo(1);
		}
		finally
		{
			Foo(25);
		}
	}

	public void testTryCatchAllFinally()
	{
		try
		{
			Foo(0);
		}
		catch (Throwable ex)
		{
			Foo(2);
		}
		finally
		{
			Foo(25);
		}
	}

	public void testTryCatchCatchAllFinally()
	{
		try
		{
			Foo(0);
		}
		catch (Exception ex)
		{
			Foo(1);
		}
		catch (Throwable ex)
		{
			Foo(2);
		}
		finally
		{
			Foo(25);
		}
	}

	public void testNestedTryCatch()
	{
		try
		{
			Foo(0);
			try
			{
				Foo(100);
			}
			catch (RuntimeException ex)
			{
				Foo(101);
			}
			Foo(10);
		}
		catch (Exception ex)
		{
			Foo(1);
		}
	}
}
