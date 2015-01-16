using System.Diagnostics;
using System.IO;
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
	/// Handle the "hello" chunk (HELO) and feature discovery.
	/// </summary>
	internal sealed class HandleHello : ChunkHandler
	{

		public static readonly int CHUNK_HELO = ChunkHandler.type("HELO");
		public static readonly int CHUNK_FEAT = ChunkHandler.type("FEAT");

		private static readonly HandleHello mInst = new HandleHello();

		private HandleHello()
		{
		}

		/// <summary>
		/// Register for the packets we expect to get from the client.
		/// </summary>
		public static void register(MonitorThread mt)
		{
			mt.registerChunkHandler(CHUNK_HELO, mInst);
		}

		/// <summary>
		/// Client is ready.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void clientReady(Client client) throws java.io.IOException
		internal override void clientReady(Client client)
		{
			Log.d("ddm-hello", "Now ready: " + client);
		}

		/// <summary>
		/// Client went away.
		/// </summary>
		internal override void clientDisconnected(Client client)
		{
			Log.d("ddm-hello", "Now disconnected: " + client);
		}

		/// <summary>
		/// Sends HELLO-type commands to the VM after a good handshake. </summary>
		/// <param name="client"> </param>
		/// <param name="serverProtocolVersion"> </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendHelloCommands(Client client, int serverProtocolVersion) throws java.io.IOException
		public static void sendHelloCommands(Client client, int serverProtocolVersion)
		{
			sendHELO(client, serverProtocolVersion);
			sendFEAT(client);
			HandleProfiling.sendMPRQ(client);
		}

		/// <summary>
		/// Chunk handler entry point.
		/// </summary>
		internal override void handleChunk(Client client, int type, ByteBuffer data, bool isReply, int msgId)
		{

			Log.d("ddm-hello", "handling " + ChunkHandler.name(type));

			if (type == CHUNK_HELO)
			{
				Debug.Assert(isReply);
				handleHELO(client, data);
			}
			else if (type == CHUNK_FEAT)
			{
				handleFEAT(client, data);
			}
			else
			{
				handleUnknownChunk(client, type, data, isReply, msgId);
			}
		}

		/*
		 * Handle a reply to our HELO message.
		 */
		private static void handleHELO(Client client, ByteBuffer data)
		{
			int version, pid, vmIdentLen, appNameLen;
			string vmIdent, appName;

            version = data.getInt();
            pid = data.getInt();
            vmIdentLen = data.getInt();
            appNameLen = data.getInt();

			vmIdent = getString(data, vmIdentLen);
			appName = getString(data, appNameLen);

			Log.d("ddm-hello", "HELO: v=" + version + ", pid=" + pid + ", vm='" + vmIdent + "', app='" + appName + "'");

			ClientData cd = client.clientData;

			lock (cd)
			{
				if (cd.pid == pid)
				{
					cd.vmIdentifier = vmIdent;
					cd.clientDescription = appName;
					cd.isDdmAware(true);
				}
				else
				{
					Log.e("ddm-hello", "Received pid (" + pid + ") does not match client pid (" + cd.pid + ")");
				}
			}

			client = checkDebuggerPortForAppName(client, appName);

			if (client != null)
			{
				client.update(Client.CHANGE_NAME);
			}
		}


		/// <summary>
		/// Send a HELO request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendHELO(Client client, int serverProtocolVersion) throws java.io.IOException
		public static void sendHELO(Client client, int serverProtocolVersion)
		{
			ByteBuffer rawBuf = allocBuffer(4);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			buf.putInt(serverProtocolVersion);

			finishChunkPacket(packet, CHUNK_HELO, buf.position);
			Log.d("ddm-hello", "Sending " + name(CHUNK_HELO) + " ID=0x" + packet.id.toHexString());
			client.sendAndConsume(packet, mInst);
		}

		/// <summary>
		/// Handle a reply to our FEAT request.
		/// </summary>
		private static void handleFEAT(Client client, ByteBuffer data)
		{
			int featureCount;
			int i;

            featureCount = data.getInt();
			for (i = 0; i < featureCount; i++)
			{
                int len = data.getInt();
				string feature = getString(data, len);
				client.clientData.addFeature(feature);

				Log.d("ddm-hello", "Feature: " + feature);
			}
		}

		/// <summary>
		/// Send a FEAT request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendFEAT(Client client) throws java.io.IOException
		public static void sendFEAT(Client client)
		{
			ByteBuffer rawBuf = allocBuffer(0);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			// no data

			finishChunkPacket(packet, CHUNK_FEAT, buf.position);
			Log.d("ddm-heap", "Sending " + name(CHUNK_FEAT));
			client.sendAndConsume(packet, mInst);
		}
	}


}