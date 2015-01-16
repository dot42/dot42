using Android.App;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;

[assembly: Application("A Simple Dot42 App")]

namespace test.Simple
{
	[Activity]
    public class SimpleActivity : Activity
    {
		private int field;
		
        protected override void OnCreate(Bundle savedInstance) 
        {
            base.OnCreate(savedInstance);
			var tv = new TextView(this);
			tv.SetText("Hello Dot42");
			SetContentView(tv);
        }

        public void CallFoo(int y, int i)
        {
            if ((y > 13) && (i < 45))
            {
                Foo(y);
            }
        }

		public int Foo(int x) 
		{
			return x + 5;
		}
    }
}