using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestCloneable : TestCase
    {
        public void test1()
        {
			ICloneable inst1 = new CustomCloneable();
			var inst2 = (CustomCloneable)inst1.Clone();

            AssertEquals("inst1", 0, ((CustomCloneable)inst1).Counter);
            AssertEquals("inst2", 1, inst2.Counter);
        }
		
		public class CustomCloneable : System.ICloneable 
		{
			public int Counter;
		
			public object Clone()
			{
				return new CustomCloneable { Counter = this.Counter + 1 };
			}
		}
    }
}
