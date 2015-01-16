using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Dot42.DdmLib.support;

/*
 * Copyright (C) 2007 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Dot42.DdmLib
{


	/// <summary>
	/// Provides control over emulated hardware of the Android emulator.
	/// <p/>This is basically a wrapper around the command line console normally used with telnet.
	/// <p/>
	/// Regarding line termination handling:<br>
	/// One of the issues is that the telnet protocol <b>requires</b> usage of <code>\r\n</code>. Most
	/// implementations don't enforce it (the dos one does). In this particular case, this is mostly
	/// irrelevant since we don't use telnet in Java, but that means we want to make
	/// sure we use the same line termination than what the console expects. The console
	/// code removes <code>\r</code> and waits for <code>\n</code>.
	/// <p/>However this means you <i>may</i> receive <code>\r\n</code> when reading from the console.
	/// <p/>
	/// <b>This API will change in the near future.</b>
	/// </summary>
	public sealed class EmulatorConsole
	{

		private const string DEFAULT_ENCODING = "ISO-8859-1"; //$NON-NLS-1$

		private const int WAIT_TIME = 5; // spin-wait sleep, in ms

		private const int STD_TIMEOUT = 5000; // standard delay, in ms

		private const string HOST = "127.0.0.1"; //$NON-NLS-1$

		private const string COMMAND_PING = "help\r\n"; //$NON-NLS-1$
		private const string COMMAND_AVD_NAME = "avd name\r\n"; //$NON-NLS-1$
		private const string COMMAND_KILL = "kill\r\n"; //$NON-NLS-1$
		private const string COMMAND_GSM_STATUS = "gsm status\r\n"; //$NON-NLS-1$
		private const string COMMAND_GSM_CALL = "gsm call %1$s\r\n"; //$NON-NLS-1$
		private const string COMMAND_GSM_CANCEL_CALL = "gsm cancel %1$s\r\n"; //$NON-NLS-1$
		private const string COMMAND_GSM_DATA = "gsm data %1$s\r\n"; //$NON-NLS-1$
		private const string COMMAND_GSM_VOICE = "gsm voice %1$s\r\n"; //$NON-NLS-1$
		private const string COMMAND_SMS_SEND = "sms send %1$s %2$s\r\n"; //$NON-NLS-1$
		private const string COMMAND_NETWORK_STATUS = "network status\r\n"; //$NON-NLS-1$
		private const string COMMAND_NETWORK_SPEED = "network speed %1$s\r\n"; //$NON-NLS-1$
		private const string COMMAND_NETWORK_LATENCY = "network delay %1$s\r\n"; //$NON-NLS-1$
		private const string COMMAND_GPS = "geo fix %1$f %2$f %3$f\r\n"; //$NON-NLS-1$

		private static readonly Regex RE_KO = new Regex("KO:\\s+(.*)"); //$NON-NLS-1$

		/// <summary>
		/// Array of delay values: no delay, gprs, edge/egprs, umts/3d
		/// </summary>
		public static readonly int[] MIN_LATENCIES = new int[] {0, 150, 80, 35};

		/// <summary>
		/// Array of download speeds: full speed, gsm, hscsd, gprs, edge/egprs, umts/3g, hsdpa.
		/// </summary>
		public readonly int[] DOWNLOAD_SPEEDS = new int[] {0, 14400, 43200, 80000, 236800, 1920000, 14400000};

		/// <summary>
		/// Arrays of valid network speeds </summary>
		public static readonly string[] NETWORK_SPEEDS = new string[] {"full", "gsm", "hscsd", "gprs", "edge", "umts", "hsdpa"};

		/// <summary>
		/// Arrays of valid network latencies </summary>
		public static readonly string[] NETWORK_LATENCIES = new string[] {"none", "gprs", "edge", "umts"};

		/// <summary>
		/// Gsm Mode enum. </summary>
		public sealed class GsmMode
		{
		    public static readonly GsmMode UNKNOWN = new GsmMode(new string[0]);
		    public static readonly GsmMode UNREGISTERED = new GsmMode(new String[] {"unregistered", "off"});
		    public static readonly GsmMode HOME = new GsmMode(new String[] {"home", "on"});
		    public static readonly GsmMode ROAMING = new GsmMode("roaming");
		    public static readonly GsmMode SEARCHING = new GsmMode("searching");
			public static readonly GsmMode DENIED= new GsmMode("denied");

		    private static readonly GsmMode[] values = new[] {UNKNOWN, UNREGISTERED, HOME, ROAMING, SEARCHING, DENIED};

			private readonly string[] tags;

			private GsmMode(string tag)
			{
			    tags = tag != null ? new[] { tag } : new string[0];
			}

		    internal GsmMode(string[] tags)
			{
				this.tags = tags;
			}

			public static GsmMode getEnum(string tag)
			{
			    return values.FirstOrDefault(x => x.tags.Contains(tag)) ?? UNKNOWN;
			}

			/// <summary>
			/// Returns the first tag of the enum.
			/// </summary>
			public string tag
			{
				get
				{
					if (tags.Length > 0)
					{
						return tags[0];
					}
					return null;
				}
			}
		}

		public const string RESULT_OK = null;

		private static readonly Regex sEmulatorRegexp = new Regex (Device.RE_EMULATOR_SN);
		private static readonly Regex sVoiceStatusRegexp = new Regex ("gsm\\s+voice\\s+state:\\s*([a-z]+)", RegexOptions.IgnoreCase); //$NON-NLS-1$
        private static readonly Regex sDataStatusRegexp = new Regex("gsm\\s+data\\s+state:\\s*([a-z]+)", RegexOptions.IgnoreCase); //$NON-NLS-1$
        private static readonly Regex sDownloadSpeedRegexp = new Regex("\\s+download\\s+speed:\\s+(\\d+)\\s+bits.*", RegexOptions.IgnoreCase); //$NON-NLS-1$
        private static readonly Regex sMinLatencyRegexp = new Regex("\\s+minimum\\s+latency:\\s+(\\d+)\\s+ms", RegexOptions.IgnoreCase); //$NON-NLS-1$

		private static readonly Dictionary<int, EmulatorConsole> sEmulators = new Dictionary<int, EmulatorConsole>();

			/// <summary>
		/// Gsm Status class </summary>
		public class GsmStatus
		{
			/// <summary>
			/// Voice status. </summary>
			public GsmMode voice = GsmMode.UNKNOWN;
			/// <summary>
			/// Data status. </summary>
			public GsmMode data = GsmMode.UNKNOWN;
		}

			/// <summary>
		/// Network Status class </summary>
		public class NetworkStatus
		{
			/// <summary>
			/// network speed status. This is an index in the <seealso cref="#DOWNLOAD_SPEEDS"/> array. </summary>
			public int speed = -1;
			/// <summary>
			/// network latency status.  This is an index in the <seealso cref="#MIN_LATENCIES"/> array. </summary>
			public int latency = -1;
		}

		private int mPort;

		private SocketChannel mSocketChannel;

		private readonly byte[] mBuffer = new byte[1024];

		/// <summary>
		/// Returns an <seealso cref="EmulatorConsole"/> object for the given <seealso cref="Device"/>. This can
		/// be an already existing console, or a new one if it hadn't been created yet. </summary>
		/// <param name="d"> The device that the console links to. </param>
		/// <returns> an <code>EmulatorConsole</code> object or <code>null</code> if the connection failed. </returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static EmulatorConsole getConsole(IDevice d)
		{
			// we need to make sure that the device is an emulator
			// get the port number. This is the console port.
			int? port = getEmulatorPort(d.serialNumber);
			if (!port.HasValue)
			{
				return null;
			}

			EmulatorConsole console = sEmulators[port.Value];

			if (console != null)
			{
				// if the console exist, we ping the emulator to check the connection.
				if (console.ping() == false)
				{
					RemoveConsole(console.mPort);
					console = null;
				}
			}

			if (console == null)
			{
				// no console object exists for this port so we create one, and start
				// the connection.
				console = new EmulatorConsole(port.Value);
				if (console.start())
				{
					sEmulators.Add(port.Value, console);
				}
				else
				{
					console = null;
				}
			}

			return console;
		}

		/// <summary>
		/// Return port of emulator given its serial number.
		/// </summary>
		/// <param name="serialNumber"> the emulator's serial number </param>
		/// <returns> the integer port or <code>null</code> if it could not be determined </returns>
		public static int? getEmulatorPort(string serialNumber)
		{
			var m = sEmulatorRegexp.Match(serialNumber);
			if (m.Success)
			{
				// get the port number. This is the console port.
				int port;
				try
				{
					port = Convert.ToInt32(m.group(1));
					if (port > 0)
					{
						return port;
					}
				}
                catch (SystemException)
				{
					// looks like we failed to get the port number. This is a bit strange since
					// it's coming from a regexp that only accept digit, but we handle the case
					// and return null.
				}
			}
			return null;
		}

		/// <summary>
		/// Removes the console object associated with a port from the map. </summary>
		/// <param name="port"> The port of the console to remove. </param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		private static void RemoveConsole(int port)
		{
			sEmulators.Remove(port);
		}

//JAVA TO C# CONVERTER TODO TASK: The following line could not be converted:
		private EmulatorConsole(int port)
		{
			mPort = port;
		}

		/// <summary>
		/// Starts the connection of the console. </summary>
		/// <returns> true if success. </returns>
		private bool start()
		{

			DnsEndPoint socketAddr;
			try
			{
				socketAddr = new DnsEndPoint(HOST, mPort);
			}
			catch (ArgumentException)
			{
				return false;
			}

			try
			{
				mSocketChannel = SocketChannel.open(socketAddr);
			}
			catch (IOException)
			{
				return false;
			}

			// read some stuff from it
			readLines();

			return true;
		}

		/// <summary>
		/// Ping the emulator to check if the connection is still alive. </summary>
		/// <returns> true if the connection is alive. </returns>
		private /*synchronized*/ bool ping()
		{
			// it looks like we can send stuff, even when the emulator quit, but we can't read
			// from the socket. So we check the return of readLines()
			if (sendCommand(COMMAND_PING))
			{
				return readLines() != null;
			}

			return false;
		}

		/// <summary>
		/// Sends a KILL command to the emulator.
		/// </summary>
		public /*synchronized*/ void kill()
		{
			if (sendCommand(COMMAND_KILL))
			{
				RemoveConsole(mPort);
			}
		}

		public /*synchronized*/ string avdName
		{
            get
            {
                if (sendCommand(COMMAND_AVD_NAME))
                {
                    string[] result = readLines();
                    if (result != null && result.Length == 2) // this should be the name on first line,
                    {
                        // and ok on 2nd line
                        return result[0];
                    }
                    else
                    {
                        // try to see if there's a message after KO
                        var m = RE_KO.Match(result[result.Length - 1]);
                        if (m.Success)
                        {
                            return m.group(1);
                        }
                    }
                }

                return null;
            }
		}

		/// <summary>
		/// Get the network status of the emulator. </summary>
		/// <returns> a <seealso cref="NetworkStatus"/> object containing the <seealso cref="GsmStatus"/>, or
		/// <code>null</code> if the query failed. </returns>
		public /*synchronized*/ NetworkStatus networkStatus
		{
            get
            {
                if (sendCommand(COMMAND_NETWORK_STATUS))
                {
                    /* Result is in the format
				    Current network status:
				    download speed:      14400 bits/s (1.8 KB/s)
				    upload speed:        14400 bits/s (1.8 KB/s)
				    minimum latency:  0 ms
				    maximum latency:  0 ms
				 */
                    string[] result = readLines();

                    if (isValid(result))
                    {
                        // we only compare agains the min latency and the download speed
                        // let's not rely on the order of the output, and simply loop through
                        // the line testing the regexp.
                        NetworkStatus status = new NetworkStatus();
                        foreach (string line in result)
                        {
                            var m = sDownloadSpeedRegexp.Match(line);
                            if (m.Success)
                            {
                                // get the string value
                                string value = m.group(1);

                                // get the index from the list
                                status.speed = getSpeedIndex(value);

                                // move on to next line.
                                continue;
                            }

                            m = sMinLatencyRegexp.Match(line);
                            if (m.Success)
                            {
                                // get the string value
                                string value = m.group(1);

                                // get the index from the list
                                status.latency = getLatencyIndex(value);

                                // move on to next line.
                                continue;
                            }
                        }

                        return status;
                    }
                }

                return null;
            }
		}

		/// <summary>
		/// Returns the current gsm status of the emulator </summary>
		/// <returns> a <seealso cref="GsmStatus"/> object containing the gms status, or <code>null</code>
		/// if the query failed. </returns>
		public /*synchronized*/ GsmStatus gsmStatus
		{
			get
			{
			    if (sendCommand(COMMAND_GSM_STATUS))
			    {
			        /*
				 * result is in the format:
				 * gsm status
				 * gsm voice state: home
				 * gsm data state:  home
				 */

			        string[] result = readLines();
			        if (isValid(result))
			        {

			            GsmStatus status = new GsmStatus();

			            // let's not rely on the order of the output, and simply loop through
			            // the line testing the regexp.
			            foreach (string line in result)
			            {
			                var m = sVoiceStatusRegexp.Match(line);
			                if (m.matches())
			                {
			                    // get the string value
			                    string value = m.group(1);

			                    // get the index from the list
			                    status.voice = GsmMode.getEnum(value.ToLower(Locale.US));

			                    // move on to next line.
			                    continue;
			                }

			                m = sDataStatusRegexp.Match(line);
			                if (m.matches())
			                {
			                    // get the string value
			                    string value = m.group(1);

			                    // get the index from the list
			                    status.data = GsmMode.getEnum(value.ToLower(Locale.US));

			                    // move on to next line.
			                    continue;
			                }
			            }

			            return status;
			        }
			    }

			    return null;
			}
		}

		/// <summary>
		/// Sets the GSM voice mode. </summary>
		/// <param name="mode"> the <seealso cref="GsmMode"/> value. </param>
		/// <returns> RESULT_OK if success, an error String otherwise. </returns>
		/// <exception cref="InvalidParameterException"> if mode is an invalid value. </exception>
		public /*synchronized*/ string gsmVoiceModeode(GsmMode mode) 
		{
			if (mode == GsmMode.UNKNOWN)
			{
				throw new ArgumentException();
			}

			string command = string.Format(COMMAND_GSM_VOICE, mode.tag);
			return processCommand(command);
		}

		/// <summary>
		/// Sets the GSM data mode. </summary>
		/// <param name="mode"> the <seealso cref="GsmMode"/> value </param>
		/// <returns> <seealso cref="#RESULT_OK"/> if success, an error String otherwise. </returns>
		/// <exception cref="InvalidParameterException"> if mode is an invalid value. </exception>
		public /*synchronized*/ string gsmDataModeode(GsmMode mode) 
		{
			if (mode == GsmMode.UNKNOWN)
			{
				throw new ArgumentException();
			}

			string command = string.Format(COMMAND_GSM_DATA, mode.tag);
			return processCommand(command);
		}

		/// <summary>
		/// Initiate an incoming call on the emulator. </summary>
		/// <param name="number"> a string representing the calling number. </param>
		/// <returns> <seealso cref="#RESULT_OK"/> if success, an error String otherwise. </returns>
		public /*synchronized*/ string call(string number)
		{
			string command = string.Format(COMMAND_GSM_CALL, number);
			return processCommand(command);
		}

		/// <summary>
		/// Cancels a current call. </summary>
		/// <param name="number"> the number of the call to cancel </param>
		/// <returns> <seealso cref="#RESULT_OK"/> if success, an error String otherwise. </returns>
		public /*synchronized*/ string cancelCall(string number)
		{
			string command = string.Format(COMMAND_GSM_CANCEL_CALL, number);
			return processCommand(command);
		}

		/// <summary>
		/// Sends an SMS to the emulator </summary>
		/// <param name="number"> The sender phone number </param>
		/// <param name="message"> The SMS message. \ characters must be escaped. The carriage return is
		/// the 2 character sequence  {'\', 'n' }
		/// </param>
		/// <returns> <seealso cref="#RESULT_OK"/> if success, an error String otherwise. </returns>
		public /*synchronized*/ string sendSms(string number, string message)
		{
			string command = string.Format(COMMAND_SMS_SEND, number, message);
			return processCommand(command);
		}

		/// <summary>
		/// Sets the network speed. </summary>
		/// <param name="selectionIndex"> The index in the <seealso cref="#NETWORK_SPEEDS"/> table. </param>
		/// <returns> <seealso cref="#RESULT_OK"/> if success, an error String otherwise. </returns>
		public /*synchronized*/ string networkSpeedeed(int selectionIndex)
		{
			string command = string.Format(COMMAND_NETWORK_SPEED, NETWORK_SPEEDS[selectionIndex]);
			return processCommand(command);
		}

		/// <summary>
		/// Sets the network latency. </summary>
		/// <param name="selectionIndex"> The index in the <seealso cref="#NETWORK_LATENCIES"/> table. </param>
		/// <returns> <seealso cref="#RESULT_OK"/> if success, an error String otherwise. </returns>
		public /*synchronized*/ string networkLatencyncy(int selectionIndex)
		{
			string command = string.Format(COMMAND_NETWORK_LATENCY, NETWORK_LATENCIES[selectionIndex]);
			return processCommand(command);
		}

        public /*synchronized*/ string sendLocation(double longitude, double latitude, double elevation)
		{

			// need to make sure the string format uses dot and not comma
			var command = string.Format(Locale.US, COMMAND_GPS, longitude, latitude, elevation);
			return processCommand(command);
		}

		/// <summary>
		/// Sends a command to the emulator console. </summary>
		/// <param name="command"> The command string. <b>MUST BE TERMINATED BY \n</b>. </param>
		/// <returns> true if success </returns>
		private bool sendCommand(string command)
		{
			bool result = false;
			try
			{
				byte[] bCommand;
				try
				{
					bCommand = command.getBytes(DEFAULT_ENCODING);
				}
				catch (ArgumentException)
				{
					// wrong encoding...
					return result;
				}

				// write the command
				AdbHelper.write(mSocketChannel, bCommand, bCommand.Length, DdmPreferences.timeOut);

				result = true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				if (result == false)
				{
					// FIXME connection failed somehow, we need to disconnect the console.
					RemoveConsole(mPort);
				}
			}

			return result;
		}

		/// <summary>
		/// Sends a command to the emulator and parses its answer. </summary>
		/// <param name="command"> the command to send. </param>
		/// <returns> <seealso cref="#RESULT_OK"/> if success, an error message otherwise. </returns>
		private string processCommand(string command)
		{
			if (sendCommand(command))
			{
				string[] result = readLines();

				if (result != null && result.Length > 0)
				{
					var m = RE_KO.Match(result[result.Length - 1]);
					if (m.matches())
					{
						return m.group(1);
					}
					return RESULT_OK;
				}

				return "Unable to communicate with the emulator";
			}

			return "Unable to send command to the emulator";
		}

		/// <summary>
		/// Reads line from the console socket. This call is blocking until we read the lines:
		/// <ul>
		/// <li>OK\r\n</li>
		/// <li>KO<msg>\r\n</li>
		/// </ul> </summary>
		/// <returns> the array of strings read from the emulator. </returns>
		private string[] readLines()
		{
			try
			{
				ByteBuffer buf = ByteBuffer.wrap(mBuffer, 0, mBuffer.Length);
				int numWaits = 0;
				bool stop = false;

				while (buf.position != buf.limit && stop == false)
				{
					int count;

					count = mSocketChannel.read(buf);
					if (count < 0)
					{
						return null;
					}
					else if (count == 0)
					{
						if (numWaits * WAIT_TIME > STD_TIMEOUT)
						{
							return null;
						}
						// non-blocking spin
						try
						{
							Thread.Sleep(WAIT_TIME);
						}
						catch (ThreadInterruptedException)
						{
						}
						numWaits++;
					}
					else
					{
						numWaits = 0;
					}

					// check the last few char aren't OK. For a valid message to test
					// we need at least 4 bytes (OK/KO + \r\n)
					if (buf.position >= 4)
					{
						int pos = buf.position;
						if (endsWithOK(pos) || lastLineIsKO(pos))
						{
							stop = true;
						}
					}
				}

				string msg = mBuffer.getString(0, buf.position, DEFAULT_ENCODING);
				return StringHelperClass.StringSplit(msg, "\r\n", true); //$NON-NLS-1$
			}
			catch (IOException)
			{
				return null;
			}
		}

		/// <summary>
		/// Returns true if the 4 characters *before* the current position are "OK\r\n" </summary>
		/// <param name="currentPosition"> The current position </param>
		private bool endsWithOK(int currentPosition)
		{
			if (mBuffer[currentPosition - 1] == '\n' && mBuffer[currentPosition - 2] == '\r' && mBuffer[currentPosition - 3] == 'K' && mBuffer[currentPosition - 4] == 'O')
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns true if the last line starts with KO and is also terminated by \r\n </summary>
		/// <param name="currentPosition"> the current position </param>
		private bool lastLineIsKO(int currentPosition)
		{
			// first check that the last 2 characters are CRLF
			if (mBuffer[currentPosition - 1] != '\n' || mBuffer[currentPosition - 2] != '\r')
			{
				return false;
			}

			// now loop backward looking for the previous CRLF, or the beginning of the buffer
			int i = 0;
			for (i = currentPosition - 3 ; i >= 0; i--)
			{
				if (mBuffer[i] == '\n')
				{
					// found \n!
					if (i > 0 && mBuffer[i - 1] == '\r')
					{
						// found \r!
						break;
					}
				}
			}

			// here it is either -1 if we reached the start of the buffer without finding
			// a CRLF, or the position of \n. So in both case we look at the characters at i+1 and i+2
			if (mBuffer[i + 1] == 'K' && mBuffer[i + 2] == 'O')
			{
				// found error!
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns true if the last line of the result does not start with KO
		/// </summary>
		private bool isValid(string[] result)
		{
			if (result != null && result.Length > 0)
			{
				return !(RE_KO.Match(result[result.Length - 1]).matches());
			}
			return false;
		}

		private int getLatencyIndex(string value)
		{
			try
			{
				// get the int value
				int latency = Convert.ToInt32(value);

				// check for the speed from the index
				for (int i = 0 ; i < MIN_LATENCIES.Length; i++)
				{
					if (MIN_LATENCIES[i] == latency)
					{
						return i;
					}
				}
			}
            catch (SystemException)
			{
				// Do nothing, we'll just return -1.
			}

			return -1;
		}

		private int getSpeedIndex(string value)
		{
			try
			{
				// get the int value
				int speed = Convert.ToInt32(value);

				// check for the speed from the index
				for (int i = 0 ; i < DOWNLOAD_SPEEDS.Length; i++)
				{
					if (DOWNLOAD_SPEEDS[i] == speed)
					{
						return i;
					}
				}
			}
            catch (SystemException)
			{
				// Do nothing, we'll just return -1.
			}

			return -1;
		}
	}

}