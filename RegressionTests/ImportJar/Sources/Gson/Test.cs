using System;
using Android.App;
using Android.Os;
using Android.Test;
using Android.Widget;
using Java.Util;
using Dot42;
using Dot42.Manifest;
using Com.Google.Gson;

namespace ImportJar.Sources.Gson
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
		
		public void test2() 
		{
			var json = new JsonObject();
			json.Add("name", JsonNull.INSTANCE);
		}
		
		public void test3() 
		{
			ArrayList<Fill> pList = GetValue(new ArrayList<Fill>());
		}
		
		public T GetValue<T>(T xDefaultValue)
        {
            const string JsonString = "[{\"Name\":\"5\"}]";
            var GSonObj = new Com.Google.Gson.Gson();
            var LastParseResult = GSonObj.FromJson<T>(JsonString, xDefaultValue.GetType());
            if (LastParseResult != null)
                return (T)LastParseResult;
            return xDefaultValue;
        }
		
		public class Fill
		{
			public string Name { get; set; }
		}
	}
}
