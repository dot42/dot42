using System;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestNullable : TestCase
    {
        internal enum E
        {
            A,
            B,
            C
        };

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
