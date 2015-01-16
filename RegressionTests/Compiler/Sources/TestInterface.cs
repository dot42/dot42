using Dot42.Tests.Compiler.Sources.InterfaceSubjects;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestInterface : TestCase
    {
        public void testOnlyViaInterface()
        {
            ISimpleInterface x = new OnlyViaInterface();
            x.Foo();
            AssertTrue(true);
        }

        public void testSimpleImplementation1()
        {
            ISimpleInterface x = new SimpleImplementation();
            x.Foo();
            AssertTrue(true);
        }

        public void testSimpleImplementation2()
        {
            var x = new SimpleImplementation();
            x.Foo();
            AssertTrue(true);
        }

        public void testExplicitImplementation1()
        {
            ISimpleInterface x = new ExplicitImplementation();
            x.Foo();
            AssertTrue(true);
        }

        public void testExtSimpleImplementation1()
        {
            ISimpleInterface x = new ExtSimpleImplementation();
            x.Foo();
            AssertTrue(true);
        }

        public void testExtSimpleImplementation2()
        {
            var x = new ExtSimpleImplementation();
            x.Foo();
            AssertTrue(true);
        }

        public void testExtSimpleImplementation3()
        {
            IExtSimpleInterface x = new ExtSimpleImplementation();
            AssertEquals(x.FooInt(), 1);
        }

        public void testExtSimpleImplementation4()
        {
            var x = new ExtSimpleImplementation();
            AssertEquals(x.FooInt(), 1);
        }

        public void testExtExplicitImplementation1()
        {
            ISimpleInterface x = new ExtExplicitImplementation();
            x.Foo();
            AssertTrue(true);
        }

        public void testExtExplicitImplementation2()
        {
            IExtSimpleInterface x = new ExtExplicitImplementation();
            AssertEquals(x.FooInt(), 1);
        }

        public void testVirtualExtSimpleImplementation1()
        {
            IExtSimpleInterface x = new VirtualExtSimpleImplementation();
            AssertEquals(x.FooInt(), 2);
        }

        public void testVirtualExtSimpleImplementation2()
        {
            var x = new VirtualExtSimpleImplementation();
            AssertEquals(x.FooInt(), 2);
        }

        public void testDerivedExtSimpleImplementation1()
        {
            IExtSimpleInterface x = new DerivedExtSimpleImplementation();
            AssertEquals(x.FooInt(), 3);
        }

        public void testDerivedExtSimpleImplementation2()
        {
            var x = new DerivedExtSimpleImplementation();
            AssertEquals(x.FooInt(), 3);
        }

        public void testSimpleAndClashImplementation1()
        {
            ISimpleInterface x = new SimpleAndClashImplementation();
            x.Foo();
            AssertTrue(true);
        }

        public void testSimpleAndClashImplementation2()
        {
            IClashInterface x = new SimpleAndClashImplementation();
            AssertEquals(x.Foo(), 1);
        }

        public void testExtSimpleAndClashImplementation1_1()
        {
            IExtSimpleInterface x = new ExtSimpleAndClashImplementation1();
            AssertEquals(x.FooInt(), 100);
        }

        public void testExtSimpleAndClashImplementation1_2()
        {
            IClashInterface2 x = new ExtSimpleAndClashImplementation1();
            AssertEquals(x.FooInt(), 2);
        }

        public void testExtSimpleAndClashImplementation2_1()
        {
            IExtSimpleInterface x = new ExtSimpleAndClashImplementation2();
            AssertEquals(x.FooInt(), 100);
        }

        public void testExtSimpleAndClashImplementation2_2()
        {
            IClashInterface2 x = new ExtSimpleAndClashImplementation2();
            AssertEquals(x.FooInt(), 2);
        }

        public void testExtSimpleAndClashImplementation3_1()
        {
            IExtSimpleInterface x = new ExtSimpleAndClashImplementation3();
            AssertEquals(x.FooInt(), 3);
        }

        public void testExtSimpleAndClashImplementation3_2()
        {
            IClashInterface2 x = new ExtSimpleAndClashImplementation3();
            AssertEquals(x.FooInt(), 3);
        }

        public void testsimpleViaDerivedImplementation()
        {
            ISimpleInterface x = new DerivedSimpleImplementation();
            x.Foo();
            AssertTrue(true);
        }

    }
}
