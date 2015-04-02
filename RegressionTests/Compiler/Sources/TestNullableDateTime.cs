using System;
using Android.Util;
using NUnit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    [TestFixture]
    public class TestNullableDateTime
    {
        [Include]
        public DateTime? datetime;

        public void testNullableDateTimeGetValueOrDefault()
        {
            DateTime? e = null;

            DateTime def = e.GetValueOrDefault();
            Assert.AssertEquals(default(DateTime), def);
            
            e = new DateTime(2002, 2, 2);
            def = e.GetValueOrDefault();
            Assert.AssertEquals(new DateTime(2002, 2, 2), def);

            e = null;
            def = e.GetValueOrDefault();
            Assert.AssertEquals( default(DateTime), def);
        }

        [Test]
        public void testNullableDateTime()
        {
            Assert.AreEqual(default(DateTime), datetime.GetValueOrDefault());
            Assert.AreEqual(DateTime.MaxValue, datetime.GetValueOrDefault(DateTime.MaxValue));
            Assert.AreEqual(new DateTime(2000,1,1), datetime.GetValueOrDefault(new DateTime(2000,1,1)));
            Assert.IsFalse(datetime.HasValue);
            Assert.IsTrue(datetime == null);
            Assert.IsFalse(datetime != null);

            try
            {
                Console.WriteLine(datetime.GetType().ToString());
                Assert.Fail("should throw");
            }
            catch (NullReferenceException)
            {
            }

            try
            {
                Console.WriteLine(datetime.Value.ToString());
                Assert.Fail("should throw");
            }
            catch (InvalidOperationException)
            {
            }

            datetime = new DateTime(2000,1,1);

            Assert.AreEqual(typeof(DateTime), datetime.GetType());
            Assert.IsNull(Nullable.GetUnderlyingType(datetime.GetType()));
            Assert.AreEqual(datetime, new DateTime(2000, 1, 1));
            Assert.AreEqual(new DateTime(2000, 1, 1), datetime.GetValueOrDefault());
            Assert.AreEqual(new DateTime(2000, 1, 1), datetime.GetValueOrDefault(new DateTime(2022, 2, 2)));
            
            Assert.IsFalse(datetime == null);
            Assert.IsTrue(datetime != null);
            Assert.IsTrue(datetime.HasValue);

            datetime = null;

            Assert.AreEqual(default(DateTime), datetime.GetValueOrDefault());
            Assert.AreEqual(new DateTime(2000, 1, 1), datetime.GetValueOrDefault(new DateTime(2000, 1, 1)));
            Assert.IsFalse(datetime.HasValue);
            Assert.AreEqual(datetime, null);
            Assert.IsTrue(datetime == null);
            Assert.IsFalse(datetime != null);


            try
            {
                Console.WriteLine(datetime.GetType().ToString());
                Assert.Fail("should throw");
            }
            catch (NullReferenceException)
            {
            }
            try
            {
                Console.WriteLine(datetime.Value.ToString());
                Assert.Fail("should throw");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Test]
        public void testToString()
        {
            DateTime? d = null;
            Assert.AreEqual(string.Empty, d.ToString());
            d = new DateTime(2000,2,2);
            Assert.AreNotEqual(string.Empty, d.ToString());
        }

        [Test]
        public void testGetUnderlyingOfTypOf()
        {
            Assert.AreEqual(typeof(DateTime), Nullable.GetUnderlyingType(typeof(DateTime?)));
        }

        [Test]
        public void testGetUnderlyingThroughReflection()
        {
            var fieldType = this.GetType().GetField("datetime").FieldType;
            var underlying = Nullable.GetUnderlyingType(fieldType);
            
            Assert.AreEqual(typeof(DateTime), underlying);
            Assert.AreEqual(typeof(DateTime?), fieldType);

            // this will not work for arbitrary structs.
            //Assert.IsTrue(fieldType.IsAssignableFrom(typeof(DateTime)));
        }

        [Test]
        public void testMakeGenericTypeOfNullable()
        {
            var fieldType = this.GetType().GetField("datetime").FieldType;
            
            Assert.IsTrue(fieldType.IsGenericType);

            var nullable = fieldType.GetGenericTypeDefinition();
            Assert.NotNull(nullable);
            Assert.IsTrue(nullable.IsGenericTypeDefinition);

            Type createdType = nullable.MakeGenericType(Nullable.GetUnderlyingType(fieldType));
            Assert.IsNotNull(createdType);
            Assert.AreEqual(typeof(DateTime?), createdType);

            Assert.AreEqual(fieldType, createdType);
            Assert.IsTrue(fieldType.IsAssignableFrom(createdType));

            DateTime? defaultValue = (DateTime?)Activator.CreateInstance(createdType);

            Assert.AreEqual(default(DateTime?), defaultValue);
            Assert.AreEqual(null, defaultValue);
        }

        //[Test]
        //public void testIsAssignableFrom_KnownToFail()
        //{
        //    Assert.IsTrue(typeof(DateTime?).IsAssignableFrom(typeof(DateTime)));
        //}

    }
}