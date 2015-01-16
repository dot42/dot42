/*
 * Copyright (C) 2011 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Android.App;
using Android.Content;
using Android.Content.Pm;
using Android.Os;
using Android.View;
using Android.Widget;

using Java.Lang;
using Java.Text;
using Java.Util;

using Dot42;
using Dot42.Manifest;

[assembly: Application("Support v4 Demos", Icon = "app_sample_code", HardwareAccelerated = true)]
[assembly: UsesPermission("android.permission.READ_CONTACTS")]

namespace com.example.android.supportv4
{
    [Activity]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "android.intent.category.DEFAULT", "android.intent.category.LAUNCHER" })]
    public class Support4Demos : ListActivity {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        
            Intent intent = GetIntent();
            string path = intent.GetStringExtra("com.example.android.apis.Path");
        
            if (path == null) {
                path = "";
            }

            SetListAdapter(new SimpleAdapter(this, GetData(path),
                    global::Android.R.Layout.Simple_list_item_1, new string[] { "title" },
                    new int[] { global::Android.R.Id.Text1 }));
            GetListView().SetTextFilterEnabled(true);
        }

        protected IList<IMap<string, object>> GetData(string prefix) {
            IList<IMap<string, object>> myData = new ArrayList<IMap<string, object>>();

            Intent mainIntent = new Intent(Intent.ACTION_MAIN, null);
            mainIntent.AddCategory("com.example.android.supportv4.SUPPORT4_SAMPLE_CODE");

            PackageManager pm = GetPackageManager();
            IList<ResolveInfo> list = pm.QueryIntentActivities(mainIntent, 0);

            if (null == list)
                return myData;

            string[] prefixPath;
            string prefixWithSlash = prefix;
        
            if (prefix.Equals("")) {
                prefixPath = null;
            } else {
                prefixPath = prefix.Split("/");
                prefixWithSlash = prefix + "/";
            }
        
            int len = list.Size();
        
            IMap<string, bool?> entries = new HashMap<string, bool?>();

            for (int i = 0; i < len; i++) {
                ResolveInfo info = list.Get(i);
                ICharSequence labelSeq = info.LoadLabel(pm);
                string label = labelSeq != null
                        ? labelSeq.ToString()
                        : info.ActivityInfo.Name;
            
                if (prefixWithSlash.Length == 0 || label.StartsWith(prefixWithSlash)) {
                
                    string[] labelPath = label.Split("/");

                    string nextLabel = prefixPath == null ? labelPath[0] : labelPath[prefixPath.Length];

                    if ((prefixPath != null ? prefixPath.Length : 0) == labelPath.Length - 1) {
                        AddItem(myData, nextLabel, ActivityIntent(
                                info.ActivityInfo.ApplicationInfo.PackageName,
                                info.ActivityInfo.Name));
                    } else {
                        if (entries.Get(nextLabel) == null) {
                            AddItem(myData, nextLabel, BrowseIntent(prefix.Equals("") ? nextLabel : prefix + "/" + nextLabel));
                            entries.Put(nextLabel, true);
                        }
                    }
                }
            }

            Collections.Sort(myData, sDisplayNameComparator);
        
            return myData;
        }

        class MyComparator: IComparator<IMap<string, object>>
        {
            private Collator collator = Collator.GetInstance();

            public int Compare(IMap<string, object> map1, IMap<string, object> map2) {
                return collator.Compare(map1.Get("title"), map2.Get("title"));
            }
        }

        private IComparator<IMap<string, object>> sDisplayNameComparator = new MyComparator();
    
        protected Intent ActivityIntent(string pkg, string componentName) {
            Intent result = new Intent();
            result.SetClassName(pkg, componentName);
            return result;
        }
    
        protected Intent BrowseIntent(string path) {
            Intent result = new Intent();
            result.SetClass(this, typeof(Support4Demos));
            result.PutExtra("com.example.android.apis.Path", path);
            return result;
        }

        protected void AddItem(IList<IMap<string, object>> data, string name, Intent intent) {
            IMap<string, object> temp = new HashMap<string, object>();
            temp.Put("title", name);
            temp.Put("intent", intent);
            data.Add(temp);
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id) {
            IMap<string, object> map = (IMap<string, object>)l.GetItemAtPosition(position);

            Intent intent = (Intent) map.Get("intent");
            StartActivity(intent);
        }
    }
}