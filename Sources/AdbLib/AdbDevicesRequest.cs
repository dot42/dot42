using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Helper for communicating with ADB over a socket connection.
    /// </summary>
    internal class AdbDevicesRequest : AdbRequest 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal AdbDevicesRequest(IPEndPoint endPoint)
            : base(endPoint)
        {
        }

        /// <summary>
        /// Return a list of available devices.
        /// </summary>
        internal IEnumerable<AndroidDevice> Devices()
        {
            Write(FormatAdbRequest("host:devices"));
            var resp = ReadAdbResponse(false);
            if (!resp.Okay)
                throw new AdbCommandRejectedException("host:devices rejected");
            int len;
            if (TryReadHex4(out len))
            {
                var buf = new byte[len];
                Read(buf);
                return ParseDevicesData(ReplyToString(buf));
            }
            return Enumerable.Empty<AndroidDevice>();
        }

        /// <summary>
        /// Return a list of available devices.
        /// </summary>
        internal void TrackDevices(Action<List<AndroidDevice>> callback)
        {
            Write(FormatAdbRequest("host:track-devices"));
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
                List<AndroidDevice> list;
                if (len > 0)
                {
                    var buf = new byte[len];
                    Read(buf);
                    list = ParseDevicesData(ReplyToString(buf)).ToList();
                }
                else
                {
                    // Empty
                    list = new List<AndroidDevice>();
                }
                callback(list);
            }
        }

        /// <summary>
        /// Parse data resulting from a Devices request.
        /// </summary>
        private static IEnumerable<AndroidDevice> ParseDevicesData(string data)
        {
            var lines = data.Split('\n', '\r');
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                var parts = line.Split('\t');
                if (parts.Length != 2)
                    continue;

                var serial = parts[0];
                var state = parts[1];

                yield return new AndroidDevice(serial, ParseDeviceState(state));
            }
        }

        /// <summary>
        /// Parse a device state
        /// </summary>
        private static AndroidDeviceStates ParseDeviceState(string s)
        {
            switch (s)
            {
                case "device":
                    return AndroidDeviceStates.Device;
                case "offline":
                    return AndroidDeviceStates.Offline;
                default:
                    throw new ArgumentException("Unknown device state: " + s);
            }
        }
    }
}
