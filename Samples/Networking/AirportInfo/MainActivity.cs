using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;
using Java.Lang;
using Exception = System.Exception;

[assembly: Application("Airport Information", Icon = "Icon")]
[assembly: UsesPermission(Android.Manifest.Permission.INTERNET)]

namespace AirportInfo
{
    [Activity]
    public class MainActivity : Activity
    {
        private static readonly string[] AirportCodes = new[] { "AMA", "ASE", "BOS", "CLE", "CVG", "DAY", "DCA", "EYW", "IAH", "IND", "LAS", 
            "MKE", "MSY", "OAK", "ORD", "PDX", "PNS", "SFO", "SNA", "TRI" };

        private CodeAndName[] airports;
        private ListView codeList;

        /// <summary>
        /// Initialize activity
        /// </summary>
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);
            airports = AirportCodes.Select(x => new CodeAndName(x)).ToArray();

            codeList = FindViewById<ListView>(R.Ids.codeList);
            codeList.Adapter = new ArrayAdapter<CodeAndName>(this, Android.R.Layout.Simple_list_item_1, airports);
            codeList.ItemClick += OnAirportClick;

            // Now get the names of all airports
            var worker = new BackgroundWorker();
            worker.DoWork += OnGetNames;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Lookup all airport names
        /// </summary>
        private void OnGetNames(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var updater = new ListViewUpdater(codeList);
            foreach (var entry in airports)
            {
                try
                {
                    var status = AirportService.GetStatus(entry.Code);
                    if (status != null)
                    {
                        entry.Name = status.Name;
                        updater.Post();                        
                    }
                }
                catch (Exception ex)
                {
                    // Ignore
                }
            }
        }

        /// <summary>
        /// Show status of an airport.
        /// </summary>
        private void OnAirportClick(object sender, ItemClickEventArgs e)
        {
            var intent = new Intent(this, typeof (StatusActivity));
            intent.PutExtra("code", airports[e.Position].Code);
            StartActivity(intent);
        }

        /// <summary>
        /// Store airport code and (optional name)
        /// </summary>
        private class CodeAndName
        {
            public readonly string Code;
            public string Name { get; set; }

            public CodeAndName(string code)
            {
                Code = code;
            }

            public override string ToString()
            {
                if (Name != null)
                    return string.Format("{0} ({1})", Name, Code);
                return Code;
            }
        }

        /// <summary>
        /// Helper used to update the listview.
        /// </summary>
        private class ListViewUpdater : IRunnable
        {
            private readonly ListView listView;

            public ListViewUpdater(ListView listView)
            {
                this.listView = listView;
            }

            /// <summary>
            /// Post an update request
            /// </summary>
            public void Post()
            {
                listView.Post(this);
            }

            /// <summary>
            /// Update now
            /// </summary>
            public void Run()
            {
                listView.InvalidateViews();
            }
        }
    }
}
