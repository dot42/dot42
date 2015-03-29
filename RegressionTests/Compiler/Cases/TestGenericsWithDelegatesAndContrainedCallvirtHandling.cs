using System;
using NUnit.Framework;
using Assert = Junit.Framework.Assert;

namespace Dot42.Tests.Compiler.Cases
{
    [TestFixture]
    internal class TestGenericsWithDelegatesAndContrainedCallvirtHandling
    {
        [Test]
        public void TestNoCallvirt()
        {
            new TestCallOnObject<DateTime>().Test(DateTime.UtcNow);
        }

        [Test]
        public void TestConstraintedCallvirt()
        {
            new TestCallOnTSource1<DateTime>().Test(DateTime.UtcNow);
        }

        [Test]
        public void TestConstrainedCallvirtWithDelegate()
        {
            new TestCallOnTSource2<DateTime>().Test(DateTime.UtcNow);
        }

        [Test]
        public void TestContrainedOnGenericField()
        {
            var a = new Test2<DateTime>(DateTime.UtcNow);
            Assert.AssertEquals(typeof(DateTime), a.element.GetType());
        }

        internal class TestCallOnObject<TSource> 
        {
            public Type Test(TSource source)
            {
                // retrieve source property getter
                var objSource = (object) source;
                var sourceType = objSource.GetType();

                Assert.AssertEquals(typeof (TSource), sourceType);
                return sourceType;
            }
        }

        internal class TestCallOnTSource1<TSource>
        {
            public void Test(TSource source)
            {
                // retrieve source property getter
                var sourceType = source.GetType();

                //Func<object, object> g = obj => obj.ToString();
                //Func<object> getSourceValue = () => g(source);

                if (sourceType != typeof (TSource))
                    Assert.Fail();
            }
        }

        internal class TestCallOnTSource2<TSource>
        {
            public void Test(TSource source)
            {
                // retrieve source property getter
                var sourceType = source.GetType();

                Func<object, object> g = obj => obj.ToString();
                Func<object> getSourceValue = () => g(source);

                if (sourceType != typeof(TSource))
                    Assert.Fail();
            }
        }

        internal class Test2<TSource>
        {
            public TSource element;

            public Test2(TSource val)
            {
                element = val;
            }

           
        }

    }

}
