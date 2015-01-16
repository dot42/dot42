using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Dot42.DdmLib.support;
using Dot42.DdmLib.utils;

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
	/// Sync service class to push/pull to/from devices/emulators, through the debug bridge.
	/// <p/>
	/// To get a <seealso cref="SyncService"/> object, use <seealso cref="Device#getSyncService()"/>.
	/// </summary>
	public sealed class SyncService
	{

		private static readonly byte[] ID_OKAY = {(byte) 'O', (byte) 'K', (byte) 'A', (byte) 'Y'};
		private static readonly byte[] ID_FAIL = {(byte) 'F', (byte) 'A', (byte) 'I', (byte) 'L'};
		private static readonly byte[] ID_STAT = {(byte) 'S', (byte) 'T', (byte) 'A', (byte) 'T'};
		private static readonly byte[] ID_RECV = {(byte) 'R', (byte) 'E', (byte) 'C', (byte) 'V'};
		private static readonly byte[] ID_DATA = {(byte) 'D', (byte) 'A', (byte) 'T', (byte) 'A'};
		private static readonly byte[] ID_DONE = {(byte) 'D', (byte) 'O', (byte) 'N', (byte) 'E'};
		private static readonly byte[] ID_SEND = {(byte) 'S', (byte) 'E', (byte) 'N', (byte) 'D'};
	//    private final static byte[] ID_LIST = { 'L', 'I', 'S', 'T' };
	//    private final static byte[] ID_DENT = { 'D', 'E', 'N', 'T' };

		private static readonly NullSyncProgresMonitor sNullSyncProgressMonitor = new NullSyncProgresMonitor();

		private const int S_ISOCK = 0xC000; // type: symbolic link
		private const int S_IFLNK = 0xA000; // type: symbolic link
		private const int S_IFREG = 0x8000; // type: regular file
		private const int S_IFBLK = 0x6000; // type: block device
		private const int S_IFDIR = 0x4000; // type: directory
		private const int S_IFCHR = 0x2000; // type: character device
		private const int S_IFIFO = 0x1000; // type: fifo
	/*
	    private final static int S_ISUID = 0x0800; // set-uid bit
	    private final static int S_ISGID = 0x0400; // set-gid bit
	    private final static int S_ISVTX = 0x0200; // sticky bit
	    private final static int S_IRWXU = 0x01C0; // user permissions
	    private final static int S_IRUSR = 0x0100; // user: read
	    private final static int S_IWUSR = 0x0080; // user: write
	    private final static int S_IXUSR = 0x0040; // user: execute
	    private final static int S_IRWXG = 0x0038; // group permissions
	    private final static int S_IRGRP = 0x0020; // group: read
	    private final static int S_IWGRP = 0x0010; // group: write
	    private final static int S_IXGRP = 0x0008; // group: execute
	    private final static int S_IRWXO = 0x0007; // other permissions
	    private final static int S_IROTH = 0x0004; // other: read
	    private final static int S_IWOTH = 0x0002; // other: write
	    private final static int S_IXOTH = 0x0001; // other: execute
	*/

		private const int SYNC_DATA_MAX = 64 * 1024;
		private const int REMOTE_PATH_MAX_LENGTH = 1024;

		/// <summary>
		/// Classes which implement this interface provide methods that deal
		/// with displaying transfer progress.
		/// </summary>
		public interface ISyncProgressMonitor
		{
			/// <summary>
			/// Sent when the transfer starts </summary>
			/// <param name="totalWork"> the total amount of work. </param>
			void start(int totalWork);
			/// <summary>
			/// Sent when the transfer is finished or interrupted.
			/// </summary>
			void stop();
			/// <summary>
			/// Sent to query for possible cancellation. </summary>
			/// <returns> true if the transfer should be stopped. </returns>
			bool canceled {get;}
			/// <summary>
			/// Sent when a sub task is started. </summary>
			/// <param name="name"> the name of the sub task. </param>
			void startSubTask(string name);
			/// <summary>
			/// Sent when some progress have been made. </summary>
			/// <param name="work"> the amount of work done. </param>
			void advance(int work);
		}

		/// <summary>
		/// A Sync progress monitor that does nothing
		/// </summary>
		private class NullSyncProgresMonitor : ISyncProgressMonitor
		{
			public void advance(int work)
			{
			}
			public bool canceled
			{
				get
				{
					return false;
				}
			}

			public void start(int totalWork)
			{
			}
			public void startSubTask(string name)
			{
			}
			public void stop()
			{
			}
		}

		private EndPoint mAddress;
		private Device mDevice;
		private SocketChannel mChannel;

		/// <summary>
		/// Buffer used to send data. Allocated when needed and reused afterward.
		/// </summary>
		private byte[] mBuffer;

		/// <summary>
		/// Creates a Sync service object. </summary>
		/// <param name="address"> The address to connect to </param>
		/// <param name="device"> the <seealso cref="Device"/> that the service connects to. </param>
		internal SyncService(EndPoint address, Device device)
		{
			mAddress = address;
			mDevice = device;
		}

		/// <summary>
		/// Opens the sync connection. This must be called before any calls to push[File] / pull[File]. </summary>
		/// <returns> true if the connection opened, false if adb refuse the connection. This can happen
		/// if the <seealso cref="Device"/> is invalid. </returns>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> If the connection to adb failed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: boolean openSync() throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		internal bool openSync()
		{
			try
			{
				mChannel = SocketChannel.open(mAddress);
				mChannel.configureBlocking(false);

				// target a specific device
				AdbHelper.setDevice(mChannel, mDevice);

				var request = AdbHelper.formAdbRequest("sync:"); //$NON-NLS-1$
				AdbHelper.write(mChannel, request, -1, DdmPreferences.timeOut);

				AdbHelper.AdbResponse resp = AdbHelper.readAdbResponse(mChannel, false); // readDiagString

				if (resp.okay == false)
				{
					Log.w("ddms", "Got unhappy response from ADB sync req: " + resp.message);
					mChannel.close();
					mChannel = null;
					return false;
				}
			}
			catch (TimeoutException e)
			{
				if (mChannel != null)
				{
					try
					{
						mChannel.close();
					}
					catch (IOException)
					{
						// we want to throw the original exception, so we ignore this one.
					}
					mChannel = null;
				}

				throw e;
			}
			catch (IOException e)
			{
				if (mChannel != null)
				{
					try
					{
						mChannel.close();
					}
					catch (IOException)
					{
						// we want to throw the original exception, so we ignore this one.
					}
					mChannel = null;
				}

				throw e;
			}

			return true;
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public void close()
		{
			if (mChannel != null)
			{
				try
				{
					mChannel.close();
				}
				catch (IOException)
				{
					// nothing to be done really...
				}
				mChannel = null;
			}
		}

		/// <summary>
		/// Returns a sync progress monitor that does nothing. This allows background tasks that don't
		/// want/need to display ui, to pass a valid <seealso cref="ISyncProgressMonitor"/>.
		/// <p/>This object can be reused multiple times and can be used by concurrent threads.
		/// </summary>
		public static ISyncProgressMonitor nullProgressMonitor
		{
			get
			{
				return sNullSyncProgressMonitor;
			}
		}

		/// <summary>
		/// Pulls file(s) or folder(s). </summary>
		/// <param name="entries"> the remote item(s) to pull </param>
		/// <param name="localPath"> The local destination. If the entries count is > 1 or
		///      if the unique entry is a folder, this should be a folder. </param>
		/// <param name="monitor"> The progress monitor. Cannot be null. </param>
		/// <exception cref="SyncException"> </exception>
		/// <exception cref="IOException"> </exception>
		/// <exception cref="TimeoutException">
		/// </exception>
		/// <seealso cref= FileListingService.FileEntry </seealso>
		/// <seealso cref= #getNullProgressMonitor() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pull(com.android.ddmlib.FileListingService.FileEntry[] entries, String localPath, ISyncProgressMonitor monitor) throws SyncException, java.io.IOException, TimeoutException
		public void pull(FileListingService.FileEntry[] entries, string localPath, ISyncProgressMonitor monitor)
		{

			// first we check the destination is a directory and exists
			if (!Directory.Exists(localPath))
			{
				throw new SyncException(SyncException.SyncError.NO_DIR_TARGET);
			}
			if (File.Exists(localPath))
			{
				throw new SyncException(SyncException.SyncError.TARGET_IS_FILE);
			}

			// get a FileListingService object
			FileListingService fls = new FileListingService(mDevice);

			// compute the number of file to move
			int total = getTotalRemoteFileSize(entries, fls);

			// start the monitor
			monitor.start(total);

			doPull(entries, localPath, fls, monitor);

			monitor.stop();
		}

		/// <summary>
		/// Pulls a single file. </summary>
		/// <param name="remote"> the remote file </param>
		/// <param name="localFilename"> The local destination. </param>
		/// <param name="monitor"> The progress monitor. Cannot be null.
		/// </param>
		/// <exception cref="IOException"> in case of an IO exception. </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
		/// <exception cref="SyncException"> in case of a sync exception.
		/// </exception>
		/// <seealso cref= FileListingService.FileEntry </seealso>
		/// <seealso cref= #getNullProgressMonitor() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pullFile(com.android.ddmlib.FileListingService.FileEntry remote, String localFilename, ISyncProgressMonitor monitor) throws java.io.IOException, SyncException, TimeoutException
		public void pullFile(FileListingService.FileEntry remote, string localFilename, ISyncProgressMonitor monitor)
		{
			int total = remote.sizeValue;
			monitor.start(total);

			doPullFile(remote.fullPath, localFilename, monitor);

			monitor.stop();
		}

		/// <summary>
		/// Pulls a single file.
		/// <p/>Because this method just deals with a String for the remote file instead of a
		/// <seealso cref="FileListingService.FileEntry"/>, the size of the file being pulled is unknown and the
		/// <seealso cref="ISyncProgressMonitor"/> will not properly show the progress </summary>
		/// <param name="remoteFilepath"> the full path to the remote file </param>
		/// <param name="localFilename"> The local destination. </param>
		/// <param name="monitor"> The progress monitor. Cannot be null.
		/// </param>
		/// <exception cref="IOException"> in case of an IO exception. </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
		/// <exception cref="SyncException"> in case of a sync exception.
		/// </exception>
		/// <seealso cref= #getNullProgressMonitor() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pullFile(String remoteFilepath, String localFilename, ISyncProgressMonitor monitor) throws TimeoutException, java.io.IOException, SyncException
		public void pullFile(string remoteFilepath, string localFilename, ISyncProgressMonitor monitor)
		{
			int? mode = readMode(remoteFilepath);
			if (mode == null)
			{
				// attempts to download anyway
			}
			else if (mode == 0)
			{
				throw new SyncException(SyncException.SyncError.NO_REMOTE_OBJECT);
			}

			monitor.start(0);
			//TODO: use the {@link FileListingService} to get the file size.

			doPullFile(remoteFilepath, localFilename, monitor);

			monitor.stop();
		}

		/// <summary>
		/// Push several files. </summary>
		/// <param name="local"> An array of loca files to push </param>
		/// <param name="remote"> the remote <seealso cref="FileListingService.FileEntry"/> representing a directory. </param>
		/// <param name="monitor"> The progress monitor. Cannot be null. </param>
		/// <exception cref="SyncException"> if file could not be pushed </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void push(String[] local, com.android.ddmlib.FileListingService.FileEntry remote, ISyncProgressMonitor monitor) throws SyncException, java.io.IOException, TimeoutException
		public void push(string[] local, FileListingService.FileEntry remote, ISyncProgressMonitor monitor)
		{
			if (remote.directory == false)
			{
				throw new SyncException(SyncException.SyncError.REMOTE_IS_FILE);
			}

			// get the total count of the bytes to transfer
			int total = getTotalLocalFileSize(local);

			monitor.start(total);

			doPush(local, remote.fullPath, monitor);

			monitor.stop();
		}

		/// <summary>
		/// Push a single file. </summary>
		/// <param name="local"> the local filepath. </param>
		/// <param name="remote"> The remote filepath. </param>
		/// <param name="monitor"> The progress monitor. Cannot be null.
		/// </param>
		/// <exception cref="SyncException"> if file could not be pushed </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pushFile(String local, String remote, ISyncProgressMonitor monitor) throws SyncException, java.io.IOException, TimeoutException
		public void pushFile(string local, string remote, ISyncProgressMonitor monitor)
		{
			if (!File.Exists(local))
			{
				throw new SyncException(SyncException.SyncError.NO_LOCAL_FILE);
			}

			if (Directory.Exists(local))
			{
				throw new SyncException(SyncException.SyncError.LOCAL_IS_DIRECTORY);
			}

			monitor.start((int)new FileInfo(local).Length);

			doPushFile(local, remote, monitor);

			monitor.stop();
		}

		/// <summary>
		/// compute the recursive file size of all the files in the list. Folder
		/// have a weight of 1. </summary>
		/// <param name="entries"> </param>
		/// <param name="fls">
		/// @return </param>
		private int getTotalRemoteFileSize(FileListingService.FileEntry[] entries, FileListingService fls)
		{
			int count = 0;
			foreach (FileListingService.FileEntry e in entries)
			{
				int type = e.type;
				if (type == FileListingService.TYPE_DIRECTORY)
				{
					// get the children
					FileListingService.FileEntry[] children = fls.getChildren(e, false, null);
					count += getTotalRemoteFileSize(children, fls) + 1;
				}
				else if (type == FileListingService.TYPE_FILE)
				{
					count += e.sizeValue;
				}
			}

			return count;
		}

		/// <summary>
		/// compute the recursive file size of all the files in the list. Folder
		/// have a weight of 1.
		/// This does not check for circular links. </summary>
		/// <param name="files">
		/// @return </param>
		private int getTotalLocalFileSize(IEnumerable<string> files)
		{
		    if (files == null) throw new ArgumentNullException("files");
		    long count = 0;

            foreach (var f in files)
            {
                if (Directory.Exists(f))
                {
                    return getTotalLocalFileSize(Directory.GetFileSystemEntries(f)) + 1;
                }
                if (File.Exists(f))
                {
                    count += new FileInfo(f).Length;
                }
            }

		    return (int) count;
		}

		/// <summary>
		/// Pulls multiple files/folders recursively. </summary>
		/// <param name="entries"> The list of entry to pull </param>
		/// <param name="localPath"> the localpath to a directory </param>
		/// <param name="fileListingService"> a FileListingService object to browse through remote directories. </param>
		/// <param name="monitor"> the progress monitor. Must be started already.
		/// </param>
		/// <exception cref="SyncException"> if file could not be pushed </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void doPull(com.android.ddmlib.FileListingService.FileEntry[] entries, String localPath, FileListingService fileListingService, ISyncProgressMonitor monitor) throws SyncException, java.io.IOException, TimeoutException
		private void doPull(FileListingService.FileEntry[] entries, string localPath, FileListingService fileListingService, ISyncProgressMonitor monitor)
		{

			foreach (FileListingService.FileEntry e in entries)
			{
				// check if we're cancelled
				if (monitor.canceled == true)
				{
					throw new SyncException(SyncException.SyncError.CANCELED);
				}

				// get type (we only pull directory and files for now)
				int type = e.type;
				if (type == FileListingService.TYPE_DIRECTORY)
				{
					monitor.startSubTask(e.fullPath);
                    var dest = Path.Combine(localPath, e.name);

					// make the directory
				    Directory.CreateDirectory(dest);

					// then recursively call the content. Since we did a ls command
					// to get the number of files, we can use the cache
					FileListingService.FileEntry[] children = fileListingService.getChildren(e, true, null);
					doPull(children, dest, fileListingService, monitor);
					monitor.advance(1);
				}
				else if (type == FileListingService.TYPE_FILE)
				{
					monitor.startSubTask(e.fullPath);
                    string dest = Path.Combine(localPath, e.name);
					doPullFile(e.fullPath, dest, monitor);
				}
			}
		}

		/// <summary>
		/// Pulls a remote file </summary>
		/// <param name="remotePath"> the remote file (length max is 1024) </param>
		/// <param name="localPath"> the local destination </param>
		/// <param name="monitor"> the monitor. The monitor must be started already. </param>
		/// <exception cref="SyncException"> if file could not be pushed </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void doPullFile(String remotePath, String localPath, ISyncProgressMonitor monitor) throws java.io.IOException, SyncException, TimeoutException
		private void doPullFile(string remotePath, string localPath, ISyncProgressMonitor monitor)
		{
		    var pullResult = new byte[8];

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int timeOut = DdmPreferences.getTimeOut();
			int timeOut = DdmPreferences.timeOut;

			try
			{
				var remotePathContent = remotePath.getBytes(AdbHelper.DEFAULT_ENCODING);

				if (remotePathContent.Length > REMOTE_PATH_MAX_LENGTH)
				{
					throw new SyncException(SyncException.SyncError.REMOTE_PATH_LENGTH);
				}

				// create the full request message
				var msg = createFileReq(ID_RECV, remotePathContent);

				// and send it.
				AdbHelper.write(mChannel, msg, -1, timeOut);

				// read the result, in a byte array containing 2 ints
				// (id, size)
				AdbHelper.read(mChannel, pullResult, -1, timeOut);

				// check we have the proper data back
				if (checkResult(pullResult, ID_DATA) == false && checkResult(pullResult, ID_DONE) == false)
				{
					throw new SyncException(SyncException.SyncError.TRANSFER_PROTOCOL_ERROR, readErrorMessage(pullResult, timeOut));
				}
			}
			catch (ArgumentException e)
			{
				throw new SyncException(SyncException.SyncError.REMOTE_PATH_ENCODING, e);
			}

			// access the destination file
			// create the stream to write in the file. We use a new try/catch block to differentiate
			// between file and network io exceptions.
			FileStream fos = null;
			try
			{
				fos = File.Create(localPath);
			}
			catch (IOException e)
			{
				Log.e("ddms", string.Format("Failed to open local file {0} for writing, Reason: {1}", localPath, e));
				throw new SyncException(SyncException.SyncError.FILE_WRITE_ERROR);
			}

            using (fos)
            {
                // the buffer to read the data
                var data = new byte[SYNC_DATA_MAX];

                // loop to get data until we're done.
                while (true)
                {
                    // check if we're cancelled
                    if (monitor.canceled == true)
                    {
                        throw new SyncException(SyncException.SyncError.CANCELED);
                    }

                    // if we're done, we stop the loop
                    if (checkResult(pullResult, ID_DONE))
                    {
                        break;
                    }
                    if (checkResult(pullResult, ID_DATA) == false)
                    {
                        // hmm there's an error
                        throw new SyncException(SyncException.SyncError.TRANSFER_PROTOCOL_ERROR, readErrorMessage(pullResult, timeOut));
                    }
                    int length = ArrayHelper.swap32bitFromArray(pullResult, 4);
                    if (length > SYNC_DATA_MAX)
                    {
                        // buffer overrun!
                        // error and exit
                        throw new SyncException(SyncException.SyncError.BUFFER_OVERRUN);
                    }

                    // now read the length we received
                    AdbHelper.read(mChannel, data, length, timeOut);

                    // get the header for the next packet.
                    AdbHelper.read(mChannel, pullResult, -1, timeOut);

                    // write the content in the file
                    fos.Write(data, 0, length);

                    monitor.advance(length);
                }

                fos.Flush();
            }
		}


		/// <summary>
		/// Push multiple files </summary>
		/// <param name="fileArray"> </param>
		/// <param name="remotePath"> </param>
		/// <param name="monitor">
		/// </param>
		/// <exception cref="SyncException"> if file could not be pushed </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void doPush(java.io.File[] fileArray, String remotePath, ISyncProgressMonitor monitor) throws SyncException, java.io.IOException, TimeoutException
		private void doPush(IEnumerable<string> fileArray, string remotePath, ISyncProgressMonitor monitor)
		{
            foreach (var f in fileArray)
            {
                // check if we're canceled
                if (monitor.canceled == true)
                {
                    throw new SyncException(SyncException.SyncError.CANCELED);
                }
                if (Directory.Exists(f))
                {
                    // append the name of the directory to the remote path
                    var name = Path.GetFileName(f);
                    string dest = remotePath + "/" + name; // $NON-NLS-1S
                    monitor.startSubTask(dest);
                    doPush(Directory.GetFileSystemEntries(f), dest, monitor);

                    monitor.advance(1);
                }
                else if (File.Exists(f))
                {
                    // append the name of the file to the remote path
                    var name = Path.GetFileName(f);
                    string remoteFile = remotePath + "/" + name; // $NON-NLS-1S
                    monitor.startSubTask(remoteFile);
                    doPushFile(f, remoteFile, monitor);
                }
            }
		}

		/// <summary>
		/// Push a single file </summary>
		/// <param name="localPath"> the local file to push </param>
		/// <param name="remotePath"> the remote file (length max is 1024) </param>
		/// <param name="monitor"> the monitor. The monitor must be started already.
		/// </param>
		/// <exception cref="SyncException"> if file could not be pushed </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void doPushFile(String localPath, String remotePath, ISyncProgressMonitor monitor) throws SyncException, java.io.IOException, TimeoutException
        private void doPushFile(string localPath, string remotePath, ISyncProgressMonitor monitor)
		{
		    byte[] msg;

		    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
		    //ORIGINAL LINE: final int timeOut = DdmPreferences.getTimeOut();
		    int timeOut = DdmPreferences.timeOut;

		    try
		    {
		        var remotePathContent = remotePath.getBytes(AdbHelper.DEFAULT_ENCODING);

		        if (remotePathContent.Length > REMOTE_PATH_MAX_LENGTH)
		        {
		            throw new SyncException(SyncException.SyncError.REMOTE_PATH_LENGTH);
		        }

		        // create the header for the action
		        //JAVA TO C# CONVERTER TODO TASK: Octal literals cannot be represented in C#:
		        msg = createSendFileReq(ID_SEND, remotePathContent, 0644);
		    }
		    catch (ArgumentException e)
		    {
		        throw new SyncException(SyncException.SyncError.REMOTE_PATH_ENCODING, e);
		    }

		    // create the stream to read the file
		    using (var fis = File.OpenRead(localPath))
		    {

		        // and send it. We use a custom try/catch block to make the difference between
		        // file and network IO exceptions.
		        AdbHelper.write(mChannel, msg, -1, timeOut);

		        // create the buffer used to read.
		        // we read max SYNC_DATA_MAX, but we need 2 4 bytes at the beginning.
		        if (mBuffer == null)
		        {
		            mBuffer = new byte[SYNC_DATA_MAX + 8];
		        }
		        Array.Copy(ID_DATA, 0, mBuffer, 0, ID_DATA.Length);

		        // look while there is something to read
		        while (true)
		        {
		            // check if we're canceled
		            if (monitor.canceled == true)
		            {
		                throw new SyncException(SyncException.SyncError.CANCELED);
		            }

		            // read up to SYNC_DATA_MAX
		            int readCount = fis.Read(mBuffer, 8, SYNC_DATA_MAX);

		            if (readCount == -1)
		            {
		                // we reached the end of the file
		                break;
		            }

		            // now send the data to the device
		            // first write the amount read
		            ArrayHelper.swap32bitsToArray(readCount, mBuffer, 4);

		            // now write it
		            AdbHelper.write(mChannel, mBuffer, readCount + 8, timeOut);

		            // and advance the monitor
		            monitor.advance(readCount);
		        }
		    }

		    // create the DONE message
		    long time = Environment.TickCount/1000;
		    msg = createReq(ID_DONE, (int) time);

		    // and send it.
		    AdbHelper.write(mChannel, msg, -1, timeOut);

		    // read the result, in a byte array containing 2 ints
		    // (id, size)
		    var result = new byte[8];
		    AdbHelper.read(mChannel, result, -1, timeOut); // full length

		    if (checkResult(result, ID_OKAY) == false)
		    {
		        throw new SyncException(SyncException.SyncError.TRANSFER_PROTOCOL_ERROR, readErrorMessage(result, timeOut));
		    }
		}

	    /// <summary>
		/// Reads an error message from the opened <seealso cref="#mChannel"/>. </summary>
		/// <param name="result"> the current adb result. Must contain both FAIL and the length of the message. </param>
		/// <param name="timeOut">
		/// @return </param>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String readErrorMessage(byte[] result, final int timeOut) throws TimeoutException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
		private string readErrorMessage(byte[] result, int timeOut)
		{
			if (checkResult(result, ID_FAIL))
			{
				int len = ArrayHelper.swap32bitFromArray(result, 4);

				if (len > 0)
				{
					AdbHelper.read(mChannel, mBuffer, len, timeOut);
					
                    var message = mBuffer.getString(0, len, Encoding.Default.EncodingName);
					Log.e("ddms", "transfer error: " + message);

					return message;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns the mode of the remote file. </summary>
		/// <param name="path"> the remote file </param>
		/// <returns> an Integer containing the mode if all went well or null
		///      otherwise </returns>
		/// <exception cref="IOException"> </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Integer readMode(String path) throws TimeoutException, java.io.IOException
		private int? readMode(string path)
		{
			// create the stat request message.
			var msg = createFileReq(ID_STAT, path);

			AdbHelper.write(mChannel, msg, -1, DdmPreferences.timeOut); // full length

			// read the result, in a byte array containing 4 ints
			// (id, mode, size, time)
			var statResult = new byte[16];
			AdbHelper.read(mChannel, statResult, -1, DdmPreferences.timeOut); // full length

			// check we have the proper data back
			if (checkResult(statResult, ID_STAT) == false)
			{
				return null;
			}

			// we return the mode (2nd int in the array)
			return ArrayHelper.swap32bitFromArray(statResult, 4);
		}

		/// <summary>
		/// Create a command with a code and an int values </summary>
		/// <param name="command"> </param>
		/// <param name="value">
		/// @return </param>
		private static byte[] createReq(byte[] command, int value)
		{
			var array = new byte[8];

			Array.Copy(command, 0, array, 0, 4);
			ArrayHelper.swap32bitsToArray(value, array, 4);

			return array;
		}

		/// <summary>
		/// Creates the data array for a stat request. </summary>
		/// <param name="command"> the 4 byte command (ID_STAT, ID_RECV, ...) </param>
		/// <param name="path"> The path of the remote file on which to execute the command </param>
		/// <returns> the byte[] to send to the device through adb </returns>
		private static byte[] createFileReq(byte[] command, string path)
		{
			byte[] pathContent;
			try
			{
				pathContent = path.getBytes(AdbHelper.DEFAULT_ENCODING);
			}
			catch (ArgumentException)
			{
				return null;
			}

			return createFileReq(command, pathContent);
		}

		/// <summary>
		/// Creates the data array for a file request. This creates an array with a 4 byte command + the
		/// remote file name. </summary>
		/// <param name="command"> the 4 byte command (ID_STAT, ID_RECV, ...). </param>
		/// <param name="path"> The path, as a byte array, of the remote file on which to
		///      execute the command. </param>
		/// <returns> the byte[] to send to the device through adb </returns>
		private static byte[] createFileReq(byte[] command, byte[] path)
		{
			var array = new byte[8 + path.Length];

			Array.Copy(command, 0, array, 0, 4);
			ArrayHelper.swap32bitsToArray(path.Length, array, 4);
			Array.Copy(path, 0, array, 8, path.Length);

			return array;
		}

		private static byte[] createSendFileReq(byte[] command, byte[] path, int mode)
		{
			// make the mode into a string
	//JAVA TO C# CONVERTER TODO TASK: Octal literals cannot be represented in C#:
			string modeStr = "," + (mode & 0777); // $NON-NLS-1S
			byte[] modeContent;
			try
			{
				modeContent = modeStr.getBytes(AdbHelper.DEFAULT_ENCODING);
			}
			catch (ArgumentException)
			{
				return null;
			}

			byte[] array = new byte[8 + path.Length + modeContent.Length];

			Array.Copy(command, 0, array, 0, 4);
			ArrayHelper.swap32bitsToArray(path.Length + modeContent.Length, array, 4);
			Array.Copy(path, 0, array, 8, path.Length);
			Array.Copy(modeContent, 0, array, 8 + path.Length, modeContent.Length);

			return array;


		}

		/// <summary>
		/// Checks the result array starts with the provided code </summary>
		/// <param name="result"> The result array to check </param>
		/// <param name="code"> The 4 byte code. </param>
		/// <returns> true if the code matches. </returns>
		private static bool checkResult(byte[] result, byte[] code)
		{
			if (result[0] != code[0] || result[1] != code[1] || result[2] != code[2] || result[3] != code[3])
			{
				return false;
			}

			return true;

		}

		private static int getFileType(int mode)
		{
			if ((mode & S_ISOCK) == S_ISOCK)
			{
				return FileListingService.TYPE_SOCKET;
			}

			if ((mode & S_IFLNK) == S_IFLNK)
			{
				return FileListingService.TYPE_LINK;
			}

			if ((mode & S_IFREG) == S_IFREG)
			{
				return FileListingService.TYPE_FILE;
			}

			if ((mode & S_IFBLK) == S_IFBLK)
			{
				return FileListingService.TYPE_BLOCK;
			}

			if ((mode & S_IFDIR) == S_IFDIR)
			{
				return FileListingService.TYPE_DIRECTORY;
			}

			if ((mode & S_IFCHR) == S_IFCHR)
			{
				return FileListingService.TYPE_CHARACTER;
			}

			if ((mode & S_IFIFO) == S_IFIFO)
			{
				return FileListingService.TYPE_FIFO;
			}

			return FileListingService.TYPE_OTHER;
		}
	}

}