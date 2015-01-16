using System;
using Dot42;
using Dot42.Manifest;

using Java.Util;

using Android.Support.V4.App;
using Android.View;
using Android.Widget;
using Android.App;
using Android.Os;
using Android.Content;

[assembly: Application("dot42 Google Plus Client", Icon = "Icon")]

namespace GooglePlusClient
{
   // Displays examples of integrating with the Google+ Platform for Android
   [ActivityAttribute]
   public class PlusSampleActivity : ListActivity
   {
      private const String FROM_TITLE = "title";
      private const String TITLE_KEY = "title";
      private const String INTENT_KEY = "intent";

      private static readonly IMap<String, String> SAMPLES_MAP;

      static PlusSampleActivity()
      {
         SAMPLES_MAP = new LinkedHashMap<String, String>();
         SAMPLES_MAP.Put("Sign in", typeof(SignInActivity).FullName);
         SAMPLES_MAP.Put("+1", typeof(PlusOneActivity).FullName);
         SAMPLES_MAP.Put("Send interactive post", typeof(ShareActivity).FullName);
         SAMPLES_MAP.Put("Write moments", typeof(MomentActivity).FullName);
         SAMPLES_MAP.Put("List & remove moments", typeof(ListMomentsActivity).FullName);
         SAMPLES_MAP.Put("List people (circled by you)", typeof(ListPeopleActivity).FullName);
         SAMPLES_MAP.Put("License info", typeof(LicenseActivity).FullName);
      }

      override protected void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);
         SetListAdapter(new SimpleAdapter(this, getSamples(),
                 R.Layouts.main_list_item, new String[] { FROM_TITLE },
                 new int[] { Android.R.Id.Text1 }));
      }

      override public bool OnCreateOptionsMenu(IMenu menu)
      {
         GetMenuInflater().Inflate(R.Menus.main_activity_menu, menu);
         return true;
      }

      override public bool OnOptionsItemSelected(IMenuItem item)
      {
         int itemId = item.GetItemId();
         if (itemId == R.Ids.change_locale)
         {
            Intent intent = new Intent(Intent.ACTION_MAIN);
            intent.SetAction(Android.Provider.Settings.ACTION_LOCALE_SETTINGS);
            intent.AddCategory(Intent.CATEGORY_DEFAULT);
            StartActivity(intent);
            return true;
         }
         return base.OnOptionsItemSelected(item);
      }

      protected IList<IMap<String, Object>> getSamples()
      {
         ArrayList<IMap<String, Object>> samples = new ArrayList<IMap<String, Object>>();
         foreach (IMap_IEntry<String, String> sample in SAMPLES_MAP.EntrySet().AsEnumerable<IMap_IEntry<String, String>>())
         {
            Intent sampleIntent = new Intent(Intent.ACTION_MAIN);
            sampleIntent.SetClassName(GetApplicationContext(), sample.GetValue());
            addItem(samples, sample.GetKey(), sampleIntent);
         }
         return samples;
      }

      private void addItem(IList<IMap<String, Object>> data, String title, Intent intent)
      {
         HashMap<String, Object> temp = new HashMap<String, Object>();
         temp.Put(TITLE_KEY, title);
         temp.Put(INTENT_KEY, intent);
         data.Add(temp);
      }

      override protected void OnListItemClick(ListView listView, View view, int position, long id)
      {
         IMap<String, Object> map = (IMap<String, Object>)listView.GetItemAtPosition(position);
         Intent intent = (Intent)map.Get(INTENT_KEY);
         StartActivity(intent);
      }
   }
}
