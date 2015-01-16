using Dot42;
using Android.Test.Suitebuilder;
using NUnit.Framework;
using System;

namespace Dot42.Tests
{
	[TestFixture]
    public class SetUpTestCase
	{
	    private bool setup;

        [SetUp]
        public void MySetUp()
        {
            setup = true;
        }

        [Test]
        public void MyTest1()
        {
            Assert.IsTrue(setup);
        }
	}
}
