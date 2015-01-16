using System;
using Java.Util;
using Junit.Framework;

namespace Dot42.Tests.Java.Util
{
    public class TestIList : TestCase
    {
        public void testCount()
        {
            IList<string> list = new ArrayList<string>();
            list.Add("hoi");
            list.Add("noot");
            AssertEquals(2, list.Count);
        }

        public void testGetItem()
        {
            IList<string> list = new ArrayList<string>();
            list.Add("hoi");
            list.Add("noot");
            list.Add("hoi2");
            list.Add("noot2");
            string x = list[1];
            AssertEquals("noot", x);
        }

        public void testSetItem()
        {
            IList<string> list = new ArrayList<string>();
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
