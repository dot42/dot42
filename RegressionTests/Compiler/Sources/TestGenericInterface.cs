using Dot42.Tests.Compiler.Sources.GenericInterfaceSubjects;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestGenericInterface : TestCase
    {
        public void testGenericImplementation1_1()
        {
            IGenericInterface<string> x = new GenericImplementation1<string>();
            x.Foo("hello");
            AssertNull(x.FooReturn());
        }

        public void testGenericImplementation1_2()
        {
            IGenericInterface<int> x = new GenericImplementation1<int>();
            x.Foo(6);
            AssertEquals(x.FooReturn(), 0);
        }

        public void testGenericImplementation1_3()
        {
            IGenericInterface<byte> x = new GenericImplementation1<byte>();
            x.Foo(6);
            AssertEquals((int)x.FooReturn(), 0);
        }

        public void testGenericImplementation1_4()
        {
            IGenericInterface<sbyte> x = new GenericImplementation1<sbyte>();
            x.Foo(6);
            AssertEquals((int)x.FooReturn(), 0);
        }

        public void testGenericImplementation1_5()
        {
            IGenericInterface<char> x = new GenericImplementation1<char>();
            x.Foo('6');
            AssertEquals(x.FooReturn(), '\0');
        }

        public void testGenericImplementation1_6()
        {
            IGenericInterface<short> x = new GenericImplementation1<short>();
            x.Foo(6);
            AssertEquals(x.FooReturn(), (short)0);
        }

        public void testGenericImplementation1_7()
        {
            IGenericInterface<long> x = new GenericImplementation1<long>();
            x.Foo(6);
            AssertEquals(x.FooReturn(), 0L);
        }

        public void testGenericImplementation1_8()
        {
            IGenericInterface<float> x = new GenericImplementation1<float>();
            x.Foo(6);
            AssertEquals(x.FooReturn(), 0.0F);
        }

        public void testGenericImplementation1_9()
        {
            IGenericInterface<double> x = new GenericImplementation1<double>();
            x.Foo(6);
            AssertEquals(x.FooReturn(), 0.0, 0.000001);
        }

        public void testExplicitGenericImplementation1_1()
        {
            IGenericInterface<string> x = new ExplicitGenericImplementation1<string>();
            x.Foo("hello");
            AssertNull(x.FooReturn());
        }

        public void testExplicitGenericImplementation1_2()
        {
            IGenericInterface<int> x = new ExplicitGenericImplementation1<int>();
            x.Foo(6);
            AssertEquals(x.FooReturn(), 0);
        }

        public void testExplicitGenericImplementation1_3()
        {
            IGenericInterface<byte> x = new ExplicitGenericImplementation1<byte>();
            x.Foo(6);
            AssertEquals((int)x.FooReturn(), 0);
        }

        public void testExplicitGenericImplementation1_4()
        {
            IGenericInterface<sbyte> x = new ExplicitGenericImplementation1<sbyte>();
            x.Foo(6);
            AssertEquals((int)x.FooReturn(), 0);
        }

        public void testExplicitGenericImplementation1_5()
        {
            IGenericInterface<char> x = new ExplicitGenericImplementation1<char>();
            x.Foo('6');
            AssertEquals(x.FooReturn(), '\0');
        }

        public void testExplicitGenericImplementation1_6()
        {
            IGenericInterface<short> x = new ExplicitGenericImplementation1<short>();
            x.Foo(6);
            AssertEquals(x.FooReturn(), (short)0);
        }

        public void testExplicitGenericImplementation1_7()
        {
            IGenericInterface<long> x = new ExplicitGenericImplementation1<long>();
            x.Foo(6);
            AssertEquals(x.FooReturn(), 0L);
        }

        public void testExplicitGenericImplementation1_8()
        {
            IGenericInterface<float> x = new ExplicitGenericImplementation1<float>();
            x.Foo(6);
            AssertEquals(x.FooReturn(), 0.0F);
        }

        public void testExplicitGenericImplementation1_9()
        {
            IGenericInterface<double> x = new ExplicitGenericImplementation1<double>();
            x.Foo(6);
            AssertEquals(x.FooReturn(), 0.0, 0.000001);
        }

        public void testGenericMethodInInterface()
        {
            int i = new ClassImplementGenericMethodFromInterface().FooReturn<int>();
            AssertEquals(0, i);
        }

        public void testGenericMethodCallsGenericMethod()
        {
            int i = new ClassCallsGenericMethodFromGenericMethod().FooReturn<int>();
            AssertEquals(0, i);
        }
        
    }
}
