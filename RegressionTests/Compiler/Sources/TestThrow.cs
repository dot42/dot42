using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestThrow : TestCase
    {
        public void testSimple1()
        {
			try
			{
				throw new Exception("Test");
				AssertTrue(false);
			}
			catch (Exception) 
			{
				AssertTrue(true);
			}
        }
    }
}
