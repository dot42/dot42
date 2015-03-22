using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Java.Text;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestEnum : TestCase
    {
        private enum TwoFields { Aap, Noot }

        public void testGetNames()
        {
            var names = Enum.GetNames(typeof(TwoFields));
            AssertEquals(2, names.Length);
        }

        public void testGetName()
        {
            var x = Enum.GetName(typeof(TwoFields), TwoFields.Aap);
            AssertEquals("Aap", x);
        }

        public void testParse1()
        {
            var x = Enum.Parse(typeof(TwoFields), "Aap");
            AssertEquals(TwoFields.Aap, x);
            x = Enum.Parse(typeof(TwoFields), "Noot");
            AssertEquals(TwoFields.Noot, x);
        }

        public void testParse2()
        {
            var x = Enum.Parse(typeof(TwoFields), "Aap", false);
            AssertEquals(TwoFields.Aap, x);
            x = Enum.Parse(typeof(TwoFields), "Noot", false);
            AssertEquals(TwoFields.Noot, x);

            x = Enum.Parse(typeof(TwoFields), "aap", true);
            AssertEquals(TwoFields.Aap, x);
            x = Enum.Parse(typeof(TwoFields), "noot", true);
            AssertEquals(TwoFields.Noot, x);
        }

        public void testTryParse1()
        {
            TwoFields result;
            var x = Enum.TryParse<TwoFields>("Aap", out result);
            AssertTrue(x);
            AssertEquals(TwoFields.Aap, result);
            x = Enum.TryParse<TwoFields>("Noot", out result);
            AssertTrue(x);
            AssertEquals(TwoFields.Noot, result);
            x = Enum.TryParse<TwoFields>("DoesNotExist", out result);
            AssertFalse(x);
        }

        public void testTryParse2()
        {
            TwoFields result;
            var x = Enum.TryParse<TwoFields>("Aap", false, out result);
            AssertTrue(x);
            AssertEquals(TwoFields.Aap, result);
            x = Enum.TryParse<TwoFields>("Noot", false, out result);
            AssertTrue(x);
            AssertEquals(TwoFields.Noot, result);
            x = Enum.TryParse<TwoFields>("DoesNotExist", false, out result);
            AssertFalse(x);

            x = Enum.TryParse<TwoFields>("aap", true, out result);
            AssertTrue(x);
            AssertEquals(TwoFields.Aap, result);
            x = Enum.TryParse<TwoFields>("noot", true, out result);
            AssertTrue(x);
            AssertEquals(TwoFields.Noot, result);
            x = Enum.TryParse<TwoFields>("doesNotExist", true, out result);
            AssertFalse(x);

            x = Enum.TryParse<TwoFields>("aap", false, out result);
            AssertFalse(x);
            x = Enum.TryParse<TwoFields>("noot", false, out result);
            AssertFalse(x);
            x = Enum.TryParse<TwoFields>("doesNotExist", false, out result);
            AssertFalse(x);
        }

        //public void testToStringDirect()
        //{
        //    AssertEquals("Aap", TwoFields.Aap.ToString());
        //    AssertEquals("Aap", TwoFields.Aap.ToString("G"));
        //    AssertEquals("00000001", TwoFields.Noot.ToString("X"));
        //    AssertEquals("1", TwoFields.Noot.ToString("d"));
        //}

        public void testStringFormat()
        {
            AssertEquals("Aap", string.Format("{0}", TwoFields.Aap));
            AssertEquals("Aap", string.Format("{0:g}", TwoFields.Aap));
            AssertEquals("00000001", string.Format("{0:X}", TwoFields.Noot));
            AssertEquals("1", string.Format("{0:D}", TwoFields.Noot));
        }

        public void testIFormatable()
        {
            var formatable = (IFormattable)(object)TwoFields.Aap;
            AssertEquals("Aap", formatable.ToString());
            AssertEquals("Aap", formatable.ToString("G", CultureInfo.InvariantCulture));

            formatable = (IFormattable)(object)TwoFields.Noot;
            AssertEquals("00000001", formatable.ToString("X", CultureInfo.InvariantCulture));
            AssertEquals("1", formatable.ToString("D", CultureInfo.InvariantCulture));
        }
    }

}
