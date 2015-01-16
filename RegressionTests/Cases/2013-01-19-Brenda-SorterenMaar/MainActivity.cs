using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Android.View;

using Dot42;
using Dot42.Manifest;
using Android.Content;
using SorterenMaar.Palette;

[assembly: Application("SorterenMaar", Icon = "laura_drawing", Theme = "android:style/Theme.Holo.Light")]
//[assembly: Application("SorterenMaar", Icon = "laura_drawing", Theme = "@style/MyTheme")]

namespace SorterenMaar
{
  [Activity]
  public class MainActivity : Activity
  {
    ActionModeHandler actionModeHandler = null;

    protected override void OnCreate(Bundle savedInstance)
    {
      base.OnCreate(savedInstance);
      SetContentView(R.Layouts.MainLayout);

      //actionModeHandler = new ActionModeHandler(this);

      var f = new FragmentClass(new SortTask("res/xml/SortGame.xml", actionModeHandler));
      CreateFragment(R.Ids.GameFrame, f);
    }

    public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenu_IContextMenuInfo menuInfo)
    {
      //GetMenuInflater().Inflate(R.Menus.OptionsMenu, menu);
    }

    private void CreateFragment(int containerId, Fragment fragment)
    {
      // Check that the activity is using the layout version with
      // the fragment_container FrameLayout
      if (FindViewById(containerId) != null)
      {
        // Add the fragment to the 'fragment_container' FrameLayout
        FragmentManager fragmentManager = GetFragmentManager();
        FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();
        fragmentTransaction.Add(containerId, fragment);
        fragmentTransaction.Commit();
      }
    }
  }
}
