using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;

namespace RekenMaar.Activities
{
  public class Preferences
  {
    public Preferences()
    {
      TableOf = 20;
      RandomQuestions = true;
      UpperLimit = 1000;
      CounterMin = 50;
      CounterMax = 100;
    }

    public int TableOf { get; set; }
    public bool RandomQuestions { get; set; }
    public int UpperLimit { get; set; }
    public int CounterMin { get; set; }
    public int CounterMax { get; set; }

    public void StorePreferences(ISharedPreferences spr)
    {
      var prefsEditor = spr.Edit();

      prefsEditor.PutInt("TableOf", TableOf);
      prefsEditor.PutString("strRandomQuestions", RandomQuestions.ToString());
      prefsEditor.PutInt("UpperLimit", UpperLimit);
      prefsEditor.PutInt("CounterMin", CounterMin);
      prefsEditor.PutInt("CounterMax", CounterMax);
      prefsEditor.Commit();
    }

    public bool RetrievePreferences(ISharedPreferences spr)
    {
      var kvps = spr.GetAll();

      if (kvps.Size() > 0)
      {
        TableOf = (int)kvps.Get("TableOf");
        string strRandomQuestions = (string)kvps.Get("strRandomQuestions");
        if (strRandomQuestions == "true")
          RandomQuestions = true;
        else
          RandomQuestions = false;
        UpperLimit = (int)kvps.Get("UpperLimit");
        CounterMin = (int)kvps.Get("CounterMin");
        CounterMax = (int)kvps.Get("CounterMax");
      }
      else
      {
        StorePreferences(spr);
      }

      return kvps.Size() > 0;
    }

  }
}
