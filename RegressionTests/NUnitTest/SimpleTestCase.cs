using Dot42;
using Android.Test.Suitebuilder;
using NUnit.Framework;
using System;

namespace Dot42.Tests
{
	[TestFixture]
    public class SimpleTestCase 
    {
        [Test]
        public void MyTst1()
        {
            Assert.IsTrue(true);
        }

        [Test]
		[ExpectedException(typeof(NotImplementedException))]
        public void MyTst2()
        {
			throw new NotImplementedException();
        }
	}
}
