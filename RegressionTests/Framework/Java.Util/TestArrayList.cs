using System;
using Java.Util;
using Junit.Framework;

namespace Dot42.Tests.Java.Util
{
    public class TestArrayList : TestCase
    {
        public void testCount()
        {
            var list = new ArrayList<string>();
            list.Add("hoi");
            list.Add("noot");
            AssertEquals(2, list.Count);
        }

        public void testGetItem()
        {
            var list = new ArrayList<string>();
            list.Add("hoi");
            list.Add("noot");
            list.Add("hoi2");
            list.Add("noot2");
            string x = list[1];
            AssertEquals("noot", x);
        }

        public void testSetItem()
        {
            var list = new ArrayList<string>();
            list.Add("hoi");
            list.Add("noot");
            list.Add("hoi2");
            list.Add("noot2");
            list[1] = "different";
            string x = list[1];
            AssertEquals("different", x);
        }
    }
}
