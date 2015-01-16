using System;

using NUnit.Framework;

namespace Case13
{
    [TestFixture]
    public class ActionHandlerTest
    {
        private event EventHandler<EventArgs> TestEventHandler;
        private EventArgs eventArguments;
        private bool fired;

        [SetUp]
        public void SetupTest()
        {
            this.eventArguments = new EventArgs();
            this.fired = false;
        }

        /// <summary>
        /// Run the code and check if it throws the expected exception.
        /// </summary>
        /// <remarks>
        /// Testing for an exception this way instead of an attribute, gives us the
        /// possibility to test for several exceptions in one test method.
        /// </remarks>
        /// <param name="exception">The expected type of the exception.</param>
        /// <param name="codeToTest">The code to call.</param>
        /// <returns>True if the expected exception was caught, false otherwise.</returns>
        private void TestForException(Type exception, Action codeToTest)
        {
            bool noExceptionCaught = false;
            try
            {
                Console.WriteLine("Calling the event handler...");
                codeToTest();
                noExceptionCaught = true;
            }
            catch (Exception e)
            {
                if (!e.GetType().Equals(exception))
                {
                    Assert.Fail(string.Format("Caught {1} instead of the expected {0}. ", exception.Name, e.GetType().Name));
                }
            }

            if (noExceptionCaught)
            {
                Assert.Fail(string.Format("Expected {0} not caught.", exception.Name));
            }
        }

        [Test]
        public void TestEventHandling()
        {
            // Call the event handler. This should fail, because no event handler is attached.
            this.TestForException(typeof(NullReferenceException), delegate { this.TestEventHandler(this, this.eventArguments); });
            Assert.IsFalse(this.fired);

            // Attach a callback so the handler is not null and calling it will not fail.
            this.TestEventHandler += delegate(object sender, EventArgs e) { };

            // Attach our callback.
            this.TestEventHandler += this.EventHandlerCallBack;

            // Call the event handler. This should call our callback.
            this.TestEventHandler(this, this.eventArguments);
            Assert.IsTrue(this.fired);

            // Detach our callback.
            this.TestEventHandler -= this.EventHandlerCallBack;

            // Call the event handler. This should not call our callback.
            this.TestEventHandler(this, this.eventArguments);
            Assert.IsTrue(this.fired);
        }

        private void EventHandlerCallBack(object sender, EventArgs e)
        {
            this.fired = !this.fired;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
