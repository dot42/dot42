using Android.App;using Android.OS;using Android.Views;
using Android.Widget;
using Dot42.Manifest;

[assembly: Application("dot42 Simple ActionBar", Icon = "Icon")]

namespace SimpleActionBar
{
    [Activity]
    public class MainActivity : Activity
    {
        private TextView tbLog;

        /// <summary>
        /// Activity is created.
        /// </summary>
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layout.MainLayout);
            tbLog = FindViewById<TextView>(R.Id.tbLog);
        }

        /// <summary>
        /// Inflate the menu resource into the given menu.
        /// </summary>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(R.Menu.Menu, menu);
            return true;
        }

        /// <summary>
        /// Menu option has been clicked.
        /// </summary>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case R.Id.item1:
                    tbLog.Text = "item1 has been clicked";
                    break;
                case R.Id.groupItem1:
                    tbLog.Text = "groupItem1 has been clicked";
                    break;
                case R.Id.submenu_item1:
                    tbLog.Text = "submenu_item1 has been clicked";
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }
    }
}
