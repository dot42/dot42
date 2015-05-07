using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;using Android.Views;using Android.OS;
using SorterenMaar.Games;

namespace SorterenMaar
{
  public class FragmentClass : Fragment
  {
    ITask game;

    public FragmentClass()
    {
      // Empty constructor needed for changing orientation?????
//      FATAL EXCEPTION: main
//java.lang.RuntimeException: Unable to start activity ComponentInfo{com.SorterenMaar/com.SorterenMaar.MainActivity}: android.app.Fragment$InstantiationException: Unable to instantiate fragment com.SorterenMaar.FragmentClass: make sure class name exists, is public, and has an empty constructor that is public
//  at android.app.ActivityThread.performLaunchActivity(ActivityThread.java:1956)
//  at android.app.ActivityThread.handleLaunchActivity(ActivityThread.java:1981)
//  at android.app.ActivityThread.handleRelaunchActivity(ActivityThread.java:3351)
//  at android.app.ActivityThread.access$700(ActivityThread.java:123)
//  at android.app.ActivityThread$H.handleMessage(ActivityThread.java:1151)
//  at android.os.Handler.dispatchMessage(Handler.java:99)
//  at android.os.Looper.loop(Looper.java:137)
//  at android.app.ActivityThread.main(ActivityThread.java:4424)
//  at java.lang.reflect.Method.invokeNative(Native Method)
//  at java.lang.reflect.Method.invoke(Method.java:511)
//  at com.android.internal.os.ZygoteInit$MethodAndArgsCaller.run(ZygoteInit.java:784)
//  at com.android.internal.os.ZygoteInit.main(ZygoteInit.java:551)
//  at dalvik.system.NativeStart.main(Native Method)
//Caused by: android.app.Fragment$InstantiationException: Unable to instantiate fragment com.SorterenMaar.FragmentClass: make sure class name exists, is public, and has an empty constructor that is public
//  at android.app.Fragment.instantiate(Fragment.java:585)
//  at android.app.FragmentState.instantiate(Fragment.java:96)
//  at android.app.FragmentManagerImpl.restoreAllState(FragmentManager.java:1682)
//  at android.app.Activity.onCreate(Activity.java:861)
//  at com.SorterenMaar.MainActivity.onCreate(D:\Programming\dot42\SorterenMaar\SorterenMaar\MainActivity.cs:24)
//  at android.app.Activity.performCreate(Activity.java:4465)
//  at android.app.Instrumentation.callActivityOnCreate(Instrumentation.java:1049)
//  at android.app.ActivityThread.performLaunchActivity(ActivityThread.java:1920)
//  ... 12 more
//Caused by: java.lang.InstantiationException: can't instantiate class com.SorterenMaar.FragmentClass; no empty constructor
//  at java.lang.Class.newInstanceImpl(Native Method)
//  at java.lang.Class.newInstance(Class.java:1319)
//  at android.app.Fragment.instantiate(Fragment.java:574)
//  ... 19 more

    }

    public FragmentClass(ITask _game)
    {
      game = _game;
    }

    public override void OnCreate(Bundle savedInstance)
    {
      base.OnCreate(savedInstance);
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle icicle)
    {
      // If recreated on screen orientation changed, game == null, check for that
      View v = null;
      if (game != null)
      {
        v = game.CreateView(inflater, container);
      }
      return v;
    }


  }
}
