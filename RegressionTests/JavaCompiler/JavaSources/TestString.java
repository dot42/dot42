package dot42.java.test;

import junit.framework.*;

public class TestString extends TestCase
{
	public void testLength()
	{
		String s = "Hello";
		assertEquals(s.length(), 5);
	}

	public void testCharAt()
	{
		String  s = "Hello";
		assertEquals(s.charAt(0), 'H');
	}

	public void testIsNullOrEmpty2()
	{
		assertEquals("".isEmpty(), true);
	}

	public void testIsNullOrEmpty3()
	{
		String  s = "Hello";
		assertEquals(s.isEmpty(), false);
	}

	public void testConcat1()
	{
		String s = "Hello";
		assertEquals(s, "Hello");
		assertEquals(s + "-there", "Hello-there");
	}

	public void testConcat2()
	{
		String s = "Hello";
		assertEquals(s, "Hello");
		assertEquals(s + 5, "Hello5");
	}

	public void testConcat3()
	{
		String s = "Hello";
		assertEquals(s, "Hello");
		assertEquals(s + 'a', "Helloa");
	}
}
