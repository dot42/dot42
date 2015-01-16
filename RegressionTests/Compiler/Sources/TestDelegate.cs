using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestDelegate : TestCase
    {
        internal enum JP2Error
        {
            A, B
        }

		public delegate void MyDelegate(int x);
		public delegate int MyIntDelegate(int x, long y);
		public delegate string MyStringDelegate(string x);
        internal delegate JP2Error Jp2Delegate(Byte[] pucData, Int16 sComponent, UInt32 ulRow, UInt32 ulStart, UInt32 ulNum, object lInputParam);

        public void testAction()
        {
            System.Action action = new System.Action(RunAction);
            action();
        }

        private void RunAction()
        {
        }
		
        public void testSimple1()
        {
			var action = new MyDelegate(MyFoo);
			action(12);
        }
		
        public void testIntDelegate1()
        {
			var action = new MyIntDelegate(MyIntFoo);
			var r = action(12, 34);
			AssertEquals(r, 17);
        }
		
        public void testString1()
        {
			var action = new MyStringDelegate(MyStringFoo);
			var r = action("x");
			AssertEquals(r, "Hello");
        }
		
        public void testJP2Delegate()
        {
            var action = new Jp2Delegate(Jp2Funct);
            var r = action(null, 0, 0, 0, 0, null);
            AssertEquals(r, JP2Error.A);
        }
		
		public void testAsyncCallback()
		{
			var myCallback = new AsyncCallback(MyAsyncCallback);
			AssertNotNull(myCallback);
		}

        public void testColorDelegate()
        {
            var d = new GetColorDelegate(Target);
        }

        private TBColor Target(float f, float f1)
        {
            throw new NotImplementedException();
        }

        public void testColorOrder()
        {
            var color = new TBColor();
            var helper = new ColorHelper();
            var result =  helper.GetColor(color);
        }

        private void MyAsyncCallback(IAsyncResult ar) 
		{
		}

		private void MyFoo(int x) {
			AssertEquals(x, 12);
		}

		private int MyIntFoo(int x, long y) {
			return x  + 5;
		}

		private string MyStringFoo(string x) {
			return "Hello";
		}

        private JP2Error Jp2Funct(Byte[] pucData, Int16 sComponent, UInt32 ulRow, UInt32 ulStart, UInt32 ulNum, object lInputParam)
        {
            return JP2Error.A;
        }

        public delegate TBColor GetColorDelegate(float x, float y);

        public class ColorHelper
        {
            public delegate TBColor ColorantConversionDelegate(float value);

            public ColorantConversionDelegate GetColor(TBColor color)
            {
                return null;
            }
        }

        public struct TBColor
        {
            public byte C1;
        }

        
    }
}
