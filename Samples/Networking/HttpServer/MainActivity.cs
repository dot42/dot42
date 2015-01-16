using System;
using System.Threading;
using Android.App;
using Android.Os;
using Android.Util;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Java.Io;
using Java.Net;
using Java.Util;
using Org.Apache.Http.Conn.Util;

[assembly: Application("dot42 SimpleHttpServer", Icon = "Icon")]

[assembly: UsesPermission(Android.Manifest.Permission.INTERNET)]
[assembly: UsesPermission(Android.Manifest.Permission.ACCESS_NETWORK_STATE)]

namespace SimpleHttpServer
{
    /// <summary>
    /// Activity that starts a very simple server (in a thread) and responds as an "HTTP server".
    /// </summary>
    [Activity]
    public class MainActivity : Activity
    {
        private const int PORT = 8088;
        private bool stop = false;

        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            var thread = new Thread(new ThreadStart(RunServer));
            thread.Start();

            var status = FindViewById<TextView>(R.Ids.status);
            status.Text = string.Format("Connect to http://{0}:{1}", GetIPAddress(), PORT);
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            stop = true;
            var status = FindViewById<TextView>(R.Ids.status);
            status.Text = "Stopped";
        }

        /// <summary>
        /// Run a very simple server.
        /// </summary>
        private void RunServer()
        {
            try
            {
                Log.I("SimpleHttpServer", "Creating server socket");
                var serverSocket = new ServerSocket(PORT);
                var requestCount = 0;
                try
                {
                    while (!stop)
                    {
                        Log.I("SimpleHttpServer", "Waiting for connection");
                        var socket = serverSocket.Accept();

                        var input = new BufferedReader(new InputStreamReader(socket.GetInputStream()));
                        var output = new BufferedWriter(new OutputStreamWriter(socket.GetOutputStream()));

                        string line;
                        Log.I("SimpleHttpServer", "Reading request");
                        while ((line = input.ReadLine()) != null)
                        {
                            Log.I("SimpleHttpServer", "Received: " + line);
                            if (line.Length == 0)
                                break;
                        }

                        Log.I("SimpleHttpServer", "Sending response");
                        output.Write("HTTP/1.1 200 OK\r\n");
                        output.Write("\r\n");
                        output.Write(string.Format("Hello world {0}\r\n", requestCount));
                        output.Flush();

                        socket.Close();
                        requestCount++;
                    }
                }
                finally
                {
                    serverSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Log.E("SimpleHttpServer", "Connection error", ex);
            }
        }

        private string GetIPAddress()
        {
            var list = Collections.List(NetworkInterface.GetNetworkInterfaces());
            foreach (var intf in list.AsEnumerable())
            {
                if (intf.IsLoopback())
                    continue;
                var addresses = Collections.List(intf.GetInetAddresses());
                foreach (var addr in addresses)
                {
                    if (InetAddressUtils.IsIPv4Address(addr.GetHostAddress()))
                        return addr.GetHostAddress();
                }
            }
            return "?";
        }
    }
}
