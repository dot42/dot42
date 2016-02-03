using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// TCP/IP connection to a debugger.
    /// </summary>
    public partial class JdwpConnection : IDisposable, IJdwpServerInfo
    {
        private const int WriteAttempts = 3;
        private const int MaxWriteErrors = 3;

        private enum States { NotConnected, AwaitingHandshake, Ready }

        /// <summary>
        /// Fired when the connection is no longer alive.
        /// </summary>
        public event EventHandler Disconnect;

        private readonly Action<Chunk> chunkHandler;
        private readonly Action<JdwpPacket> packetHandler;
        private readonly int pid;
        private static readonly byte[] Handshake = Encoding.ASCII.GetBytes("JDWP-Handshake");
        private readonly TcpClient tcpClient;
        private States state;
        private Thread readThread;
        private Thread writeThread;
        private int lastId;
        private readonly ConcurrentDictionary<int,Action<JdwpPacket>> callbacks = new ConcurrentDictionary<int, Action<JdwpPacket>>();
        private readonly Queue<JdwpPacket> writeQueue = new Queue<JdwpPacket>();
        private readonly object writeQueueLock = new object();
        private readonly object infoLock = new object();
        private IdSizeInfo idSizeInfo;
        private VersionInfo versionInfo;

        /// <summary>
        /// Default ctor
        /// </summary>
        public JdwpConnection(IPEndPoint endPoint, Action<Chunk> chunkHandler, int pid, Action<JdwpPacket> packetHandler)
        {
            this.chunkHandler = chunkHandler;
            this.pid = pid;
            this.packetHandler = packetHandler;
            state = States.NotConnected;

            // Setup connection
            tcpClient = new TcpClient { ExclusiveAddressUse = true };
            tcpClient.Connect(endPoint);
            state = States.AwaitingHandshake;

            // Start packet reader
            readThread = new Thread(ReadLoop) { IsBackground =  true, Name = "JDWP Read Thread" };
            readThread.Start();

            SendHandshake();

            // Start packet writer
            writeThread = new Thread(WriteLoop) { IsBackground = true, Name = "JDWP Write Thread"};
            writeThread.Start();

            // HELO
            SendHelo();
        }

        public VersionInfo VmVersion
        {
            get
            {
                if (versionInfo == null)
                    throw new Exception("debugging not active");
                return versionInfo;
            }
        }

        /// <summary>
        /// Prepare this connection for actual debugging.
        /// </summary>
        public Task StartDebuggingAsync()
        {
            // Request version of debugger in VM.
            var versionTask = VersionAsync().ContinueWith(x => {
                x.ForwardException();
                SetVersionInfo(x.Result);
            });

            // Request ID size information
            var idSizesTask = IdSizesAsync().ContinueWith(x => {
                x.ForwardException();
                SetIdSizeInfo(x.Result);
            });      
      
            // Now wait for result
            return Task.Factory.StartNew(() => Task.WaitAll(versionTask, idSizesTask));
        }

        /// <summary>
        /// Close the connection.
        /// </summary>
        public void Close()
        {
            try
            {
                readThread = null;
                writeThread = null;
                tcpClient.Close();
                lock (writeQueueLock)
                {
                    writeQueue.Clear();
                    Monitor.PulseAll(writeQueueLock);
                }

                // Finish callbacks
                while (!callbacks.IsEmpty)
                {
                    var key = callbacks.Keys.FirstOrDefault();
                    Action<JdwpPacket> callback;
                    if (callbacks.TryRemove(key, out callback))
                    {
                        try
                        {
                            callback(JdwpPacket.VmDeadError);
                        }
                        catch
                        {
                            // Ignore
                        }
                    }
                }
            }
            catch
            {
                // Ignore
            }
        }

        /// <summary>
        /// Send handshake packet.
        /// </summary>
        private void SendHandshake()
        {
            DLog.Debug(DContext.DebuggerLibJdwpConnection, "SendHandshake for pid {0}", pid);

            // Send handshape
            Write(Handshake);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        void IDisposable.Dispose()
        {
            Close();
        }

        /// <summary>
        /// Record a packet-id, callback pair.
        /// </summary>
        protected void RegisterCallback(int packetId, Action<JdwpPacket> callback)
        {
            callbacks.AddOrUpdate(packetId, callback, (key, oldvalue) => callback);
        }

        /// <summary>
        /// Read incoming packets
        /// </summary>
        private void ReadLoop()
        {
            var myThread = Thread.CurrentThread;
            var readBuffer = new byte[1024 * 4024];
            var readBufferOffset = 0;

            while (readThread == myThread)
            {
                // Should we stop?
                if ((!tcpClient.Connected) || (readThread != myThread))
                    return; 

                // Read all available data, at least if we are processing the input fast enough
                var available = tcpClient.Available;
                if (available > 0 && readBufferOffset + available < readBuffer.Length)
                {
                    Read(readBuffer, readBufferOffset, available);
                    readBufferOffset += available;
                }
                else if (readBufferOffset == 0)
                {
                    // Wait a while
                    Thread.Sleep(10);
                }

                // Try to find packet and process them
                if (readBufferOffset == 0)
                    continue;                

                if (state == States.AwaitingHandshake)
                {
                    // Look for handshake
                    if (readBufferOffset < Handshake.Length) 
                        continue;
                    if (Equals(readBuffer, Handshake, Handshake.Length))
                    {
                        DLog.Debug(DContext.DebuggerLibJdwpConnection, "Valid handshake received from VM");
                        // Found correct handshake
                        ConsumeReadBuffer(Handshake.Length, readBuffer, ref readBufferOffset);
                        state = States.Ready;
                        NotifyWriteQueue();
                    }
                    else
                    {
                        // Invalid handshake
                        DLog.Error(DContext.DebuggerLibJdwpConnection, "Received invalid handshape");
                    }
                }
                else if (state == States.Ready)
                {
                    // Normal packet expected
                    var packet = FindPacket(this, readBuffer, readBufferOffset);
                    if (packet == null)
                        continue;

                    // Remove packet from buffer
                    var length = packet.Length;
                    ConsumeReadBuffer(length, readBuffer, ref readBufferOffset);

#if DEBUG
                    DLog.Debug(DContext.DebuggerLibJdwpConnection,
                               "Read packet " + (packet.IsChunk() ? packet.AsChunk() : packet));
#endif
                    try
                    {
                        // Find callback
                        var processed = false;
                        if (packet.IsReply)
                        {
                            var packetId = packet.Id;
                            Action<JdwpPacket> callback;
                            if (callbacks.TryRemove(packetId, out callback))
                            {
                                // Perform callback.
                                processed = true;
                                callback(packet);
                            }
                        }
                        if (!processed)
                        {
                            if (packet.IsChunk())
                            {
                                // Handle non-reply chunks
                                chunkHandler(packet.AsChunk());
                            }
                            else if (!packet.IsReply)
                            {
                                // Handle non-reply packet
                                packetHandler(packet);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DLog.Error(DContext.DebuggerLibJdwpConnection, "JdwpConnection: Exception while handling packet. IsReply={1}; {2}", ex, packet.IsReply, packet);
                    }
                }
            }            
        }

        /// <summary>
        /// Change the buffer such that the first length bytes are being used.
        /// </summary>
        private static void ConsumeReadBuffer(int length, byte[] readBuffer, ref int readBufferOffset)
        {
            if (length == readBufferOffset)
            {
                // The entire buffer is used up
                readBufferOffset = 0;
            }
            else
            {
                // Move unused part to beginning
                Array.Copy(readBuffer, length, readBuffer, 0, readBufferOffset - length);
                readBufferOffset -= length;
            }
        }

        /// <summary>
        /// Try to find a packet in the given buffer.
        /// </summary>
        /// <returns>Null if not found</returns>
        private static JdwpPacket FindPacket(IJdwpServerInfo serverInfo, byte[] buffer, int length)
        {
            if (length < JdwpPacket.HeaderLength)
                return null;
            var pkt = new JdwpPacket(serverInfo, buffer, 0);
            if (pkt.Length > length)
                return null;
            return new JdwpPacket(serverInfo, buffer, 0, true);
        }

        /// <summary>
        /// Write queued packets
        /// </summary>
        private void WriteLoop()
        {
            var myThread = Thread.CurrentThread;
            var errorCount = 0;

            while ((writeThread == myThread))
            {
                // Too many errors or disconnected?
                if ((errorCount > MaxWriteErrors) || !tcpClient.Connected)
                {
                    Disconnect.Fire(this);
                    return;
                }

                JdwpPacket packet = null;
                lock (writeQueueLock)
                {
                    if ((writeQueue.Count == 0) || (state == States.AwaitingHandshake))
                    {
                        // Signal all waiters
                        Monitor.PulseAll(writeQueueLock);

                        // Now wait ourselves for new packets
                        Monitor.Wait(writeQueueLock);
                    }
                    else
                    {
                        packet = writeQueue.Dequeue();
                    }
                }

                // Disconnected?
                if (!tcpClient.Connected)
                {
                    Disconnect.Fire(this);
                    return;
                }

                if ((packet != null) && (writeThread == myThread))
                {
                    var attempt = 0;
                    var succeeded = false;
                    while ((attempt < WriteAttempts) && !succeeded)
                    {
                        try
                        {
                            attempt++;
                            DLog.Debug(DContext.DebuggerLibJdwpConnection, "Write {0}", packet);
                            packet.WriteTo(tcpClient.GetStream());
                            succeeded = true;
                            errorCount = Math.Max(errorCount - 1, 0);
                        }
                        catch
                        {
                        }
                    }
                    if (!succeeded)
                    {
                        errorCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Reads from the socket until the array is filled, or no more data is coming (because
        /// the socket closed or the timeout expired).
        /// <p/>This uses the default time out value.
        /// </summary>
        /// <param name="data"> the buffer to store the read data into. </param>
        /// <param name="offset">Offset in data</param>
        /// <param name="length">Bytes to read</param>
        /// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
        /// <exception cref="IOException"> in case of I/O error on the connection. </exception>
        private void Read(byte[] data, int offset, int length)
        {
            var stream = tcpClient.GetStream();
            var read = stream.Read(data, offset, length);
            if (read != length)
            {
                throw new IOException(string.Format("Not enough data (expected {0}, got {1})", length, read));
            }
        }

        /// <summary>
        /// Write until all data in "data" is written or the connection fails or times out.
        /// <p/>This uses the default time out value. </summary>
        /// <param name="data"> the buffer to send. </param>
        private void Write(byte[] data)
        {
            tcpClient.GetStream().Write(data, 0, data.Length);
        }

        /// <summary>
        /// Send the entire packet towards the target VM.
        /// </summary>
        protected internal Task<T> SendAsync<T>(T packet)
            where T : JdwpPacket 
        {
            packet.Id = GetNextId();
            var tcs = new TaskCompletionSource<T>();
            RegisterCallback(packet.Id, x => {
                var result = x;
                if (typeof(T) == typeof(Chunk))
                {
                    result = x.AsChunk();
                }
                tcs.SetResult((T) result);
            });
            lock (writeQueueLock)
            {
                writeQueue.Enqueue(packet);
                Monitor.Pulse(writeQueueLock);
            }
            return tcs.Task;
        }

        /// <summary>
        /// Send the entire packet towards the target VM.
        /// </summary>
        protected internal void AddToWriteQueue(JdwpPacket packet)
        {
            packet.Id = GetNextId();
            lock (writeQueueLock)
            {
                writeQueue.Enqueue(packet);
                Monitor.Pulse(writeQueueLock);
            }
        }

        /// <summary>
        /// Unlock waiters on the write queue.
        /// </summary>
        private void NotifyWriteQueue()
        {
            lock (writeQueueLock)
            {
                Monitor.Pulse(writeQueueLock);
            }
        }

        /// <summary>
        /// Wait until the write queue is empty.
        /// </summary>
        internal void WaitUntilWriteQueueEmpty()
        {
            lock (writeQueueLock)
            {
                while (writeQueue.Count > 0)
                {
                    Monitor.Wait(writeQueueLock);
                }
            }
        }

        /// <summary>
        /// Gets the next packet id.
        /// </summary>
        protected int GetNextId()
        {
            return Interlocked.Increment(ref lastId);
        }

        /// <summary>
        /// Record the version info.
        /// </summary>
        private void SetVersionInfo(VersionInfo info)
        {
            lock (infoLock)
            {
                versionInfo = info;
                Monitor.PulseAll(infoLock);
            }
        }

        /// <summary>
        /// Gets the ID size info.
        /// Wait's for available data if needed.
        /// </summary>
        internal IdSizeInfo GetIdSizeInfo()
        {
            if (idSizeInfo != null) return idSizeInfo;
            lock (infoLock)
            {
                while (idSizeInfo == null)
                {
                    Monitor.Wait(infoLock);
                }
                return idSizeInfo;
            }
        }

        /// <summary>
        /// Record the ID size info.
        /// </summary>
        private void SetIdSizeInfo(IdSizeInfo info)
        {
            lock (infoLock)
            {
                idSizeInfo = info;
                Monitor.PulseAll(infoLock);
            }
        }

        /// <summary>
        /// Are both array's the same in terms of content?
        /// </summary>
        private static bool Equals(byte[] arr1, byte[] arr2, int length)
        {

            if (length > arr1.Length)
                return false;
            if (length > arr2.Length)
                return false;
            for (var i = 0; i < length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the ID size information.
        /// </summary>
        IdSizeInfo IJdwpServerInfo.IdSizeInfo
        {
            get { return GetIdSizeInfo(); }
        }

    }
}
