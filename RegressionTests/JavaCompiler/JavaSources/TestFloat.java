package dot42.java.test;

import junit.framework.*;

public class TestFloat extends TestCase
{
	private static final float delta = 0.0001f;

	public void testSimpleEqual1()
	{
		float i = 5.0f;
		assertTrue(i == 5.0f);
	}

	public void testSimpleEqual2()
	{
		float i = 7.0f;
		assertTrue(i == 7.0f);
	}

	public void testAdd1()
	{
		float i = 5.0f;
		assertEquals(i + 4.0f, 9.0f, delta);
	}

	public void testAdd2()
	{
		float i = 5.0f;
		assertTrue(i + 4.0f == 9.0f);
	}

	public void testAdd3()
	{
		float i = 5002.0f;
		assertTrue(i + 4.0f == 5006.0f);
	}

	public void testAdd4()
	{
		float i = 500002.0f;
		assertTrue(i + 400.0f == 500402.0f);
	}

	public void testSub1()
	{
		float i = 5.0f;
		assertEquals(i - 4.0f, 1.0f, delta);
	}

	public void testSub2()
	{
		float i = 5.0f;
		assertTrue(i - 17.0f == -12.0f);
	}

	public void testSub3()
	{
		float i = 5002.0f;
		assertTrue(i - 14.0f == 4988.0f);
	}

	public void testSub4()
	{
		float i = 500002.0f;
		assertTrue(400.0f - i == -499602.0f);
	}

	public void testMul1()
	{
		float i = -2.0f;
		assertEquals(i * 4.0f, -8.0f, delta);
	}

	public void testMul2()
	{
		float i = 50.0f;
		assertTrue(i * 17.0f == 850.0f);
	}

	public void testMul3()
	{
		float i = 5002.0f;
		assertTrue(i * -14.0f == -70028.0f);
	}

	public void testMul4()
	{
		float i = 2.0f;
		assertTrue(10.0f / i == 5.0f);
	}

	public void testDiv1()
	{
		float i = 3.0f;
		assertEquals(i / 2.0f, 1.5f, delta);
	}

	public void testDiv2()
	{
		float i = 50.0f;
		assertTrue(i / 5.0f == 10.0f);
	}

	public void testDiv3()
	{
		float i = 5002.0f;
		assertEquals(i / -14.0f, -357.28571428f, delta);
	}

	public void testDiv4()
	{
		float i = 0.0f;
		assertTrue(i / 100.0f == 0f);
	}

	public void testRem1()
	{
		float i = 3.0f;
		assertEquals(i % 2.0f, 1.0f, delta);
	}

	public void testRem2()
	{
		float i = 50.0f;
		assertTrue(i % 5.0f == 0.0f);
	}

	public void testRem3()
	{
		float i = 5002.0f;
		assertTrue(i % -14.0f == 4.0f);
	}

	public void testRem4()
	{
		float i = 0.0f;
		assertTrue(i % 100.0f == 0.0f);
	}
}
