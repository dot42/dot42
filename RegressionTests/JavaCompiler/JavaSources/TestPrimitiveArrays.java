package dot42.java.test;

import junit.framework.*;

public class TestPrimitiveArrays extends TestCase
{
	public void testAlloc()
	{
		char[] s = new char[4];
		assertNotNull(s);
	}

	public void testLength()
	{
		char[] s = new char[4];
		assertEquals(s.length, 4);
	}

	public void testSetByte()
	{
		byte[] s = new byte[342];
		s[10] = 5;
	}

	public void testGetByte1()
	{
		byte[] s = new byte[4];
		s[1] = 34;
		assertEquals((int)s[1], 34);
	}

	public void testGetByte2()
	{
		byte[] s = new byte[4];
		s[3] = -127; 
		assertEquals((int)s[3], -127);
	}

	public void testSetBool()
	{
		boolean[] s = new boolean[342];
		s[10] = false;
	}

	public void testGetBool()
	{
		boolean[] s = new boolean[4];
		s[1] = true;
		assertEquals(s[1], true);
	}

	public void testSetChar()
	{
		char[] s = new char[4];
		s[0] = 'd';
	}

	public void testGetChar()
	{
		char[] s = new char[4];
		s[0] = 'd';
		assertEquals(s[0], 'd');
	}

	public void testSetShort()
	{
		short[] s = new short[4];
		s[1] = 12000;
	}

	public void testGetShort()
	{
		short[] s = new short[34];
		s[33] = 5523;
		assertEquals((short)s[33], (short)5523);
	}

	public void testSetInt()
	{
		int[] s = new int[4];
		s[1] = 12000;
	}

	public void testGetInt()
	{
		int[] s = new int[34];
		s[33] = 5523;
		assertEquals(s[33], 5523);
	}
}
