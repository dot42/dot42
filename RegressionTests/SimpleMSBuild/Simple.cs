using Android.App;
using Android.Os;
using Android.View;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

[assembly: Application("A Simple Drojd")]

namespace Test.Simple
{
	[Activity]
    public class SimpleActivity : Activity, View.IOnClickListener
    {
		private int field;
		
        protected override void OnCreate(Bundle savedInstance) 
        {
            base.OnCreate(savedInstance);
            //CallFoo(5, 26);
            CallFoo(5, 6);
        }

        public void CallFoo(int y, int i)
        {
            if ((y < i) && (i > 23))
            {
                var tx = new TextView(this);
                tx.Text = "y < i";
                SetContentView(tx);
            } else {
				TestForLoop();
			}
        }

        public void TestForLoop()
        {
            SetContentView(R.Layouts.main);
            for (var i = 0; i < 20; i++)
            {
                AddContentView(new TextView(this) { Text = "hoi"}, new ViewGroup.LayoutParams(1, 1));
            }
        }

		public int Foo(int x) 
		{
			return x + 5;
		}

	    public void OnClick(View obj0)
	    {
            SetContentView(R.Layouts.main);
        }
		
		public void TryFinallyTest() 
		{
			try {
				Foo(0);
			} finally {
				Foo(25);
			}
		}
		
		public void TryCatchTest() 
		{
			try {
				Foo(0);
			} catch (System.Exception ex) {
				Foo(1);
			}
		}
		
		public void TryCatchFinallyTest() 
		{
			try {
				Foo(0);
			} catch (System.Exception ex) {
				Foo(1);
			} finally {
				Foo(25);
			}
		}
		
		public void TryCatchAllFinallyTest() 
		{
			try {
				Foo(0);
			} catch {
				Foo(2);
			} finally {
				Foo(25);
			}
		}
		
		public void TryCatchCatchAllFinallyTest() 
		{
			try {
				Foo(0);
			} catch (System.Exception ex) {
				Foo(1);
			}
			catch {
				Foo(2);
			} finally {
				Foo(25);
			}
		}

		public void NestedTryCatchTest() {
			try {
				Foo(0);
				try {
					Foo(100);
				}catch (Java.Lang.RuntimeException ex) {
					Foo(101);
				}
				Foo(10);
			}catch (System.Exception ex) {
				Foo(1);
			}
		}
    }
}