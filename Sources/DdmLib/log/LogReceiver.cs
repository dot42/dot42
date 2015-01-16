using System;
using Dot42.DdmLib.utils;

/*
 * Copyright (C) 2008 The Android Open Source Project
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

namespace Dot42.DdmLib.log
{
    /// <summary>
	/// Receiver able to provide low level parsing for device-side log services.
	/// </summary>
	public sealed class LogReceiver
	{

		private const int ENTRY_HEADER_SIZE = 20; // 2*2 + 4*4; see LogEntry.

		/// <summary>
		/// Represents a log entry and its raw data.
		/// </summary>
		public sealed class LogEntry
		{
			/*
			 * See //device/include/utils/logger.h
			 */
		/// <summary>
			/// 16bit unsigned: length of the payload. </summary>
			public int len; // This is normally followed by a 16 bit padding
		/// <summary>
			/// pid of the process that generated this <seealso cref="LogEntry"/> </summary>
			public int pid;
		/// <summary>
			/// tid of the process that generated this <seealso cref="LogEntry"/> </summary>
			public int tid;
		/// <summary>
			/// Seconds since epoch. </summary>
			public int sec;
		/// <summary>
			/// nanoseconds. </summary>
			public int nsec;
		/// <summary>
			/// The entry's raw data. </summary>
			public byte[] data;
		}

		/// <summary>
		/// Classes which implement this interface provide a method that deals
		/// with <seealso cref="LogEntry"/> objects coming from log service through a <seealso cref="LogReceiver"/>.
		/// <p/>This interface provides two methods.
		/// <ul>
		/// <li><seealso cref="#newEntry(com.android.ddmlib.log.LogReceiver.LogEntry)"/> provides a
		/// first level of parsing, extracting <seealso cref="LogEntry"/> objects out of the log service output.</li>
		/// <li><seealso cref="#newData(byte[], int, int)"/> provides a way to receive the raw information
		/// coming directly from the log service.</li>
		/// </ul>
		/// </summary>
		public interface ILogListener
		{
			/// <summary>
			/// Sent when a new <seealso cref="LogEntry"/> has been parsed by the <seealso cref="LogReceiver"/>. </summary>
			/// <param name="entry"> the new log entry. </param>
			void newEntry(LogEntry entry);

			/// <summary>
			/// Sent when new raw data is coming from the log service. </summary>
			/// <param name="data"> the raw data buffer. </param>
			/// <param name="offset"> the offset into the buffer signaling the beginning of the new data. </param>
			/// <param name="length"> the length of the new data. </param>
			void newData(byte[] data, int offset, int length);
		}

			/// <summary>
		/// Current <seealso cref="LogEntry"/> being read, before sending it to the listener. </summary>
		private LogEntry mCurrentEntry;

			/// <summary>
		/// Temp buffer to store partial entry headers. </summary>
		private readonly byte[] mEntryHeaderBuffer = new byte[ENTRY_HEADER_SIZE];
			/// <summary>
		/// Offset in the partial header buffer </summary>
		private int mEntryHeaderOffset = 0;
			/// <summary>
		/// Offset in the partial entry data </summary>
		private int mEntryDataOffset = 0;

			/// <summary>
		/// Listener waiting for receive fully read <seealso cref="LogEntry"/> objects </summary>
		private ILogListener mListener;

		private bool mIsCancelled = false;

		/// <summary>
		/// Creates a <seealso cref="LogReceiver"/> with an <seealso cref="ILogListener"/>.
		/// <p/>
		/// The <seealso cref="ILogListener"/> will receive new log entries as they are parsed, in the form 
		/// of <seealso cref="LogEntry"/> objects. </summary>
		/// <param name="listener"> the listener to receive new log entries. </param>
		public LogReceiver(ILogListener listener)
		{
			mListener = listener;
		}


		/// <summary>
		/// Parses new data coming from the log service. </summary>
		/// <param name="data"> the data buffer </param>
		/// <param name="offset"> the offset into the buffer signaling the beginning of the new data. </param>
		/// <param name="length"> the length of the new data. </param>
		public void parseNewData(byte[] data, int offset, int length)
		{
			// notify the listener of new raw data
			if (mListener != null)
			{
				mListener.newData(data, offset, length);
			}

			// loop while there is still data to be read and the receiver has not be cancelled.
			while (length > 0 && mIsCancelled == false)
			{
				// first check if we have no current entry.
				if (mCurrentEntry == null)
				{
					if (mEntryHeaderOffset + length < ENTRY_HEADER_SIZE)
					{
						// if we don't have enough data to finish the header, save
						// the data we have and return
						Array.Copy(data, offset, mEntryHeaderBuffer, mEntryHeaderOffset, length);
						mEntryHeaderOffset += length;
						return;
					}
					else
					{
						// we have enough to fill the header, let's do it.
						// did we store some part at the beginning of the header?
						if (mEntryHeaderOffset != 0)
						{
							// copy the rest of the entry header into the header buffer
							int size = ENTRY_HEADER_SIZE - mEntryHeaderOffset;
							Array.Copy(data, offset, mEntryHeaderBuffer, mEntryHeaderOffset, size);

							// create the entry from the header buffer
							mCurrentEntry = createEntry(mEntryHeaderBuffer, 0);

							// since we used the whole entry header buffer, we reset  the offset
							mEntryHeaderOffset = 0;

							// adjust current offset and remaining length to the beginning
							// of the entry data
							offset += size;
							length -= size;
						}
						else
						{
							// create the entry directly from the data array
							mCurrentEntry = createEntry(data, offset);

							// adjust current offset and remaining length to the beginning
							// of the entry data
							offset += ENTRY_HEADER_SIZE;
							length -= ENTRY_HEADER_SIZE;
						}
					}
				}

				// at this point, we have an entry, and offset/length have been updated to skip
				// the entry header.

				// if we have enough data for this entry or more, we'll need to end this entry
				if (length >= mCurrentEntry.len - mEntryDataOffset)
				{
					// compute and save the size of the data that we have to read for this entry,
					// based on how much we may already have read.
					int dataSize = mCurrentEntry.len - mEntryDataOffset;

					// we only read what we need, and put it in the entry buffer.
					Array.Copy(data, offset, mCurrentEntry.data, mEntryDataOffset, dataSize);

					// notify the listener of a new entry
					if (mListener != null)
					{
						mListener.newEntry(mCurrentEntry);
					}

					// reset some flags: we have read 0 data of the current entry.
					// and we have no current entry being read.
					mEntryDataOffset = 0;
					mCurrentEntry = null;

					// and update the data buffer info to the end of the current entry / start
					// of the next one.
					offset += dataSize;
					length -= dataSize;
				}
				else
				{
					// we don't have enough data to fill this entry, so we store what we have
					// in the entry itself.
					Array.Copy(data, offset, mCurrentEntry.data, mEntryDataOffset, length);

					// save the amount read for the data.
					mEntryDataOffset += length;
					return;
				}
			}
		}

		/// <summary>
		/// Returns whether this receiver is canceling the remote service.
		/// </summary>
		public bool cancelled
		{
			get
			{
				return mIsCancelled;
			}
		}

		/// <summary>
		/// Cancels the current remote service.
		/// </summary>
		public void cancel()
		{
			mIsCancelled = true;
		}

		/// <summary>
		/// Creates a <seealso cref="LogEntry"/> from the array of bytes. This expects the data buffer size
		/// to be at least <code>offset + <seealso cref="#ENTRY_HEADER_SIZE"/></code>. </summary>
		/// <param name="data"> the data buffer the entry is read from. </param>
		/// <param name="offset"> the offset of the first byte from the buffer representing the entry. </param>
		/// <returns> a new <seealso cref="LogEntry"/> or <code>null</code> if some error happened. </returns>
		private LogEntry createEntry(byte[] data, int offset)
		{
			if (data.Length < offset + ENTRY_HEADER_SIZE)
			{
				throw new ArgumentException("Buffer not big enough to hold full LoggerEntry header");
			}

			// create the new entry and fill it.
			LogEntry entry = new LogEntry();
			entry.len = ArrayHelper.swapU16bitFromArray(data, offset);

			// we've read only 16 bits, but since there's also a 16 bit padding,
			// we can skip right over both.
			offset += 4;

			entry.pid = ArrayHelper.swap32bitFromArray(data, offset);
			offset += 4;
			entry.tid = ArrayHelper.swap32bitFromArray(data, offset);
			offset += 4;
			entry.sec = ArrayHelper.swap32bitFromArray(data, offset);
			offset += 4;
			entry.nsec = ArrayHelper.swap32bitFromArray(data, offset);
			offset += 4;

			// allocate the data
			entry.data = new byte[entry.len];

			return entry;
		}

	}

}