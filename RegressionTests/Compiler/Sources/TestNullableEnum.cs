using System;
using NUnit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    [TestFixture]
    public class TestNullableEnum
    {
        [Include]
        public JsonToken? token;


        [Test]
        public void testNullableEnum()
        {
            Console.WriteLine(typeof(JsonToken?).ToString());
            Console.WriteLine(Nullable.GetUnderlyingType(typeof(JsonToken?)).ToString());

            Assert.AreEqual(0, (int) token.GetValueOrDefault());
            Assert.AreEqual(JsonToken.EndArray, token.GetValueOrDefault(JsonToken.EndArray));
            Assert.IsFalse(token.HasValue);
            Assert.IsTrue(token == null);
            Assert.IsFalse(token != null);

            try
            {
                Console.WriteLine(token.GetType().ToString());
                Assert.Fail("should throw");
            }
            catch (NullReferenceException)
            {
            }

            try
            {
                Console.WriteLine(token.Value.ToString());
                Assert.Fail("should throw");
            }
            catch (InvalidOperationException)
            {
            }

            token = JsonToken.Boolean;

            Assert.AreEqual(typeof(JsonToken), token.GetType());
            Assert.IsNull(Nullable.GetUnderlyingType(token.GetType()));
            Assert.AreEqual(token, JsonToken.Boolean);
            Assert.AreEqual(JsonToken.Boolean, token.GetValueOrDefault());
            Assert.AreEqual(JsonToken.Boolean, token.GetValueOrDefault(JsonToken.EndArray));
            
            Assert.IsFalse(token == null);
            Assert.IsTrue(token != null);
            Assert.IsTrue(token.HasValue);
            

            token = null;

            Assert.AreEqual(0, (int)token.GetValueOrDefault());
            Assert.AreEqual(JsonToken.Bytes, token.GetValueOrDefault(JsonToken.Bytes));
            Assert.IsFalse(token.HasValue);
            Assert.AreEqual(token, null);
            Assert.IsTrue(token == null);
            Assert.IsFalse(token != null);


            try
            {
                Console.WriteLine(token.GetType().ToString());
                Assert.Fail("should throw");
            }
            catch (NullReferenceException)
            {
            }
            try
            {
                Console.WriteLine(token.Value.ToString());
                Assert.Fail("should throw");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Test]
        public void testToString()
        {
            JsonToken? t = null;
            Assert.AreEqual(string.Empty, t.ToString());
            t = JsonToken.EndArray;
            Assert.AreNotEqual(string.Empty, t.ToString());
        }


        [Test]
        public void testGetUnderlyingOfTypOf()
        {
            Assert.AreEqual(typeof(JsonToken), Nullable.GetUnderlyingType(typeof(JsonToken?)));
        }

        [Test]
        public void testGetUnderlyingThroughReflection()
        {
            var fieldType = this.GetType().GetField("token").FieldType;
            var underlying = Nullable.GetUnderlyingType(fieldType);
            
            Assert.AreEqual(typeof(JsonToken), underlying);
            Assert.AreEqual(typeof(JsonToken?), fieldType);

            Assert.IsTrue(fieldType.IsAssignableFrom(typeof(JsonToken)));
        }

        [Test]
        public void testCreateGenericInstanceOfNullable()
        {
            var fieldType = this.GetType().GetField("token").FieldType;
            Junit.Framework.Assert.AssertTrue(fieldType.IsGenericType);

            var nullable = fieldType.GetGenericTypeDefinition();
            Assert.NotNull(nullable);
            Assert.IsTrue(nullable.IsGenericTypeDefinition);

            Type createdType = nullable.MakeGenericType(Nullable.GetUnderlyingType(fieldType));
            Assert.IsNotNull(createdType);
            Assert.AreEqual(typeof(JsonToken?), createdType);

            // should actually be the very same type...
            Assert.AreEqual(fieldType, createdType);
            Assert.IsTrue(fieldType.IsAssignableFrom(createdType));

            JsonToken? defaultValue = (JsonToken?)Activator.CreateInstance(createdType);

            Assert.AreEqual(default(JsonToken?), defaultValue);
            Assert.AreEqual(null, defaultValue);
        }

        [Test]
        public void testByRef()
        {
            JsonToken? val = default(JsonToken);

            Assert.AreEqual(default(JsonToken), GetValueOrDefault(ref val));
            Assert.AreEqual(default(JsonToken), GetValue(ref val));
            Assert.AreEqual(true, HasValue(ref val));

            SetToNull(ref val);
            Assert.IsFalse(val.HasValue);

            Assert.AreEqual(default(JsonToken), GetValueOrDefault(ref val));
            Assert.AreEqual(false, HasValue(ref val));


            SetToDefault(ref val);
            Assert.AreEqual(JsonToken.Default, val.Value);

            Assert.AreEqual(JsonToken.Default, GetValueOrDefault(ref val));
            Assert.AreEqual(JsonToken.Default, GetValue(ref val));
            Assert.AreEqual(true, HasValue(ref val));

        }

        void SetToNull(ref JsonToken? val)
        {
            val = null;
        }

        void SetToDefault(ref JsonToken? val)
        {
            val = JsonToken.Default;
        }

        bool HasValue(ref JsonToken? val)
        {
            return val.HasValue;
        }

        JsonToken GetValueOrDefault(ref JsonToken? val)
        {
            return val.GetValueOrDefault();
        }

        JsonToken GetValue(ref JsonToken? val)
        {
            return val.Value;
        }

        [Test]
        public void testIsAssignableFrom()
        {
            Assert.IsTrue(typeof(JsonToken?).IsAssignableFrom(typeof(JsonToken)));
        }


        /// <summary>
        /// Specifies the type of JSON token.
        /// </summary>
        public enum JsonToken
        {
            /// <summary>
            /// A comment.
            /// </summary>
            Default = 5,

            /// <summary>
            /// A boolean.
            /// </summary>
            Boolean = 10,

            /// <summary>
            /// An array end token.
            /// </summary>
            EndArray = 14,

            /// <summary>
            /// Byte data.
            /// </summary>
            Bytes = 17
        }

    }
}