using System.ComponentModel;
using System.Text;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;

namespace AirportInfo
{
    /// <summary>
    /// View the status of a specific airport.
    /// </summary>
    [Activity(VisibleInLauncher = false)]
    public class StatusActivity : Activity
    {
        private string code;
        private BackgroundWorker worker;

        /// <summary>
        /// Initialize activity
        /// </summary>
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.StatusLayout);

            worker = new BackgroundWorker();
            worker.DoWork += OnDoWork;
            worker.RunWorkerCompleted += OnRunWorkerCompleted;
            code = GetIntent().GetStringExtra("code");
            Refresh();
        }

        /// <summary>
        /// Refresh the status
        /// </summary>
        private void Refresh()
        {
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Perform the status request (on a background thread)
        /// </summary>
        private void OnDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = AirportService.GetStatus(code);
        }

        /// <summary>
        /// Process the result
        /// </summary>
        private void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var info = FindViewById<TextView>(R.Ids.airportInfo);
            if (e.Error != null)
            {
                info.Text = "Failed to get status because " + e.Error.Message;
            }
            else if (e.Result == null)
            {
                info.Text = "Failed to get status from webservice";                
            }
            else
            {
                var status = (AirportStatus) e.Result;
                var builder = new StringBuilder();
                builder.Append(string.Format("Name: {0}\n", status.Name));
                builder.Append(string.Format("State: {0}\n", status.State));
                builder.Append(string.Format("City: {0}\n", status.City));
                builder.Append("\nWeather\n");
                builder.Append(string.Format("Temp: {0}\n", status.Temperature));
                builder.Append(string.Format("Wind: {0}\n", status.Wind));

                info.Text = builder.ToString();
            }
        }
    }
}
