using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
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
	/// This represents a single client, usually a DAlvik VM process.
	/// <p/>This class gives access to basic client information, as well as methods to perform actions
	/// on the client.
	/// <p/>More detailed information, usually updated in real time, can be access through the
	/// <seealso cref="ClientData"/> class. Each <code>Client</code> object has its own <code>ClientData</code>
	/// accessed through <seealso cref="#getClientData()"/>.
	/// </summary>
	public class Client
	{

		private const int SERVER_PROTOCOL_VERSION = 1;

	/// <summary>
		/// Client change bit mask: application name change </summary>
		public const int CHANGE_NAME = 0x0001;
	/// <summary>
		/// Client change bit mask: debugger status change </summary>
		public const int CHANGE_DEBUGGER_STATUS = 0x0002;
	/// <summary>
		/// Client change bit mask: debugger port change </summary>
		public const int CHANGE_PORT = 0x0004;
	/// <summary>
		/// Client change bit mask: thread update flag change </summary>
		public const int CHANGE_THREAD_MODE = 0x0008;
	/// <summary>
		/// Client change bit mask: thread data updated </summary>
		public const int CHANGE_THREAD_DATA = 0x0010;
	/// <summary>
		/// Client change bit mask: heap update flag change </summary>
		public const int CHANGE_HEAP_MODE = 0x0020;
	/// <summary>
		/// Client change bit mask: head data updated </summary>
		public const int CHANGE_HEAP_DATA = 0x0040;
	/// <summary>
		/// Client change bit mask: native heap data updated </summary>
		public const int CHANGE_NATIVE_HEAP_DATA = 0x0080;
	/// <summary>
		/// Client change bit mask: thread stack trace updated </summary>
		public const int CHANGE_THREAD_STACKTRACE = 0x0100;
	/// <summary>
		/// Client change bit mask: allocation information updated </summary>
		public const int CHANGE_HEAP_ALLOCATIONS = 0x0200;
	/// <summary>
		/// Client change bit mask: allocation information updated </summary>
		public const int CHANGE_HEAP_ALLOCATION_STATUS = 0x0400;
	/// <summary>
		/// Client change bit mask: allocation information updated </summary>
		public const int CHANGE_METHOD_PROFILING_STATUS = 0x0800;

		/// <summary>
		/// Client change bit mask: combination of <seealso cref="Client#CHANGE_NAME"/>,
		/// <seealso cref="Client#CHANGE_DEBUGGER_STATUS"/>, and <seealso cref="Client#CHANGE_PORT"/>.
		/// </summary>
		public static readonly int CHANGE_INFO = CHANGE_NAME | CHANGE_DEBUGGER_STATUS | CHANGE_PORT;

		private SocketChannel mChan;

		// debugger we're associated with, if any
		private Debugger mDebugger;
		private int mDebuggerListenPort;

		// list of IDs for requests we have sent to the client
		private Dictionary<int?, ChunkHandler> mOutstandingReqs;

		// chunk handlers stash state data in here
		private ClientData mClientData;

		// User interface state.  Changing the value causes a message to be
		// sent to the client.
		private bool mThreadUpdateEnabled;
		private bool mHeapUpdateEnabled;

		/*
		 * Read/write buffers.  We can get large quantities of data from the
		 * client, e.g. the response to a "give me the list of all known classes"
		 * request from the debugger.  Requests from the debugger, and from us,
		 * are much smaller.
		 *
		 * Pass-through debugger traffic is sent without copying.  "mWriteBuffer"
		 * is only used for data generated within Client.
		 */
		private const int INITIAL_BUF_SIZE = 2 * 1024;
		private const int MAX_BUF_SIZE = 200 * 1024 * 1024;
		private ByteBuffer mReadBuffer;

		private const int WRITE_BUF_SIZE = 256;
		private ByteBuffer mWriteBuffer;

		private Device mDevice;

		private int mConnState;

		private const int ST_INIT = 1;
		private const int ST_NOT_JDWP = 2;
		private const int ST_AWAIT_SHAKE = 10;
		private const int ST_NEED_DDM_PKT = 11;
		private const int ST_NOT_DDM = 12;
		private const int ST_READY = 13;
		private const int ST_ERROR = 20;
		private const int ST_DISCONNECTED = 21;


		/// <summary>
		/// Create an object for a new client connection.
		/// </summary>
		/// <param name="device"> the device this client belongs to </param>
		/// <param name="chan"> the connected <seealso cref="SocketChannel"/>. </param>
		/// <param name="pid"> the client pid. </param>
		internal Client(Device device, SocketChannel chan, int pid)
		{
			mDevice = device;
			mChan = chan;

			mReadBuffer = ByteBuffer.allocate(INITIAL_BUF_SIZE);
			mWriteBuffer = ByteBuffer.allocate(WRITE_BUF_SIZE);

			mOutstandingReqs = new Dictionary<int?, ChunkHandler>();

			mConnState = ST_INIT;

			mClientData = new ClientData(pid);

			mThreadUpdateEnabled = DdmPreferences.initialThreadUpdate;
			mHeapUpdateEnabled = DdmPreferences.initialHeapUpdate;
		}

		/// <summary>
		/// Returns a string representation of the <seealso cref="Client"/> object.
		/// </summary>
		public override string ToString()
		{
			return "[Client pid: " + mClientData.pid + "]";
		}

		/// <summary>
		/// Returns the <seealso cref="IDevice"/> on which this Client is running.
		/// </summary>
		public virtual IDevice device
		{
			get
			{
				return mDevice;
			}
		}

		/// <summary>
		/// Returns the <seealso cref="Device"/> on which this Client is running.
		/// </summary>
		internal virtual Device deviceImpl
		{
			get
			{
				return mDevice;
			}
		}

		/// <summary>
		/// Returns the debugger port for this client.
		/// </summary>
		public virtual int debuggerListenPort
		{
			get
			{
				return mDebuggerListenPort;
			}
		}

		/// <summary>
		/// Returns <code>true</code> if the client VM is DDM-aware.
		/// 
		/// Calling here is only allowed after the connection has been
		/// established.
		/// </summary>
		public virtual bool ddmAware
		{
			get
			{
				switch (mConnState)
				{
					case ST_INIT:
					case ST_NOT_JDWP:
					case ST_AWAIT_SHAKE:
					case ST_NEED_DDM_PKT:
					case ST_NOT_DDM:
					case ST_ERROR:
					case ST_DISCONNECTED:
						return false;
					case ST_READY:
						return true;
					default:
						Debug.Assert(false);
						return false;
				}
			}
		}

		/// <summary>
		/// Returns <code>true</code> if a debugger is currently attached to the client.
		/// </summary>
		public virtual bool debuggerAttached
		{
			get
			{
				return mDebugger.debuggerAttached;
			}
		}

		/// <summary>
		/// Return the Debugger object associated with this client.
		/// </summary>
		internal virtual Debugger debugger
		{
			get
			{
				return mDebugger;
			}
		}

		/// <summary>
		/// Returns the <seealso cref="ClientData"/> object containing this client information.
		/// </summary>
		public virtual ClientData clientData
		{
			get
			{
				return mClientData;
			}
		}

		/// <summary>
		/// Forces the client to execute its garbage collector.
		/// </summary>
		public virtual void executeGarbageCollector()
		{
			try
			{
				HandleHeap.sendHPGC(this);
			}
			catch (IOException)
			{
				Log.w("ddms", "Send of HPGC message failed");
				// ignore
			}
		}

		/// <summary>
		/// Makes the VM dump an HPROF file
		/// </summary>
		public virtual void dumpHprof()
		{
			bool canStream = mClientData.hasFeature(ClientData.FEATURE_HPROF_STREAMING);
			try
			{
				if (canStream)
				{
					HandleHeap.sendHPDS(this);
				}
				else
				{
					string file = "/sdcard/" + mClientData.clientDescription.replaceAll("\\:.*", "") + ".hprof";
					HandleHeap.sendHPDU(this, file);
				}
			}
			catch (IOException)
			{
				Log.w("ddms", "Send of HPDU message failed");
				// ignore
			}
		}

		public virtual void toggleMethodProfiling()
		{
			bool canStream = mClientData.hasFeature(ClientData.FEATURE_PROFILING_STREAMING);
			try
			{
				if (mClientData.methodProfilingStatus == ClientData.MethodProfilingStatus.ON)
				{
					if (canStream)
					{
						HandleProfiling.sendMPSE(this);
					}
					else
					{
						HandleProfiling.sendMPRE(this);
					}
				}
				else
				{
					int bufferSize = DdmPreferences.profilerBufferSizeMb * 1024 * 1024;
					if (canStream)
					{
						HandleProfiling.sendMPSS(this, bufferSize, 0); //flags
					}
					else
					{
						string file = "/sdcard/" + mClientData.clientDescription.replaceAll("\\:.*", "") + DdmConstants.DOT_TRACE;
						HandleProfiling.sendMPRS(this, file, bufferSize, 0); //flags
					}
				}
			}
			catch (IOException)
			{
				Log.w("ddms", "Toggle method profiling failed");
				// ignore
			}
		}

		/// <summary>
		/// Sends a request to the VM to send the enable status of the method profiling.
		/// This is asynchronous.
		/// <p/>The allocation status can be accessed by <seealso cref="ClientData#getAllocationStatus()"/>.
		/// The notification that the new status is available will be received through
		/// <seealso cref="IClientChangeListener#clientChanged(Client, int)"/> with a <code>changeMask</code>
		/// containing the mask <seealso cref="#CHANGE_HEAP_ALLOCATION_STATUS"/>.
		/// </summary>
		public virtual void requestMethodProfilingStatus()
		{
			try
			{
				HandleHeap.sendREAQ(this);
			}
			catch (IOException e)
			{
				Log.e("ddmlib", e);
			}
		}


		/// <summary>
		/// Enables or disables the thread update.
		/// <p/>If <code>true</code> the VM will be able to send thread information. Thread information
		/// must be requested with <seealso cref="#requestThreadUpdate()"/>. </summary>
		/// <param name="enabled"> the enable flag. </param>
		public virtual bool threadUpdateEnabled
		{
			set
			{
				mThreadUpdateEnabled = value;
				if (value == false)
				{
					mClientData.clearThreads();
				}
    
				try
				{
					HandleThread.sendTHEN(this, value);
				}
				catch (IOException ioe)
				{
					// ignore it here; client will clean up shortly
					Console.WriteLine(ioe.ToString());
					Console.Write(ioe.StackTrace);
				}
    
				update(CHANGE_THREAD_MODE);
			}
			get
			{
				return mThreadUpdateEnabled;
			}
		}


		/// <summary>
		/// Sends a thread update request. This is asynchronous.
		/// <p/>The thread info can be accessed by <seealso cref="ClientData#getThreads()"/>. The notification
		/// that the new data is available will be received through
		/// <seealso cref="IClientChangeListener#clientChanged(Client, int)"/> with a <code>changeMask</code>
		/// containing the mask <seealso cref="#CHANGE_THREAD_DATA"/>.
		/// </summary>
		public virtual void requestThreadUpdate()
		{
			HandleThread.requestThreadUpdate(this);
		}

		/// <summary>
		/// Sends a thread stack trace update request. This is asynchronous.
		/// <p/>The thread info can be accessed by <seealso cref="ClientData#getThreads()"/> and
		/// <seealso cref="ThreadInfo#getStackTrace()"/>.
		/// <p/>The notification that the new data is available
		/// will be received through <seealso cref="IClientChangeListener#clientChanged(Client, int)"/>
		/// with a <code>changeMask</code> containing the mask <seealso cref="#CHANGE_THREAD_STACKTRACE"/>.
		/// </summary>
		public virtual void requestThreadStackTrace(int threadId)
		{
			HandleThread.requestThreadStackCallRefresh(this, threadId);
		}

		/// <summary>
		/// Enables or disables the heap update.
		/// <p/>If <code>true</code>, any GC will cause the client to send its heap information.
		/// <p/>The heap information can be accessed by <seealso cref="ClientData#getVmHeapData()"/>.
		/// <p/>The notification that the new data is available
		/// will be received through <seealso cref="IClientChangeListener#clientChanged(Client, int)"/>
		/// with a <code>changeMask</code> containing the value <seealso cref="#CHANGE_HEAP_DATA"/>. </summary>
		/// <param name="enabled"> the enable flag </param>
		public virtual bool heapUpdateEnabled
		{
			set
			{
				mHeapUpdateEnabled = value;
    
				try
				{
					HandleHeap.sendHPIF(this, value ? HandleHeap.HPIF_WHEN_EVERY_GC : HandleHeap.HPIF_WHEN_NEVER);
    
					HandleHeap.sendHPSG(this, value ? HandleHeap.WHEN_GC : HandleHeap.WHEN_DISABLE, HandleHeap.WHAT_MERGE);
				}
				catch (IOException)
				{
					// ignore it here; client will clean up shortly
				}
    
				update(CHANGE_HEAP_MODE);
			}
			get
			{
				return mHeapUpdateEnabled;
			}
		}


		/// <summary>
		/// Sends a native heap update request. this is asynchronous.
		/// <p/>The native heap info can be accessed by <seealso cref="ClientData#getNativeAllocationList()"/>.
		/// The notification that the new data is available will be received through
		/// <seealso cref="IClientChangeListener#clientChanged(Client, int)"/> with a <code>changeMask</code>
		/// containing the mask <seealso cref="#CHANGE_NATIVE_HEAP_DATA"/>.
		/// </summary>
		public virtual bool requestNativeHeapInformation()
		{
			try
			{
				HandleNativeHeap.sendNHGT(this);
				return true;
			}
			catch (IOException e)
			{
				Log.e("ddmlib", e);
			}

			return false;
		}

		/// <summary>
		/// Enables or disables the Allocation tracker for this client.
		/// <p/>If enabled, the VM will start tracking allocation informations. A call to
		/// <seealso cref="#requestAllocationDetails()"/> will make the VM sends the information about all the
		/// allocations that happened between the enabling and the request. </summary>
		/// <param name="enable"> </param>
		/// <seealso cref= #requestAllocationDetails() </seealso>
		public virtual void enableAllocationTracker(bool enable)
		{
			try
			{
				HandleHeap.sendREAE(this, enable);
			}
			catch (IOException e)
			{
				Log.e("ddmlib", e);
			}
		}

		/// <summary>
		/// Sends a request to the VM to send the enable status of the allocation tracking.
		/// This is asynchronous.
		/// <p/>The allocation status can be accessed by <seealso cref="ClientData#getAllocationStatus()"/>.
		/// The notification that the new status is available will be received through
		/// <seealso cref="IClientChangeListener#clientChanged(Client, int)"/> with a <code>changeMask</code>
		/// containing the mask <seealso cref="#CHANGE_HEAP_ALLOCATION_STATUS"/>.
		/// </summary>
		public virtual void requestAllocationStatus()
		{
			try
			{
				HandleHeap.sendREAQ(this);
			}
			catch (IOException e)
			{
				Log.e("ddmlib", e);
			}
		}

		/// <summary>
		/// Sends a request to the VM to send the information about all the allocations that have
		/// happened since the call to <seealso cref="#enableAllocationTracker(boolean)"/> with <var>enable</var>
		/// set to <code>null</code>. This is asynchronous.
		/// <p/>The allocation information can be accessed by <seealso cref="ClientData#getAllocations()"/>.
		/// The notification that the new data is available will be received through
		/// <seealso cref="IClientChangeListener#clientChanged(Client, int)"/> with a <code>changeMask</code>
		/// containing the mask <seealso cref="#CHANGE_HEAP_ALLOCATIONS"/>.
		/// </summary>
		public virtual void requestAllocationDetails()
		{
			try
			{
				HandleHeap.sendREAL(this);
			}
			catch (IOException e)
			{
				Log.e("ddmlib", e);
			}
		}

		/// <summary>
		/// Sends a kill message to the VM.
		/// </summary>
		public virtual void kill()
		{
			try
			{
				HandleExit.sendEXIT(this, 1);
			}
			catch (IOException)
			{
				Log.w("ddms", "Send of EXIT message failed");
				// ignore
			}
		}

		/// <summary>
		/// Registers the client with a Selector.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void register(java.nio.channels.Selector sel) throws java.io.IOException
		internal virtual void register(Selector sel)
		{
			if (mChan != null)
			{
				mChan.register(sel, SelectionKey.OP_READ, this);
			}
		}

		/// <summary>
		/// Sets the client to accept debugger connection on the "selected debugger port".
		/// </summary>
		/// <seealso cref= AndroidDebugBridge#setSelectedClient(Client) </seealso>
		/// <seealso cref= DdmPreferences#setSelectedDebugPort(int) </seealso>
		public virtual void setAsSelectedClient()
		{
			MonitorThread monitorThread = MonitorThread.instance;
			if (monitorThread != null)
			{
				monitorThread.selectedClient = this;
			}
		}

		/// <summary>
		/// Returns whether this client is the current selected client, accepting debugger connection
		/// on the "selected debugger port".
		/// </summary>
		/// <seealso cref= #setAsSelectedClient() </seealso>
		/// <seealso cref= AndroidDebugBridge#setSelectedClient(Client) </seealso>
		/// <seealso cref= DdmPreferences#setSelectedDebugPort(int) </seealso>
		public virtual bool selectedClient
		{
			get
			{
				MonitorThread monitorThread = MonitorThread.instance;
				if (monitorThread != null)
				{
					return monitorThread.selectedClient == this;
				}
    
				return false;
			}
		}

		/// <summary>
		/// Tell the client to open a server socket channel and listen for
		/// connections on the specified port.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void listenForDebugger(int listenPort) throws java.io.IOException
		internal virtual void listenForDebugger(int listenPort)
		{
			mDebuggerListenPort = listenPort;
			mDebugger = new Debugger(this, listenPort);
		}

		/// <summary>
		/// Initiate the JDWP handshake.
		/// 
		/// On failure, closes the socket and returns false.
		/// </summary>
		internal virtual bool sendHandshake()
		{
			Debug.Assert(mWriteBuffer.position == 0);

			try
			{
				// assume write buffer can hold 14 bytes
				JdwpPacket.putHandshake(mWriteBuffer);
				int expectedLen = mWriteBuffer.position;
				mWriteBuffer.flip();
				if (mChan.write(mWriteBuffer) != expectedLen)
				{
					throw new IOException("partial handshake write");
				}
			}
			catch (IOException ioe)
			{
				Log.e("ddms-client", "IO error during handshake: " + ioe.Message);
				mConnState = ST_ERROR;
				close(true); // notify
				return false;
			}
			finally
			{
				mWriteBuffer.clear();
			}

			mConnState = ST_AWAIT_SHAKE;

			return true;
		}


		/// <summary>
		/// Send a non-DDM packet to the client.
		/// 
		/// Equivalent to sendAndConsume(packet, null).
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void sendAndConsume(JdwpPacket packet) throws java.io.IOException
		internal virtual void sendAndConsume(JdwpPacket packet)
		{
			sendAndConsume(packet, null);
		}

		/// <summary>
		/// Send a DDM packet to the client.
		/// 
		/// Ideally, we can do this with a single channel write.  If that doesn't
		/// happen, we have to prevent anybody else from writing to the channel
		/// until this packet completes, so we synchronize on the channel.
		/// 
		/// Another goal is to avoid unnecessary buffer copies, so we write
		/// directly out of the JdwpPacket's ByteBuffer.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void sendAndConsume(JdwpPacket packet, ChunkHandler replyHandler) throws java.io.IOException
		internal virtual void sendAndConsume(JdwpPacket packet, ChunkHandler replyHandler)
		{

			if (mChan == null)
			{
				// can happen for e.g. THST packets
				Log.v("ddms", "Not sending packet -- client is closed");
				return;
			}

			if (replyHandler != null)
			{
				/*
				 * Add the ID to the list of outstanding requests.  We have to do
				 * this before sending the packet, in case the response comes back
				 * before our thread returns from the packet-send function.
				 */
				addRequestId(packet.id, replyHandler);
			}

			lock (mChan)
			{
				try
				{
					packet.writeAndConsume(mChan);
				}
				catch (IOException ioe)
				{
					removeRequestId(packet.id);
					throw ioe;
				}
			}
		}

		/// <summary>
		/// Forward the packet to the debugger (if still connected to one).
		/// 
		/// Consumes the packet.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void forwardPacketToDebugger(JdwpPacket packet) throws java.io.IOException
		internal virtual void forwardPacketToDebugger(JdwpPacket packet)
		{

			Debugger dbg = mDebugger;

			if (dbg == null)
			{
				Log.d("ddms", "Discarding packet");
				packet.consume();
			}
			else
			{
				dbg.sendAndConsume(packet);
			}
		}

		/// <summary>
		/// Read data from our channel.
		/// 
		/// This is called when data is known to be available, and we don't yet
		/// have a full packet in the buffer.  If the buffer is at capacity,
		/// expand it.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void read() throws java.io.IOException, java.nio.BufferOverflowException
		internal virtual void read()
		{

			int count;

			if (mReadBuffer.position == mReadBuffer.capacity)
			{
				if (mReadBuffer.capacity * 2 > MAX_BUF_SIZE)
				{
					Log.e("ddms", "Exceeded MAX_BUF_SIZE!");
					throw new OverflowException();
				}
				Log.d("ddms", "Expanding read buffer to " + mReadBuffer.capacity * 2);

				ByteBuffer newBuffer = ByteBuffer.allocate(mReadBuffer.capacity * 2);

				// copy entire buffer to new buffer
				mReadBuffer.position =(0);
				newBuffer.put(mReadBuffer); // leaves "position" at end of copied

				mReadBuffer = newBuffer;
			}

			count = mChan.read(mReadBuffer);
			if (count < 0)
			{
				throw new IOException("read failed");
			}

			if (Log.Config.LOGV)
			{
				Log.v("ddms", "Read " + count + " bytes from " + this);
			}
			//Log.hexDump("ddms", Log.DEBUG, mReadBuffer.array(),
			//    mReadBuffer.arrayOffset(), mReadBuffer.position());
		}

		/// <summary>
		/// Return information for the first full JDWP packet in the buffer.
		/// 
		/// If we don't yet have a full packet, return null.
		/// 
		/// If we haven't yet received the JDWP handshake, we watch for it here
		/// and consume it without admitting to have done so.  Upon receipt
		/// we send out the "HELO" message, which is why this can throw an
		/// IOException.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JdwpPacket getJdwpPacket() throws java.io.IOException
		internal virtual JdwpPacket jdwpPacket
		{
			get
			{
    
				/*
				 * On entry, the data starts at offset 0 and ends at "position".
				 * "limit" is set to the buffer capacity.
				 */
				if (mConnState == ST_AWAIT_SHAKE)
				{
					/*
					 * The first thing we get from the client is a response to our
					 * handshake.  It doesn't look like a packet, so we have to
					 * handle it specially.
					 */
					int result;
    
					result = JdwpPacket.findHandshake(mReadBuffer);
					//Log.v("ddms", "findHand: " + result);
					switch (result)
					{
						case JdwpPacket.HANDSHAKE_GOOD:
							Log.d("ddms", "Good handshake from client, sending HELO to " + mClientData.pid);
							JdwpPacket.consumeHandshake(mReadBuffer);
							mConnState = ST_NEED_DDM_PKT;
							HandleHello.sendHelloCommands(this, SERVER_PROTOCOL_VERSION);
							// see if we have another packet in the buffer
							return jdwpPacket;
						case JdwpPacket.HANDSHAKE_BAD:
							Log.d("ddms", "Bad handshake from client");
							if (MonitorThread.instance.retryOnBadHandshake)
							{
								// we should drop the client, but also attempt to reopen it.
								// This is done by the DeviceMonitor.
								mDevice.monitor.addClientToDropAndReopen(this, DebugPortManager.DebugPortProvider.NO_STATIC_PORT);
							}
							else
							{
								// mark it as bad, close the socket, and don't retry
								mConnState = ST_NOT_JDWP;
								close(true); // notify
							}
							break;
						case JdwpPacket.HANDSHAKE_NOTYET:
							Log.d("ddms", "No handshake from client yet.");
							break;
						default:
							Log.e("ddms", "Unknown packet while waiting for client handshake");
						break;
					}
					return null;
				}
				else if (mConnState == ST_NEED_DDM_PKT || mConnState == ST_NOT_DDM || mConnState == ST_READY)
				{
					/*
					 * Normal packet traffic.
					 */
					if (mReadBuffer.position != 0)
					{
						if (Log.Config.LOGV)
						{
							Log.v("ddms", "Checking " + mReadBuffer.position + " bytes");
						}
					}
					return JdwpPacket.findPacket(mReadBuffer);
				}
				else
				{
					/*
					 * Not expecting data when in this state.
					 */
					Log.e("ddms", "Receiving data in state = " + mConnState);
				}
    
				return null;
			}
		}

		/*
		 * Add the specified ID to the list of request IDs for which we await
		 * a response.
		 */
		private void addRequestId(int id, ChunkHandler handler)
		{
			lock (mOutstandingReqs)
			{
				if (Log.Config.LOGV)
				{
					Log.v("ddms", "Adding req 0x" + id.toHexString() + " to set");
				}
				mOutstandingReqs.Add(id, handler);
			}
		}

		/*
		 * Remove the specified ID from the list, if present.
		 */
		internal virtual void removeRequestId(int id)
		{
			lock (mOutstandingReqs)
			{
				if (Log.Config.LOGV)
				{
                    Log.v("ddms", "Removing req 0x" + id.toHexString() + " from set");
				}
				mOutstandingReqs.Remove(id);
			}

			//Log.w("ddms", "Request " + Integer.toHexString(id)
			//    + " could not be removed from " + this);
		}

		/// <summary>
		/// Determine whether this is a response to a request we sent earlier.
		/// If so, return the ChunkHandler responsible.
		/// </summary>
		internal virtual ChunkHandler isResponseToUs(int id)
		{

			lock (mOutstandingReqs)
			{
				ChunkHandler handler = mOutstandingReqs[id];
				if (handler != null)
				{
					if (Log.Config.LOGV)
					{
						Log.v("ddms", "Found 0x" + id.toHexString() + " in request set - " + handler);
					}
					return handler;
				}
			}

			return null;
		}

		/// <summary>
		/// An earlier request resulted in a failure.  This is the expected
		/// response to a HELO message when talking to a non-DDM client.
		/// </summary>
		internal virtual void packetFailed(JdwpPacket reply)
		{
			if (mConnState == ST_NEED_DDM_PKT)
			{
				Log.d("ddms", "Marking " + this + " as non-DDM client");
				mConnState = ST_NOT_DDM;
			}
			else if (mConnState != ST_NOT_DDM)
			{
				Log.w("ddms", "WEIRD: got JDWP failure packet on DDM req");
			}
		}

		/// <summary>
		/// The MonitorThread calls this when it sees a DDM request or reply.
		/// If we haven't seen a DDM packet before, we advance the state to
		/// ST_READY and return "false".  Otherwise, just return true.
		/// 
		/// The idea is to let the MonitorThread know when we first see a DDM
		/// packet, so we can send a broadcast to the handlers when a client
		/// connection is made.  This method is synchronized so that we only
		/// send the broadcast once.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual bool ddmSeen()
		{
			if (mConnState == ST_NEED_DDM_PKT)
			{
				mConnState = ST_READY;
				return false;
			}
			else if (mConnState != ST_READY)
			{
				Log.w("ddms", "WEIRD: in ddmSeen with state=" + mConnState);
			}
			return true;
		}

		/// <summary>
		/// Close the client socket channel.  If there is a debugger associated
		/// with us, close that too.
		/// 
		/// Closing a channel automatically unregisters it from the selector.
		/// However, we have to iterate through the selector loop before it
		/// actually lets them go and allows the file descriptors to close.
		/// The caller is expected to manage that. </summary>
		/// <param name="notify"> Whether or not to notify the listeners of a change. </param>
		internal virtual void close(bool notify)
		{
			Log.d("ddms", "Closing " + this.ToString());

			mOutstandingReqs.Clear();

			try
			{
				if (mChan != null)
				{
					mChan.close();
					mChan = null;
				}

				if (mDebugger != null)
				{
					mDebugger.close();
					mDebugger = null;
				}
			}
			catch (IOException)
			{
				Log.w("ddms", "failed to close " + this);
				// swallow it -- not much else to do
			}

			mDevice.removeClient(this, notify);
		}

		/// <summary>
		/// Returns whether this <seealso cref="Client"/> has a valid connection to the application VM.
		/// </summary>
		public virtual bool valid
		{
			get
			{
				return mChan != null;
			}
		}

		internal virtual void update(int changeMask)
		{
			mDevice.update(this, changeMask);
		}
	}


}