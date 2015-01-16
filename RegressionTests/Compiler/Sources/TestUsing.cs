using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestUsing : TestCase
    {
        public void test1()
        {
            var d = new MyDisposable();
            var r = disp(d);

            AssertEquals("Result not correct", 0, r);
            AssertTrue("Disposed not called", d.disposed);
        }

        public void test2()
        {
            var d = new MyDisposable();
            var r = disp2(d);

            AssertEquals("Result not correct", 0, r);
            AssertTrue("Disposed not called", d.disposed);
            AssertTrue("Finally not called", d.finallyVisited);
        }

        public void test3()
        {
            var d = new MyDisposable();
            var d2 = new MyDisposable();
            var r = disp3(d, d2);

            AssertEquals("Result not correct", 0, r);
            AssertTrue("Disposed not called", d.disposed);
            AssertTrue("Disposed (2) not called", d2.disposed);
            AssertTrue("Finally not called", d.finallyVisited);
        }

        public void _test4()
        {
            var d = new MyDisposable();
            var r = disp4(d);

            AssertEquals("Result not correct", 0, r);
            AssertTrue("Disposed not called", d.disposed);
            AssertTrue("Finally not called", d.finallyVisited);
        }

        public void _test5()
        {
            var d = new MyDisposable();
            var d2 = new MyDisposable();
            var r = disp5(d, d2);

            AssertEquals("Result not correct", 0, r);
            AssertTrue("Disposed not called", d.disposed);
            AssertTrue("Disposed (2) not called", d2.disposed);
            AssertTrue("Finally not called", d.finallyVisited);
        }

        private int disp(MyDisposable d)
        {
            using (d)
            {
                if (d != null)
                    return 0;

                return 1;
            }
        }

        private int disp2(MyDisposable d)
        {
			try 
			{
				using (d)
				{
					if (d != null)
						return 0;

					return 1;
				}
			} 
			finally
			{
				d.finallyVisited = true;
			}
        }

        private int disp3(MyDisposable d, MyDisposable d2)
        {
			using (d)
			{
				try {
					if (d != null)
						return 0;

					return 1;
				} 
				finally 
				{
					d.finallyVisited = true;
					using (d2)
					{
						d2.finallyVisited = true;
					}
				}
			}
        }
       
        private int disp4(MyDisposable d)
        {
			try
			{
				d.finallyVisited = false;
				using (d)
				{
					throw new ArgumentException();
				}
				return 1;
			}
			catch (ArgumentException)
			{
				return 0;
			}
			finally 
			{
				d.finallyVisited = true;
			}
        }

        private int disp5(MyDisposable d, MyDisposable d2)
        {
			try
			{
				using (d)
				{
					throw new Exception();
				}
			}
			catch (Exception)
			{
				using (d2)
				{
					try {
						if (d2 != null)
							return 0;

						return 1;
					} 
					finally 
					{
						d2.finallyVisited = true;
					}
				}
			}
			finally 
			{
				d.finallyVisited = true;
			}
        }
    }

     internal class MyDisposable: IDisposable
     {
         public bool disposed = false;
         public bool finallyVisited = false;

         public void Dispose()
         {
             disposed = true;
         }
     }
}
