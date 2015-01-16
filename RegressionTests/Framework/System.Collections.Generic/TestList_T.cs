using System.Collections;
using System.IO;
using Junit.Framework;

using System.Collections.Generic;

namespace Dot42.Tests.System.Collections.Generic
{
    public class TestList_T : TestCase
    {
        public interface IMyClass
        {
            int MyInt { get; }
        }

        public class MyClass : IMyClass
        {
            public MyClass(int myInt)
            {
                MyInt = myInt;
            }

            public int MyInt { get; private set; }
        }

        public class MyClass2
        {
            public MyClass2(int myInt)
            {
                MyInt = myInt;
            }

            public int MyInt;
        }

        internal sealed class MyClassList : List<MyClass>
        {
            public int MyAdditionalInt;
        }

        public void test1()
        {
            IList<IMyClass> list = new List<IMyClass>();
            list.Add(new MyClass(12));

            AssertNotNull(list);
            AssertSame(1, list.Count);
            AssertSame(12, list[0].MyInt);

            foreach (var t in list)
            {
                AssertSame(12, t.MyInt);
            }

            IEnumerable<IMyClass> iEnum = list;

            var enumerator = iEnum.GetEnumerator();
            enumerator.MoveNext();

            var list2 = new List<MyClass2>();
            list2.Add(new MyClass2(12));
            foreach (var t in list2)
            {
                AssertSame(12, t.MyInt);
            }

            ICollection<MyClass2> iColl2 = list2;

            var enumerator2 = iColl2.GetEnumerator();
            bool result = enumerator2.MoveNext();
            AssertTrue(result);
            AssertSame(12, enumerator2.Current.MyInt);

            result = enumerator2.MoveNext();
            AssertFalse(result);
        }

        public void test2()
        {
            IList<MyClass> list3 = new List<MyClass>();
            list3.Add(new MyClass(12));
            foreach (var x in list3)
            {
                AssertSame(12, x.MyInt);
            }

            var myClassList = new MyClassList();
            myClassList.Add(new MyClass(12));
            foreach (var x in myClassList)
            {
                AssertSame(12, x.MyInt);
            }
        }

    }
}
