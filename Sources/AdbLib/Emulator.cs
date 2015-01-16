using System;
using System.Net.Sockets;
using System.Text;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Emulator helpers
    /// </summary>
    internal static class Emulator
    {
        internal const string SerialPrefix = "emulator-";
        private static readonly string[] NoResponse = new string[1];

        /// <summary>
        /// Gets the port number for the given serial.
        /// </summary>
        internal static int GetPort(string serial)
        {
            if (string.IsNullOrEmpty(serial))
                return -1;
            if (!serial.StartsWith(SerialPrefix))
                return -1;
            var portStr = serial.Substring(SerialPrefix.Length);
            int port;
            if (!int.TryParse(portStr, out port))
                return -1;
            return port;
        }

        /// <summary>
        /// Contact the emulator to get the name of the AVD.
        /// </summary>
        internal static bool TryGetAvdName(int emulatorPort, out string name)
        {
            string[] response;
            var ok = TryExecuteCommand(emulatorPort, "avd name", out response);
            name = response[0];
            return ok;
        }

        /// <summary>
        /// Contact the emulator to get the status of the AVD.
        /// </summary>
        internal static bool TryGetAvdStatus(int emulatorPort, out string status)
        {
            string[] response;
            var ok = TryExecuteCommand(emulatorPort, "avd status", out response);
            status = response[0];
            return ok;
        }

        /// <summary>
        /// Contact the emulator to execute a command.
        /// </summary>
        internal static bool TryExecuteCommand(int emulatorPort, string command, out string[] response)
        {
            response = NoResponse;
            if (emulatorPort < 0)
                return false;
            try
            {
                using (var client = new TcpClient("localhost", emulatorPort))
                {
                    var networkStream = client.GetStream();
                    var prefix = ReadResponse(networkStream);
                    if (prefix[prefix.Length - 1] != "OK")
                        return false;
                    SendCommand(networkStream, command + '\n');
                    response = ReadResponse(networkStream);
                    return (response[response.Length - 1] == "OK");
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Send a command
        /// </summary>
        private static void SendCommand(NetworkStream stream, string command)
        {
            var buf = Encoding.ASCII.GetBytes(command);
            stream.Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// Read telnet response.
        /// </summary>
        private static string[] ReadResponse(NetworkStream stream)
        {
            var sb = new StringBuilder();
            // Read first byte (we'll wait for that)
            var ch = stream.ReadByte();
            sb.Append((char)ch);

            // Read remaining data
            while (stream.DataAvailable)
            {
                ch = stream.ReadByte();
                sb.Append((char) ch);
            }
            return sb.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
