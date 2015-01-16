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

using System.Collections.Generic;
using System.Globalization;
using Dot42.DdmLib.support;

namespace Dot42.DdmLib
{


	/// <summary>
	/// Holds an Allocation information.
	/// </summary>
	public class AllocationInfo : IStackTraceInfo
	{
		private readonly string mAllocatedClass;
		private readonly int mAllocNumber;
		private readonly int mAllocationSize;
		private readonly short mThreadId;
		private readonly StackTraceElement[] mStackTrace;

		public enum SortMode
		{
			NUMBER,
			SIZE,
			CLASS,
			THREAD,
			IN_CLASS,
			IN_METHOD
		}

		public sealed class AllocationSorter : IComparer<AllocationInfo>
		{

			private SortMode mSortMode = SortMode.SIZE;
			private bool mDescending = true;

			public AllocationSorter()
			{
			}

			public SortMode sortMode
			{
				set
				{
					if (mSortMode == value)
					{
						mDescending = !mDescending;
					}
					else
					{
						mSortMode = value;
					}
				}
				get
				{
					return mSortMode;
				}
			}


			public bool descending
			{
				get
				{
					return mDescending;
				}
			}

			public int Compare(AllocationInfo o1, AllocationInfo o2)
			{
				int diff = 0;
				switch (mSortMode)
				{
					case AllocationInfo.SortMode.NUMBER:
						diff = o1.mAllocNumber - o2.mAllocNumber;
						break;
					case AllocationInfo.SortMode.SIZE:
						// pass, since diff is init with 0, we'll use SIZE compare below
						// as a back up anyway.
						break;
					case AllocationInfo.SortMode.CLASS:
						diff = o1.mAllocatedClass.CompareTo(o2.mAllocatedClass);
						break;
					case AllocationInfo.SortMode.THREAD:
						diff = o1.mThreadId - o2.mThreadId;
						break;
					case AllocationInfo.SortMode.IN_CLASS:
						string class1 = o1.firstTraceClassName;
						string class2 = o2.firstTraceClassName;
						diff = compareOptionalString(class1, class2);
						break;
					case AllocationInfo.SortMode.IN_METHOD:
						string method1 = o1.firstTraceMethodName;
						string method2 = o2.firstTraceMethodName;
						diff = compareOptionalString(method1, method2);
						break;
				}

				if (diff == 0)
				{
					// same? compare on size
					diff = o1.mAllocationSize - o2.mAllocationSize;
				}

				if (mDescending)
				{
					diff = -diff;
				}

				return diff;
			}

	/// <summary>
			/// compares two strings that could be null </summary>
			private int compareOptionalString(string str1, string str2)
			{
				if (str1 != null)
				{
					if (str2 == null)
					{
						return -1;
					}
					else
					{
						return str1.CompareTo(str2);
					}
				}
				else
				{
					if (str2 == null)
					{
						return 0;
					}
					else
					{
						return 1;
					}
				}
			}
		}

		/*
		 * Simple constructor.
		 */
		internal AllocationInfo(int allocNumber, string allocatedClass, int allocationSize, short threadId, StackTraceElement[] stackTrace)
		{
			mAllocNumber = allocNumber;
			mAllocatedClass = allocatedClass;
			mAllocationSize = allocationSize;
			mThreadId = threadId;
			mStackTrace = stackTrace;
		}

		/// <summary>
		/// Returns the allocation number. Allocations are numbered as they happen with the most
		/// recent one having the highest number
		/// </summary>
		public virtual int allocNumber
		{
			get
			{
				return mAllocNumber;
			}
		}

		/// <summary>
		/// Returns the name of the allocated class.
		/// </summary>
		public virtual string allocatedClass
		{
			get
			{
				return mAllocatedClass;
			}
		}

		/// <summary>
		/// Returns the size of the allocation.
		/// </summary>
		public virtual int size
		{
			get
			{
				return mAllocationSize;
			}
		}

		/// <summary>
		/// Returns the id of the thread that performed the allocation.
		/// </summary>
		public virtual short threadId
		{
			get
			{
				return mThreadId;
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
				return mStackTrace;
			}
		}

		public virtual int compareTo(AllocationInfo otherAlloc)
		{
			return otherAlloc.mAllocationSize - mAllocationSize;
		}

		public virtual string firstTraceClassName
		{
			get
			{
				if (mStackTrace.Length > 0)
				{
					return mStackTrace[0].className;
				}
    
				return null;
			}
		}

		public virtual string firstTraceMethodName
		{
			get
			{
				if (mStackTrace.Length > 0)
				{
					return mStackTrace[0].methodName;
				}
    
				return null;
			}
		}

		/// <summary>
		/// Returns true if the given filter matches case insensitively (according to
		/// the given locale) this allocation info.
		/// </summary>
		public virtual bool filter(string filter, bool fullTrace, CultureInfo locale)
		{
			if (mAllocatedClass.ToLower(locale).Contains(filter))
			{
				return true;
			}

			if (mStackTrace.Length > 0)
			{
				// check the top of the stack trace always
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int length = fullTrace ? mStackTrace.length : 1;
				int length = fullTrace ? mStackTrace.Length : 1;

				for (int i = 0 ; i < length ; i++)
				{
					if (mStackTrace[i].className.ToLower(locale).Contains(filter))
					{
						return true;
					}

					if (mStackTrace[i].methodName.ToLower(locale).Contains(filter))
					{
						return true;
					}
				}
			}

			return false;
		}
	}

}