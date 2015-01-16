/*
 * Copyright (C) 2009 The Android Open Source Project
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

using System.IO;
using System.Text;
using Dot42.DdmLib.support;

namespace Dot42.DdmLib
{
    /// <summary>
	/// Handle heap status updates.
	/// </summary>
	internal sealed class HandleProfiling : ChunkHandler
	{

		public static readonly int CHUNK_MPRS = type("MPRS");
		public static readonly int CHUNK_MPRE = type("MPRE");
		public static readonly int CHUNK_MPSS = type("MPSS");
		public static readonly int CHUNK_MPSE = type("MPSE");
		public static readonly int CHUNK_MPRQ = type("MPRQ");
		public new static readonly int CHUNK_FAIL = type("FAIL");

		private static readonly HandleProfiling mInst = new HandleProfiling();

		private HandleProfiling()
		{
		}

		/// <summary>
		/// Register for the packets we expect to get from the client.
		/// </summary>
		public static void register(MonitorThread mt)
		{
			mt.registerChunkHandler(CHUNK_MPRE, mInst);
			mt.registerChunkHandler(CHUNK_MPSE, mInst);
			mt.registerChunkHandler(CHUNK_MPRQ, mInst);
		}

		/// <summary>
		/// Client is ready.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void clientReady(Client client) throws java.io.IOException
		
        internal override void clientReady(Client client)
		{
		}
        
        /// <summary>
        /// Client went away.
        /// </summary>
        internal override void clientDisconnected(Client client)
		{
		}

        /// <summary>
        /// Chunk handler entry point.
        /// </summary>
        internal override void handleChunk(Client client, int type, ByteBuffer data, bool isReply, int msgId)
		{

			Log.d("ddm-prof", "handling " + ChunkHandler.name(type));

			if (type == CHUNK_MPRE)
			{
				handleMPRE(client, data);
			}
			else if (type == CHUNK_MPSE)
			{
				handleMPSE(client, data);
			}
			else if (type == CHUNK_MPRQ)
			{
				handleMPRQ(client, data);
			}
			else if (type == CHUNK_FAIL)
			{
				handleFAIL(client, data);
			}
			else
			{
				handleUnknownChunk(client, type, data, isReply, msgId);
			}
		}

		/// <summary>
		/// Send a MPRS (Method PRofiling Start) request to the client.
		/// 
		/// The arguments to this method will eventually be passed to
		/// android.os.Debug.startMethodTracing() on the device.
		/// </summary>
		/// <param name="fileName"> is the name of the file to which profiling data
		///          will be written (on the device); it will have <seealso cref="DdmConstants#DOT_TRACE"/>
		///          appended if necessary </param>
		/// <param name="bufferSize"> is the desired buffer size in bytes (8MB is good) </param>
		/// <param name="flags"> see startMethodTracing() docs; use 0 for default behavior </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendMPRS(Client client, String fileName, int bufferSize, int flags) throws java.io.IOException
		public static void sendMPRS(Client client, string fileName, int bufferSize, int flags)
		{

			ByteBuffer rawBuf = allocBuffer(3 * 4 + fileName.Length * 2);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			buf.putInt(bufferSize);
			buf.putInt(flags);
			buf.putInt(fileName.Length);
			putString(buf, fileName);

			finishChunkPacket(packet, CHUNK_MPRS, buf.position);
			Log.d("ddm-prof", "Sending " + name(CHUNK_MPRS) + " '" + fileName + "', size=" + bufferSize + ", flags=" + flags);
			client.sendAndConsume(packet, mInst);

			// record the filename we asked for.
			client.clientData.pendingMethodProfiling = fileName;

			// send a status query. this ensure that the status is properly updated if for some
			// reason starting the tracing failed.
			sendMPRQ(client);
		}

		/// <summary>
		/// Send a MPRE (Method PRofiling End) request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendMPRE(Client client) throws java.io.IOException
		public static void sendMPRE(Client client)
		{
			ByteBuffer rawBuf = allocBuffer(0);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			// no data

			finishChunkPacket(packet, CHUNK_MPRE, buf.position);
			Log.d("ddm-prof", "Sending " + name(CHUNK_MPRE));
			client.sendAndConsume(packet, mInst);
		}

		/// <summary>
		/// Handle notification that method profiling has finished writing
		/// data to disk.
		/// </summary>
		private void handleMPRE(Client client, ByteBuffer data)
		{
		    // get the filename and make the client not have pending HPROF dump anymore.
			string filename = client.clientData.pendingMethodProfiling;
			client.clientData.pendingMethodProfiling = null;

			byte result = data.get();

			// get the app-level handler for method tracing dump
			ClientData.IMethodProfilingHandler handler = ClientData.methodProfilingHandler;
			if (handler != null)
			{
				if (result == 0)
				{
					handler.onSuccess(filename, client);

					Log.d("ddm-prof", "Method profiling has finished");
				}
				else
				{
					handler.onEndFailure(client, null); //message

					Log.w("ddm-prof", "Method profiling has failed (check device log)");
				}
			}

			client.clientData.methodProfilingStatus = ClientData.MethodProfilingStatus.OFF;
			client.update(Client.CHANGE_METHOD_PROFILING_STATUS);
		}

		/// <summary>
		/// Send a MPSS (Method Profiling Streaming Start) request to the client.
		/// 
		/// The arguments to this method will eventually be passed to
		/// android.os.Debug.startMethodTracing() on the device.
		/// </summary>
		/// <param name="bufferSize"> is the desired buffer size in bytes (8MB is good) </param>
		/// <param name="flags"> see startMethodTracing() docs; use 0 for default behavior </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendMPSS(Client client, int bufferSize, int flags) throws java.io.IOException
		public static void sendMPSS(Client client, int bufferSize, int flags)
		{

			ByteBuffer rawBuf = allocBuffer(2 * 4);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			buf.putInt(bufferSize);
			buf.putInt(flags);

			finishChunkPacket(packet, CHUNK_MPSS, buf.position);
			Log.d("ddm-prof", "Sending " + name(CHUNK_MPSS) + "', size=" + bufferSize + ", flags=" + flags);
			client.sendAndConsume(packet, mInst);

			// send a status query. this ensure that the status is properly updated if for some
			// reason starting the tracing failed.
			sendMPRQ(client);
		}

		/// <summary>
		/// Send a MPSE (Method Profiling Streaming End) request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendMPSE(Client client) throws java.io.IOException
		public static void sendMPSE(Client client)
		{
			ByteBuffer rawBuf = allocBuffer(0);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			// no data

			finishChunkPacket(packet, CHUNK_MPSE, buf.position);
			Log.d("ddm-prof", "Sending " + name(CHUNK_MPSE));
			client.sendAndConsume(packet, mInst);
		}

		/// <summary>
		/// Handle incoming profiling data.  The MPSE packet includes the
		/// complete .trace file.
		/// </summary>
		private void handleMPSE(Client client, ByteBuffer data)
		{
			ClientData.IMethodProfilingHandler handler = ClientData.methodProfilingHandler;
			if (handler != null)
			{
				var stuff = new byte[data.capacity];
				data.get(stuff, 0, stuff.Length);

				Log.d("ddm-prof", "got trace file, size: " + stuff.Length + " bytes");

				handler.onSuccess(stuff, client);
			}

			client.clientData.methodProfilingStatus = ClientData.MethodProfilingStatus.OFF;
			client.update(Client.CHANGE_METHOD_PROFILING_STATUS);
		}

		/// <summary>
		/// Send a MPRQ (Method PRofiling Query) request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendMPRQ(Client client) throws java.io.IOException
		public static void sendMPRQ(Client client)
		{
			ByteBuffer rawBuf = allocBuffer(0);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			// no data

			finishChunkPacket(packet, CHUNK_MPRQ, buf.position);
			Log.d("ddm-prof", "Sending " + name(CHUNK_MPRQ));
			client.sendAndConsume(packet, mInst);
		}

		/// <summary>
		/// Receive response to query.
		/// </summary>
		private void handleMPRQ(Client client, ByteBuffer data)
		{
		    var result = data.get();

			if (result == 0)
			{
				client.clientData.methodProfilingStatus = ClientData.MethodProfilingStatus.OFF;
				Log.d("ddm-prof", "Method profiling is not running");
			}
			else
			{
				client.clientData.methodProfilingStatus = ClientData.MethodProfilingStatus.ON;
				Log.d("ddm-prof", "Method profiling is running");
			}
			client.update(Client.CHANGE_METHOD_PROFILING_STATUS);
		}

		private void handleFAIL(Client client, ByteBuffer data)
		{
			/*int errorCode =*/	 data.getInt();
            int length = data.getInt() * 2;
			string message = null;
			if (length > 0)
			{
				var messageBuffer = new byte[length];
				data.get(messageBuffer, 0, length);
				message = Encoding.Default.GetString(messageBuffer);
			}

			// this can be sent if
			// - MPRS failed (like wrong permission)
			// - MPSE failed for whatever reason

			string filename = client.clientData.pendingMethodProfiling;
			if (filename != null)
			{
				// reset the pending file.
				client.clientData.pendingMethodProfiling = null;

				// and notify of failure
				ClientData.IMethodProfilingHandler handler = ClientData.methodProfilingHandler;
				if (handler != null)
				{
					handler.onStartFailure(client, message);
				}
			}
			else
			{
				// this is MPRE
				// notify of failure
				ClientData.IMethodProfilingHandler handler = ClientData.methodProfilingHandler;
				if (handler != null)
				{
					handler.onEndFailure(client, message);
				}
			}

			// send a query to know the current status
			try
			{
				sendMPRQ(client);
			}
			catch (IOException e)
			{
				Log.e("HandleProfiling", e);
			}
		}
	}


}