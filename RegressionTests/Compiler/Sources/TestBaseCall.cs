using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestBaseClass : TestCase
    {
        public void testAdd1()
        {
            var x = new DerivedClass();
            x.Add();
        }

        private class BaseClass
        {
            public virtual void Add()
            {
            }
        }

        private class DerivedClass : BaseClass
        {
            public override void Add()
            {
                Foo();
            }

            public void Foo()
            {
                base.Add();
            }
        }
    }
}
