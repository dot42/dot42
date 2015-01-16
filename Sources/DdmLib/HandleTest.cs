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
	/// Handle thread status updates.
	/// </summary>
	internal sealed class HandleTest : ChunkHandler
	{

		public static readonly int CHUNK_TEST = type("TEST");

		private static readonly HandleTest mInst = new HandleTest();


		private HandleTest()
		{
		}

		/// <summary>
		/// Register for the packets we expect to get from the client.
		/// </summary>
		public static void register(MonitorThread mt)
		{
			mt.registerChunkHandler(CHUNK_TEST, mInst);
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

			Log.d("ddm-test", "handling " + ChunkHandler.name(type));

			if (type == CHUNK_TEST)
			{
				handleTEST(client, data);
			}
			else
			{
				handleUnknownChunk(client, type, data, isReply, msgId);
			}
		}

		/*
		 * Handle a thread creation message.
		 */
		private void handleTEST(Client client, ByteBuffer data)
		{
			/*
			 * Can't call data.array() on a read-only ByteBuffer, so we make
			 * a copy.
			 */
			var copy = new byte[data.limit];
			data.get(copy);

			Log.d("ddm-test", "Received:");
			Log.hexDump("ddm-test", Log.LogLevel.DEBUG, copy, 0, copy.Length);
		}
	}


}