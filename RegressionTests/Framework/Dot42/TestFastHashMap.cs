using System;
using Dot42.Collections.Specialized;
using Junit.Framework;

namespace Dot42.Tests.Dot42
{
    public class TestOpenHashMap : TestCase
    {
        class MyKey { }
        class MyValue { }

        private IOpenHashMap<MyKey, MyValue> CreateWeakMap()
        {
            var map = new OpenWeakReferenceHashMap<MyKey, MyValue>(16);
            map.Put(new MyKey(), new MyValue());
            return map;
        }
        public void testWeakHashMap1()
        {
            var key1 = new MyKey();
            var key2 = new MyKey();
            var value1 = new MyValue();
            var value2 = new MyValue();

            var map = new OpenWeakReferenceHashMap<MyKey, MyValue>(16);

            map.Put(key1, value1);
            map.Put(key2, value2);

            AssertEquals(value1, map.Get(key1));
            AssertEquals(value2, map.Get(key2));
            AssertEquals(2, map.Size);
        }

        public void testWeakPutIfAbsent()
        {
            var key1 = new MyKey();
            var key2 = new MyKey();
            var value1 = new MyValue();
            var value2 = new MyValue();

            var map = new OpenWeakReferenceHashMap<MyKey, MyValue>(16);

            AssertEquals(null, map.Put(key1, value1));
            AssertEquals(value1, map.Put(key1, value2));
            AssertEquals(value2, map.Put(key1, value1));

            AssertEquals(value1, map.PutIfAbsent(key1, value2));
            AssertEquals(null, map.PutIfAbsent(key2, value2));
            AssertEquals(value2, map.PutIfAbsent(key2, value1));
        }


        public void testWeakHashMapCollect()
        {
            var map = CreateWeakMap();
            var key2 = new MyKey();
            map.Put(key2, new MyValue());
            GC.Collect();
            map = map.Clone(map.Size);

            AssertNotNull(map.Get(key2));
            AssertEquals(1, map.Size);
        }

        public void testPutIfAbsent()
        {
            var key1 = new MyKey();
            var key2 = new MyKey();
            var key3 = new MyKey();
            var value1 = new MyValue();
            var value2 = new MyValue();
            var value3 = new MyValue();

            var map = new OpenIdentityHashMap<MyKey, MyValue>();

            AssertEquals(null, map.Put(key1, value1));
            AssertEquals(value1, map.Put(key1, value2));
            AssertEquals(value2, map.Put(key1, value1));
            AssertEquals(null, map.Put(key3, value3));

            AssertEquals(value1, map.PutIfAbsent(key1, value2));
            AssertEquals(null, map.PutIfAbsent(key2, value2));
            AssertEquals(value2, map.PutIfAbsent(key2, value1));
            AssertEquals(value3, map.Put(key3, value2));
        }

        private void Reference(object obj)
        {
            if (obj != null)
            {
            }
        }
    }
}
