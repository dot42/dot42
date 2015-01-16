using System.Collections.Generic;
using System.Linq;
using Junit.Framework;

namespace Dot42.Tests.System.Linq
{
    public class TestSelect : TestCase
    {
        public void test1()
        {
            var array = new List<int>();
            array.Add(1);
            array.Add(2);
            array.Add(3);
            array.Add(4);
            var t = 2;
            foreach (var x in array.Select(x => x + 1))
            {
                AssertEquals(t, x);
                t++;
            }
        }
    }
}
