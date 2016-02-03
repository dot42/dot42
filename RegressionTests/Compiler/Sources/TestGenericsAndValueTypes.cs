using System;
using System.Globalization;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestGenericsAndValueTypes : TestCase
    {
        private enum TwoFields { Aap, Noot }

        private struct Immutable
        {
            public readonly int x;
            public readonly int y;

            public Immutable(int x)
            {
                this.x = x;
                y = 0;
            }

        }

        struct Mutable
        {
            public int x; public int y;

            public bool Equals(Mutable other)
            {
                return x == other.x && y == other.y;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (x*397) ^ y;
                }
            }
           
            public override string ToString()
            {
                return string.Format("Mutable(x={0} y={1})", x, y);
            }

            public override bool Equals(object o)
            {
                if (ReferenceEquals(null, o)) return false;
                return o is Mutable && Equals((Mutable) o);
            }
        }
        

        public void testInt()
        {
            AssertEquals(42, new GC<int>(42).GetT());
            AssertEquals(0, new GC<int>().DefaultT());
            AssertEquals(0, new GC<int>().GetT());
        }

        public void testEnum()
        {
            AssertEquals(TwoFields.Noot, new GC<TwoFields>(TwoFields.Noot).GetT());
            AssertNotNull(new GC<TwoFields>().GetT());
            AssertNotNull(new GC<TwoFields>().DefaultT());
            AssertEquals(TwoFields.Aap, new GC<TwoFields>().DefaultT());
            AssertEquals(TwoFields.Aap, new GC<TwoFields>().GetT());
        }

        public void testMutableStruct()
        {
            Mutable m1;m1.x = 1; m1.y = 2;
            AssertEquals(m1, new GC<Mutable>(m1).GetT());
            AssertNotNull(new GC<Mutable>().GetT());
            AssertNotNull(new GC<Mutable>().DefaultT());
            AssertEquals(default(Mutable), new GC<Mutable>().DefaultT());
            AssertEquals(default(Mutable), new GC<Mutable>().GetT());
            AssertEquals(default(Mutable), new GC<Mutable>().GetT());
        }
#if NOT_IMPLEMENTED
        public void testMutableStructCopy()
        {
            Mutable m1; m1.x = 1; m1.y = 2;
            var gc = new GC<Mutable>(m1);
            m1.x = 2;
            Mutable m2 = gc.GetT();
            if (m1.Equals(m2))
                Fail(string.Format("should not be equal: {0} {1}", m1, m2));
        }
#endif

        //public void testMutableStructNull()
        //{
        //    Func<object> o = () => null;
        //    Mutable m1 = (Mutable) o();
        //    AssertNotNull(m1);
        //}

        public void testImmutableStruct()
        {
            var i = new Immutable(5);
            AssertEquals(5, i.x);
        }

        public void testMutableStructByReference()
        {
            Mutable m1; m1.x = 1; m1.y = 2;
            Mutable m2;

            new GC<Mutable>(m1).GetT(out m2);
            AssertEquals(m1, m2);

            new GC<Mutable>().DefaultT(out m2);
            AssertNotNull(m2);
            AssertEquals(default(Mutable), m2);
        }

        public void testOverrideGenericWithEnum()
        {
            int cv = (int)new TestValueConverterEnum().Convert(TwoFields.Noot, typeof(int), null, null);
            AssertEquals((int)TwoFields.Noot, cv);
        }

        public abstract class MvxValueConverter<TFrom, TTo>
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Convert((TFrom)value, targetType, parameter, culture);
            }

            protected virtual TTo Convert(TFrom value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private class TestValueConverterEnum : MvxValueConverter<TwoFields, int>
        {
            protected override int Convert(TwoFields value, Type targetType, object parameter, CultureInfo culture)
            {
                return Convert(value);
            }

            private int Convert(TwoFields value)
            {
                return (int)value;
            }
        }

        class GC<T>
        {
            private readonly T _val;

            public GC()
            {
            }

            public GC(T val)
            {
                _val = val;
            }


            public T GetT()
            {
                return _val;
            }

            public T DefaultT()
            {
                return default(T);
            }

            public void DefaultT(out T val)
            {
                val = default(T);
            }

            public void GetT(out T val)
            {
                val = _val;
            }
        }
    }
}
