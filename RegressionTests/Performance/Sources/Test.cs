using System;
using Android.App;using Android.OS;
using Android.Test;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using TallComponents.PDF;
using Junit.Framework;

namespace Performance.Sources
{
    public class Test : TestCase
    {
		public void test1() 
		{
			var vector = glyphlistVector.vector;
			AssertNotNull(vector);
		}
    }
}
