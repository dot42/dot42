package dot42.java.test;

import junit.framework.*;

public class TestInt extends TestCase
{
	public void testSimpleEqual1()
	{
		int i = 5;
		assertTrue(i == 5);
	}

	public void testSimpleEqual2()
	{
		int i = 7;
		assertTrue(i == 7);
	}

	public void testAdd1()
	{
		int i = 5;
		assertEquals(i + 4, 9);
	}

	public void testAdd2()
	{
		int i = 5;
		assertTrue(i + 4 == 9);
	}

	public void testAdd3()
	{
		int i = 5002;
		assertTrue(i + 4 == 5006);
	}

	public void testAdd4()
	{
		int i = 500002;
		assertTrue(i + 400 == 500402);
	}

	public void testSub1()
	{
		int i = 5;
		assertEquals(i - 4, 1);
	}

	public void testSub2()
	{
		int i = 5;
		assertTrue(i - 17 == -12);
	}

	public void testSub3()
	{
		int i = 5002;
		assertTrue(i - 14 == 4988);
	}

	public void testSub4()
	{
		int i = 500002;
		assertTrue(400 - i == -499602);
	}

	public void testMul1()
	{
		int i = -2;
		assertEquals(i * 4, -8);
	}

	public void testMul2()
	{
		int i = 50;
		assertTrue(i * 17 == 850);
	}

	public void testMul3()
	{
		int i = 5002;
		assertTrue(i * -14 == -70028);
	}

	public void testMul4()
	{
		int i = 2;
		assertTrue(10 / i == 5);
	}

	public void testDiv1()
	{
		int i = 3;
		assertEquals(i  / 2, 1);
	}

	public void testDiv2()
	{
		int i = 50;
		assertTrue(i / 5 == 10);
	}

	public void testDiv3()
	{
		int i = 5002;
		assertTrue(i / -14 == -357);
	}

	public void testDiv4()
	{
		int i = 0;
		assertTrue(i /100 == 0);
	}

	public void testRem1()
	{
		int i = 3;
		assertEquals(i % 2, 1);
	}

	public void testRem2()
	{
		int i = 50;
		assertTrue(i % 5 == 0);
	}

	public void testRem3()
	{
		int i = 5002;
		assertTrue(i % -14 == 4);
	}

	public void testRem4()
	{
		int i = 0;
		assertTrue(i % 100 == 0);
	}
}
