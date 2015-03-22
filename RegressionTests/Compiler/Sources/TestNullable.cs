using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestNullable : TestCase
    {
        
        public void testIntSet()
        {
            int? i;
            i = 10;

            int? j = 12;
            
            int? k = null;
            k = 15;

            int? l = k;

            var m = k;

            AssertEquals(i.HasValue, true);
            AssertEquals(i.Value, 10);

            AssertEquals(j.HasValue, true);
            AssertEquals(j.Value, 12);

            AssertEquals(k.HasValue, true);
            AssertEquals(k.Value, 15);

            AssertEquals(l.HasValue, true);
            AssertEquals(l.Value, 15);

            AssertEquals(m.HasValue, true);
            AssertEquals(m.Value, 15);
        }



        public void testIntNull()
        {
            int? i = 2;
            int? j = null;
            int? k = 3;

            k = null;
            i = k;

            AssertEquals(i.HasValue, false);
            AssertEquals(j.HasValue, false);
            AssertEquals(k.HasValue, false);
        }
       
        public void testBool()
        {
            bool? x = null;
            bool? y = x;

            AssertEquals(x.HasValue, false);
            AssertTrue(Nullable.Equals(x,y));
        }

        public void testMappingItem()
        {
            var mappingItem = new MappingItem();
            mappingItem.CharacterCode = 12;

            var str = mappingItem.ToString();

            AssertEquals("Val=12", str);
        }

        internal class MappingItem
        {
            public int? CharacterCode { get; set; }

            public override string ToString()
            {
                return string.Format("Val={0}", CharacterCode.HasValue ? CharacterCode.ToString() : "none");
            }
        }

        internal enum E { A, B, C };

        private int? _nullableInt = 2;
        private E? _nullableEnum = E.B;
        private DateTime? _nullableDateTime;


        public void testNullableEnumGetValueOrDefaultWithParam()
        {
            E? e = null;

            var def = e.GetValueOrDefault(_nullableEnum.Value);
            Assert.AssertEquals(def, E.B);

            e = E.A;
            def = e.GetValueOrDefault(_nullableEnum.Value);
            Assert.AssertEquals(def, E.A);
        }

        public void testNullableEnumGetValueOrDefault()
        {
            E? e = null;

            var def = e.GetValueOrDefault();
            Assert.AssertEquals(def, E.A);

            e = E.B;
            def = e.GetValueOrDefault();
            Assert.AssertEquals(def, E.B);

            e = null;
            def = e.GetValueOrDefault();
            Assert.AssertEquals(def, E.A);
        }

        public void testNullableIntToString()
        {
            Assert.AssertEquals("2", _nullableInt.ToString());
            _nullableInt = null;
            Assert.AssertEquals("", _nullableInt.ToString());
        }

        public void testNullableIntGetValueOrDefaultWithParam()
        {
            int? e = null;

            var def = e.GetValueOrDefault(_nullableInt.Value);
            Assert.AssertEquals(def, 2);

            e = 1;
            def = e.GetValueOrDefault(_nullableInt.Value);
            Assert.AssertEquals(def, 1);
        }

        public void testNullableIntGetValueOrDefault()
        {
            int? e = null;

            var def = e.GetValueOrDefault();
            Assert.AssertEquals(def, 0);

            e = 2;
            def = e.GetValueOrDefault();
            Assert.AssertEquals(def, 2);

            e = null;
            def = e.GetValueOrDefault();
            Assert.AssertEquals(def, 0);
        }

        public void testNullableDateTimeGetValueOrDefaultWithParam()
        {
            _nullableDateTime = new DateTime(2000, 1, 1);
            DateTime? e = null;

            var def = e.GetValueOrDefault(_nullableDateTime.Value);
            Assert.AssertEquals(def, new DateTime(2000, 1, 1));

            e = new DateTime(2002, 2, 2);
            def = e.GetValueOrDefault(_nullableDateTime.Value);
            Assert.AssertEquals(def, new DateTime(2002, 2, 2));
        }

        public void testNullableDateTimeGetValueOrDefault()
        {
            DateTime? e = null;

            var def = e.GetValueOrDefault();
            Assert.AssertEquals(def, default(DateTime));

            e = new DateTime(2002, 2, 2);
            def = e.GetValueOrDefault(_nullableDateTime.Value);
            Assert.AssertEquals(def, new DateTime(2002, 2, 2));

            e = null;
            def = e.GetValueOrDefault();
            Assert.AssertEquals(def, default(DateTime));
        }

    }
}
