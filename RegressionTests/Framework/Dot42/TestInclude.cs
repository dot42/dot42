using System;
using Android.View;
using Junit.Framework;

namespace Dot42.Tests.Dot42
{
    public class TestInclude : TestCase
    {
        public void testInclude1()
        {
            var type = typeof (IncludeAll);
			AssertEquals("#Methods", 1, type.JavaGetDeclaredMethods().Length);
			AssertEquals("#Fields", 1, type.JavaGetDeclaredFields().Length);
        }

        public void testInclude2()
        {
            var type = typeof (IncludeNone);
			AssertEquals("#Methods", 0, type.JavaGetDeclaredMethods().Length);
			AssertEquals("#Fields", 0, type.JavaGetDeclaredFields().Length);
        }

        public void testInclude3()
        {
            var type = typeof (TestInclude);
        }

        [Include]
        public void OnClick(View sender)
        {
            
        }
		
		[Include(ApplyToMembers = true)]
		private class IncludeAll 
		{
			public void NotReferenced() { }
			private int Foo;
		}
		
		private class IncludeNone
		{
			public void NotReferenced() { }
			private int Foo;
		}
    }
}
