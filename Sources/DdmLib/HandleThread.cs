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

using System.IO;
using System.Threading;
using Dot42.DdmLib.support;

namespace Dot42.DdmLib
{


	/// <summary>
	/// Handle thread status updates.
	/// </summary>
	internal sealed class HandleThread : ChunkHandler
	{

		public static readonly int CHUNK_THEN = type("THEN");
		public static readonly int CHUNK_THCR = type("THCR");
		public static readonly int CHUNK_THDE = type("THDE");
		public static readonly int CHUNK_THST = type("THST");
		public static readonly int CHUNK_THNM = type("THNM");
		public static readonly int CHUNK_STKL = type("STKL");

		private static readonly HandleThread mInst = new HandleThread();

		// only read/written by requestThreadUpdates()
		private static volatile bool mThreadStatusReqRunning = false;
		private static volatile bool mThreadStackTraceReqRunning = false;

		private HandleThread()
		{
		}


		/// <summary>
		/// Register for the packets we expect to get from the client.
		/// </summary>
		public static void register(MonitorThread mt)
		{
			mt.registerChunkHandler(CHUNK_THCR, mInst);
			mt.registerChunkHandler(CHUNK_THDE, mInst);
			mt.registerChunkHandler(CHUNK_THST, mInst);
			mt.registerChunkHandler(CHUNK_THNM, mInst);
			mt.registerChunkHandler(CHUNK_STKL, mInst);
		}

		/// <summary>
		/// Client is ready.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void clientReady(Client client) throws java.io.IOException
		internal override void clientReady(Client client)
		{
			Log.d("ddm-thread", "Now ready: " + client);
			if (client.threadUpdateEnabled)
			{
				sendTHEN(client, true);
			}
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

			Log.d("ddm-thread", "handling " + ChunkHandler.name(type));

			if (type == CHUNK_THCR)
			{
				handleTHCR(client, data);
			}
			else if (type == CHUNK_THDE)
			{
				handleTHDE(client, data);
			}
			else if (type == CHUNK_THST)
			{
				handleTHST(client, data);
			}
			else if (type == CHUNK_THNM)
			{
				handleTHNM(client, data);
			}
			else if (type == CHUNK_STKL)
			{
				handleSTKL(client, data);
			}
			else
			{
				handleUnknownChunk(client, type, data, isReply, msgId);
			}
		}

		/*
		 * Handle a thread creation message.
		 *
		 * We should be tolerant of receiving a duplicate create message.  (It
		 * shouldn't happen with the current implementation.)
		 */
		private void handleTHCR(Client client, ByteBuffer data)
		{
			int threadId, nameLen;
			string name;

			threadId = data.getInt();
            nameLen = data.getInt();
			name = getString(data, nameLen);

			Log.v("ddm-thread", "THCR: " + threadId + " '" + name + "'");

			client.clientData.addThread(threadId, name);
			client.update(Client.CHANGE_THREAD_DATA);
		}

		/*
		 * Handle a thread death message.
		 */
		private void handleTHDE(Client client, ByteBuffer data)
		{
			int threadId;

            threadId = data.getInt();
			Log.v("ddm-thread", "THDE: " + threadId);

			client.clientData.removeThread(threadId);
			client.update(Client.CHANGE_THREAD_DATA);
		}

		/*
		 * Handle a thread status update message.
		 *
		 * Response has:
		 *  (1b) header len
		 *  (1b) bytes per entry
		 *  (2b) thread count
		 * Then, for each thread:
		 *  (4b) threadId (matches value from THCR)
		 *  (1b) thread status
		 *  (4b) tid
		 *  (4b) utime
		 *  (4b) stime
		 */
		private void handleTHST(Client client, ByteBuffer data)
		{
			int headerLen, bytesPerEntry, extraPerEntry;
			int threadCount;

			headerLen = (data.get() & 0xff);
			bytesPerEntry = (data.get() & 0xff);
            threadCount = data.getShort();

			headerLen -= 4; // we've read 4 bytes
			while (headerLen-- > 0)
			{
				data.get();
			}

			extraPerEntry = bytesPerEntry - 18; // we want 18 bytes

			Log.v("ddm-thread", "THST: threadCount=" + threadCount);

			/*
			 * For each thread, extract the data, find the appropriate
			 * client, and add it to the ClientData.
			 */
			for (int i = 0; i < threadCount; i++)
			{
				int threadId, status, tid, utime, stime;
				bool isDaemon = false;

                threadId = data.getInt();
				status = data.get();
                tid = data.getInt();
                utime = data.getInt();
                stime = data.getInt();
				if (bytesPerEntry >= 18)
				{
					isDaemon = (data.get() != 0);
				}

				Log.v("ddm-thread", "  id=" + threadId + ", status=" + status + ", tid=" + tid + ", utime=" + utime + ", stime=" + stime);

				ClientData cd = client.clientData;
				ThreadInfo threadInfo = cd.getThread(threadId);
				if (threadInfo != null)
				{
					threadInfo.updateThread(status, tid, utime, stime, isDaemon);
				}
				else
				{
					Log.d("ddms", "Thread with id=" + threadId + " not found");
				}

				// slurp up any extra
				for (int slurp = extraPerEntry; slurp > 0; slurp--)
				{
					data.get();
				}
			}

			client.update(Client.CHANGE_THREAD_DATA);
		}

		/*
		 * Handle a THNM (THread NaMe) message.  We get one of these after
		 * somebody calls Thread.setName() on a running thread.
		 */
		private void handleTHNM(Client client, ByteBuffer data)
		{
			int threadId, nameLen;
			string name;

            threadId = data.getInt();
            nameLen = data.getInt();
			name = getString(data, nameLen);

			Log.v("ddm-thread", "THNM: " + threadId + " '" + name + "'");

			ThreadInfo threadInfo = client.clientData.getThread(threadId);
			if (threadInfo != null)
			{
				threadInfo.threadName = name;
				client.update(Client.CHANGE_THREAD_DATA);
			}
			else
			{
				Log.d("ddms", "Thread with id=" + threadId + " not found");
			}
		}


		/// <summary>
		/// Parse an incoming STKL.
		/// </summary>
		private void handleSTKL(Client client, ByteBuffer data)
		{
			StackTraceElement[] trace;
			int i, threadId, stackDepth;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") int future;
			int future;

            future = data.getInt();
            threadId = data.getInt();

			Log.v("ddms", "STKL: " + threadId);

			/* un-serialize the StackTraceElement[] */
            stackDepth = data.getInt();
			trace = new StackTraceElement[stackDepth];
			for (i = 0; i < stackDepth; i++)
			{
				string className, methodName, fileName;
				int len, lineNumber;

                len = data.getInt();
				className = getString(data, len);
                len = data.getInt();
				methodName = getString(data, len);
                len = data.getInt();
				if (len == 0)
				{
					fileName = null;
				}
				else
				{
					fileName = getString(data, len);
				}
                lineNumber = data.getInt();

				trace[i] = new StackTraceElement(className, methodName, fileName, lineNumber);
			}

			ThreadInfo threadInfo = client.clientData.getThread(threadId);
			if (threadInfo != null)
			{
				threadInfo.stackCall = trace;
				client.update(Client.CHANGE_THREAD_STACKTRACE);
			}
			else
			{
				Log.d("STKL", string.Format("Got stackcall for thread {0:D}, which does not exists (anymore?).", threadId)); //$NON-NLS-1$
			}
		}


		/// <summary>
		/// Send a THEN (THread notification ENable) request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendTHEN(Client client, boolean enable) throws java.io.IOException
		public static void sendTHEN(Client client, bool enable)
		{

			ByteBuffer rawBuf = allocBuffer(1);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			if (enable)
			{
				buf.put(1);
			}
			else
			{
				buf.put(0);
			}

			finishChunkPacket(packet, CHUNK_THEN, buf.position);
			Log.d("ddm-thread", "Sending " + name(CHUNK_THEN) + ": " + enable);
			client.sendAndConsume(packet, mInst);
		}


		/// <summary>
		/// Send a STKL (STacK List) request to the client.  The VM will suspend
		/// the target thread, obtain its stack, and return it.  If the thread
		/// is no longer running, a failure result will be returned.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendSTKL(Client client, int threadId) throws java.io.IOException
		public static void sendSTKL(Client client, int threadId)
		{
            /*
			if (false)
			{
				Log.d("ddm-thread", "would send STKL " + threadId);
				return;
			}
            */

			ByteBuffer rawBuf = allocBuffer(4);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			buf.putInt(threadId);

			finishChunkPacket(packet, CHUNK_STKL, buf.position);
			Log.d("ddm-thread", "Sending " + name(CHUNK_STKL) + ": " + threadId);
			client.sendAndConsume(packet, mInst);
		}


		/// <summary>
		/// This is called periodically from the UI thread.  To avoid locking
		/// the UI while we request the updates, we create a new thread.
		/// 
		/// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: static void requestThreadUpdate(final Client client)
		internal static void requestThreadUpdate(Client client)
		{
			if (client.ddmAware && client.threadUpdateEnabled)
			{
				if (mThreadStatusReqRunning)
				{
					Log.w("ddms", "Waiting for previous thread update req to finish");
					return;
				}

                ThreadStart threadRun = () => {
						mThreadStatusReqRunning = true;
						try
						{
							sendTHST(client);
						}
						catch (IOException ioe)
						{
							Log.d("ddms", "Unable to request thread updates from " + client + ": " + ioe.Message);
						}
						finally
						{
							mThreadStatusReqRunning = false;
						}
                };
                var thread = new Thread(threadRun);
                thread.Start();
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: static void requestThreadStackCallRefresh(final Client client, final int threadId)
		internal static void requestThreadStackCallRefresh(Client client, int threadId)
		{
			if (client.ddmAware && client.threadUpdateEnabled)
			{
				if (mThreadStackTraceReqRunning)
				{
					Log.w("ddms", "Waiting for previous thread stack call req to finish");
					return;
				}

                ThreadStart threadRun = () => {
                                            mThreadStackTraceReqRunning = true;
                                            try
                                            {
                                                sendSTKL(client, threadId);
                                            }
                                            catch (IOException ioe)
                                            {
                                                Log.d("ddms",
                                                      "Unable to request thread stack call updates from " + client +
                                                      ": " + ioe.Message);
                                            }
                                            finally
                                            {
                                                mThreadStackTraceReqRunning = false;
                                            }
                                        };
                var thread = new Thread(threadRun) { Name = "Thread Status Req"};
			    thread.Start();
			}

		}

		/*
		 * Send a THST request to the specified client.
		 */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void sendTHST(Client client) throws java.io.IOException
		private static void sendTHST(Client client)
		{
			ByteBuffer rawBuf = allocBuffer(0);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			// nothing much to say

			finishChunkPacket(packet, CHUNK_THST, buf.position);
			Log.d("ddm-thread", "Sending " + name(CHUNK_THST));
			client.sendAndConsume(packet, mInst);
		}
	}


}