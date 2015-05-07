using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Dot42.Manifest;

namespace RekenMaar.Activities
{
  [Activity]
  class SettingsActivity : Activity
  {
    TextView txtV;
    TextView txtV1;
    SeekBar seekbar;
    Switch swV;
    RadioButton rbtnTo10;
    RadioButton rbtnTo50;
    RadioButton rbtnTo100;
    RadioButton rbtnTo500;
    RadioButton rbtnTo1000;
    RadioButton rbtn0To10;
    RadioButton rbtn10To20;
    RadioButton rbtn20To50;
    RadioButton rbtn50To100;
    
    
    Preferences gamePreferences;

    protected override void OnCreate(Bundle savedInstance)
    {
      base.OnCreate(savedInstance);
      SetContentView(R.Layout.SettingActivityLayout);

      ActionBar actionBar = GetActionBar();
      // To enable the icon for up navigation (which displays the "up" indicator next to the icon)
      actionBar.SetDisplayHomeAsUpEnabled(true);


      gamePreferences = new Preferences();
      gamePreferences.RetrievePreferences(GetSharedPreferences("GamePreferences", MODE_MULTI_PROCESS));

      txtV = FindViewById<TextView>(R.Id.tableChoosen);
      txtV1 = FindViewById<TextView>(R.Id.tableChoosenTxt);

      seekbar = FindViewById<SeekBar>(R.Id.seekbar);
      seekbar.ProgressChanged += new EventHandler<ProgressChangedEventArgs>(seekbar_ProgressChanged);
      seekbar.SetProgress(gamePreferences.TableOf);

      var btn = FindViewById<Button>(R.Id.Ok);
      btn.Click += new EventHandler(btn_ClickOk);
      btn = FindViewById<Button>(R.Id.Cancel);
      btn.Click += new EventHandler(btn_ClickCancel);

      swV = FindViewById<Switch>(R.Id.isRandom);
      swV.CheckedChanged += new EventHandler<CheckedChangedEventArgs>(swV_CheckedChanged);
      swV.SetChecked(!gamePreferences.RandomQuestions);

      rbtnTo10 = FindViewById<RadioButton>(R.Id.To10);
      if (gamePreferences.UpperLimit == 10)
      {
        rbtnTo10.SetChecked(true);
      }
      rbtnTo50 = FindViewById<RadioButton>(R.Id.To50);
      if (gamePreferences.UpperLimit == 50)
      {
        rbtnTo50.SetChecked(true);
      }
      rbtnTo100 = FindViewById<RadioButton>(R.Id.To100);
      if (gamePreferences.UpperLimit == 100)
      {
        rbtnTo100.SetChecked(true);
      }
      rbtnTo500 = FindViewById<RadioButton>(R.Id.To500);
      if (gamePreferences.UpperLimit == 500)
      {
        rbtnTo500.SetChecked(true);
      }
      rbtnTo1000 = FindViewById<RadioButton>(R.Id.To1000);
      if (gamePreferences.UpperLimit == 1000)
      {
        rbtnTo1000.SetChecked(true);
      }
      rbtnTo10.CheckedChanged += new EventHandler<CheckedChangedEventArgs>(rbtnTo_CheckedChanged);
      rbtnTo50.CheckedChanged += new EventHandler<CheckedChangedEventArgs>(rbtnTo_CheckedChanged);
      rbtnTo100.CheckedChanged += new EventHandler<CheckedChangedEventArgs>(rbtnTo_CheckedChanged);
      rbtnTo500.CheckedChanged += new EventHandler<CheckedChangedEventArgs>(rbtnTo_CheckedChanged);
      rbtnTo1000.CheckedChanged += new EventHandler<CheckedChangedEventArgs>(rbtnTo_CheckedChanged);

      rbtn0To10 = FindViewById<RadioButton>(R.Id.With0To10);
      if (gamePreferences.CounterMin == 0)
      {
        rbtn0To10.SetChecked(true);
      }
      rbtn10To20 = FindViewById<RadioButton>(R.Id.With10To20);
      if (gamePreferences.CounterMin == 10)
      {
        rbtn10To20.SetChecked(true);
      }
      rbtn20To50 = FindViewById<RadioButton>(R.Id.With20To50);
      if (gamePreferences.CounterMin == 20)
      {
        rbtn20To50.SetChecked(true);
      }
      rbtn50To100 = FindViewById<RadioButton>(R.Id.With50To100);
      if (gamePreferences.CounterMin == 50)
      {
        rbtn50To100.SetChecked(true);
      }

      EnabledButtons();
    }

    void rbtnTo_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
      EnabledButtons();
    }


    void swV_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
      if (!e.IsChecked)
      {
        txtV1.Text = "(Deel)tafel van : ";
      }
      else
      {
        txtV1.Text = "(Deel)tafels tot : ";
      }
    }

    void btn_ClickOk(object sender, EventArgs e)
    {
      gamePreferences.TableOf = seekbar.GetProgress();
      gamePreferences.RandomQuestions = !swV.IsChecked();
      if (rbtnTo10.IsChecked())
      {
          gamePreferences.UpperLimit = 10;
      }
      else if (rbtnTo50.IsChecked())
      {
          gamePreferences.UpperLimit = 50;
      }
      else if (rbtnTo100.IsChecked())
      {
        gamePreferences.UpperLimit = 100;
      }
      else if (rbtnTo500.IsChecked())
      {
          gamePreferences.UpperLimit = 500;
      }
      else if (rbtnTo1000.IsChecked())
      {
        gamePreferences.UpperLimit = 1000;
      }

      if (rbtn0To10.IsChecked())
      {
        gamePreferences.CounterMin = 0;
        gamePreferences.CounterMax = 10;
      }
      if (rbtn10To20.IsChecked())
      {
        gamePreferences.CounterMin = 10;
        gamePreferences.CounterMax = 20;
      }
      if (rbtn20To50.IsChecked())
      {
        gamePreferences.CounterMin = 20;
        gamePreferences.CounterMax = 50;
      }
      if (rbtn50To100.IsChecked())
      {
        gamePreferences.CounterMin = 50;
        gamePreferences.CounterMax = 100;
      }
      gamePreferences.StorePreferences(GetSharedPreferences("GamePreferences", MODE_MULTI_PROCESS));
      SetResult(RESULT_OK);
      Finish();
    }


    void btn_ClickCancel(object sender, EventArgs e)
    {
      SetResult(RESULT_CANCELED);
      Finish();
    }

    public override bool OnOptionsItemSelected(Android.View.IMenuItem item)
    {
      bool result = true;

      switch (item.GetItemId())
      {
        //    case android.R.id.home:
        case 1: //Android.R.Id.Home.:
          // app icon in action bar clicked; go home
          var intent = new Intent(this, typeof(MainActivity));
          intent.AddFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
          StartActivity(intent);
          break;
        default:
          result = base.OnOptionsItemSelected(item);
          break;
      }

      {// remove when switch is working
        var intent = new Intent(this, typeof(MainActivity));
        intent.AddFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
        StartActivity(intent);
      }
      return result;
    }

    void seekbar_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      txtV.SetText(e.Progress.ToString());
    }


    private void EnabledButtons()
    {
      if (rbtnTo10.IsChecked())
      {
        // Disable count with radiobutton
        rbtn10To20.SetEnabled(false);
        rbtn20To50.SetEnabled(false);
        rbtn50To100.SetEnabled(false);
        // Set radiobutton checked
        rbtn0To10.SetChecked(true);
      }
      else if (rbtnTo50.IsChecked())
      {
        // Enable count with radiobutton
        rbtn10To20.SetEnabled(true);
        rbtn20To50.SetEnabled(true);
        // Disable count with radiobutton
        rbtn50To100.SetEnabled(false);
        // Set enabled radiobutton checked
        if (!(rbtn0To10.IsChecked() || rbtn10To20.IsChecked() || rbtn20To50.IsChecked()))
        {
          rbtn20To50.SetChecked(true);
        }
      }
      else
      {
        // Enable count with radiobutton
        rbtn10To20.SetEnabled(true);
        rbtn20To50.SetEnabled(true);
        rbtn50To100.SetEnabled(true);
      }
    }
  }
}
