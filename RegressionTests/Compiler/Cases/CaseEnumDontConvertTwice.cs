using System;
using Android.Util;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Cases
{
    public class CaseEnumDontConvertTwice : TestCase
    {
        /// <summary>
        /// Specifies the type of JSON token.
        /// </summary>
        public enum JsonToken
        {
            /// <summary>
            /// This is returned by the <see cref="JsonReader"/> if a <see cref="JsonReader.Read"/> method has not been called. 
            /// </summary>
            None = 0,

            /// <summary>
            /// An object start token.
            /// </summary>
            StartObject = 1,

            /// <summary>
            /// An array start token.
            /// </summary>
            StartArray = 2,

            /// <summary>
            /// A constructor start token.
            /// </summary>
            StartConstructor = 3,

            /// <summary>
            /// An object property name.
            /// </summary>
            PropertyName = 4,

            /// <summary>
            /// A comment.
            /// </summary>
            Comment = 5,

            /// <summary>
            /// Raw JSON.
            /// </summary>
            Raw = 6,

            /// <summary>
            /// An integer.
            /// </summary>
            Integer = 7,

            /// <summary>
            /// A float.
            /// </summary>
            Float = 8,

            /// <summary>
            /// A string.
            /// </summary>
            String = 9,

            /// <summary>
            /// A boolean.
            /// </summary>
            Boolean = 10,

            /// <summary>
            /// A null token.
            /// </summary>
            Null = 11,

            /// <summary>
            /// An undefined token.
            /// </summary>
            Undefined = 12,

            /// <summary>
            /// An object end token.
            /// </summary>
            EndObject = 13,

            /// <summary>
            /// An array end token.
            /// </summary>
            EndArray = 14,

            /// <summary>
            /// A constructor end token.
            /// </summary>
            EndConstructor = 15,

            /// <summary>
            /// A Date.
            /// </summary>
            Date = 16,

            /// <summary>
            /// Byte data.
            /// </summary>
            Bytes = 17
        }


        internal int BuildStateArray()
        {

            foreach (JsonToken valueToken in Enum.GetValues(typeof(JsonToken)))
            {
                if (GetVal() <= (int)valueToken)
                {
                    if (valueToken == JsonToken.Boolean)
                        return 1;
                    else
                        return 2;
                }
            }

            return 0;
        }

        protected virtual int GetVal()
        {
            if (GetType().Name == "")
                return 1;
            return 0;
        }

        public void testEnumDontConvertTwice()
        {
            var x = BuildStateArray();
            Assert.AssertTrue(x > 0);
        }
    }
}
