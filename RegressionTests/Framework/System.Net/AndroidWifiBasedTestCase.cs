using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.Test;
using Android.Util;
using Exception = Java.Lang.Exception;
using TimeoutException = Java.Util.Concurrent.TimeoutException;

namespace Dot42.Tests.System.Net
{
    abstract class AndroidWifiBasedTestCase : AndroidTestCase
    {
        private static bool wifiWasDisabled;

        protected override void SetUp()
        {
            base.SetUp();
            EnableWifi();
        }

        protected abstract void RunConnectivityTest();

        private void EnableWifi()
        {
            var context = Context;
            if (context == null) throw new global::Java.Lang.Exception("Cannot get Context");

            var wifiManager = (WifiManager)context.GetSystemService(Context.WIFI_SERVICE);
            if (wifiManager == null) throw new global::Java.Lang.Exception("Cannot get WifiManager");

            var networks = wifiManager.ConfiguredNetworks;
            if ((networks == null) || (networks.Count == 0))
            {
                Log.D("dot42", "No configured WIFI networks.");
                // don't hang if no network is available, e.g. in emulator.
                throw new global::System.Exception("No configured WIFI networks.");
            }

            var connectivityManager = (ConnectivityManager)Context.GetSystemService(Context.CONNECTIVITY_SERVICE);
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

                    if (totalTimeInMs > 60000) throw new global::Java.Util.Concurrent.TimeoutException("Network cannot disconnect within a minute.");
                }
            }

            // Now let's turn it on again
            if (connectivityManager.GetNetworkInfo(1).GetState() != NetworkInfo.State.CONNECTED) //neworkType 1 is Wifi
            {
                if (!wifiManager.IsWifiEnabled)
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

                        if (totalTimeInMs > 60000) throw new global::Java.Util.Concurrent.TimeoutException("Network cannot connect within a minute.");
                    }

                    var retry = true;
                    while (retry)
                    {
                        try
                        {
                            RunConnectivityTest();
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
    }
}
