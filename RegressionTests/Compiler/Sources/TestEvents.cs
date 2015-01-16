using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestEvents : TestCase
    {
        public void testEventHandler()
        {
            var flag = false;
            var flag2 = false;
            AssertFalse("flag must not yet be set", flag);
            AssertFalse("flag2 must not yet be set", flag2);

            var tester = new EventTester();
            tester.MyEvent += (s, x) => { flag = true; };
            tester.MyEvent += (s, x) => { flag2 = true; };
            tester.OnMyEvent();
			
            AssertTrue("flag must be set", flag);
            AssertTrue("flag2 must be set", flag2);
        }

        public void testEventHandler2()
        {
            var flag = false;
            var flag2 = false;
            AssertFalse("flag must not yet be set", flag);
            AssertFalse("flag2 must not yet be set", flag2);

            var tester = new EventTester();
            EventHandler handler = (s, x) => { flag = true; };
            tester.MyEvent += handler;
            tester.MyEvent += (s, x) => { flag2 = true; };
            tester.MyEvent -= handler;
            tester.OnMyEvent();

            AssertFalse("flag must not be set", flag);
            AssertTrue("flag2 must be set", flag2);
        }

        public void testEventHandler3()
        {
            var flag = false;
            var flag2 = false;
            AssertFalse("flag must not yet be set", flag);
            AssertFalse("flag2 must not yet be set", flag2);

            var tester = new EventTester();
            tester.MyEvent += (s, x) => { flag = true; };
            EventHandler handler = (s, x) => { flag2 = true; };
            tester.MyEvent += handler;
            tester.MyEvent -= handler;
            tester.OnMyEvent();

            AssertTrue("flag must be set", flag);
            AssertFalse("flag2 must not be set", flag2);
        }

        public void testEventHandler4()
        {
            var flag = false;
            var flag2 = false;
            AssertFalse("flag must not yet be set", flag);
            AssertFalse("flag2 must not yet be set", flag2);

            var tester = new EventTester();
            tester.MyEvent += (s, x) => { flag = true; };
            EventHandler handler = (s, x) => { flag2 = true; };
            tester.MyEvent += handler;
            tester.MyEvent += (s, x) => { flag = true; };
            tester.MyEvent -= handler;
            tester.OnMyEvent();

            AssertTrue("flag must be set", flag);
            AssertFalse("flag2 must not be set", flag2);
        }

        public void testEventHandlerT()
        {
            var flag = false;
            var flag2 = false;
            AssertFalse("flag must not yet be set", flag);
            AssertFalse("flag2 must not yet be set", flag2);

            var tester = new EventTester<EventArgs>();
            tester.MyEvent += (s, x) => { flag = true; };
            tester.MyEvent += (s, x) => { flag2 = true; };
            tester.OnMyEvent(EventArgs.Empty);
			
            AssertTrue("flag must be set", flag);
            AssertTrue("flag2 must be set", flag2);
        }

        public void testCustomHandler()
        {
            var flag = false;
            var flag2 = false;
            AssertFalse("flag must not yet be set", flag);
            AssertFalse("flag2 must not yet be set", flag2);

            var tester = new CustomHandlerEventTester();
            tester.MyEvent += () => { flag = true; };
            tester.MyEvent += () => { flag2 = true; };
            tester.OnMyEvent();
			
            AssertTrue("flag must be set", flag);
            AssertTrue("flag2 must be set", flag2);
        }

		private class EventTester
		{
		    public event EventHandler MyEvent;

            public void OnMyEvent()
            {
                if (MyEvent != null)
                {
                    MyEvent(this, EventArgs.Empty);
                }
            }
		}

		private class EventTester<T>
			where T : EventArgs
		{
		    public event EventHandler<T> MyEvent;

            public void OnMyEvent(T e)
            {
                if (MyEvent != null)
                {
                    MyEvent(this, e);
                }
            }
		}
		
		private delegate void CustomHandler();

		private class CustomHandlerEventTester
		{
		    public event CustomHandler MyEvent;

            public void OnMyEvent()
            {
                if (MyEvent != null)
                {
                    MyEvent();
                }
            }
		}
	}
}
