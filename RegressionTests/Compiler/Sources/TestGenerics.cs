using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestGenerics : TestCase
    {
        public void test1()
        {
            var f = new Field<string>("dot42");
            AssertTrue(f.Fld is string);
            AssertEquals("dot42", f.Fld);
        }

        public void _test1a()
        {
            Field<string>.StaticFld = "dot42";
            AssertTrue(Field<string>.StaticFld is string);
            AssertEquals("dot42", Field<string>.StaticFld);
        }

        public void test1b()
        {
            var f = new Field<string>("dot42");
            f.Array = new[] { "aap", "noot" };
            AssertTrue(f.Array is string[]);
            AssertEquals("aap", f.Array[0]);
        }

        public class Field<F>
        {
            public readonly F Fld;
            public static F StaticFld;
            public F[] Array;

            public Field(F fld)
            {
                Fld = fld;
            }
        }

        public void _test2()
        {
            var sa = new StructArray<MyStruct>();
            AssertTrue(sa.Array is Array);
            AssertEquals(16, sa.Array.Length);
            AssertNotNull(sa.Array[0]);
            AssertEquals(0, sa.Array[0].A);
        }

        public void test3()
        {
            var myDerivedClass = new MyDerivedClass();
            myDerivedClass.Foo(12);
            AssertSame(12, myDerivedClass.Value);
            AssertSame(22, myDerivedClass.FooInt(22));

            myDerivedClass.Foo2(32, true);
            AssertSame(32, myDerivedClass.Value);
		}

        public void _test3a()
        {
            var myDerivedClass = new MyDerivedClass();
            AssertSame(42, myDerivedClass.ReturnOnly());
        }

        public void _test3b()
        {
            IReturnOnly<int> myDerivedClass = new MyDerivedClass();
            AssertSame(42, myDerivedClass.ReturnOnly());

            IReturnOnly returnOnly = new MyDerivedClass();
            AssertSame(42, returnOnly.ReturnOnly());
        }

        public void test4()
        {
            var myDerivedClass = new MyDerivedClass2<int>();
            myDerivedClass.Foo(12);

            AssertSame(12, myDerivedClass.Value);
        }

        public void test4b()
        {
            var myDerivedClass = new MyDerivedClass3();
            myDerivedClass.Foo(new object());
			AssertNotNull(myDerivedClass.Value);
        }

        public void test5()
        {
            var pageCollection = new PageCollection();
            var page = new Page{Index = 42};

            var index = pageCollection.IndexOf(page);
            AssertSame(42, index);
        }

        public void test6()
        {
            var myDerivedClass = new MyDerivedClass6();
            myDerivedClass.Foo(42);

            AssertSame(42, myDerivedClass.Value);
            AssertTrue(myDerivedClass.Called);
        }

        public class StructArray<T>
        {
            public StructArray()
            {
                Array = new T[16];
            }

            public readonly T[] Array;
        }

        public struct MyStruct
        {
            public int A;
        }

        public class MyDerivedClass : MyClass<int>, IReturnOnly<int>, IReturnOnly
        {
            public int Value;

            public override void Foo(int v)
            {
                Value = v;
            }

            public override void Foo2(int v, bool b1)
            {
                Value = v;
            }

            public override int FooInt(int v)
            {
                Value = v;
				return v;
            }

            public override int ReturnOnly()
            {
                return 42;
            }

            object IReturnOnly.ReturnOnly()
            {
                return null;
            }
        }

        public class MyDerivedClass2<dataType> : MyClass<dataType>
        {
            public dataType Value;

            public override void Foo(dataType v)
            {
                Value = v;
            }
        }

        public class MyDerivedClass3 : MyClass<object>
        {
            public object Value;
            public override void Foo(object v)
            {
                Value = v;
            }
        }

        public class MyClass6<dataType>
        {
            public bool Called;

            public virtual void Foo(dataType v)
            {
                Called = true;
            }
        }

        public class MyDerivedClass6 : MyClass6<int>
        {
            public int Value;
            
            public override void Foo(int v)
            {
                Value = v;

                base.Foo(v);
            }
        }

        public class MyClass<dataType> : MyBase
        {
            public virtual void Foo(dataType v)
            {
                Fail("Overriden version should be called");
            }

            public virtual void Foo2(dataType v, bool b)
            {
                Fail("Overriden version should be called");
            }

            public virtual int FooInt(dataType v)
            {
                Fail("Overriden version should be called");
				return -1;
            }

            public virtual dataType ReturnOnly()
            {
                Fail("Overriden version should be called");
                return default(dataType);
            }
        }

        public class MyBase
        {
            [Dot42.Include]
            public virtual object ReturnOnly()
            {
                Fail("Overriden version should be called");
                return null;
            }
        }

        public interface IReturnOnly<T>
        {
            T ReturnOnly();
        }

        public interface IReturnOnly
        {
            object ReturnOnly();
        }

        internal class Page
        {
            public int Index;
        }

        internal class PageCollection : IIndexOf<Page>
        {
            public int IndexOf(Page item)
            {
                return item.Index;
            }
        }

        internal interface IIndexOf<T>
        {
            int IndexOf(T item);
        }


    }
}
