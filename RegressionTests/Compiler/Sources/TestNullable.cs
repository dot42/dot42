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
    }
}
