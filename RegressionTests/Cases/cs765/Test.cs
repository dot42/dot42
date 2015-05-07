using System;
using Android.App;using Android.OS;
using Android.Test;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Com.Parse;

[assembly: Instrumentation(Label = "dot42 parse Tests", FunctionalTest = true)]
[assembly: UsesLibrary("android.test.runner")]

namespace dot42parse
{
    public class Test : ActivityInstrumentationTestCase2<MainActivity>
    {
		public Test() : base(typeof(MainActivity)) 
		{
		}
		
		public void test1() 
		{
			var activity = Activity;
			AssertNotNull(activity);
		}
		
		public void testParseObject() 
		{
			ParseObject testObject = new ParseObject("TestObject");		
		}
    }
}
