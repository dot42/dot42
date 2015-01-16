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
    class TestHttpWebRequest : AndroidTestCase
    {
        private static bool wifiWasDisabled;

        protected override void SetUp()
        {
            base.SetUp();
            EnableWifi();
        }

        public void testWebRequestGet()
        {
            var uri = new Uri("http://www.google.com");
            using(var stream  = WebInvoke( uri, Verb.Get, null,true))
            {
                AssertTrue(stream.Length > 100);
            }
        }

        public void testSync()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://www.google.com");
            AssertNotNull("req:If Modified Since: ", req.IfModifiedSince);

            req.UserAgent = "MonoClient v1.0";
            AssertEquals("#A1","User-Agent".ToLower(), req.Headers.GetKey(0));
            AssertEquals("#A2", "MonoClient v1.0", req.Headers.Get(0));

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            AssertEquals("#B1", "OK", res.StatusCode.ToString());
            AssertEquals("#B2", "OK", res.StatusDescription);

            AssertEquals("#C1", "text/html; charset=ISO-8859-1", res.Headers.Get("Content-Type"));
            AssertNotNull("#C2", res.LastModified);
            //Assert.AreEqual(0, res.Cookies.Count, "#C3");

            res.Close();
        }

        private void EnableWifi()
        {
            var context = GetContext();
            if (context == null) throw new Exception("Cannot get Context");

            var wifiManager = (WifiManager) context.GetSystemService(Context.WIFI_SERVICE);
            if (wifiManager == null) throw new Exception("Cannot get WifiManager");

			var networks = wifiManager.GetConfiguredNetworks();
			if ((networks == null) || (networks.Count == 0))
			{
				Log.D("dot42", "No configured WIFI networks");
				return;
			}
			
            var connectivityManager = (ConnectivityManager) GetContext().GetSystemService(Context.CONNECTIVITY_SERVICE);
            if (connectivityManager == null) throw new Exception("Cannot get ConnectivityManager");

            if (!wifiWasDisabled)
            {
                wifiWasDisabled = true;

                // Turn WIFI off first
                wifiManager.SetWifiEnabled(false);

                var totalTimeInMs = 0;
                while (connectivityManager.GetNetworkInfo(1).GetState() != NetworkInfo.State.DISCONNECTED)
                {
                    lock (this)
                    {
                        //wait and retry
                        JavaWait(200);
                        totalTimeInMs += 200;
                    }

                    if (totalTimeInMs > 60000) throw new TimeoutException("Network cannot disconnect within a minute.");
                }
            }

            // Now let's turn it on again
            if (connectivityManager.GetNetworkInfo(1).GetState() != NetworkInfo.State.CONNECTED) //neworkType 1 is Wifi
            {
                if (!wifiManager.IsWifiEnabled())
                {
                    wifiManager.SetWifiEnabled(true);
                    var totalTimeInMs = 0;
                    while (connectivityManager.GetNetworkInfo(1).GetState() != NetworkInfo.State.CONNECTED)
                    {
                        lock (this)
                        {
                            //wait and retry
                            JavaWait(200);
                            totalTimeInMs += 200;
                        }

                        if (totalTimeInMs > 60000) throw new TimeoutException("Network cannot connect within a minute.");
                    }

                    var retry = true;
                    while (retry)
                    {
                        try
                        {
                            testWebRequestGet();
                            retry = false;
                        }
                        catch
                        {
                            lock (this)
                            {
                                //wait and retry
                                JavaWait(200);
                                totalTimeInMs += 200;
                            }
                        }

                        if (totalTimeInMs > 60000) throw new TimeoutException("Network cannot be accessed within a minute.");
                    }
                    
                }
            }
        }

        private enum Verb
        {
            Get,
            Post,
            Put,
            Delete
        }

        private static Stream WebInvoke(Uri uri, Verb verb, Stream input, bool hasOutput)
        {
            var request = WebRequest.Create(uri);
            switch (verb)
            {
                case Verb.Get:
                    request.Method = "GET";
                    break;
                case Verb.Post:
                    request.Method = "POST";
                    break;
                case Verb.Put:
                    request.Method = "PUT";
                    break;
                case Verb.Delete:
                    request.Method = "DELETE";
                    break;
            }

            WebResponse response;

            if (input != null)
            {
                input.Seek(0, SeekOrigin.Begin);

                request.ContentType = "application/xml";
                request.ContentLength = input.Length;

                using (var requestStream = request.GetRequestStream())
                {
                    var bytes = new byte[1024];
                    var read = 1;
                    while (read > 0)
                    {
                        read = input.Read(bytes, 0, bytes.Length);
                        requestStream.Write(bytes, 0, read);
                    }
                }
            }

            using (response = request.GetResponse())
			{
				if (!hasOutput)
					return null;
				var temp = new MemoryStream();
				response.GetResponseStream().CopyTo(temp);
				temp.Seek(0L, SeekOrigin.Begin);
				return temp;
			}
        }
    }
}
