package dot42.java.test;

import junit.framework.*;
import android.graphics.*;

public class TestInitializedArrays extends TestCase
{
	public void testSByte1()
	{
		byte[] s = new byte[] { 0, 5, 11, 127, -128 };
		assertNotNull(s);
		assertEquals(s[0], (byte)0);
		assertEquals(s[1], (byte)5);
		assertEquals(s[2], (byte)11);
		assertEquals(s[3], (byte)127);
		assertEquals(s[4], (byte)-128);
	}

	public void testChar()
	{
		char[] s = new char[] { 'g', 'b', (char)0xFF };
		assertNotNull(s);
		assertEquals(s[0], 'g');
		assertEquals(s[1], 'b');
		assertEquals(s[2], 0xFF);
	}

	public void testShort()
	{
		short[] s = new short[] { 0, 23, -300, 1600, 22 };
		assertNotNull(s);
		assertEquals(s[0], (short)0);
		assertEquals(s[1], (short)23);
		assertEquals(s[2], (short)-300);
		assertEquals(s[3], (short)1600);
		assertEquals(s[4], (short)22);
	}

	public void testInt()
	{
		int[] s = new int[] { 0, 23, -30000, 1600, 27334235 };
		assertNotNull(s);
		assertEquals(s[0], 0);
		assertEquals(s[1], 23);
		assertEquals(s[2], -30000);
		assertEquals(s[3], 1600);
		assertEquals(s[4], 27334235);
	}

	public void testFloat()
	{
		float[] s = new float[] { 0.0f, 23f, -30000.5f, 1600.7f, 27334235.2f };
		assertNotNull(s);
		assertEquals(s[0], 0.0f);
		assertEquals(s[1], 23f);
		assertEquals(s[2], -30000.5f);
		assertEquals(s[3], 1600.7f);
		assertEquals(s[4], 27334235.2f);
	}

	public void testLong()
	{
		long[] s = new long[] { 0, 23, -300004477887522L, 27334235375625483L };
		assertNotNull(s);
		assertEquals(s[0], 0L);
		assertEquals(s[1], 23L);
		assertEquals(s[2], -300004477887522L);
		assertEquals(s[3], 27334235375625483L);
	}

	public void testDouble()
	{
		double[] s = new double[] { 0.0d, 23d, -3000321364768924600.5d, 1600.7d, 273342327556925872625.2d };
		assertNotNull(s);
		assertEquals(s[0], 0.0d, 0.0001d);
		assertEquals(s[1], 23d); // Converted to object on purpose!
		assertEquals(s[2], -3000321364768924600.5d, 0.0001d);
		assertEquals(s[3], 1600.7d, 0.0001d);
		assertEquals(s[4], 273342327556925872625.2d, 0.0001d);
	}

	public void testString1()
	{
		String[] s = new String[] { "aap", "noot", "mies" };
		assertNotNull(s);
		assertEquals(s[0], "aap");
		assertEquals(s[1], "noot");
		assertEquals(s[2], "mies");
	}

	public void testMyObject()
	{
		MyObject[] s = new MyObject[] { new MyObject("aap"), new MyObject("noot"), new MyObject("mies") };
		assertNotNull(s);
		assertEquals(s[0].S, "aap");
		assertEquals(s[1].S, "noot");
		assertEquals(s[2].S, "mies");
	}

	public void testMyEnum()
	{
		MyEnum[] s = new MyEnum[] { MyEnum.Aap, MyEnum.Mies, MyEnum.Mies, MyEnum.Noot, MyEnum.Aap, MyEnum.Noot };
		assertNotNull(s);
		assertEquals(s[0], MyEnum.Aap);
		assertEquals(s[1], MyEnum.Mies);
		assertEquals(s[2], MyEnum.Mies);
		assertEquals(s[3], MyEnum.Noot);
		assertEquals(s[4], MyEnum.Aap);
		assertEquals(s[5], MyEnum.Noot);
	}

	public void testJavaEnum()
	{
		Paint.Join[] s = new Paint.Join[] { Paint.Join.BEVEL, Paint.Join.MITER, Paint.Join.BEVEL, Paint.Join.ROUND, Paint.Join.MITER };
		assertNotNull(s);
		assertEquals(s[0], Paint.Join.BEVEL);
		assertEquals(s[1], Paint.Join.MITER);
		assertEquals(s[2], Paint.Join.BEVEL);
		assertEquals(s[3], Paint.Join.ROUND);
		assertEquals(s[4], Paint.Join.MITER);
	}

	private class MyObject
	{
		public final String S;

		public MyObject(String s)
		{
			S = s;
		}
	}

	private enum MyEnum
	{
		Aap,
		Noot,
		Mies
	}
}
