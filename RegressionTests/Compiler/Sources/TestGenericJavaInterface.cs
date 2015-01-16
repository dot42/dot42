using Junit.Framework;

using Java.Util;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestGenericJavaInterface : TestCase
    {
        class MyComparator : IComparator<IMap<string, object>>
        {
            public int Compare(IMap<string, object> map1, IMap<string, object> map2)
            {
                return 42;
            }
        }

        public void testGenericJavaImplementation1_1()
        {
            IComparator<IMap<string, object>> sDisplayNameComparator = new MyComparator();

            IList<IMap<string, object>> myData = new ArrayList<IMap<string, object>>();

            myData.Add(new HashMap<string, object>());
            myData.Add(new HashMap<string, object>());

            Java.Util.Collections.Sort(myData, sDisplayNameComparator);

            AssertNotNull(myData);
        }
    }
}
