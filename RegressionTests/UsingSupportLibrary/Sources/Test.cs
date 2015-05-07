using System;
using Android.App;using Android.OS;
using Android.Test;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

namespace UsingSuportLibrary.Sources
{
    public class Test : ActivityInstrumentationTestCase2<FragmentDialogActivity>
    {
		public Test() : base(typeof(FragmentDialogActivity)) 
		{
		}
		
		public void test1() 
		{
			var activity = Activity;
			AssertNotNull(activity);
		}
    }
}
