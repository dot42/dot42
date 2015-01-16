using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Os;
using Android.View;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 Dutch Radio", Icon = "Icon")]
[assembly: UsesPermission("android.permission.INTERNET")]

namespace Dot42Radio
{
    [Activity]
    public class MainActivity : Activity 
    {
        private readonly Station[] UrlList = new[] {
            new Station("Radio 1", "http://icecast.omroep.nl/radio1-bb-mp3"),
            new Station("Radio 2", "http://icecast.omroep.nl/radio2-bb-mp3")
        };
        private MediaPlayer player;

        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            // Initialize list
            var listView = FindViewById<ListView>(R.Ids.playList);
            listView.ItemClick += (s, x) => OnPlay(x.Position);
            listView.ItemLongClick += (s, x) => { OnStopPlayer(); x.IsHandled = true; };
            listView.SetAdapter(new StationAdapter(this, UrlList));
        }

        /// <summary>
        /// Start playing a new station.
        /// </summary>
        private void OnPlay(int position)
        {
            OnStopPlayer();
            try
            {
                var url = UrlList[position].Url;
                var name = UrlList[position].Name;

                player = new MediaPlayer();
                player.SetDataSource(this, Android.Net.Uri.Parse(url));
                player.Prepared += (s, x) => player.Start();
                player.PrepareAsync();
                ShowToast(string.Format("Loading {0}", name));
            }
            catch (Exception ex)
            {
                var msg = string.Format("Playing audio failed because: {0}", ex.Message);
                ShowToast(msg);
            }
        }

        /// <summary>
        /// Stop the player and release it.
        /// </summary>
        private void OnStopPlayer()
        {
            if (player != null)
            {
                player.Stop();
                player.Release();
                player = null;
            }            
        }

        private void ShowToast(string msg)
        {
            Toast.MakeText(this, msg, Toast.LENGTH_SHORT).Show();            
        }

        private class Station
        {
            public readonly string Name;
            public readonly string Url;

            public Station(string name, string url)
            {
                Name = name;
                Url = url;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private class StationAdapter : ArrayAdapter<Station>
        {
            public StationAdapter(Context context, Station[] list)
                : base(context, R.Layouts.PlayItemLayout, list)
            {
            }
        }
    }
}
