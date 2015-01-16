package dot42.java.test;

import junit.framework.*;

public class TestTypeOf extends TestCase
{
	public void test1()
	{
		Class type = TestTypeOf.class;
		assertEquals("TestTypeOf", type.getSimpleName());
	}
}
