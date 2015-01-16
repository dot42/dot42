using System;
using System.Collections.Generic;
using System.Linq;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestDictionaryKeyValue : TestCase
    {
        public void test1()
        {
			var dict = new Dictionary<string, List<string>>();
			dict["a"] = new List<string>();
			dict["a"].Add("1");
			dict["a"].Add("1");
			dict["a"].Add("2");
			dict["b"] = new List<string>();
			dict["b"].Add("4");
			dict["b"].Add("4");

			foreach (var c in dict)
			{
				var res = (c.Value.Distinct().Count() == 1);

			// do somethingwith res
			}
        }
	}
}
