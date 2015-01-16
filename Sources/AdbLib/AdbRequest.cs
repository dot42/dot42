using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Dot42.DeviceLib;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Helper for communicating with ADB over a socket connection.
    /// </summary>
    internal class AdbRequest : IDisposable
    {
        internal static readonly Encoding DefaultEncoding = Encoding.GetEncoding("ISO-8859-1");
        private readonly TcpClient tcpClient;

        /// <summary>
        /// Response data
        /// </summary>
        protected class AdbResponse
        {
            /// <summary>
            /// First 4 bytes in response were "OKAY"?
            /// </summary>
            public bool Okay; 

            /// <summary>
            /// Diagnostic string if #okay is false
            /// </summary>
            public string Message = string.Empty;  
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AdbRequest(IPEndPoint endPoint)
        {
            tcpClient = new TcpClient { ExclusiveAddressUse = false };
            tcpClient.Connect(endPoint);
        }

        /// <summary>
        /// Close the connection.
        /// </summary>
        public void Close()
        {
            try
            {
                tcpClient.Close();
            }
            catch
            {
                // Ignore
            }
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        void IDisposable.Dispose()
        {
            Close();
        }

        /// <summary>
        /// Execute a shell command.
        /// </summary>
        internal void ExecuteShellCommand(IShellOutputReceiver receiver, IDevice device, int timeout, string command, params string[] args)
        {
            // Connect to the given device
            SetDevice(device);

            // Send request
            var req = new StringBuilder();
            req.Append("shell:");
            req.Append(Quote(command));
            foreach (var arg in args)
            {
                req.Append(' ');
                req.Append(Quote(arg));
            }
            Write(FormatAdbRequest(req.ToString()));

            // Read response
            var resp = ReadAdbResponse(false);
            if (!resp.Okay)
            {
                throw new AdbCommandRejectedException(resp.Message);
            }

            var data = new byte[16384];
            var start = DateTime.Now;
            while (true)
            {
                if (receiver != null && receiver.IsCancelled)
                {
                    break;
                }

                var count = tcpClient.GetStream().Read(data, 0, data.Length);
                if (count == 0)
                {
                    // we're at the end, we flush the output
                    if (receiver != null) 
                        receiver.Completed();
                    break;
                }
                // send data to receiver if present
                if (receiver != null)
                {
                    receiver.AddOutput(data, 0, count);
                }
            }
        }

        /// <summary>
        /// Keep reading log messages until cancelled by the listener or closed.
        /// </summary>
        internal void RunLogService(ILogListener listener, IDevice device, string logName)
        {
            // Connect to the given device
            SetDevice(device);

            // Send request
            Write(FormatAdbRequest("log:" + logName));

            // Read response
            var resp = ReadAdbResponse(false);
            if (!resp.Okay)
            {
                throw new AdbCommandRejectedException(resp.Message);
            }

            var data = new byte[16384];
            var receiver = new LogOutputReceiver(listener);
            while (true)
            {
                if (receiver.IsCancelled)
                {
                    break;
                }

                var count = tcpClient.GetStream().Read(data, 0, data.Length);
                if (count == 0)
                {
                    // we're at the end, we flush the output
                    break;
                }
                // send data to receiver 
                receiver.AddOutput(data, 0, count);
            }
        }


        /// <summary>
        /// Connect jdwp to local post.
        /// </summary>
        internal void ForwardJdwp(IDevice device, int localPort, int pid)
        {
            // Create command
            var command = string.Format("host-serial:{0}:forward:tcp:{1};jdwp:{2}", device.Serial, localPort, pid);

            // Send request
            Write(FormatAdbRequest(command));

            // Read response
            var resp = ReadAdbResponse(false);
            if (!resp.Okay)
            {
                throw new AdbCommandRejectedException(resp.Message);
            }
        }


        /// <summary>
        /// Add double quotes if needed.
        /// </summary>
        private static string Quote(string value)
        {
            if (value.IndexOf(' ') >= 0)
                return '\"' + value + '\"';
            return value;
        }

        /// <summary>
        /// tells adb to talk to a specific device
        /// </summary>
        /// <param name="device"> The device to talk to. </param>
        /// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
        /// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
        /// <exception cref="IOException"> in case of I/O error on the connection. </exception>
        protected void SetDevice(IDevice device)
        {
            // if the device is not -1, then we first tell adb we're looking to talk
            // to a specific device
            if (device == null)
                return;
            var msg = "host:transport:" + device.Serial;
            var req = FormatAdbRequest(msg);
            Write(req);

            var resp = ReadAdbResponse(false);
            if (!resp.Okay)
            {
                throw new AdbCommandRejectedException(resp.Message, true); //errorDuringDeviceSelection
            }
        }

        /// <summary>
        /// Reads from the socket until the array is filled, or no more data is coming (because
        /// the socket closed or the timeout expired).
        /// <p/>This uses the default time out value.
        /// </summary>
        /// <param name="chan"> the opened socket to read from. It must be in non-blocking
        ///      mode for timeouts to work </param>
        /// <param name="data"> the buffer to store the read data into. </param>
        /// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
        /// <exception cref="IOException"> in case of I/O error on the connection. </exception>
        [DebuggerNonUserCode]
        protected bool Read(byte[] data)
        {
            int read;
            try
            {
                read = tcpClient.GetStream().Read(data, 0, data.Length);
            }
            catch (IOException)
            {
                if (!tcpClient.Connected)
                    return false;
                throw;
            }
            if (read != data.Length)
            {
                throw new IOException("Not enough data");
            }
            return true;
        }

        /// <summary>
        /// Write until all data in "data" is written or the connection fails or times out.
        /// <p/>This uses the default time out value. </summary>
        /// <param name="data"> the buffer to send. </param>
        protected void Write(byte[] data)
        {
            tcpClient.GetStream().Write(data, 0, data.Length);
        }

        /// <summary>
        /// Reads the response from ADB after a command. </summary>
        /// <param name="readDiagString"> If true, we're expecting an OKAY response to be
        ///      followed by a diagnostic string. Otherwise, we only expect the
        ///      diagnostic string to follow a FAIL. </param>
        protected AdbResponse ReadAdbResponse(bool readDiagString)
        {
            var resp = new AdbResponse();

            var reply = new byte[4];
            Read(reply);

            if (IsOkay(reply))
            {
                resp.Okay = true;
            }
            else
            {
                readDiagString = true; // look for a reason after the FAIL
                resp.Okay = false;
            }

            // not a loop -- use "while" so we can use "break"
            try
            {
                while (readDiagString)
                {
                    int len;
                    if (!TryReadHex4(out len))
                        break;

                    var msg = new byte[len];
                    Read(msg);

                    resp.Message = ReplyToString(msg);
                    //Log.v("ddms", "Got reply '" + replyToString(reply) + "', diag='" + resp.message + "'");

                    break;
                }
            }
            catch (Exception)
            {
                // ignore those, since it's just reading the diagnose string, the response will
                // contain okay==false anyway.
            }

            return resp;
        }

        /// <summary>
        /// Create an ASCII string preceeded by four hex digits. The opening "####"
        /// is the length of the rest of the string, encoded as ASCII hex (case
        /// doesn't matter). "port" and "host" are what we want to forward to. If
        /// we're on the host side connecting into the device, "addrStr" should be
        /// null.
        /// </summary>
        protected static byte[] FormatAdbRequest(string req)
        {
            var resultStr = string.Format("{0:X4}{1}", req.Length, req); 
            byte[] result;
            try
            {
                result = DefaultEncoding.GetBytes(resultStr);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Error in encoding", ex);
            }
            Debug.Assert(result.Length == req.Length + 4);
            return result;
        }

        /// <summary>
        /// Checks to see if the first four bytes in "reply" are OKAY.
        /// </summary>
        private static bool IsOkay(byte[] reply)
        {
            return reply[0] == (byte)'O' && reply[1] == (byte)'K' && reply[2] == (byte)'A' && reply[3] == (byte)'Y';
        }

        /// <summary>
        /// Converts an ADB reply to a string.
        /// </summary>
        protected static string ReplyToString(byte[] reply)
        {
           try
            {
                return DefaultEncoding.GetString(reply);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex.ToString());
                Debug.Write(ex.StackTrace); // not expected
                return string.Empty;
            }
        }

        /// <summary>
        /// Read a 4byte hex encoded integer.
        /// </summary>
        protected bool TryReadHex4(out int value)
        {
            var buf = new byte[4];
            if (!Read(buf))
            {
                value = 0;
                return false;
            }
            var hex4 = ReplyToString(buf);
            try
            {
                value = Convert.ToInt32(hex4, 16);
                return true;
            }
            catch (Exception)
            {
                value = 0;
                return false;
            }
        }
    }
}
