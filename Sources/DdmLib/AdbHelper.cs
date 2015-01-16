using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Dot42.DdmLib.log;
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
	/// Helper class to handle requests and connections to adb.
	/// <p/><seealso cref="DebugBridgeServer"/> is the public API to connection to adb, while <seealso cref="AdbHelper"/>
	/// does the low level stuff.
	/// <p/>This currently uses spin-wait non-blocking I/O. A Selector would be more efficient,
	/// but seems like overkill for what we're doing here.
	/// </summary>
	internal sealed class AdbHelper
	{

		// public static final long kOkay = 0x59414b4fL;
		// public static final long kFail = 0x4c494146L;

		internal const int WAIT_TIME = 5; // spin-wait sleep, in ms

		internal const string DEFAULT_ENCODING = "ISO-8859-1"; //$NON-NLS-1$

	/// <summary>
		/// do not instantiate </summary>
		private AdbHelper()
		{
		}

		/// <summary>
		/// Response from ADB.
		/// </summary>
		internal class AdbResponse
		{
			public AdbResponse()
			{
				message = "";
			}

			public bool okay; // first 4 bytes in response were "OKAY"?

			public string message; // diagnostic string if #okay is false
		}

		/// <summary>
		/// Create and connect a new pass-through socket, from the host to a port on
		/// the device.
		/// </summary>
		/// <param name="adbSockAddr"> </param>
		/// <param name="device"> the device to connect to. Can be null in which case the connection will be
		/// to the first available device. </param>
		/// <param name="devicePort"> the port we're opening </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.nio.channels.SocketChannel open(java.net.InetSocketAddress adbSockAddr, Device device, int devicePort) throws java.io.IOException, TimeoutException, AdbCommandRejectedException
		public static SocketChannel open(EndPoint adbSockAddr, Device device, int devicePort)
		{

			SocketChannel adbChan = SocketChannel.open(adbSockAddr);
			try
			{
				adbChan.socket().NoDelay = true;
				adbChan.configureBlocking(false);

				// if the device is not -1, then we first tell adb we're looking to
				// talk to a specific device
				setDevice(adbChan, device);

				var req = createAdbForwardRequest(null, devicePort);
				// Log.hexDump(req);

				write(adbChan, req);

				AdbResponse resp = readAdbResponse(adbChan, false);
				if (resp.okay == false)
				{
					throw new AdbCommandRejectedException(resp.message);
				}

				adbChan.configureBlocking(true);
			}
			catch (TimeoutException e)
			{
				adbChan.close();
				throw e;
			}
			catch (IOException e)
			{
				adbChan.close();
				throw e;
			}

			return adbChan;
		}

		/// <summary>
		/// Creates and connects a new pass-through socket, from the host to a port on
		/// the device.
		/// </summary>
		/// <param name="adbSockAddr"> </param>
		/// <param name="device"> the device to connect to. Can be null in which case the connection will be
		/// to the first available device. </param>
		/// <param name="pid"> the process pid to connect to. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.nio.channels.SocketChannel createPassThroughConnection(java.net.InetSocketAddress adbSockAddr, Device device, int pid) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public static SocketChannel createPassThroughConnection(EndPoint adbSockAddr, Device device, int pid)
		{

			SocketChannel adbChan = SocketChannel.open(adbSockAddr);
			try
			{
				adbChan.socket().NoDelay = true;
				adbChan.configureBlocking(false);

				// if the device is not -1, then we first tell adb we're looking to
				// talk to a specific device
				setDevice(adbChan, device);

				var req = createJdwpForwardRequest(pid);
				// Log.hexDump(req);

				write(adbChan, req);

				AdbResponse resp = readAdbResponse(adbChan, false); // readDiagString
				if (resp.okay == false)
				{
					throw new AdbCommandRejectedException(resp.message);
				}

				adbChan.configureBlocking(true);
			}
			catch (TimeoutException e)
			{
				adbChan.close();
				throw e;
			}
			catch (IOException e)
			{
				adbChan.close();
				throw e;
			}

			return adbChan;
		}

		/// <summary>
		/// Creates a port forwarding request for adb. This returns an array
		/// containing "####tcp:{port}:{addStr}". </summary>
		/// <param name="addrStr"> the host. Can be null. </param>
		/// <param name="port"> the port on the device. This does not need to be numeric. </param>
		private static byte[] createAdbForwardRequest(string addrStr, int port)
		{
			string reqStr;

			if (addrStr == null)
			{
				reqStr = "tcp:" + port;
			}
			else
			{
				reqStr = "tcp:" + port + ":" + addrStr;
			}
			return formAdbRequest(reqStr);
		}

		/// <summary>
		/// Creates a port forwarding request to a jdwp process. This returns an array
		/// containing "####jwdp:{pid}". </summary>
		/// <param name="pid"> the jdwp process pid on the device. </param>
		private static byte[] createJdwpForwardRequest(int pid)
		{
			string reqStr = string.Format("jdwp:{0:D}", pid); //$NON-NLS-1$
			return formAdbRequest(reqStr);
		}

		/// <summary>
		/// Create an ASCII string preceeded by four hex digits. The opening "####"
		/// is the length of the rest of the string, encoded as ASCII hex (case
		/// doesn't matter). "port" and "host" are what we want to forward to. If
		/// we're on the host side connecting into the device, "addrStr" should be
		/// null.
		/// </summary>
		internal static byte[] formAdbRequest(string req)
		{
			string resultStr = string.Format("{0:X4}{1}", req.Length, req); //$NON-NLS-1$
			byte[] result;
			try
			{
				result = resultStr.getBytes(DEFAULT_ENCODING);
			}
			catch (ArgumentException uee)
			{
				Console.WriteLine(uee.ToString());
				Console.Write(uee.StackTrace); // not expected
				return null;
			}
			Debug.Assert(result.Length == req.Length + 4);
			return result;
		}

		/// <summary>
		/// Reads the response from ADB after a command. </summary>
		/// <param name="chan"> The socket channel that is connected to adb. </param>
		/// <param name="readDiagString"> If true, we're expecting an OKAY response to be
		///      followed by a diagnostic string. Otherwise, we only expect the
		///      diagnostic string to follow a FAIL. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static AdbResponse readAdbResponse(java.nio.channels.SocketChannel chan, boolean readDiagString) throws TimeoutException, java.io.IOException
		internal static AdbResponse readAdbResponse(SocketChannel chan, bool readDiagString)
		{

			AdbResponse resp = new AdbResponse();

			var reply = new byte[4];
			read(chan, reply);

			if (isOkay(reply))
			{
				resp.okay = true;
			}
			else
			{
				readDiagString = true; // look for a reason after the FAIL
				resp.okay = false;
			}

			// not a loop -- use "while" so we can use "break"
			try
			{
				while (readDiagString)
				{
					// length string is in next 4 bytes
					var lenBuf = new byte[4];
					read(chan, lenBuf);

					string lenStr = replyToString(lenBuf);

					int len;
					try
					{
						len = Convert.ToInt32(lenStr, 16);
					}
                    catch (SystemException)
					{
						Log.w("ddms", "Expected digits, got '" + lenStr + "': " + lenBuf[0] + " " + lenBuf[1] + " " + lenBuf[2] + " " + lenBuf[3]);
						Log.w("ddms", "reply was " + replyToString(reply));
						break;
					}

					var msg = new byte[len];
					read(chan, msg);

					resp.message = replyToString(msg);
					Log.v("ddms", "Got reply '" + replyToString(reply) + "', diag='" + resp.message + "'");

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
		/// Retrieve the frame buffer from the device. </summary>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static RawImage getFrameBuffer(java.net.InetSocketAddress adbSockAddr, Device device) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		internal static RawImage getFrameBuffer(EndPoint adbSockAddr, Device device)
		{

			RawImage imageParams = new RawImage();
			var request = formAdbRequest("framebuffer:"); //$NON-NLS-1$
			byte[] nudge = {0};
			byte[] reply;

			SocketChannel adbChan = null;
			try
			{
				adbChan = SocketChannel.open(adbSockAddr);
				adbChan.configureBlocking(false);

				// if the device is not -1, then we first tell adb we're looking to talk
				// to a specific device
				setDevice(adbChan, device);

				write(adbChan, request);

				AdbResponse resp = readAdbResponse(adbChan, false); // readDiagString
				if (resp.okay == false)
				{
					throw new AdbCommandRejectedException(resp.message);
				}

				// first the protocol version.
				reply = new byte[4];
				read(adbChan, reply);

				ByteBuffer buf = ByteBuffer.wrap(reply);
				buf.order = ByteOrder.LITTLE_ENDIAN;

                int version = buf.getInt();

				// get the header size (this is a count of int)
				int headerSize = RawImage.getHeaderSize(version);

				// read the header
				reply = new byte[headerSize * 4];
				read(adbChan, reply);

				buf = ByteBuffer.wrap(reply);
				buf.order = ByteOrder.LITTLE_ENDIAN;

				// fill the RawImage with the header
				if (imageParams.readHeader(version, buf) == false)
				{
					Log.e("Screenshot", "Unsupported protocol: " + version);
					return null;
				}

				Log.d("ddms", "image params: bpp=" + imageParams.bpp + ", size=" + imageParams.size + ", width=" + imageParams.width + ", height=" + imageParams.height);

				write(adbChan, nudge);

				reply = new byte[imageParams.size];
				read(adbChan, reply);

				imageParams.data = reply;
			}
			finally
			{
				if (adbChan != null)
				{
					adbChan.close();
				}
			}

			return imageParams;
		}

		/// <summary>
		/// Executes a shell command on the device and retrieve the output. The output is
		/// handed to <var>rcvr</var> as it arrives.
		/// </summary>
		/// <param name="adbSockAddr"> the <seealso cref="InetSocketAddress"/> to adb. </param>
		/// <param name="command"> the shell command to execute </param>
		/// <param name="device"> the <seealso cref="IDevice"/> on which to execute the command. </param>
		/// <param name="rcvr"> the <seealso cref="IShellOutputReceiver"/> that will receives the output of the shell
		///            command </param>
		/// <param name="maxTimeToOutputResponse"> max time between command output. If more time passes
		///            between command output, the method will throw
		///            <seealso cref="ShellCommandUnresponsiveException"/>. A value of 0 means the method will
		///            wait forever for command output and never throw. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection when sending the command. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="ShellCommandUnresponsiveException"> in case the shell command doesn't send any output
		///            for a period longer than <var>maxTimeToOutputResponse</var>. </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection.
		/// </exception>
		/// <seealso cref= DdmPreferences#getTimeOut() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void executeRemoteCommand(java.net.InetSocketAddress adbSockAddr, String command, IDevice device, IShellOutputReceiver rcvr, int maxTimeToOutputResponse) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException
		internal static void executeRemoteCommand(EndPoint adbSockAddr, string command, IDevice device, IShellOutputReceiver rcvr, int maxTimeToOutputResponse)
		{
			Log.v("ddms", "execute: running " + command);

			SocketChannel adbChan = null;
			try
			{
				adbChan = SocketChannel.open(adbSockAddr);
				adbChan.configureBlocking(false);

				// if the device is not -1, then we first tell adb we're looking to
				// talk
				// to a specific device
				setDevice(adbChan, device);

				var request = formAdbRequest("shell:" + command); //$NON-NLS-1$
				write(adbChan, request);

				AdbResponse resp = readAdbResponse(adbChan, false); // readDiagString
				if (resp.okay == false)
				{
					Log.e("ddms", "ADB rejected shell command (" + command + "): " + resp.message);
					throw new AdbCommandRejectedException(resp.message);
				}

				var data = new byte[16384];
				ByteBuffer buf = ByteBuffer.wrap(data);
				int timeToResponseCount = 0;
				while (true)
				{
					int count;

					if (rcvr != null && rcvr.cancelled)
					{
						Log.v("ddms", "execute: cancelled");
						break;
					}

					count = adbChan.read(buf);
					if (count < 0)
					{
						// we're at the end, we flush the output
						rcvr.flush();
						Log.v("ddms", "execute '" + command + "' on '" + device + "' : EOF hit. Read: " + count);
						break;
					}
                    else if (count == 0)
                    {
                        int wait = WAIT_TIME*5;
                        timeToResponseCount += wait;
                        if (maxTimeToOutputResponse > 0 && timeToResponseCount > maxTimeToOutputResponse)
                        {
                            throw new ShellCommandUnresponsiveException();
                        }
                        Thread.Sleep(wait);
                    }
                    else
                    {
                        // reset timeout
                        timeToResponseCount = 0;

                        // send data to receiver if present
                        if (rcvr != null)
                        {
                            rcvr.addOutput(buf.array(), buf.arrayOffset(), buf.position);
                        }
                        buf.rewind();
                    }
				}
			}
			finally
			{
				if (adbChan != null)
				{
					adbChan.close();
				}
				Log.v("ddms", "execute: returning");
			}
		}

		/// <summary>
		/// Runs the Event log service on the <seealso cref="Device"/>, and provides its output to the
		/// <seealso cref="LogReceiver"/>.
		/// <p/>This call is blocking until <seealso cref="LogReceiver#isCancelled()"/> returns true. </summary>
		/// <param name="adbSockAddr"> the socket address to connect to adb </param>
		/// <param name="device"> the Device on which to run the service </param>
		/// <param name="rcvr"> the <seealso cref="LogReceiver"/> to receive the log output </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void runEventLogService(java.net.InetSocketAddress adbSockAddr, Device device, com.android.ddmlib.log.LogReceiver rcvr) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public static void runEventLogService(EndPoint adbSockAddr, Device device, LogReceiver rcvr)
		{
			runLogService(adbSockAddr, device, "events", rcvr); //$NON-NLS-1$
		}

		/// <summary>
		/// Runs a log service on the <seealso cref="Device"/>, and provides its output to the <seealso cref="LogReceiver"/>.
		/// <p/>This call is blocking until <seealso cref="LogReceiver#isCancelled()"/> returns true. </summary>
		/// <param name="adbSockAddr"> the socket address to connect to adb </param>
		/// <param name="device"> the Device on which to run the service </param>
		/// <param name="logName"> the name of the log file to output </param>
		/// <param name="rcvr"> the <seealso cref="LogReceiver"/> to receive the log output </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void runLogService(java.net.InetSocketAddress adbSockAddr, Device device, String logName, com.android.ddmlib.log.LogReceiver rcvr) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public static void runLogService(EndPoint adbSockAddr, Device device, string logName, LogReceiver rcvr)
		{
			SocketChannel adbChan = null;

			try
			{
				adbChan = SocketChannel.open(adbSockAddr);
				adbChan.configureBlocking(false);

				// if the device is not -1, then we first tell adb we're looking to talk
				// to a specific device
				setDevice(adbChan, device);

				var request = formAdbRequest("log:" + logName);
				write(adbChan, request);

				AdbResponse resp = readAdbResponse(adbChan, false); // readDiagString
				if (resp.okay == false)
				{
					throw new AdbCommandRejectedException(resp.message);
				}

				var data = new byte[16384];
				ByteBuffer buf = ByteBuffer.wrap(data);
				while (true)
				{
					int count;

					if (rcvr != null && rcvr.cancelled)
					{
						break;
					}

					count = adbChan.read(buf);
					if (count < 0)
					{
						break;
					}
                    else if (count == 0)
                    {
                        Thread.Sleep(WAIT_TIME*5);
                    }
                    else
                    {
                        if (rcvr != null)
                        {
                            rcvr.parseNewData(buf.array(), buf.arrayOffset(), buf.position);
                        }
                        buf.rewind();
                    }
				}
			}
			finally
			{
				if (adbChan != null)
				{
					adbChan.close();
				}
			}
		}

		/// <summary>
		/// Creates a port forwarding between a local and a remote port. </summary>
		/// <param name="adbSockAddr"> the socket address to connect to adb </param>
		/// <param name="device"> the device on which to do the port fowarding </param>
		/// <param name="localPort"> the local port to forward </param>
		/// <param name="remotePort"> the remote port. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void createForward(java.net.InetSocketAddress adbSockAddr, Device device, int localPort, int remotePort) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public static void createForward(EndPoint adbSockAddr, Device device, int localPort, int remotePort)
		{

			SocketChannel adbChan = null;
			try
			{
				adbChan = SocketChannel.open(adbSockAddr);
				adbChan.configureBlocking(false);

				var request = formAdbRequest(string.Format("host-serial:{0}:forward:tcp:{1:D};tcp:{2:D}", device.serialNumber, localPort, remotePort)); //$NON-NLS-1$

				write(adbChan, request);

				AdbResponse resp = readAdbResponse(adbChan, false); // readDiagString
				if (resp.okay == false)
				{
					Log.w("create-forward", "Error creating forward: " + resp.message);
					throw new AdbCommandRejectedException(resp.message);
				}
			}
			finally
			{
				if (adbChan != null)
				{
					adbChan.close();
				}
			}
		}

		/// <summary>
		/// Remove a port forwarding between a local and a remote port. </summary>
		/// <param name="adbSockAddr"> the socket address to connect to adb </param>
		/// <param name="device"> the device on which to remove the port fowarding </param>
		/// <param name="localPort"> the local port of the forward </param>
		/// <param name="remotePort"> the remote port. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void removeForward(java.net.InetSocketAddress adbSockAddr, Device device, int localPort, int remotePort) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public static void removeForward(EndPoint adbSockAddr, Device device, int localPort, int remotePort)
		{

			SocketChannel adbChan = null;
			try
			{
				adbChan = SocketChannel.open(adbSockAddr);
				adbChan.configureBlocking(false);

				var request = formAdbRequest(string.Format("host-serial:{0}:killforward:tcp:{1:D};tcp:{2:D}", device.serialNumber, localPort, remotePort)); //$NON-NLS-1$

				write(adbChan, request);

				AdbResponse resp = readAdbResponse(adbChan, false); // readDiagString
				if (resp.okay == false)
				{
					Log.w("remove-forward", "Error creating forward: " + resp.message);
					throw new AdbCommandRejectedException(resp.message);
				}
			}
			finally
			{
				if (adbChan != null)
				{
					adbChan.close();
				}
			}
		}

		/// <summary>
		/// Checks to see if the first four bytes in "reply" are OKAY.
		/// </summary>
		internal static bool isOkay(byte[] reply)
		{
			return reply[0] == (byte)'O' && reply[1] == (byte)'K' && reply[2] == (byte)'A' && reply[3] == (byte)'Y';
		}

		/// <summary>
		/// Converts an ADB reply to a string.
		/// </summary>
		internal static string replyToString(byte[] reply)
		{
			string result;
			try
			{
				result = reply.getString(DEFAULT_ENCODING);
			}
			catch (ArgumentException uee)
			{
				Console.WriteLine(uee.ToString());
				Console.Write(uee.StackTrace); // not expected
				result = "";
			}
			return result;
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void read(java.nio.channels.SocketChannel chan, byte[] data) throws TimeoutException, java.io.IOException
		internal static void read(SocketChannel chan, byte[] data)
		{
			read(chan, data, -1, DdmPreferences.timeOut);
		}

		/// <summary>
		/// Reads from the socket until the array is filled, the optional length
		/// is reached, or no more data is coming (because the socket closed or the
		/// timeout expired). After "timeout" milliseconds since the
		/// previous successful read, this will return whether or not new data has
		/// been found.
		/// </summary>
		/// <param name="chan"> the opened socket to read from. It must be in non-blocking
		///      mode for timeouts to work </param>
		/// <param name="data"> the buffer to store the read data into. </param>
		/// <param name="length"> the length to read or -1 to fill the data buffer completely </param>
		/// <param name="timeout"> The timeout value. A timeout of zero means "wait forever". </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void read(java.nio.channels.SocketChannel chan, byte[] data, int length, int timeout) throws TimeoutException, java.io.IOException
		internal static void read(SocketChannel chan, byte[] data, int length, int timeout)
		{
			ByteBuffer buf = ByteBuffer.wrap(data, 0, length != -1 ? length : data.Length);
			int numWaits = 0;

			while (buf.position != buf.limit)
			{
				int count;

				count = chan.read(buf);
				if (count < 0)
				{
					Log.d("ddms", "read: channel EOF");
					throw new IOException("EOF");
				}
				else if (count == 0)
				{
					// TODO: need more accurate timeout?
					if (timeout != 0 && numWaits * WAIT_TIME > timeout)
					{
						Log.d("ddms", "read: timeout");
						throw new TimeoutException();
					}
					// non-blocking spin
                    Thread.Sleep(WAIT_TIME);
					numWaits++;
				}
				else
				{
					numWaits = 0;
				}
			}
		}

		/// <summary>
		/// Write until all data in "data" is written or the connection fails or times out.
		/// <p/>This uses the default time out value. </summary>
		/// <param name="chan"> the opened socket to write to. </param>
		/// <param name="data"> the buffer to send. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void write(java.nio.channels.SocketChannel chan, byte[] data) throws TimeoutException, java.io.IOException
		internal static void write(SocketChannel chan, byte[] data)
		{
			write(chan, data, -1, DdmPreferences.timeOut);
		}

		/// <summary>
		/// Write until all data in "data" is written, the optional length is reached,
		/// the timeout expires, or the connection fails. Returns "true" if all
		/// data was written. </summary>
		/// <param name="chan"> the opened socket to write to. </param>
		/// <param name="data"> the buffer to send. </param>
		/// <param name="length"> the length to write or -1 to send the whole buffer. </param>
		/// <param name="timeout"> The timeout value. A timeout of zero means "wait forever". </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void write(java.nio.channels.SocketChannel chan, byte[] data, int length, int timeout) throws TimeoutException, java.io.IOException
		internal static void write(SocketChannel chan, byte[] data, int length, int timeout)
		{
			ByteBuffer buf = ByteBuffer.wrap(data, 0, length != -1 ? length : data.Length);
			int numWaits = 0;

			while (buf.position != buf.limit)
			{
				int count;

				count = chan.write(buf);
				if (count < 0)
				{
					Log.d("ddms", "write: channel EOF");
					throw new IOException("channel EOF");
				}
				else if (count == 0)
				{
					// TODO: need more accurate timeout?
					if (timeout != 0 && numWaits * WAIT_TIME > timeout)
					{
						Log.d("ddms", "write: timeout");
						throw new TimeoutException();
					}
					// non-blocking spin
                    Thread.Sleep(WAIT_TIME);
					numWaits++;
				}
				else
				{
					numWaits = 0;
				}
			}
		}

		/// <summary>
		/// tells adb to talk to a specific device
		/// </summary>
		/// <param name="adbChan"> the socket connection to adb </param>
		/// <param name="device"> The device to talk to. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void setDevice(java.nio.channels.SocketChannel adbChan, IDevice device) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		internal static void setDevice(SocketChannel adbChan, IDevice device)
		{
			// if the device is not -1, then we first tell adb we're looking to talk
			// to a specific device
			if (device != null)
			{
				string msg = "host:transport:" + device.serialNumber; //$NON-NLS-1$
				var device_query = formAdbRequest(msg);

				write(adbChan, device_query);

				AdbResponse resp = readAdbResponse(adbChan, false); // readDiagString
				if (resp.okay == false)
				{
					throw new AdbCommandRejectedException(resp.message, true); //errorDuringDeviceSelection
				}
			}
		}

		/// <summary>
		/// Reboot the device.
		/// </summary>
		/// <param name="into"> what to reboot into (recovery, bootloader).  Or null to just reboot. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void reboot(String into, java.net.InetSocketAddress adbSockAddr, Device device) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public static void reboot(string into, EndPoint adbSockAddr, Device device)
		{
			byte[] request;
			if (into == null)
			{
				request = formAdbRequest("reboot:"); //$NON-NLS-1$
			}
			else
			{
				request = formAdbRequest("reboot:" + into); //$NON-NLS-1$
			}

			SocketChannel adbChan = null;
			try
			{
				adbChan = SocketChannel.open(adbSockAddr);
				adbChan.configureBlocking(false);

				// if the device is not -1, then we first tell adb we're looking to talk
				// to a specific device
				setDevice(adbChan, device);

				write(adbChan, request);
			}
			finally
			{
				if (adbChan != null)
				{
					adbChan.close();
				}
			}
		}
	}

}