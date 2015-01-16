using System.Diagnostics;
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
	/// Handle the "wait" chunk (WAIT).  These are sent up when the client is
	/// waiting for something, e.g. for a debugger to attach.
	/// </summary>
	internal sealed class HandleWait : ChunkHandler
	{

		public static readonly int CHUNK_WAIT = ChunkHandler.type("WAIT");

		private static readonly HandleWait mInst = new HandleWait();


		private HandleWait()
		{
		}

		/// <summary>
		/// Register for the packets we expect to get from the client.
		/// </summary>
		public static void register(MonitorThread mt)
		{
			mt.registerChunkHandler(CHUNK_WAIT, mInst);
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

			Log.d("ddm-wait", "handling " + ChunkHandler.name(type));

			if (type == CHUNK_WAIT)
			{
				Debug.Assert(!isReply);
				handleWAIT(client, data);
			}
			else
			{
				handleUnknownChunk(client, type, data, isReply, msgId);
			}
		}

		/*
		 * Handle a reply to our WAIT message.
		 */
		private static void handleWAIT(Client client, ByteBuffer data)
		{
		    var reason = data.get();

			Log.d("ddm-wait", "WAIT: reason=" + reason);


			ClientData cd = client.clientData;
			lock (cd)
			{
				cd.debuggerConnectionStatus = ClientData.DebuggerStatus.WAITING;
			}

			client.update(Client.CHANGE_DEBUGGER_STATUS);
		}
	}


}