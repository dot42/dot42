using System;
using System.Collections;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    /// <summary>
    /// Test casting of non array instances to IEnumerable, ICollection and IList.
    /// </summary>
    public class TestNonCastSpecial : TestCase
    {
        public void testIs1()
        {
            var x = new object();
            AssertFalse(IsIEnumerable(x));
            AssertFalse(IsICollection(x));
            AssertFalse(IsIList(x));
        }

        public void testIs2()
        {
            var x = new MyEnumerable();
            AssertTrue(IsIEnumerable(x));
            AssertFalse(IsICollection(x));
            AssertFalse(IsIList(x));
        }

        public void testIs3()
        {
            var x = new MyCollection();
            AssertTrue(IsIEnumerable(x));
            AssertTrue(IsICollection(x));
            AssertFalse(IsIList(x));
        }

        public void testIs4()
        {
            var x = new MyList();
            AssertTrue(IsIEnumerable(x));
            AssertTrue(IsICollection(x));
            AssertTrue(IsIList(x));
        }

        public void testAs1()
        {
            var x = new object();
            AssertNull(AsIEnumerable(x));
            AssertNull(AsICollection(x));
            AssertNull(AsIList(x));
        }

        public void testAs2()
        {
            var x = new MyEnumerable();
            AssertNotNull(AsIEnumerable(x));
            AssertNull(AsICollection(x));
            AssertNull(AsIList(x));
        }

        public void testAs3()
        {
            var x = new MyCollection();
            AssertNotNull(AsIEnumerable(x));
            AssertNotNull(AsICollection(x));
            AssertNull(AsIList(x));
        }

        public void testAs4()
        {
            var x = new MyList();
            AssertNotNull(AsIEnumerable(x));
            AssertNotNull(AsICollection(x));
            AssertNotNull(AsIList(x));
        }

        public void testCast1()
        {
            var x = new object();
            try { CastToIEnumerable(x); Fail("invalid cast expected"); }
            catch (InvalidCastException) { }
            try { CastToICollection(x); Fail("invalid cast expected"); }
            catch (InvalidCastException) { }
            try { CastToIList(x); Fail("invalid cast expected"); }
            catch (InvalidCastException) { }
        }

        public void testCast2()
        {
            var x = new MyEnumerable();
            try
            {
                var e = CastToIEnumerable(x);
                AssertNull(e.GetEnumerator());
            }
            catch (InvalidCastException)
            {
                Fail("valid cast expected");                
            }
            try { CastToICollection(x); Fail("invalid cast expected"); }
            catch (InvalidCastException) { }
            try { CastToIList(x); Fail("invalid cast expected"); }
            catch (InvalidCastException) { }
        }

        public void testCast3()
        {
            var x = new MyCollection();
            try
            {
                var e = CastToIEnumerable(x);
                AssertNull(e.GetEnumerator());
            }
            catch (InvalidCastException)
            {
                Fail("valid cast expected");
            }
            try
            {
                var c = CastToICollection(x);
                AssertEquals(7, c.Count);
            }
            catch (InvalidCastException)
            {
                Fail("valid cast expected");                
            }
            try { CastToIList(x); Fail("invalid cast expected"); }
            catch (InvalidCastException) { }
        }

        public void testCast4()
        {
            var x = new MyList();
            try
            {
                var e = CastToIEnumerable(x);
                AssertNull(e.GetEnumerator());
            }
            catch (InvalidCastException)
            {
                Fail("valid cast expected");
            }
            try
            {
                var c = CastToICollection(x);
                AssertEquals(7, c.Count);
            }
            catch (InvalidCastException)
            {
                Fail("valid cast expected");
            }
            try
            {
                var l = CastToIList(x);
                AssertEquals(true, l.IsFixedSize);
            }
            catch (InvalidCastException)
            {
                Fail("valid cast expected");
            }
        }

        public static bool IsIEnumerable(object x) { return x is IEnumerable; }
        public static bool IsICollection(object x) { return x is ICollection; }
        public static bool IsIList(object x) { return x is IList; }

        public static IEnumerable AsIEnumerable(object x) { return x as IEnumerable; }
        public static ICollection AsICollection(object x) { return x as ICollection; }
        public static IList AsIList(object x) { return x as IList; }

        public static IEnumerable CastToIEnumerable(object x) { return (IEnumerable)x; }
        public static ICollection CastToICollection(object x) { return (ICollection)x; }
        public static IList CastToIList(object x) { return (IList)x; }

        private class MyEnumerable : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                return null;
            }
        }

        private class MyCollection : MyEnumerable, ICollection
        {
            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public int Count { get { return 7; } }
            public bool IsSynchronized { get; private set; }
            public object SyncRoot { get; private set; }
        }

        private class MyList : MyCollection, IList
        {
            public int Add(object element)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(object element)
            {
                throw new NotImplementedException();
            }

            public int IndexOf(object element)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, object element)
            {
                throw new NotImplementedException();
            }

            public void Remove(object element)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public bool IsFixedSize { get { return true; } }
            public bool IsReadOnly { get; private set; }

            public object this[int index]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }
    }
}
