using System;
using NUnit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    [TestFixture]
    public class NullableIntTest
    {
        [Include]
        public int? intval;

        public void testNullableIntGetValueOrDefault()
        {
            int? e = null;

            int def = e.GetValueOrDefault();
            Assert.AssertEquals(0, def);

            e = 10;
            def = e.GetValueOrDefault();
            Assert.AssertEquals(10, def);

            e = null;
            def = e.GetValueOrDefault();
            Assert.AssertEquals(default(int), def);
        }

        [Test]
        public void testNullableInt()
        {
            Assert.AreEqual(0, intval.GetValueOrDefault());
            Assert.AreEqual(20, intval.GetValueOrDefault(20));
            Assert.IsFalse(intval.HasValue);
            Assert.IsTrue(intval == null);
            Assert.IsFalse(intval != null);

            try
            {
                Console.WriteLine(intval.GetType().ToString());
                Assert.Fail("should throw");
            }
            catch (NullReferenceException)
            {
            }

            try
            {
                Console.WriteLine(intval.Value.ToString());
                Assert.Fail("should throw");
            }
            catch (InvalidOperationException)
            {
            }

            intval = 30;

            Assert.AreEqual(typeof(int), intval.GetType());
            Assert.IsNull(Nullable.GetUnderlyingType(intval.GetType()));
            Assert.AreEqual(30, intval);
            Assert.AreEqual(30, intval.GetValueOrDefault());
            Assert.AreEqual(30, intval.GetValueOrDefault(40));
            
            Assert.IsFalse(intval == null);
            Assert.IsTrue(intval != null);
            Assert.IsTrue(intval.HasValue);

            intval = null;

            Assert.AreEqual(0, intval.GetValueOrDefault());
            Assert.AreEqual(10, intval.GetValueOrDefault(10));
            Assert.IsFalse(intval.HasValue);
            Assert.AreEqual(intval, null);
            Assert.IsTrue(intval == null);
            Assert.IsFalse(intval != null);


            try
            {
                Console.WriteLine(intval.GetType().ToString());
                Assert.Fail("should throw");
            }
            catch (NullReferenceException)
            {
            }
            try
            {
                Console.WriteLine(intval.Value.ToString());
                Assert.Fail("should throw");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Test]
        public void testToString()
        {
            int? d = null;
            Assert.AreEqual(string.Empty, d.ToString());
            d = 20;
            Assert.AreEqual("20", d.ToString());
        }

        [Test]
        public void testGetUnderlyingOfTypOf()
        {
            Assert.AreEqual(typeof(int), Nullable.GetUnderlyingType(typeof(int?)));
        }

        [Test]
        public void testGetUnderlyingThroughReflection()
        {
            var fieldType = this.GetType().GetField("intval").FieldType;
            var underlying = Nullable.GetUnderlyingType(fieldType);

            Assert.AreEqual(typeof(int), underlying);
            Assert.AreEqual(typeof(int?), fieldType);

            // this will not work for primitive types
            //Assert.IsTrue(fieldType.IsAssignableFrom(typeof(DateTime)));
        }

        [Test]
        public void testGetCreateGenericInstanceOfNullable()
        {
            var fieldType = this.GetType().GetField("intval").FieldType;
            
            Assert.IsTrue(fieldType.IsGenericType);

            var nullable = fieldType.GetGenericTypeDefinition();
            Assert.NotNull(nullable);
            Assert.IsTrue(nullable.IsGenericTypeDefinition);
            Assert.AreEqual(nullable, typeof(Nullable<>));

            Type createdType = nullable.MakeGenericType(fieldType);
            Assert.IsNotNull(createdType);
            Assert.AreEqual(createdType, typeof(int?));

            // should actually be the very same type...
            // this will not work for arbitrary structs.
            // Assert.IsTrue(fieldType.IsAssignableFrom(createdType));

            int? defaultValue = (int?)Activator.CreateInstance(createdType);

            Assert.AreEqual(null, defaultValue);
            Assert.AreEqual(default(int?), defaultValue);
        }

        //[Test]
        //public void testIsAssignableFrom_KnownToFail()
        //{
        //    Assert.IsTrue(typeof(int?).IsAssignableFrom(typeof(int)));
        //}


    }
}