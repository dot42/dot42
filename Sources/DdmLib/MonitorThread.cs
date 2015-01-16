using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
	/// Monitor open connections.
	/// </summary>
	internal sealed class MonitorThread
	{
	    private readonly Thread thread;

		// For broadcasts to message handlers
		//private static final int CLIENT_CONNECTED = 1;

		private const int CLIENT_READY = 2;

		private const int CLIENT_DISCONNECTED = 3;

		private volatile bool mQuit = false;

		// List of clients we're paying attention to
		private List<Client> mClientList;

		// The almighty mux
		private Selector mSelector;

		// Map chunk types to handlers
		private Dictionary<int?, ChunkHandler> mHandlerMap;

		// port for "debug selected"
		private ServerSocketChannel mDebugSelectedChan;

		private int mNewDebugSelectedPort;

		private int mDebugSelectedPort = -1;

		/// <summary>
		/// "Selected" client setup to answer debugging connection to the mNewDebugSelectedPort port.
		/// </summary>
		private Client mSelectedClient = null;

		// singleton
		private static MonitorThread mInstance;

		/// <summary>
		/// Generic constructor.
		/// </summary>
		private MonitorThread() 
		{
            thread = new Thread(run);
			mClientList = new List<Client>();
			mHandlerMap = new Dictionary<int?, ChunkHandler>();

			mNewDebugSelectedPort = DdmPreferences.selectedDebugPort;
		}

        internal void start()
        {
            thread.Start();
        }

        internal bool IsAlive { get { return thread.IsAlive; }}

		/// <summary>
		/// Creates and return the singleton instance of the client monitor thread.
		/// </summary>
		internal static MonitorThread createInstance()
		{
			return mInstance = new MonitorThread();
		}

		/// <summary>
		/// Get singleton instance of the client monitor thread.
		/// </summary>
		internal static MonitorThread instance
		{
			get
			{
				return mInstance;
			}
		}


		/// <summary>
		/// Sets or changes the port number for "debug selected".
		/// </summary>
		void debugSelectedPortort(int port)
		{
            lock (this)
            {
                {
                    if (mInstance == null)
                    {
                        return;
                    }

                    if (AndroidDebugBridge.clientSupport == false)
                    {
                        return;
                    }

                    if (mDebugSelectedChan != null)
                    {
                        Log.d("ddms", "Changing debug-selected port to " + port);
                        mNewDebugSelectedPort = port;
                        wakeup();
                    }
                    else
                    {
                        // we set mNewDebugSelectedPort instead of mDebugSelectedPort so that it's automatically
                        // opened on the first run loop.
                        mNewDebugSelectedPort = port;
                    }
                }
            }
		}

		/// <summary>
		/// Sets the client to accept debugger connection on the custom "Selected debug port". </summary>
		/// <param name="selectedClient"> the client. Can be null. </param>
		void selectedClientent(Client selectedClient)
		{
		    lock (this)
		    {
		        if (mInstance == null)
		        {
		            return;
		        }

		        if (mSelectedClient != selectedClient)
		        {
		            Client oldClient = mSelectedClient;
		            mSelectedClient = selectedClient;

		            if (oldClient != null)
		            {
		                oldClient.update(Client.CHANGE_PORT);
		            }

		            if (mSelectedClient != null)
		            {
		                mSelectedClient.update(Client.CHANGE_PORT);
		            }
		        }
		    }
		}

	    /// <summary>
		/// Returns the client accepting debugger connection on the custom "Selected debug port".
		/// </summary>
	    internal Client selectedClient
	    {
	        get { return mSelectedClient; }
	        set { throw new NotImplementedException(); }
	    }


	    /// <summary>
		/// Returns "true" if we want to retry connections to clients if we get a bad
		/// JDWP handshake back, "false" if we want to just mark them as bad and
		/// leave them alone.
		/// </summary>
		internal bool retryOnBadHandshake
		{
			get{
                return true; // TODO? make configurable
            }
		}

		/// <summary>
		/// Get an array of known clients.
		/// </summary>
		Client[] clients
		{
            get
            {
                lock (mClientList)
                {
                    return mClientList.ToArray();
                }
            }
		}

		/// <summary>
		/// Register "handler" as the handler for type "type".
		/// </summary>
		internal void registerChunkHandler(int type, ChunkHandler handler)
		{
            lock (this)
            {
                if(mInstance == null)
                {
                    return;
                }

                lock (mHandlerMap)
                {
                    if(mHandlerMap[type] == null)
                    {
                        mHandlerMap.Add(type, handler);
                    }
                }
            }
		}

		/// <summary>
		/// Watch for activity from clients and debuggers.
		/// </summary>
		public void run()
		{
			Log.d("ddms", "Monitor is up");

			// create a selector
			try
			{
				mSelector = Selector.open();
			}
			catch (IOException ioe)
			{
				Log.logAndDisplay(Log.LogLevel.ERROR, "ddms", "Failed to initialize Monitor Thread: " + ioe.Message);
				return;
			}

			while (!mQuit)
			{

				try
				{
					/*
					 * sync with new registrations: we wait until addClient is done before going through
					 * and doing mSelector.select() again.
					 * @see {@link #addClient(Client)}
					 */
					lock (mClientList)
					{
					}

					// (re-)open the "debug selected" port, if it's not opened yet or
					// if the port changed.
					try
					{
						if (AndroidDebugBridge.clientSupport)
						{
							if ((mDebugSelectedChan == null || mNewDebugSelectedPort != mDebugSelectedPort) && mNewDebugSelectedPort != -1)
							{
								if (reopenDebugSelectedPort())
								{
									mDebugSelectedPort = mNewDebugSelectedPort;
								}
							}
						}
					}
					catch (IOException ioe)
					{
						Log.e("ddms", "Failed to reopen debug port for Selected Client to: " + mNewDebugSelectedPort);
						Log.e("ddms", ioe);
						mNewDebugSelectedPort = mDebugSelectedPort; // no retry
					}

					int count;
					try
					{
						count = mSelector.select();
					}
					catch (Exception ioe)
					{
						Console.WriteLine(ioe.ToString());
						Console.Write(ioe.StackTrace);
						continue;
					}
					/*catch (CancelledKeyException cke)
					{
						continue;
					}*/

					if (count == 0)
					{
						// somebody called wakeup() ?
						// Log.i("ddms", "selector looping");
						continue;
					}

					var keys = mSelector.selectedKeys();
					foreach (var key in keys)
					{
						//SelectionKey key = iter.Current;
						//iter.remove();

						try
						{
							if (key.attachment() is Client)
							{
								processClientActivity(key);
							}
							else if (key.attachment() is Debugger)
							{
								processDebuggerActivity(key);
							}
							else if (key.attachment() is MonitorThread)
							{
								processDebugSelectedActivity(key);
							}
							else
							{
								Log.e("ddms", "unknown activity key");
							}
						}
						catch (Exception e)
						{
							// we don't want to have our thread be killed because of any uncaught
							// exception, so we intercept all here.
							Log.e("ddms", "Exception during activity from Selector.");
							Log.e("ddms", e);
						}
					}
				}
				catch (Exception e)
				{
					// we don't want to have our thread be killed because of any uncaught
					// exception, so we intercept all here.
					Log.e("ddms", "Exception MonitorThread.run()");
					Log.e("ddms", e);
				}
			}
		}


		/// <summary>
		/// Returns the port on which the selected client listen for debugger
		/// </summary>
		internal int debugSelectedPort
		{
		    get { return mDebugSelectedPort; }
            set { mDebugSelectedPort = value; }
		}

	    /*
		 * Something happened. Figure out what.
		 */
		private void processClientActivity(SelectionKey key)
		{
			Client client = (Client)key.attachment();

			try
			{
				if (key.readable == false || key.valid == false)
				{
					Log.d("ddms", "Invalid key from " + client + ". Dropping client.");
					dropClient(client, true); // notify
					return;
				}

				client.read();

				/*
				 * See if we have a full packet in the buffer. It's possible we have
				 * more than one packet, so we have to loop.
				 */
				JdwpPacket packet = client.jdwpPacket;
				while (packet != null)
				{
					if (packet.ddmPacket)
					{
						// unsolicited DDM request - hand it off
						Debug.Assert(!packet.reply);
						callHandler(client, packet, null);
						packet.consume();
					}
					else if (packet.reply && client.isResponseToUs(packet.id) != null)
					{
						// reply to earlier DDM request
						ChunkHandler handler = client.isResponseToUs(packet.id);
						if (packet.error)
						{
							client.packetFailed(packet);
						}
						else if (packet.empty)
						{
							Log.d("ddms", "Got empty reply for 0x" + packet.id.toHexString() + " from " + client);
						}
						else
						{
							callHandler(client, packet, handler);
						}
						packet.consume();
						client.removeRequestId(packet.id);
					}
					else
					{
                        Log.v("ddms", "Forwarding client " + (packet.reply ? "reply" : "event") + " 0x" + packet.id.toHexString() + " to " + client.debugger);
						client.forwardPacketToDebugger(packet);
					}

					// find next
					packet = client.jdwpPacket;
				}
			}
			catch (CancelledKeyException)
			{
				// key was canceled probably due to a disconnected client before we could
				// read stuff coming from the client, so we drop it.
				dropClient(client, true); // notify
			}
			catch (IOException)
			{
				// something closed down, no need to print anything. The client is simply dropped.
				dropClient(client, true); // notify
			}
			catch (Exception ex)
			{
				Log.e("ddms", ex);

				/* close the client; automatically un-registers from selector */
				dropClient(client, true); // notify

				if (ex is OverflowException)
				{
					Log.w("ddms", "Client data packet exceeded maximum buffer size " + client);
				}
				else
				{
					// don't know what this is, display it
					Log.e("ddms", ex);
				}
			}
		}

		/*
		 * Process an incoming DDM packet. If this is a reply to an earlier request,
		 * "handler" will be set to the handler responsible for the original
		 * request. The spec allows a JDWP message to include multiple DDM chunks.
		 */
		private void callHandler(Client client, JdwpPacket packet, ChunkHandler handler)
		{

			// on first DDM packet received, broadcast a "ready" message
			if (!client.ddmSeen())
			{
				broadcast(CLIENT_READY, client);
			}

			ByteBuffer buf = packet.payload;
			int type, length;
			bool reply = true;

            type = buf.getInt();
            length = buf.getInt();

			if (handler == null)
			{
				// not a reply, figure out who wants it
				lock (mHandlerMap)
				{
					handler = mHandlerMap[type];
					reply = false;
				}
			}

			if (handler == null)
			{
				Log.w("ddms", "Received unsupported chunk type " + ChunkHandler.name(type) + " (len=" + length + ")");
			}
			else
			{
				Log.d("ddms", "Calling handler for " + ChunkHandler.name(type) + " [" + handler + "] (len=" + length + ")");
				ByteBuffer ibuf = buf.slice();
				ByteBuffer roBuf = ibuf.asReadOnlyBuffer(); // enforce R/O
				roBuf.order = ChunkHandler.CHUNK_ORDER;
				// do the handling of the chunk synchronized on the client list
				// to be sure there's no concurrency issue when we look for HOME
				// in hasApp()
				lock (mClientList)
				{
					handler.handleChunk(client, type, roBuf, reply, packet.id);
				}
			}
		}

		/// <summary>
		/// Drops a client from the monitor.
		/// <p/>This will lock the <seealso cref="Client"/> list of the <seealso cref="Device"/> running <var>client</var>. </summary>
		/// <param name="client"> </param>
		/// <param name="notify"> </param>
		internal void dropClient(Client client, bool notify)
		{
            lock (this)
            {
                if (mInstance == null)
                {
                    return;
                }

                lock (mClientList)
                {
                    if (mClientList.Remove(client) == false)
                    {
                        return;
                    }
                }
                client.close(notify);
                broadcast(CLIENT_DISCONNECTED, client);

                /*
			 * http://forum.java.sun.com/thread.jspa?threadID=726715&start=0
			 * http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=5073504
			 */
                wakeup();
            }
		}

		/*
		 * Process activity from one of the debugger sockets. This could be a new
		 * connection or a data packet.
		 */
		private void processDebuggerActivity(SelectionKey key)
		{
			Debugger dbg = (Debugger)key.attachment();

			try
			{
				if (key.acceptable)
				{
					try
					{
						acceptNewDebugger(dbg, null);
					}
					catch (IOException ioe)
					{
						Log.w("ddms", "debugger accept() failed");
						Console.WriteLine(ioe.ToString());
						Console.Write(ioe.StackTrace);
					}
				}
				else if (key.readable)
				{
					processDebuggerData(key);
				}
				else
				{
					Log.d("ddm-debugger", "key in unknown state");
				}
			}
			catch (CancelledKeyException)
			{
				// key has been cancelled we can ignore that.
			}
		}

		/*
		 * Accept a new connection from a debugger. If successful, register it with
		 * the Selector.
		 */
		private void acceptNewDebugger(Debugger dbg, ServerSocketChannel acceptChan) 
		{

			lock (mClientList)
			{
				SocketChannel chan;

				if (acceptChan == null)
				{
					chan = dbg.accept();
				}
				else
				{
					chan = dbg.accept(acceptChan);
				}

				if (chan != null)
				{
					chan.socket().NoDelay = true;

					wakeup();

					try
					{
						chan.register(mSelector, SelectionKey.OP_READ, dbg);
					}
					catch (IOException ioe)
					{
						// failed, drop the connection
						dbg.closeData();
						throw ioe;
					}
					catch (Exception re)
					{
						// failed, drop the connection
						dbg.closeData();
						throw re;
					}
				}
				else
				{
					Log.w("ddms", "ignoring duplicate debugger");
					// new connection already closed
				}
			}
		}

		/*
		 * We have incoming data from the debugger. Forward it to the client.
		 */
		private void processDebuggerData(SelectionKey key)
		{
			Debugger dbg = (Debugger)key.attachment();

			try
			{
				/*
				 * Read pending data.
				 */
				dbg.read();

				/*
				 * See if we have a full packet in the buffer. It's possible we have
				 * more than one packet, so we have to loop.
				 */
				JdwpPacket packet = dbg.jdwpPacket;
				while (packet != null)
				{
					Log.v("ddms", "Forwarding dbg req 0x" + packet.id.toHexString() + " to " + dbg.client);

					dbg.forwardPacketToClient(packet);

					packet = dbg.jdwpPacket;
				}
			}
			catch (IOException)
			{
				/*
				 * Close data connection; automatically un-registers dbg from
				 * selector. The failure could be caused by the debugger going away,
				 * or by the client going away and failing to accept our data.
				 * Either way, the debugger connection does not need to exist any
				 * longer. We also need to recycle the connection to the client, so
				 * that the VM sees the debugger disconnect. For a DDM-aware client
				 * this won't be necessary, and we can just send a "debugger
				 * disconnected" message.
				 */
				Log.d("ddms", "Closing connection to debugger " + dbg);
				dbg.closeData();
				Client client = dbg.client;
				if (client.ddmAware)
				{
					// TODO: soft-disconnect DDM-aware clients
					Log.d("ddms", " (recycling client connection as well)");

					// we should drop the client, but also attempt to reopen it.
					// This is done by the DeviceMonitor.
					client.deviceImpl.monitor.addClientToDropAndReopen(client, DebugPortManager.DebugPortProvider.NO_STATIC_PORT);
				}
				else
				{
					Log.d("ddms", " (recycling client connection as well)");
					// we should drop the client, but also attempt to reopen it.
					// This is done by the DeviceMonitor.
					client.deviceImpl.monitor.addClientToDropAndReopen(client, DebugPortManager.DebugPortProvider.NO_STATIC_PORT);
				}
			}
		}

		/*
		 * Tell the thread that something has changed.
		 */
		private void wakeup()
		{
			mSelector.wakeup();
		}

		/// <summary>
		/// Tell the thread to stop. Called from UI thread.
		/// </summary>
		internal void quit()
		{
            lock (this)
            {
                mQuit = true;
                wakeup();
                Log.d("ddms", "Waiting for Monitor thread");
                try
                {
                    thread.Join();
                    // since we're quitting, lets drop all the client and disconnect
                    // the DebugSelectedPort
                    lock (mClientList)
                    {
                        foreach (Client c in mClientList)
                        {
                            c.close(false); // notify
                            broadcast(CLIENT_DISCONNECTED, c);
                        }
                        mClientList.Clear();
                    }

                    if (mDebugSelectedChan != null)
                    {
                        mDebugSelectedChan.close();
                        mDebugSelectedChan.socket().Close();
                        mDebugSelectedChan = null;
                    }
                    mSelector.close();
                }
                catch (ThreadInterruptedException ie)
                {
                    Console.WriteLine(ie.ToString());
                    Console.Write(ie.StackTrace);
                }
                catch (IOException e)
                {
                    // TODO Auto-generated catch block
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }

                mInstance = null;
            }
		}

		/// <summary>
		/// Add a new Client to the list of things we monitor. Also adds the client's
		/// channel and the client's debugger listener to the selection list. This
		/// should only be called from one thread (the VMWatcherThread) to avoid a
		/// race between "alreadyOpen" and Client creation.
		/// </summary>
		internal void addClient(Client client)
		{
            lock (this)
            {
                if (mInstance == null)
                {
                    return;
                }

                Log.d("ddms", "Adding new client " + client);

                lock (mClientList)
                {
                    mClientList.Add(client);

                    /*
				 * Register the Client's socket channel with the selector. We attach
				 * the Client to the SelectionKey. If you try to register a new
				 * channel with the Selector while it is waiting for I/O, you will
				 * block. The solution is to call wakeup() and then hold a lock to
				 * ensure that the registration happens before the Selector goes
				 * back to sleep.
				 */
                    try
                    {
                        wakeup();

                        client.register(mSelector);

                        Debugger dbg = client.debugger;
                        if (dbg != null)
                        {
                            dbg.registerListener(mSelector);
                        }
                    }
                    catch (IOException ioe)
                    {
                        // not really expecting this to happen
                        Console.WriteLine(ioe.ToString());
                        Console.Write(ioe.StackTrace);
                    }
                }
            }
		}

		/*
		 * Broadcast an event to all message handlers.
		 */
		private void broadcast(int @event, Client client)
		{
			Log.d("ddms", "broadcast " + @event + ": " + client);

			/*
			 * The handler objects appear once in mHandlerMap for each message they
			 * handle. We want to notify them once each, so we convert the HashMap
			 * to a HashSet before we iterate.
			 */
			HashSet<ChunkHandler> set;
			lock (mHandlerMap)
			{
				ICollection<ChunkHandler> values = mHandlerMap.Values;
				set = new HashSet<ChunkHandler>(values);
			}

			IEnumerator<ChunkHandler> iter = set.GetEnumerator();
			while (iter.MoveNext())
			{
				ChunkHandler handler = iter.Current;
				switch (@event)
				{
					case CLIENT_READY:
						try
						{
							handler.clientReady(client);
						}
						catch (IOException)
						{
							// Something failed with the client. It should
							// fall out of the list the next time we try to
							// do something with it, so we discard the
							// exception here and assume cleanup will happen
							// later. May need to propagate farther. The
							// trouble is that not all values for "event" may
							// actually throw an exception.
							Log.w("ddms", "Got exception while broadcasting 'ready'");
							return;
						}
						break;
					case CLIENT_DISCONNECTED:
						handler.clientDisconnected(client);
						break;
					default:
						throw new NotSupportedException();
				}
			}

		}

		/// <summary>
		/// Opens (or reopens) the "debug selected" port and listen for connections. </summary>
		/// <returns> true if the port was opened successfully. </returns>
		/// <exception cref="IOException"> </exception>
		private bool reopenDebugSelectedPort() 
		{

			Log.d("ddms", "reopen debug-selected port: " + mNewDebugSelectedPort);
			if (mDebugSelectedChan != null)
			{
				mDebugSelectedChan.close();
			}

			mDebugSelectedChan = ServerSocketChannel.open();
			mDebugSelectedChan.configureBlocking(false); // required for Selector

			var addr = new DnsEndPoint("localhost", mNewDebugSelectedPort); //$NON-NLS-1$
			mDebugSelectedChan.socket().ExclusiveAddressUse = false; // enable SO_REUSEADDR

			try
			{
				mDebugSelectedChan.socket().Bind(addr);
				if (mSelectedClient != null)
				{
					mSelectedClient.update(Client.CHANGE_PORT);
				}

				mDebugSelectedChan.register(mSelector, SelectionKey.OP_ACCEPT, this);

				return true;
			}
			catch (Exception)
			{
				displayDebugSelectedBindError(mNewDebugSelectedPort);

				// do not attempt to reopen it.
				mDebugSelectedChan = null;
				mNewDebugSelectedPort = -1;

				return false;
			}
		}

		/*
		 * We have some activity on the "debug selected" port. Handle it.
		 */
		private void processDebugSelectedActivity(SelectionKey key)
		{
			Debug.Assert(key.acceptable);

			ServerSocketChannel acceptChan = (ServerSocketChannel)key.channel();

			/*
			 * Find the debugger associated with the currently-selected client.
			 */
			if (mSelectedClient != null)
			{
				Debugger dbg = mSelectedClient.debugger;

				if (dbg != null)
				{
					Log.d("ddms", "Accepting connection on 'debug selected' port");
					try
					{
						acceptNewDebugger(dbg, acceptChan);
					}
					catch (IOException)
					{
						// client should be gone, keep going
					}

					return;
				}
			}

			Log.w("ddms", "Connection on 'debug selected' port, but none selected");
			try
			{
				SocketChannel chan = acceptChan.accept();
				chan.close();
			}
			catch (IOException)
			{
				// not expected; client should be gone, keep going
			}
			catch (NotYetBoundException)
			{
				displayDebugSelectedBindError(mDebugSelectedPort);
			}
		}

		private void displayDebugSelectedBindError(int port)
		{
			string message = string.Format("Could not open Selected VM debug port ({0:D}). Make sure you do not have another instance of DDMS or of the eclipse plugin running. If it's being used by something else, choose a new port number in the preferences.", port);

			Log.logAndDisplay(Log.LogLevel.ERROR, "ddms", message);
		}
	}

}