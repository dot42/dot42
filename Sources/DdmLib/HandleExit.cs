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

using Dot42.DdmLib.support;

namespace Dot42.DdmLib
{


	/// <summary>
	/// Submit an exit request.
	/// </summary>
	internal sealed class HandleExit : ChunkHandler
	{

		public static readonly int CHUNK_EXIT = type("EXIT");

		private static readonly HandleExit mInst = new HandleExit();


		private HandleExit()
		{
		}

		/// <summary>
		/// Register for the packets we expect to get from the client.
		/// </summary>
		public static void register(MonitorThread mt)
		{
		}

		/// <summary>
		/// Client is ready.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void clientReady(Client client) throws java.io.IOException
		internal override void clientReady(Client client)
		/// <summary>
		/// Client went away.
		/// </summary>
		{
		}

	    internal override void clientDisconnected(Client client)
		/// <summary>
		/// Chunk handler entry point.
		/// </summary>
		{
		}

	    internal override void handleChunk(Client client, int type, ByteBuffer data, bool isReply, int msgId)
		{
			handleUnknownChunk(client, type, data, isReply, msgId);
		}

		/// <summary>
		/// Send an EXIT request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendEXIT(Client client, int status) throws java.io.IOException
		public static void sendEXIT(Client client, int status)
		{
			ByteBuffer rawBuf = allocBuffer(4);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			buf.putInt(status);

			finishChunkPacket(packet, CHUNK_EXIT, buf.position);
			Log.d("ddm-exit", "Sending " + name(CHUNK_EXIT) + ": " + status);
			client.sendAndConsume(packet, mInst);
		}
	}


}