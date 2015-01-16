package dot42.java.test;

import junit.framework.*;

public class TestDouble extends TestCase
{
	private static final double delta = 0.0001;

	public void testSimpleEqual1()
	{
		double i = 5.0;
		assertTrue(i == 5.0);
	}

	public void testSimpleEqual2()
	{
		double i = 7.0;
		assertTrue(i == 7.0);
	}

	public void testAdd1()
	{
		double i = 5.0;
		assertEquals(i + 4.0, 9.0, delta);
	}

	public void testAdd2()
	{
		double i = 5.0;
		assertTrue(i + 4.0 == 9.0);
	}

	public void testAdd3()
	{
		double i = 5002.0;
		assertTrue(i + 4.0 == 5006.0);
	}

	public void testAdd4()
	{
		double i = 500002.0;
		assertTrue(i + 400.0 == 500402.0);
	}

	public void testSub1()
	{
		double i = 5.0;
		assertEquals(i - 4.0, 1.0, delta);
	}

	public void testSub2()
	{
		double i = 5.0;
		assertTrue(i - 17.0 == -12.0);
	}

	public void testSub3()
	{
		double i = 5002.0;
		assertTrue(i - 14.0 == 4988.0);
	}

	public void testSub4()
	{
		double i = 500002.0;
		assertTrue(400.0 - i == -499602.0);
	}

	public void testMul1()
	{
		double i = -2.0;
		assertEquals(i * 4.0, -8.0, delta);
	}

	public void testMul2()
	{
		double i = 50.0;
		assertTrue(i * 17.0 == 850.0);
	}

	public void testMul3()
	{
		double i = 5002.0;
		assertTrue(i * -14.0 == -70028.0);
	}

	public void testMul4()
	{
		double i = 2.0;
		assertTrue(10.0 / i == 5.0);
	}

	public void testDiv1()
	{
		double i = 3.0;
		assertEquals(i / 2.0, 1.5, delta);
	}

	public void testDiv2()
	{
		double i = 50.0;
		assertTrue(i / 5.0 == 10.0);
	}

	public void testDiv3()
	{
		double i = 5002.0;
		assertEquals(i / -14.0,-357.28571428, delta);
	}

	public void testDiv4()
	{
		double i = 0.0;
		assertTrue(i / 100.0 == 0);
	}

	public void testRem1()
	{
		double i = 3.0;
		assertEquals(i % 2.0, 1.0, delta);
	}

	public void testRem2()
	{
		double i = 50.0;
		assertTrue(i % 5.0 == 0.0);
	}

	public void testRem3()
	{
		double i = 5002.0;
		assertTrue(i % -14.0 == 4.0);
	}

	public void testRem4()
	{
		double i = 0.0;
		assertTrue(i % 100.0 == 0.0);
	}
}
