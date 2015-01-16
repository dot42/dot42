using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dot42.DeviceLib;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Helper for communicating with ADB over a socket connection.
    /// </summary>
    internal class AdbJdwpRequest : AdbRequest 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal AdbJdwpRequest(IPEndPoint endPoint)
            : base(endPoint)
        {
        }

        /// <summary>
        /// Return a list of available jdwp pid's.
        /// </summary>
        internal IEnumerable<int> JdwpProcessIds(IDevice device)
        {
            SetDevice(device);
            Write(FormatAdbRequest("jdwp"));
            var resp = ReadAdbResponse(false);
            if (!resp.Okay)
                throw new AdbCommandRejectedException("jdwp rejected");
            int len;
            if (TryReadHex4(out len))
            {
                var buf = new byte[len];
                Read(buf);
                return ParseJdwpData(ReplyToString(buf));
            }
            return Enumerable.Empty<int>();
        }

        /// <summary>
        /// Return a list of available devices.
        /// </summary>
        internal void TrackJdwpProcessIds(IDevice device, Action<List<int>> callback)
        {
            SetDevice(device);
            Write(FormatAdbRequest("track-jdwp"));
            var resp = ReadAdbResponse(false);
            if (!resp.Okay)
                throw new AdbCommandRejectedException("host:track-devices rejected");

            while (true)
            {
                int len;
                if (!TryReadHex4(out len))
                {
                    break;
                }
                List<int> list;
                if (len > 0)
                {
                    var buf = new byte[len];
                    Read(buf);
                    list = ParseJdwpData(ReplyToString(buf)).ToList();
                }
                else
                {
                    // Empty
                    list = new List<int>();
                }
                callback(list);
            }
        }

        /// <summary>
        /// Parse data resulting from a jdwp request.
        /// </summary>
        private static IEnumerable<int> ParseJdwpData(string data)
        {
            var lines = data.Split('\n', '\r');
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                var pidStr = line.Trim();
                int pid;
                if (int.TryParse(pidStr, out pid))
                    yield return pid;
            }
        }
    }
}
