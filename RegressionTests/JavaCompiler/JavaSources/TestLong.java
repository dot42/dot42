package dot42.java.test;

import junit.framework.*;

public class TestLong extends TestCase
{
	public void testSimpleEqual1()
	{
		long i = 5L;
		assertTrue(i == 5L);
	}

	public void testSimpleEqual2()
	{
		long i = 7L;
		assertTrue(i == 7L);
	}

	public void testAdd1()
	{
		long i = 5L;
		assertEquals(i + 4L, 9L);
	}

	public void testAdd2()
	{
		long i = 5L;
		assertTrue(i + 4L == 9L);
	}

	public void testAdd3()
	{
		long i = 5002L;
		assertTrue(i + 4L == 5006L);
	}

	public void testAdd4()
	{
		long i = 500002L;
		assertTrue(i + 400L == 500402L);
	}

	public void testSub1()
	{
		long i = 5L;
		assertEquals(i - 4L, 1L);
	}

	public void testSub2()
	{
		long i = 5L;
		assertTrue(i - 17L == -12L);
	}

	public void testSub3()
	{
		long i = 5002L;
		assertTrue(i - 14L == 4988L);
	}

	public void testSub4()
	{
		long i = 500002L;
		assertTrue(400L - i == -499602L);
	}

	public void testMul1()
	{
		long i = -2L;
		assertEquals(i * 4L, -8L);
	}

	public void testMul2()
	{
		long i = 50L;
		assertTrue(i * 17L == 850L);
	}

	public void testMul3()
	{
		long i = 5002L;
		assertTrue(i * -14L == -70028L);
	}

	public void testMul4()
	{
		long i = 2L;
		assertTrue(10L / i == 5L);
	}

	public void testDiv1()
	{
		long i = 3L;
		assertEquals(i  / 2L, 1L);
	}

	public void testDiv2()
	{
		long i = 50L;
		assertTrue(i / 5L == 10L);
	}

	public void testDiv3()
	{
		long i = 5002L;
		assertTrue(i / -14L == -357L);
	}

	public void testDiv4()
	{
		long i = 0L;
		assertTrue(i /100L == 0L);
	}

	public void testRem1()
	{
		long i = 3L;
		assertEquals(i % 2L, 1L);
	}

	public void testRem2()
	{
		long i = 50L;
		assertTrue(i % 5L == 0L);
	}

	public void testRem3()
	{
		long i = 5002L;
		assertTrue(i % -14L == 4L);
	}

	public void testRem4()
	{
		long i = 0L;
		assertTrue(i % 100L == 0L);
	}
}
