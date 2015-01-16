using System;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestIDisposable : TestCase
    {
        public void testUsing()
        {
            AssertFalse(MyDisposable.IsCalled);

            using (var myDisp = new MyDisposable())
            {
                AssertNotNull(myDisp);
            }

            AssertTrue(MyDisposable.IsCalled);
        }

        internal class MyDisposable : IDisposable
        {
            public static bool IsCalled;

            //nothing to do here
            public void Dispose()
            {
                IsCalled = true;
            }
        }

    }
}
