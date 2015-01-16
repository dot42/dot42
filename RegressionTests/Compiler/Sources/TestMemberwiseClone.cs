using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestMemberwiseClone : TestCase
    {
        public void test1()
        {
            var class1 = new Class1();
            var duplicate = class1.Duplicate();

            AssertNotNull(duplicate);
        }

        public void test2()
        {
            var class2 = new Class2();
            var duplicate = class2.Duplicate();

            AssertNotNull(duplicate);
        }

        public void test3()
        {
            var class3 = new Class3();
            var duplicate = class3.Duplicate();

            AssertNotNull(duplicate);
        }

        public void test4()
        {
            var struct1 = new Struct1();
            var duplicate = struct1.Duplicate();

            AssertNotNull(duplicate);
        }

        public void test5()
        {
            var class5 = new Class5();
            var duplicate = class5.Duplicate();

            AssertNotNull(duplicate);
        }
		
		public class Class1
		{
			public Class1 Duplicate()
			{
                return (Class1)this.MemberwiseClone();
			}
		}

        public class Class2 : Class1
        {
            public Class2 Duplicate()
            {
                return (Class2)this.MemberwiseClone();
            }
        }

        public class Class3 : Java.Util.NoSuchElementException
        {
            public Class3 Duplicate()
            {
                return (Class3)this.MemberwiseClone();
            }
        }

        public class Class5 : ICloneable
        {
            public Class5 Duplicate()
            {
                return (Class5)this.MemberwiseClone();
            }

            public object Clone()
            {
                throw new NotImplementedException();
            }
        }

        public struct Struct1
        {
            public Struct1 Duplicate()
            {
                return (Struct1)this.MemberwiseClone();
            }
        }
    }
}
