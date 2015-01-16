using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
	/// Stores native allocation information.
	/// <p/>Contains number of allocations, their size and the stack trace.
	/// <p/>Note: the ddmlib does not resolve the stack trace automatically. While this class provides
	/// storage for resolved stack trace, this is merely for convenience.
	/// </summary>
	public sealed class NativeAllocationInfo
	{
		/* Keywords used as delimiters in the string representation of a NativeAllocationInfo */
		public const string END_STACKTRACE_KW = "EndStacktrace";
		public const string BEGIN_STACKTRACE_KW = "BeginStacktrace:";
		public const string TOTAL_SIZE_KW = "TotalSize:";
		public const string SIZE_KW = "Size:";
		public const string ALLOCATIONS_KW = "Allocations:";

		/* constants for flag bits */
		private static readonly int FLAG_ZYGOTE_CHILD = (1 << 31);
		private static readonly int FLAG_MASK = (FLAG_ZYGOTE_CHILD);

	/// <summary>
		/// Libraries whose methods will be assumed to be not part of the user code. </summary>
		private static readonly IList<string> FILTERED_LIBRARIES = new List<string> { "libc.so", "libc_malloc_debug_leak.so"};

	/// <summary>
		/// Method names that should be assumed to be not part of the user code. </summary>
		private static readonly IList<Regex> FILTERED_METHOD_NAME_PATTERNS = new List<Regex> {
            new Regex("malloc", RegexOptions.IgnoreCase), 
            new Regex("calloc", RegexOptions.IgnoreCase), 
            new Regex("realloc", RegexOptions.IgnoreCase), 
            new Regex("operator new", RegexOptions.IgnoreCase), 
            new Regex("memalign", RegexOptions.IgnoreCase)
        };

		private readonly int mSize;

		private readonly bool mIsZygoteChild;

		private readonly int mAllocations;

		private readonly List<long> mStackCallAddresses = new List<long>();

		private List<NativeStackCallInfo> mResolvedStackCall = null;

		private bool mIsStackCallResolved = false;

		/// <summary>
		/// Constructs a new <seealso cref="NativeAllocationInfo"/>. </summary>
		/// <param name="size"> The size of the allocations. </param>
		/// <param name="allocations"> the allocation count </param>
		public NativeAllocationInfo(int size, int allocations)
		{
			this.mSize = size & ~FLAG_MASK;
			this.mIsZygoteChild = ((size & FLAG_ZYGOTE_CHILD) != 0);
			this.mAllocations = allocations;
		}

		/// <summary>
		/// Adds a stack call address for this allocation. </summary>
		/// <param name="address"> The address to add. </param>
		public void addStackCallAddress(long address)
		{
			mStackCallAddresses.Add(address);
		}

		/// <summary>
		/// Returns the total size of this allocation.
		/// </summary>
		public int size
		{
			get
			{
				return mSize;
			}
		}

		/// <summary>
		/// Returns whether the allocation happened in a child of the zygote
		/// process.
		/// </summary>
		public bool zygoteChild
		{
			get
			{
				return mIsZygoteChild;
			}
		}

		/// <summary>
		/// Returns the allocation count.
		/// </summary>
		public int allocationCount
		{
			get
			{
				return mAllocations;
			}
		}

		/// <summary>
		/// Returns whether the stack call addresses have been resolved into
		/// <seealso cref="NativeStackCallInfo"/> objects.
		/// </summary>
		public bool stackCallResolved
		{
			get
			{
				return mIsStackCallResolved;
			}
		}

		/// <summary>
		/// Returns the stack call of this allocation as raw addresses. </summary>
		/// <returns> the list of addresses where the allocation happened. </returns>
		public IList<long> stackCallAddresses
		{
			get
			{
				return mStackCallAddresses;
			}
		}

		/// <summary>
		/// Sets the resolved stack call for this allocation.
		/// <p/>
		/// If <code>resolvedStackCall</code> is non <code>null</code> then
		/// <seealso cref="#isStackCallResolved()"/> will return <code>true</code> after this call. </summary>
		/// <param name="resolvedStackCall"> The list of <seealso cref="NativeStackCallInfo"/>. </param>
		//[MethodImpl(MethodImplOptions.Synchronized)]
		public IList<NativeStackCallInfo> resolvedStackCall
		{
			set
			{
				if (mResolvedStackCall == null)
				{
					mResolvedStackCall = new List<NativeStackCallInfo>();
				}
				else
				{
					mResolvedStackCall.Clear();
				}
				mResolvedStackCall.AddRange(value);
				mIsStackCallResolved = mResolvedStackCall.Count != 0;
			}
			get
			{
				if (mIsStackCallResolved)
				{
					return mResolvedStackCall;
				}
    
				return null;
			}
		}

		/// <summary>
		/// Returns the resolved stack call. </summary>
		/// <returns> An array of <seealso cref="NativeStackCallInfo"/> or <code>null</code> if the stack call
		/// was not resolved. </returns>
		/// <seealso cref= #setResolvedStackCall(ArrayList) </seealso>
		/// <seealso cref= #isStackCallResolved() </seealso>
		//[MethodImpl(MethodImplOptions.Synchronized)]

		/// <summary>
		/// Indicates whether some other object is "equal to" this one. </summary>
		/// <param name="obj"> the reference object with which to compare. </param>
		/// <returns> <code>true</code> if this object is equal to the obj argument;
		/// <code>false</code> otherwise. </returns>
		/// <seealso cref= java.lang.Object#equals(java.lang.Object) </seealso>
		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			if (obj is NativeAllocationInfo)
			{
				NativeAllocationInfo mi = (NativeAllocationInfo)obj;
				// quick compare of size, alloc, and stackcall size
				if (mSize != mi.mSize || mAllocations != mi.mAllocations || mStackCallAddresses.Count != mi.mStackCallAddresses.Count)
				{
					return false;
				}
				// compare the stack addresses
				int count = mStackCallAddresses.Count;
				for (int i = 0 ; i < count ; i++)
				{
					long a = mStackCallAddresses[i];
					long b = mi.mStackCallAddresses[i];
					if (a != b)
					{
						return false;
					}
				}

				return true;
			}
			return false;
		}


		public override int GetHashCode()
		{
			// Follow Effective Java's recipe re hash codes.
			// Includes all the fields looked at by equals().

			int result = 17; // arbitrary starting point

			result = 31 * result + mSize;
			result = 31 * result + mAllocations;
			result = 31 * result + mStackCallAddresses.Count;

			foreach (long addr in mStackCallAddresses)
			{
				result = 31 * result + (int)(addr ^ ((long)((ulong)addr >> 32)));
			}

			return result;
		}

		/// <summary>
		/// Returns a string representation of the object. </summary>
		/// <seealso cref= java.lang.Object#toString() </seealso>
		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();
			buffer.Append(ALLOCATIONS_KW);
			buffer.Append(' ');
			buffer.Append(mAllocations);
			buffer.Append('\n');

			buffer.Append(SIZE_KW);
			buffer.Append(' ');
			buffer.Append(mSize);
			buffer.Append('\n');

			buffer.Append(TOTAL_SIZE_KW);
			buffer.Append(' ');
			buffer.Append(mSize * mAllocations);
			buffer.Append('\n');

			if (mResolvedStackCall != null)
			{
				buffer.Append(BEGIN_STACKTRACE_KW);
				buffer.Append('\n');
				foreach (NativeStackCallInfo source in mResolvedStackCall)
				{
					long addr = source.address;
					if (addr == 0)
					{
						continue;
					}

					if (source.lineNumber != -1)
					{
						buffer.Append(string.Format("\t{0:x8}\t{1} --- {2} --- {3}:{4:D}\n", addr, source.libraryName, source.methodName, source.sourceFile, source.lineNumber));
					}
					else
					{
						buffer.Append(string.Format("\t{0:x8}\t{1} --- {2} --- {3}\n", addr, source.libraryName, source.methodName, source.sourceFile));
					}
				}
				buffer.Append(END_STACKTRACE_KW);
				buffer.Append('\n');
			}

			return buffer.ToString();
		}

		/// <summary>
		/// Returns the first <seealso cref="NativeStackCallInfo"/> that is relevant.
		/// <p/>
		/// A relevant <code>NativeStackCallInfo</code> is a stack call that is not deep in the
		/// lower level of the libc, but the actual method that performed the allocation. </summary>
		/// <returns> a <code>NativeStackCallInfo</code> or <code>null</code> if the stack call has not
		/// been processed from the raw addresses. </returns>
		/// <seealso cref= #setResolvedStackCall(ArrayList) </seealso>
		/// <seealso cref= #isStackCallResolved() </seealso>
		//[MethodImpl(MethodImplOptions.Synchronized)]
		public NativeStackCallInfo relevantStackCallInfo
		{
			get
			{
				if (mIsStackCallResolved && mResolvedStackCall != null)
				{
					foreach (NativeStackCallInfo info in mResolvedStackCall)
					{
						if (isRelevantLibrary(info.libraryName) && isRelevantMethod(info.methodName))
						{
							return info;
						}
					}
    
					// couldnt find a relevant one, so we'll return the first one if it exists.
					if (mResolvedStackCall.Count > 0)
					{
						return mResolvedStackCall[0];
					}
				}
    
				return null;
			}
		}

		private bool isRelevantLibrary(string libPath)
		{
			foreach (string l in FILTERED_LIBRARIES)
			{
				if (libPath.EndsWith(l))
				{
					return false;
				}
			}

			return true;
		}

		private bool isRelevantMethod(string methodName)
		{
			foreach (var p in FILTERED_METHOD_NAME_PATTERNS)
			{
				var m = p.Match(methodName);
				if (m.Success)
				{
					return false;
				}
			}

			return true;
		}
	}

}