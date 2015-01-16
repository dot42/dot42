using Android.App;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;

[assembly: Application("dot42 SimpleListItem1 sample")]

namespace SimpleListItem1
{
    [Activity(Icon = "Icon", Label = "dot42 Using Simple_list_item_1 !")]
    public class MainActivity : ListActivity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            var titles = new[] { "Title1", "Title2" };
            SetListAdapter(new ArrayAdapter<string>(this, Android.R.Layout.Simple_list_item_1, titles));
        }
    }
}
