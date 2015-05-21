using System;
using NUnit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    [TestFixture]
    public class TestNullableInt
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

            Assert.AreEqual(typeof(int), intval.GetTypeReflectionSafe());
            Assert.IsNull(Nullable.GetUnderlyingType(intval.GetTypeReflectionSafe()));
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
            //Assert.IsTrue(fieldType.IsAssignableFrom(typeof(int)));
        }

        [Test]
        public void testCreateGenericInstanceOfNullable()
        {
            var fieldType = this.GetType().GetField("intval").FieldType;
            
            Assert.IsTrue(fieldType.IsGenericType);

            var nullable = fieldType.GetGenericTypeDefinition();
            Assert.NotNull(nullable);
            Assert.IsTrue(nullable.IsGenericTypeDefinition);
            Assert.AreEqual(nullable, typeof(Nullable<>));

            Type createdType = nullable.MakeGenericType(typeof(int));
            Assert.IsNotNull(createdType);
            Assert.AreEqual(createdType, typeof(int?));

            // should actually be the very same type...
            // this will not work for arbitrary structs.
            // Assert.IsTrue(fieldType.IsAssignableFrom(createdType));

            int? defaultValue = (int?)Activator.CreateInstance(createdType);

            Assert.AreEqual(null, defaultValue);
            Assert.AreEqual(default(int?), defaultValue);
        }

        [Test]
        public void testByRef()
        {
            int? val = 0;

            Assert.AreEqual(0, GetValueOrDefault(ref val));
            Assert.AreEqual(0, GetValue(ref val));
            Assert.AreEqual(true, HasValue(ref val));

            SetToNull(ref val);
            Assert.IsFalse(val.HasValue);
            Assert.AreEqual(0, GetValueOrDefault(ref val));
            Assert.AreEqual(false, HasValue(ref val));

            SetTo1(ref val);
            Assert.AreEqual(1, val.Value);
            Assert.AreEqual(1, GetValueOrDefault(ref val));
            Assert.AreEqual(true, HasValue(ref val));
            Assert.AreEqual(1, GetValue(ref val));
        }

        void SetToNull(ref int? val)
        {
            val = null;
        }

        void SetTo1(ref int? val)
        {
            val = 1;
        }

        bool HasValue(ref int? val)
        {
            return val.HasValue;
        }

        int GetValue(ref int? val)
        {
            return val.Value;
        }

        int GetValueOrDefault(ref int? val)
        {
            return val.GetValueOrDefault();
        }


        //[Test]
        //public void testIsAssignableFrom_KnownToFail()
        //{
        //    Assert.IsTrue(typeof(int?).IsAssignableFrom(typeof(int)));
        //}


    }
}