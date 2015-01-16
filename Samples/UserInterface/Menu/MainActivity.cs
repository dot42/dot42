using Android.App;
using Android.Os;
using Android.View;
using Android.Widget;
using Dot42.Manifest;

[assembly: Application("dot42 Simple Menu", Icon = "Icon")]

namespace SimpleMenu
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
            SetContentView(R.Layouts.MainLayout);
            tbLog = FindViewById<TextView>(R.Ids.tbLog);
        }

        /// <summary>
        /// Inflate the menu resource into the given menu.
        /// </summary>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(R.Menus.Menu, menu);
            return true;
        }

        /// <summary>
        /// Menu option has been clicked.
        /// </summary>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.GetItemId())
            {
                case R.Ids.item1:
                    tbLog.Text = "item1 has been clicked";
                    break;
                case R.Ids.groupItem1:
                    tbLog.Text = "groupItem1 has been clicked";
                    break;
                case R.Ids.submenu_item1:
                    tbLog.Text = "submenu_item1 has been clicked";
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }
    }
}
