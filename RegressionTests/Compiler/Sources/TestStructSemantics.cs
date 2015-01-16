using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestStructSemantics : TestCase
    {
		public void testLocalVar1() 
		{
			var x = new SimpleStruct();
			x.a = 5;
			var y = x;
			y.a = 6;
			AssertEquals("x.a", 5, x.a);
			AssertEquals("y.a", 6, y.a);
		}
		
		public void testLocalVar2() 
		{
			var x = new DerivedStruct();
			x.simple.a = 5;
			x.c = 50;
			var y = x;
			y.simple.a = 6;
			y.c = 60;
			AssertEquals("x.simple.a", 5, x.simple.a);
			AssertEquals("y.simple.a", 6, y.simple.a);
			AssertEquals("x.c", 50, x.c);
			AssertEquals("y.c", 60, y.c);
		}
		
		public void testCall1() 
		{
			var x = new SimpleStruct();
			var a = ModifyA(x);
			AssertEquals("x.a", 0, x.a);
			AssertEquals("a", 27, a);
		}
		
		public void testCall2() 
		{
			var x = new DerivedStruct();
			var c = ModifyAandC(x);
			AssertEquals("x.simple.a", 0, x.simple.a);
			AssertEquals("x.c", 0, x.c);
			AssertEquals("c", 270, c);
		}
		
		public void testArray1() 
		{
			var x = new SimpleStruct[5];
			var y = new SimpleStruct[5];
			x[0].a = 5;
			y[0] = x[0];
			y[0].a = 6;
			AssertEquals("x[0].a", 5, x[0].a);
			AssertEquals("y[0].a", 6, y[0].a);
		}
		
		public void testArray2() 
		{
			var x = new DerivedStruct[5];
			var y = new DerivedStruct[5];
			x[0].simple.a = 5;
			x[0].c = 50;
			y[0] = x[0];
			y[0].simple.a = 6;
			y[0].c = 60;
			AssertEquals("x[0].simple.a", 5, x[0].simple.a);
			AssertEquals("y[0].simple.a", 6, y[0].simple.a);
			AssertEquals("x[0].c", 50, x[0].c);
			AssertEquals("y[0].c", 60, y[0].c);
		}
		
		private static int ModifyA(SimpleStruct value)
		{
			value.a = 27;
			return value.a;
		}
		
		private static int ModifyAandC(DerivedStruct value)
		{
			value.simple.a = 27;
			value.c = 270;
			return value.c;
		}
		
		private struct SimpleStruct 
		{
			internal int a;
			internal string b;
		}			

		private struct DerivedStruct 
		{
			internal SimpleStruct simple;
			internal int c;
			internal string d;
		}			
	}
}
