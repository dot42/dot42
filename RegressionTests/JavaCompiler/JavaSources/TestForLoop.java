package dot42.java.test;

import junit.framework.*;

public class TestForLoop extends TestCase
{
	public void testSimple1()
	{
		int j = 1;
		for (int i = 0; i < 10; i++)
		{
			assertEquals(i + 1, j);
			j++;
		}
	}

	public void testSimple2()
	{
		int j = 1;
		for (int i = 0; i < 10; i += 2)
		{
			assertEquals(i + 1, j);
			j++;
			j++;
		}
	}

	public void testSimple3()
	{
		int j = 1;
		for (int i = 0; i < /*Count*/(10); i++)
		{
			j++;
		}
		for (int i = 0; i < /*Count*/(5); i++)
		{
			assertEquals(i + 11, j);
			j++;
		}
	}

	public int Count(int i)
	{
		return i;
	}

}
