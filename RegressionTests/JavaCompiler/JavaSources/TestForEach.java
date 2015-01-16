package dot42.java.test;

import java.util.*;
import junit.framework.*;

public class TestForEach extends TestCase
{
	public void testIntArray()
	{
		int[] arr = new int[] { 1, 2, 3, 4, 5, 6 };
		int index = 0;
		for (int value : arr)
		{
			assertEquals(index + 1, value);
			index++;
		}
		assertEquals(index, 6);
	}

	public void testStringArray()
	{
		String[] arr = new String[] { "a", "bb", "ccc", "dddd" };
		int index = 0;
		for (String value : arr)
		{
			assertEquals(index + 1, value.length());
			index++;
		}
		assertEquals(index, 4);
	}

	public void testStringList()
	{
		ArrayList<String> arr = new ArrayList<String>();
		arr.add("a");
		arr.add("bb");
		arr.add("ccc");
		arr.add("dddd");
		int index = 0;
		for (String value : arr)
		{
			assertEquals(index + 1, value.length());
			index++;
		}
		assertEquals(index, 4);
	}
}
