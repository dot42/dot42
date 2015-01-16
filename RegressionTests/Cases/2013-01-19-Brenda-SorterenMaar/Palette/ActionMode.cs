using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.View;
using Android.App;
using Android.Widget;

namespace SorterenMaar.Palette
{
  public class ActionModeHandler : ActionMode.ICallback, View.IOnLongClickListener
  {
    ActionMode actionMode = null;
    Activity mainActivity = null;

    public ActionModeHandler(Activity activity)
    {
      mainActivity = activity;
    }

    public bool OnLongClick(View v)
    {
      if (actionMode != null)
        return false;
      else
        actionMode = mainActivity.StartActionMode(this);
      return true;
    }

    /** Invoked whenever the action mode is shown. This is invoked immediately after onCreateActionMode */
    public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
    {
      return false;
    }

    /** Called when user exits action mode */
    public void OnDestroyActionMode(ActionMode mode)
    {
      actionMode = null;
    }

    /** This is called when the action mode is created. This is called by startActionMode() */
    public bool OnCreateActionMode(ActionMode mode, IMenu menu)
    {
      //mode.SetTitle("Demo");
      mainActivity.GetMenuInflater().Inflate(R.Menus.OptionsMenu, menu);
      return true;
    }

    /** This is called when an item in the context menu is selected */
    public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
    {
      switch (item.GetItemId())
      {
        case R.Ids.submenu:
          Toast.MakeText(mainActivity.GetBaseContext(), "Selected Action1 ", Toast.LENGTH_LONG).Show();
          mode.Finish();    // Automatically exists the action mode, when the user selects this action
          break;
      }
      return false;
    }
  };

}
