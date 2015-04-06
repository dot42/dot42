using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Junit.Framework;

namespace Cases.Case657
{
    public class Test : TestCase
    {
		public enum NUMBER { ONE = 1, TWO = 2, THREE = 3 }; 
		
        // Local output parameter s must not be initialized
        public void test657()
        {
			var dict2 = new System.Collections.Generic.Dictionary<NUMBER, string>(); 
			dict2.Add(NUMBER.ONE, "One"); 
			dict2.Add(NUMBER.TWO, "Two"); 
			string s = "-"; 
			dict2.TryGetValue(NUMBER.TWO, out s); 
			AssertEquals(s, "Two");
        }
    }
}
