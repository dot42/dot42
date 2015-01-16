using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Junit.Framework;

namespace Cases.Case656
{
	public class Test : TestCase 
	{
		// Local output parameter s must not be initialized
		public void test656() 
		{
			var dict2 = new System.Collections.Generic.Dictionary<int, string>(); 
			dict2.Add(1, "One"); 
			dict2.Add(2, "Two"); 
			string s; // = "-"; 
			dict2.TryGetValue(2, out s); 
			AssertEquals(s, "Two");
		}

		// Local output parameter s must not be initialized
		public void test656_int() 
		{
			var dict2 = new System.Collections.Generic.Dictionary<int, int>(); 
			dict2.Add(1, 100); 
			dict2.Add(2, 200); 
			int s;
			dict2.TryGetValue(2, out s); 
			AssertEquals(s, 200);
		}
	}
}
