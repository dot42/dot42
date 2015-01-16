using System.Linq;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;

namespace FileBrowser
{
    [Activity(VisibleInLauncher = false)]
    public class OpenFileActivity : Activity
    {
        public const string StartPath = "StartPath";
        public const string ResultPath = "ResultPath";
        public const string Root = "/";

        private Java.Io.File[] entries;
        private string startPath;

        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetResult(RESULT_CANCELED);
            SetContentView(R.Layouts.OpenFileLayout);

            var intent = GetIntent();
            var list = FindViewById<ListView>(R.Ids.list);
            list.ItemClick += OnItemClick;

            startPath = intent.GetStringExtra(StartPath) ?? Root;
            SetPath(startPath);
        }

        void OnItemClick(object sender, ItemClickEventArgs e)
        {
            var entry = entries[e.Position];
            if (entry.IsDirectory())
            {
                SetPath(entry);
            }
            else
            {
                var intent = GetIntent();
                intent.PutExtra(ResultPath, entry);
                SetResult(RESULT_OK, intent);
                Finish();
            }
        }

        private void SetPath(string path)
        {
            var lbPath = FindViewById<TextView>(R.Ids.lbPath);
            lbPath.Text = path;

            entries = GetEntries(path);
            var names = entries.Select(x => x.IsDirectory() ? "/" + x.Name : x.Name).ToArray();

            var list = FindViewById<ListView>(R.Ids.list);
            list.SetAdapter(new ArrayAdapter<string>(this, R.Layouts.OpenFileRowLayout, names));
            
        }

        private Java.Io.File[] GetEntries(string path)
        {
            var dir = new Java.Io.File(path);
            if (dir.Exists() && dir.IsDirectory())
            {
                var entries = dir.ListFiles();
                if (entries != null)
                {
                    var list = entries.OrderBy(x => x.Name).ToList();
                    if (path != startPath)
                    {
                        var parent = dir.ParentFile;
                        if (parent != null)
                        {
                            list.Insert(0, parent);
                        }
                    }
                    return list.ToArray();
                }
            }
            return new Java.Io.File[0];
        }
    }
}
