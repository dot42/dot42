using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 Using Spinner Sample")]

namespace UsingSpinner
{
    [Activity(Icon = "Icon", Label = "dot42 Using Spinner!")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            var animalSpinner = (Spinner)FindViewById(R.Ids.animalSpinner);
            // Create an ArrayAdapter using the string array and a default spinner layout
            var adapter = ArrayAdapter<string>.CreateFromResource(this, R.Arrays.my_animals, Android.R.Layout.Simple_spinner_item);
            // Specify the layout to use when the list of choices appears
            adapter.SetDropDownViewResource(Android.R.Layout.Simple_spinner_dropdown_item);
            // Apply the adapter to the spinner
            animalSpinner.SetAdapter(adapter);
        }
    }
}
