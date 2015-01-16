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

using System;
using Dot42.DdmLib.support;

namespace Dot42.DdmLib
{

	/// <summary>
	/// Holds a thread information.
	/// </summary>
	public sealed class ThreadInfo : IStackTraceInfo
	{
		private int mThreadId;
		private string mThreadName;
		private int mStatus;
		private int mTid;
		private int mUtime;
		private int mStime;
		private bool mIsDaemon;
		private StackTraceElement[] mTrace;
		private long mTraceTime;

		// priority?
		// total CPU used?
		// method at top of stack?

		/// <summary>
		/// Construct with basic identification.
		/// </summary>
		internal ThreadInfo(int threadId, string threadName)
		{
			mThreadId = threadId;
			mThreadName = threadName;

			mStatus = -1;
			//mTid = mUtime = mStime = 0;
			//mIsDaemon = false;
		}

		/// <summary>
		/// Set with the values we get from a THST chunk.
		/// </summary>
		internal void updateThread(int status, int tid, int utime, int stime, bool isDaemon)
		{

			mStatus = status;
			mTid = tid;
			mUtime = utime;
			mStime = stime;
			mIsDaemon = isDaemon;
		}

		/// <summary>
		/// Sets the stack call of the thread. </summary>
		/// <param name="trace"> stackcall information. </param>
		internal StackTraceElement[] stackCall
		{
			set
			{
				mTrace = value;
				mTraceTime = Environment.TickCount;
			}
		}

		/// <summary>
		/// Returns the thread's ID.
		/// </summary>
		public int threadId
		{
			get
			{
				return mThreadId;
			}
		}

		/// <summary>
		/// Returns the thread's name.
		/// </summary>
		public string threadName
		{
			get
			{
				return mThreadName;
			}
			set
			{
				mThreadName = value;
			}
		}


		/// <summary>
		/// Returns the system tid.
		/// </summary>
		public int tid
		{
			get
			{
				return mTid;
			}
		}

		/// <summary>
		/// Returns the VM thread status.
		/// </summary>
		public int status
		{
			get
			{
				return mStatus;
			}
		}

		/// <summary>
		/// Returns the cumulative user time.
		/// </summary>
		public int utime
		{
			get
			{
				return mUtime;
			}
		}

		/// <summary>
		/// Returns the cumulative system time.
		/// </summary>
		public int stime
		{
			get
			{
				return mStime;
			}
		}

		/// <summary>
		/// Returns whether this is a daemon thread.
		/// </summary>
		public bool daemon
		{
			get
			{
				return mIsDaemon;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IStackTraceInfo#getStackTrace()
		 */
		public  StackTraceElement[] stackTrace
		{
			get
			{
				return mTrace;
			}
		}

		/// <summary>
		/// Returns the approximate time of the stacktrace data. </summary>
		/// <seealso cref= #getStackTrace() </seealso>
		public long stackCallTime
		{
			get
			{
				return mTraceTime;
			}
		}
	}


}