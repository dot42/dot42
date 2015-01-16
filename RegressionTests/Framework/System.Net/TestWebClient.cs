using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.Test;
using Android.Util;
using Java.Lang;
using Java.Util.Concurrent;
using Junit.Framework;
using Uri = System.Uri;

namespace Dot42.Tests.System.Net
{
    class TestWebClient : AndroidTestCase
    {
        /*private static bool wifiWasDisabled;

        protected override void SetUp()
        {
            base.SetUp();
            EnableWifi();
        }*/

        public void testDownloadString()
        {
            var client = new WebClient();
            var result = client.DownloadString("https://www.google.com");
            Log.D("WebClient", string.Format("DownloadString: Length={0}, Prefix={1}", result.Length, result.Substring(0, 200)));
            AssertTrue(result.Length > 200);
            AssertTrue(result.IndexOf("<html") >= 0);
        }

        public void testDownloadStringAsync()
        {
            var client = new WebClient();
            var result = client.DownloadStringTaskAsync("https://www.google.com").Result;
            Log.D("WebClient", string.Format("DownloadStringAsync: Length={0}, Prefix={1}", result.Length, result.Substring(0, 200)));
            AssertTrue(result.Length > 200);
            AssertTrue(result.IndexOf("<html") >= 0);
        }
    }
}
