using System;
using System.Collections.Generic;
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
	/// Handle heap status updates.
	/// </summary>
	internal sealed class HandleHeap : ChunkHandler
	{

		public static readonly int CHUNK_HPIF = type("HPIF");
		public static readonly int CHUNK_HPST = type("HPST");
		public static readonly int CHUNK_HPEN = type("HPEN");
		public static readonly int CHUNK_HPSG = type("HPSG");
		public static readonly int CHUNK_HPGC = type("HPGC");
		public static readonly int CHUNK_HPDU = type("HPDU");
		public static readonly int CHUNK_HPDS = type("HPDS");
		public static readonly int CHUNK_REAE = type("REAE");
		public static readonly int CHUNK_REAQ = type("REAQ");
		public static readonly int CHUNK_REAL = type("REAL");

		// args to sendHPSG
		public const int WHEN_DISABLE = 0;
		public const int WHEN_GC = 1;
		public const int WHAT_MERGE = 0; // merge adjacent objects
		public const int WHAT_OBJ = 1; // keep objects distinct

		// args to sendHPIF
		public const int HPIF_WHEN_NEVER = 0;
		public const int HPIF_WHEN_NOW = 1;
		public const int HPIF_WHEN_NEXT_GC = 2;
		public const int HPIF_WHEN_EVERY_GC = 3;

		private static readonly HandleHeap mInst = new HandleHeap();

		private HandleHeap()
		{
		}

		/// <summary>
		/// Register for the packets we expect to get from the client.
		/// </summary>
		public static void register(MonitorThread mt)
		{
			mt.registerChunkHandler(CHUNK_HPIF, mInst);
			mt.registerChunkHandler(CHUNK_HPST, mInst);
			mt.registerChunkHandler(CHUNK_HPEN, mInst);
			mt.registerChunkHandler(CHUNK_HPSG, mInst);
			mt.registerChunkHandler(CHUNK_HPDS, mInst);
			mt.registerChunkHandler(CHUNK_REAQ, mInst);
			mt.registerChunkHandler(CHUNK_REAL, mInst);
		}

		/// <summary>
		/// Client is ready.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void clientReady(Client client) throws java.io.IOException
		internal override void clientReady(Client client)
		{
			if (client.heapUpdateEnabled)
			{
				//sendHPSG(client, WHEN_GC, WHAT_MERGE);
				sendHPIF(client, HPIF_WHEN_EVERY_GC);
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
			Log.d("ddm-heap", "handling " + ChunkHandler.name(type));

			if (type == CHUNK_HPIF)
			{
				handleHPIF(client, data);
			}
			else if (type == CHUNK_HPST)
			{
				handleHPST(client, data);
			}
			else if (type == CHUNK_HPEN)
			{
				handleHPEN(client, data);
			}
			else if (type == CHUNK_HPSG)
			{
				handleHPSG(client, data);
			}
			else if (type == CHUNK_HPDU)
			{
				handleHPDU(client, data);
			}
			else if (type == CHUNK_HPDS)
			{
				handleHPDS(client, data);
			}
			else if (type == CHUNK_REAQ)
			{
				handleREAQ(client, data);
			}
			else if (type == CHUNK_REAL)
			{
				handleREAL(client, data);
			}
			else
			{
				handleUnknownChunk(client, type, data, isReply, msgId);
			}
		}

		/*
		 * Handle a heap info message.
		 */
		private void handleHPIF(Client client, ByteBuffer data)
		{
			Log.d("ddm-heap", "HPIF!");
			try
			{
                int numHeaps = data.getInt();

				for (int i = 0; i < numHeaps; i++)
				{
                    int heapId = data.getInt();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") long timeStamp = data.getLong();
                    long timeStamp = data.getLong();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") byte reason = data.get();
					var reason = data.get();
					long maxHeapSize = (long)data.getInt() & 0x00ffffffff;
					long heapSize = (long)data.getInt() & 0x00ffffffff;
					long bytesAllocated = (long)data.getInt() & 0x00ffffffff;
                    long objectsAllocated = (long)data.getInt() & 0x00ffffffff;

					client.clientData.setHeapInfo(heapId, maxHeapSize, heapSize, bytesAllocated, objectsAllocated);
					client.update(Client.CHANGE_HEAP_DATA);
				}
			}
			catch (BufferUnderflowException)
			{
				Log.w("ddm-heap", "malformed HPIF chunk from client");
			}
		}

		/// <summary>
		/// Send an HPIF (HeaP InFo) request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendHPIF(Client client, int when) throws java.io.IOException
		public static void sendHPIF(Client client, int when)
		{
			ByteBuffer rawBuf = allocBuffer(1);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			buf.put((byte)when);

			finishChunkPacket(packet, CHUNK_HPIF, buf.position);
			Log.d("ddm-heap", "Sending " + name(CHUNK_HPIF) + ": when=" + when);
			client.sendAndConsume(packet, mInst);
		}

		/*
		 * Handle a heap segment series start message.
		 */
		private void handleHPST(Client client, ByteBuffer data)
		{
			/* Clear out any data that's sitting around to
			 * get ready for the chunks that are about to come.
			 */
	//xxx todo: only clear data that belongs to the heap mentioned in <data>.
			client.clientData.vmHeapData.clearHeapData();
		}

		/*
		 * Handle a heap segment series end message.
		 */
		private void handleHPEN(Client client, ByteBuffer data)
		{
			/* Let the UI know that we've received all of the
			 * data for this heap.
			 */
	//xxx todo: only seal data that belongs to the heap mentioned in <data>.
			client.clientData.vmHeapData.sealHeapData();
			client.update(Client.CHANGE_HEAP_DATA);
		}

		/*
		 * Handle a heap segment message.
		 */
		private void handleHPSG(Client client, ByteBuffer data)
		{
			var dataCopy = new byte[data.limit];
			data.rewind();
			data.get(dataCopy);
			data = ByteBuffer.wrap(dataCopy);
			client.clientData.vmHeapData.addHeapData(data);
	//xxx todo: add to the heap mentioned in <data>
		}

		/// <summary>
		/// Sends an HPSG (HeaP SeGment) request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendHPSG(Client client, int when, int what) throws java.io.IOException
		public static void sendHPSG(Client client, int when, int what)
		{

			ByteBuffer rawBuf = allocBuffer(2);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			buf.put((byte)when);
			buf.put((byte)what);

			finishChunkPacket(packet, CHUNK_HPSG, buf.position);
			Log.d("ddm-heap", "Sending " + name(CHUNK_HPSG) + ": when=" + when + ", what=" + what);
			client.sendAndConsume(packet, mInst);
		}

		/// <summary>
		/// Sends an HPGC request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendHPGC(Client client) throws java.io.IOException
		public static void sendHPGC(Client client)
		{
			ByteBuffer rawBuf = allocBuffer(0);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			// no data

			finishChunkPacket(packet, CHUNK_HPGC, buf.position);
			Log.d("ddm-heap", "Sending " + name(CHUNK_HPGC));
			client.sendAndConsume(packet, mInst);
		}

		/// <summary>
		/// Sends an HPDU request to the client.
		/// 
		/// We will get an HPDU response when the heap dump has completed.  On
		/// failure we get a generic failure response.
		/// </summary>
		/// <param name="fileName"> name of output file (on device) </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendHPDU(Client client, String fileName) throws java.io.IOException
		public static void sendHPDU(Client client, string fileName)
		{
			ByteBuffer rawBuf = allocBuffer(4 + fileName.Length * 2);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			buf.putInt(fileName.Length);
			putString(buf, fileName);

			finishChunkPacket(packet, CHUNK_HPDU, buf.position);
			Log.d("ddm-heap", "Sending " + name(CHUNK_HPDU) + " '" + fileName + "'");
			client.sendAndConsume(packet, mInst);
			client.clientData.pendingHprofDump = fileName;
		}

		/// <summary>
		/// Sends an HPDS request to the client.
		/// 
		/// We will get an HPDS response when the heap dump has completed.  On
		/// failure we get a generic failure response.
		/// 
		/// This is more expensive for the device than HPDU, because the entire
		/// heap dump is held in RAM instead of spooled out to a temp file.  On
		/// the other hand, permission to write to /sdcard is not required.
		/// </summary>
		/// <param name="fileName"> name of output file (on device) </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendHPDS(Client client) throws java.io.IOException
		public static void sendHPDS(Client client)
		{
			ByteBuffer rawBuf = allocBuffer(0);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			finishChunkPacket(packet, CHUNK_HPDS, buf.position);
			Log.d("ddm-heap", "Sending " + name(CHUNK_HPDS));
			client.sendAndConsume(packet, mInst);
		}

		/*
		 * Handle notification of completion of a HeaP DUmp.
		 */
		private void handleHPDU(Client client, ByteBuffer data)
		{
		    // get the filename and make the client not have pending HPROF dump anymore.
			string filename = client.clientData.pendingHprofDump;
			client.clientData.pendingHprofDump = null;

			// get the dump result
			var result = data.get();

			// get the app-level handler for HPROF dump
			ClientData.IHprofDumpHandler handler = ClientData.hprofDumpHandler;
			if (handler != null)
			{
				if (result == 0)
				{
					handler.onSuccess(filename, client);

					Log.d("ddm-heap", "Heap dump request has finished");
				}
				else
				{
					handler.onEndFailure(client, null);
					Log.w("ddm-heap", "Heap dump request failed (check device log)");
				}
			}
		}

		/*
		 * Handle HeaP Dump Streaming response.  "data" contains the full
		 * hprof dump.
		 */
		private void handleHPDS(Client client, ByteBuffer data)
		{
			ClientData.IHprofDumpHandler handler = ClientData.hprofDumpHandler;
			if (handler != null)
			{
				var stuff = new byte[data.capacity];
				data.get(stuff, 0, stuff.Length);

				Log.d("ddm-hprof", "got hprof file, size: " + data.capacity + " bytes");

				handler.onSuccess(stuff, client);
			}
		}

		/// <summary>
		/// Sends a REAE (REcent Allocation Enable) request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendREAE(Client client, boolean enable) throws java.io.IOException
		public static void sendREAE(Client client, bool enable)
		{
			ByteBuffer rawBuf = allocBuffer(1);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			buf.put((byte)(enable ? 1 : 0));

			finishChunkPacket(packet, CHUNK_REAE, buf.position);
			Log.d("ddm-heap", "Sending " + name(CHUNK_REAE) + ": " + enable);
			client.sendAndConsume(packet, mInst);
		}

		/// <summary>
		/// Sends a REAQ (REcent Allocation Query) request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendREAQ(Client client) throws java.io.IOException
		public static void sendREAQ(Client client)
		{
			ByteBuffer rawBuf = allocBuffer(0);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			// no data

			finishChunkPacket(packet, CHUNK_REAQ, buf.position);
			Log.d("ddm-heap", "Sending " + name(CHUNK_REAQ));
			client.sendAndConsume(packet, mInst);
		}

		/// <summary>
		/// Sends a REAL (REcent ALlocation) request to the client.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sendREAL(Client client) throws java.io.IOException
		public static void sendREAL(Client client)
		{
			ByteBuffer rawBuf = allocBuffer(0);
			JdwpPacket packet = new JdwpPacket(rawBuf);
			ByteBuffer buf = getChunkDataBuf(rawBuf);

			// no data

			finishChunkPacket(packet, CHUNK_REAL, buf.position);
			Log.d("ddm-heap", "Sending " + name(CHUNK_REAL));
			client.sendAndConsume(packet, mInst);
		}

		/*
		 * Handle the response from our REcent Allocation Query message.
		 */
		private void handleREAQ(Client client, ByteBuffer data)
		{
			bool enabled;

			enabled = (data.get() != 0);
			Log.d("ddm-heap", "REAQ says: enabled=" + enabled);

			client.clientData.allocationStatus = enabled ? ClientData.AllocationTrackingStatus.ON : ClientData.AllocationTrackingStatus.OFF;
			client.update(Client.CHANGE_HEAP_ALLOCATION_STATUS);
		}

		/// <summary>
		/// Converts a VM class descriptor string ("Landroid/os/Debug;") to
		/// a dot-notation class name ("android.os.Debug").
		/// </summary>
		private string descriptorToDot(string str)
		{
			// count the number of arrays.
			int array = 0;
			while (str.StartsWith("["))
			{
				str = str.Substring(1);
				array++;
			}

			int len = str.Length;

			/* strip off leading 'L' and trailing ';' if appropriate */
			if (len >= 2 && str[0] == 'L' && str[len - 1] == ';')
			{
				str = str.Substring(1, len - 1 - 1);
				str = str.Replace('/', '.');
			}
			else
			{
				// convert the basic types
				if ("C".Equals(str))
				{
					str = "char";
				}
				else if ("B".Equals(str))
				{
					str = "byte";
				}
				else if ("Z".Equals(str))
				{
					str = "boolean";
				}
				else if ("S".Equals(str))
				{
					str = "short";
				}
				else if ("I".Equals(str))
				{
					str = "int";
				}
				else if ("J".Equals(str))
				{
					str = "long";
				}
				else if ("F".Equals(str))
				{
					str = "float";
				}
				else if ("D".Equals(str))
				{
					str = "double";
				}
			}

			// now add the array part
			for (int a = 0 ; a < array; a++)
			{
				str = str + "[]";
			}

			return str;
		}

		/// <summary>
		/// Reads a string table out of "data".
		/// 
		/// This is just a serial collection of strings, each of which is a
		/// four-byte length followed by UTF-16 data.
		/// </summary>
		private void readStringTable(ByteBuffer data, string[] strings)
		{
			int count = strings.Length;
			int i;

			for (i = 0; i < count; i++)
			{
                int nameLen = data.getInt();
				string descriptor = getString(data, nameLen);
				strings[i] = descriptorToDot(descriptor);
			}
		}

		/*
		 * Handle a REcent ALlocation response.
		 *
		 * Message header (all values big-endian):
		 *   (1b) message header len (to allow future expansion); includes itself
		 *   (1b) entry header len
		 *   (1b) stack frame len
		 *   (2b) number of entries
		 *   (4b) offset to string table from start of message
		 *   (2b) number of class name strings
		 *   (2b) number of method name strings
		 *   (2b) number of source file name strings
		 *   For each entry:
		 *     (4b) total allocation size
		 *     (2b) threadId
		 *     (2b) allocated object's class name index
		 *     (1b) stack depth
		 *     For each stack frame:
		 *       (2b) method's class name
		 *       (2b) method name
		 *       (2b) method source file
		 *       (2b) line number, clipped to 32767; -2 if native; -1 if no source
		 *   (xb) class name strings
		 *   (xb) method name strings
		 *   (xb) source file strings
		 *
		 *   As with other DDM traffic, strings are sent as a 4-byte length
		 *   followed by UTF-16 data.
		 */
		private void handleREAL(Client client, ByteBuffer data)
		{
			Log.e("ddm-heap", "*** Received " + name(CHUNK_REAL));
			int messageHdrLen, entryHdrLen, stackFrameLen;
			int numEntries, offsetToStrings;
			int numClassNames, numMethodNames, numFileNames;

			/*
			 * Read the header.
			 */
			messageHdrLen = (data.get() & 0xff);
			entryHdrLen = (data.get() & 0xff);
			stackFrameLen = (data.get() & 0xff);
			numEntries = (data.getShort() & 0xffff);
            offsetToStrings = data.getInt();
			numClassNames = (data.getShort() & 0xffff);
			numMethodNames = (data.getShort()& 0xffff);
			numFileNames = (data.getShort()& 0xffff);


			/*
			 * Skip forward to the strings and read them.
			 */
			data.position = offsetToStrings;

			string[] classNames = new string[numClassNames];
			string[] methodNames = new string[numMethodNames];
			string[] fileNames = new string[numFileNames];

			readStringTable(data, classNames);
			readStringTable(data, methodNames);
			//System.out.println("METHODS: "
			//    + java.util.Arrays.deepToString(methodNames));
			readStringTable(data, fileNames);

			/*
			 * Skip back to a point just past the header and start reading
			 * entries.
			 */
			data.position = messageHdrLen;

			List<AllocationInfo> list = new List<AllocationInfo>(numEntries);
			int allocNumber = numEntries; // order value for the entry. This is sent in reverse order.
			for (int i = 0; i < numEntries; i++)
			{
				int totalSize;
				int threadId, classNameIndex, stackDepth;

                totalSize = data.getInt();
				threadId = (data.getShort() & 0xffff);
				classNameIndex = (data.getShort() & 0xffff);
				stackDepth = (data.get() & 0xff);
				/* we've consumed 9 bytes; gobble up any extra */
				for (int skip = 9; skip < entryHdrLen; skip++)
				{
					data.get();
				}

				StackTraceElement[] steArray = new StackTraceElement[stackDepth];

				/*
				 * Pull out the stack trace.
				 */
				for (int sti = 0; sti < stackDepth; sti++)
				{
					int methodClassNameIndex, methodNameIndex;
					int methodSourceFileIndex;
					short lineNumber;
					string methodClassName, methodName, methodSourceFile;

					methodClassNameIndex = (data.getShort() & 0xffff);
					methodNameIndex = (data.getShort()& 0xffff);
					methodSourceFileIndex = (data.getShort()& 0xffff);
					lineNumber = data.getShort();

					methodClassName = classNames[methodClassNameIndex];
					methodName = methodNames[methodNameIndex];
					methodSourceFile = fileNames[methodSourceFileIndex];

					steArray[sti] = new StackTraceElement(methodClassName, methodName, methodSourceFile, lineNumber);

					/* we've consumed 8 bytes; gobble up any extra */
					for (int skip = 9; skip < stackFrameLen; skip++)
					{
						data.get();
					}
				}

				list.Add(new AllocationInfo(allocNumber--, classNames[classNameIndex], totalSize, (short) threadId, steArray));
			}

			client.clientData.allocations = list.ToArray();
			client.update(Client.CHANGE_HEAP_ALLOCATIONS);
		}

		/*
		 * For debugging: dump the contents of an AllocRecord array.
		 *
		 * The array starts with the oldest known allocation and ends with
		 * the most recent allocation.
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private static void dumpRecords(AllocationInfo[] records)
		private static void dumpRecords(AllocationInfo[] records)
		{
			Console.WriteLine("Found " + records.Length + " records:");

			foreach (AllocationInfo rec in records)
			{
				Console.WriteLine("tid=" + rec.threadId + " " + rec.allocatedClass + " (" + rec.size + " bytes)");

				foreach (StackTraceElement ste in rec.stackTrace)
				{
					if (ste.nativeMethod)
					{
						Console.WriteLine("    " + ste.className + "." + ste.methodName + " (Native method)");
					}
					else
					{
						Console.WriteLine("    " + ste.className + "." + ste.methodName + " (" + ste.fileName + ":" + ste.lineNumber + ")");
					}
				}
			}
		}

	}


}