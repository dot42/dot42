using System;
using System.Linq;
using System.Runtime.CompilerServices;
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
	/// Contains the data of a <seealso cref="Client"/>.
	/// </summary>
	public class ClientData
	{
		/* This is a place to stash data associated with a Client, such as thread
		* states or heap data.  ClientData maps 1:1 to Client, but it's a little
		* cleaner if we separate the data out.
		*
		* Message handlers are welcome to stash arbitrary data here.
		*
		* IMPORTANT: The data here is written by HandleFoo methods and read by
		* FooPanel methods, which run in different threads.  All non-trivial
		* access should be synchronized against the ClientData object.
		*/


	/// <summary>
		/// Temporary name of VM to be ignored. </summary>
		private const string PRE_INITIALIZED = "<pre-initialized>"; //$NON-NLS-1$

		public enum DebuggerStatus
		{
			/// <summary>
			/// Debugger connection status: not waiting on one, not connected to one, but accepting
			/// </summary>
			DEFAULT,
			/// <summary>
			/// Debugger connection status: the application's VM is paused, waiting for a debugger to
			/// </summary>
			WAITING,
			/// <summary>
			/// Debugger connection status : Debugger is connected </summary>
			ATTACHED,
			/// <summary>
			/// Debugger connection status: The listening port for debugger connection failed to listen.
			/// </summary>
			ERROR
		}

		public enum AllocationTrackingStatus
		{
			/// <summary>
			/// Allocation tracking status: unknown.
			/// <p/>This happens right after a <seealso cref="Client"/> is discovered
			/// by the <seealso cref="AndroidDebugBridge"/>, and before the <seealso cref="Client"/> answered the query
			/// regarding its allocation tracking status. </summary>
			/// <seealso cref= Client#requestAllocationStatus() </seealso>
			UNKNOWN,
			/// <summary>
			/// Allocation tracking status: the <seealso cref="Client"/> is not tracking allocations. </summary>
			OFF,
			/// <summary>
			/// Allocation tracking status: the <seealso cref="Client"/> is tracking allocations. </summary>
			ON
		}

		public enum MethodProfilingStatus
		{
			/// <summary>
			/// Method profiling status: unknown.
			/// <p/>This happens right after a <seealso cref="Client"/> is discovered
			/// by the <seealso cref="AndroidDebugBridge"/>, and before the <seealso cref="Client"/> answered the query
			/// regarding its method profiling status. </summary>
			/// <seealso cref= Client#requestMethodProfilingStatus() </seealso>
			UNKNOWN,
			/// <summary>
			/// Method profiling status: the <seealso cref="Client"/> is not profiling method calls. </summary>
			OFF,
			/// <summary>
			/// Method profiling status: the <seealso cref="Client"/> is profiling method calls. </summary>
			ON
		}

		/// <summary>
		/// Name of the value representing the max size of the heap, in the <seealso cref="Map"/> returned by
		/// <seealso cref="#getVmHeapInfo(int)"/>
		/// </summary>
		public const string HEAP_MAX_SIZE_BYTES = "maxSizeInBytes"; //$NON-NLS-1$
		/// <summary>
		/// Name of the value representing the size of the heap, in the <seealso cref="Map"/> returned by
		/// <seealso cref="#getVmHeapInfo(int)"/>
		/// </summary>
		public const string HEAP_SIZE_BYTES = "sizeInBytes"; //$NON-NLS-1$
		/// <summary>
		/// Name of the value representing the number of allocated bytes of the heap, in the
		/// <seealso cref="Map"/> returned by <seealso cref="#getVmHeapInfo(int)"/>
		/// </summary>
		public const string HEAP_BYTES_ALLOCATED = "bytesAllocated"; //$NON-NLS-1$
		/// <summary>
		/// Name of the value representing the number of objects in the heap, in the <seealso cref="Map"/>
		/// returned by <seealso cref="#getVmHeapInfo(int)"/>
		/// </summary>
		public const string HEAP_OBJECTS_ALLOCATED = "objectsAllocated"; //$NON-NLS-1$

		/// <summary>
		/// String for feature enabling starting/stopping method profiling </summary>
		/// <seealso cref= #hasFeature(String) </seealso>
		public const string FEATURE_PROFILING = "method-trace-profiling"; //$NON-NLS-1$

		/// <summary>
		/// String for feature enabling direct streaming of method profiling data </summary>
		/// <seealso cref= #hasFeature(String) </seealso>
		public const string FEATURE_PROFILING_STREAMING = "method-trace-profiling-streaming"; //$NON-NLS-1$

		/// <summary>
		/// String for feature allowing to dump hprof files </summary>
		/// <seealso cref= #hasFeature(String) </seealso>
		public const string FEATURE_HPROF = "hprof-heap-dump"; //$NON-NLS-1$

		/// <summary>
		/// String for feature allowing direct streaming of hprof dumps </summary>
		/// <seealso cref= #hasFeature(String) </seealso>
		public const string FEATURE_HPROF_STREAMING = "hprof-heap-dump-streaming"; //$NON-NLS-1$

		private static IHprofDumpHandler sHprofDumpHandler;
		private static IMethodProfilingHandler sMethodProfilingHandler;

		// is this a DDM-aware client?
		private bool mIsDdmAware;

		// the client's process ID
		private readonly int mPid;

		// Java VM identification string
		private string mVmIdentifier;

		// client's self-description
		private string mClientDescription;

		// how interested are we in a debugger?
		private DebuggerStatus mDebuggerInterest;

		// List of supported features by the client.
		private readonly HashSet<string> mFeatures = new HashSet<string>();

		// Thread tracking (THCR, THDE).
		private SortedDictionary<int?, ThreadInfo> mThreadMap;

		/// <summary>
		/// VM Heap data </summary>
		private readonly HeapData mHeapData = new HeapData();
		/// <summary>
		/// Native Heap data </summary>
		private readonly HeapData mNativeHeapData = new HeapData();

		private Dictionary<int?, Dictionary<string, long?>> mHeapInfoMap = new Dictionary<int?, Dictionary<string, long?>>();


		/// <summary>
		/// library map info. Stored here since the backtrace data
		/// is computed on a need to display basis.
		/// </summary>
		private List<NativeLibraryMapInfo> mNativeLibMapInfo = new List<NativeLibraryMapInfo>();

		/// <summary>
		/// Native Alloc info list </summary>
		private List<NativeAllocationInfo> mNativeAllocationList = new List<NativeAllocationInfo>();
		private int mNativeTotalMemory;

		private AllocationInfo[] mAllocations;
		private AllocationTrackingStatus mAllocationStatus = AllocationTrackingStatus.UNKNOWN;

		private string mPendingHprofDump;

		private MethodProfilingStatus mProfilingStatus = MethodProfilingStatus.UNKNOWN;
		private string mPendingMethodProfiling;

		/// <summary>
		/// Heap Information.
		/// <p/>The heap is composed of several <seealso cref="HeapSegment"/> objects.
		/// <p/>A call to <seealso cref="#isHeapDataComplete()"/> will indicate if the segments (available through
		/// <seealso cref="#getHeapSegments()"/>) represent the full heap.
		/// </summary>
		public class HeapData
		{
			private HashSet<HeapSegment> mHeapSegments = new HashSet<HeapSegment>();
			private bool mHeapDataComplete = false;
			private sbyte[] mProcessedHeapData;
			private IDictionary<int?, List<HeapSegment.HeapSegmentElement>> mProcessedHeapMap;

			/// <summary>
			/// Abandon the current list of heap segments.
			/// </summary>
			[MethodImpl(MethodImplOptions.Synchronized)]
			public virtual void clearHeapData()
			{
				/* Abandon the old segments instead of just calling .clear().
				 * This lets the user hold onto the old set if it wants to.
				 */
				mHeapSegments = new HashSet<HeapSegment>();
				mHeapDataComplete = false;
			}

			/// <summary>
			/// Add raw HPSG chunk data to the list of heap segments.
			/// </summary>
			/// <param name="data"> The raw data from an HPSG chunk. </param>
			[MethodImpl(MethodImplOptions.Synchronized)]
			internal virtual void addHeapData(ByteBuffer data)
			{
				HeapSegment hs;

				if (mHeapDataComplete)
				{
					clearHeapData();
				}

				try
				{
					hs = new HeapSegment(data);
				}
				catch (BufferUnderflowException)
				{
					Console.Error.WriteLine("Discarding short HPSG data (length " + data.limit + ")");
					return;
				}

				mHeapSegments.Add(hs);
			}

			/// <summary>
			/// Called when all heap data has arrived.
			/// </summary>
			[MethodImpl(MethodImplOptions.Synchronized)]
			internal virtual void sealHeapData()
			{
				mHeapDataComplete = true;
			}

			/// <summary>
			/// Returns whether the heap data has been sealed.
			/// </summary>
			public virtual bool heapDataComplete
			{
				get
				{
					return mHeapDataComplete;
				}
			}

			/// <summary>
			/// Get the collected heap data, if sealed.
			/// </summary>
			/// <returns> The list of heap segments if the heap data has been sealed, or null if it hasn't. </returns>
			public virtual ICollection<HeapSegment> heapSegments
			{
				get
				{
					if (heapDataComplete)
					{
						return mHeapSegments;
					}
					return null;
				}
			}

			/// <summary>
			/// Sets the processed heap data.
			/// </summary>
			/// <param name="heapData"> The new heap data (can be null) </param>
			public virtual sbyte[] processedHeapData
			{
				set
				{
					mProcessedHeapData = value;
				}
				get
				{
					return mProcessedHeapData;
				}
			}


			public virtual void setProcessedHeapMap(IDictionary<int?, List<HeapSegment.HeapSegmentElement>> heapMap)
			{
				mProcessedHeapMap = heapMap;
			}

			public virtual IDictionary<int?, List<HeapSegment.HeapSegmentElement>> processedHeapMap
			{
				get
				{
					return mProcessedHeapMap;
				}
			}
		}

		/// <summary>
		/// Handlers able to act on HPROF dumps.
		/// </summary>
		public interface IHprofDumpHandler
		{
			/// <summary>
			/// Called when a HPROF dump succeeded. </summary>
			/// <param name="remoteFilePath"> the device-side path of the HPROF file. </param>
			/// <param name="client"> the client for which the HPROF file was. </param>
			void onSuccess(string remoteFilePath, Client client);

			/// <summary>
			/// Called when a HPROF dump was successful. </summary>
			/// <param name="data"> the data containing the HPROF file, streamed from the VM </param>
			/// <param name="client"> the client that was profiled. </param>
			void onSuccess(byte[] data, Client client);

			/// <summary>
			/// Called when a hprof dump failed to end on the VM side </summary>
			/// <param name="client"> the client that was profiled. </param>
			/// <param name="message"> an optional (<code>null<code> ok) error message to be displayed. </param>
			void onEndFailure(Client client, string message);
		}

		/// <summary>
		/// Handlers able to act on Method profiling info
		/// </summary>
		public interface IMethodProfilingHandler
		{
			/// <summary>
			/// Called when a method tracing was successful. </summary>
			/// <param name="remoteFilePath"> the device-side path of the trace file. </param>
			/// <param name="client"> the client that was profiled. </param>
			void onSuccess(string remoteFilePath, Client client);

			/// <summary>
			/// Called when a method tracing was successful. </summary>
			/// <param name="data"> the data containing the trace file, streamed from the VM </param>
			/// <param name="client"> the client that was profiled. </param>
			void onSuccess(byte[] data, Client client);

			/// <summary>
			/// Called when method tracing failed to start </summary>
			/// <param name="client"> the client that was profiled. </param>
			/// <param name="message"> an optional (<code>null<code> ok) error message to be displayed. </param>
			void onStartFailure(Client client, string message);

			/// <summary>
			/// Called when method tracing failed to end on the VM side </summary>
			/// <param name="client"> the client that was profiled. </param>
			/// <param name="message"> an optional (<code>null<code> ok) error message to be displayed. </param>
			void onEndFailure(Client client, string message);
		}

		/// <summary>
		/// Sets the handler to receive notifications when an HPROF dump succeeded or failed.
		/// </summary>
		public static IHprofDumpHandler hprofDumpHandler
		{
			set
			{
				sHprofDumpHandler = value;
			}
			get
			{
				return sHprofDumpHandler;
			}
		}


		/// <summary>
		/// Sets the handler to receive notifications when an HPROF dump succeeded or failed.
		/// </summary>
		public static IMethodProfilingHandler methodProfilingHandler
		{
			set
			{
				sMethodProfilingHandler = value;
			}
			get
			{
				return sMethodProfilingHandler;
			}
		}


		/// <summary>
		/// Generic constructor.
		/// </summary>
		internal ClientData(int pid)
		{
			mPid = pid;

			mDebuggerInterest = DebuggerStatus.DEFAULT;
			mThreadMap = new SortedDictionary<int?, ThreadInfo>();
		}

		/// <summary>
		/// Returns whether the process is DDM-aware.
		/// </summary>
		public virtual bool ddmAware
		{
			get
			{
				return mIsDdmAware;
			}
		}

		/// <summary>
		/// Sets DDM-aware status.
		/// </summary>
		internal virtual void isDdmAware(bool aware)
		{
			mIsDdmAware = aware;
		}

		/// <summary>
		/// Returns the process ID.
		/// </summary>
		public virtual int pid
		{
			get
			{
				return mPid;
			}
		}

		/// <summary>
		/// Returns the Client's VM identifier.
		/// </summary>
		public virtual string vmIdentifier
		{
			get
			{
				return mVmIdentifier;
			}
			set
			{
				mVmIdentifier = value;
			}
		}


		/// <summary>
		/// Returns the client description.
		/// <p/>This is generally the name of the package defined in the
		/// <code>AndroidManifest.xml</code>.
		/// </summary>
		/// <returns> the client description or <code>null</code> if not the description was not yet
		/// sent by the client. </returns>
		public virtual string clientDescription
		{
			get
			{
				return mClientDescription;
			}
			set
			{
				if (mClientDescription == null && value.Length > 0)
				{
					/*
					 * The application VM is first named <pre-initialized> before being assigned
					 * its real name.
					 * Depending on the timing, we can get an APNM chunk setting this name before
					 * another one setting the final actual name. So if we get a SetClientDescription
					 * with this value we ignore it.
					 */
					if (PRE_INITIALIZED.Equals(value) == false)
					{
						mClientDescription = value;
					}
				}
			}
		}


		/// <summary>
		/// Returns the debugger connection status.
		/// </summary>
		public virtual DebuggerStatus debuggerConnectionStatus
		{
			get
			{
				return mDebuggerInterest;
			}
			set
			{
				mDebuggerInterest = value;
			}
		}


		/// <summary>
		/// Sets the current heap info values for the specified heap.
		/// </summary>
		/// <param name="heapId"> The heap whose info to update </param>
		/// <param name="sizeInBytes"> The size of the heap, in bytes </param>
		/// <param name="bytesAllocated"> The number of bytes currently allocated in the heap </param>
		/// <param name="objectsAllocated"> The number of objects currently allocated in
		///                         the heap </param>
		// TODO: keep track of timestamp, reason
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual void setHeapInfo(int heapId, long maxSizeInBytes, long sizeInBytes, long bytesAllocated, long objectsAllocated)
		{
			Dictionary<string, long?> heapInfo = new Dictionary<string, long?>();
			heapInfo.Add(HEAP_MAX_SIZE_BYTES, maxSizeInBytes);
			heapInfo.Add(HEAP_SIZE_BYTES, sizeInBytes);
			heapInfo.Add(HEAP_BYTES_ALLOCATED, bytesAllocated);
			heapInfo.Add(HEAP_OBJECTS_ALLOCATED, objectsAllocated);
			mHeapInfoMap.Add(heapId, heapInfo);
		}

		/// <summary>
		/// Returns the <seealso cref="HeapData"/> object for the VM.
		/// </summary>
		public virtual HeapData vmHeapData
		{
			get
			{
				return mHeapData;
			}
		}

		/// <summary>
		/// Returns the <seealso cref="HeapData"/> object for the native code.
		/// </summary>
		internal virtual HeapData nativeHeapData
		{
			get
			{
				return mNativeHeapData;
			}
		}

		/// <summary>
		/// Returns an iterator over the list of known VM heap ids.
		/// <p/>
		/// The caller must synchronize on the <seealso cref="ClientData"/> object while iterating.
		/// </summary>
		/// <returns> an iterator over the list of heap ids </returns>
		//[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual IEnumerator<int?> vmHeapIds
		{
			get
			{
				return mHeapInfoMap.Keys.GetEnumerator();
			}
		}

		/// <summary>
		/// Returns the most-recent info values for the specified VM heap.
		/// </summary>
		/// <param name="heapId"> The heap whose info should be returned </param>
		/// <returns> a map containing the info values for the specified heap.
		///         Returns <code>null</code> if the heap ID is unknown. </returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual IDictionary<string, long?> getVmHeapInfo(int heapId)
		{
			return mHeapInfoMap[heapId];
		}

		/// <summary>
		/// Adds a new thread to the list.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual void addThread(int threadId, string threadName)
		{
			ThreadInfo attr = new ThreadInfo(threadId, threadName);
			mThreadMap.Add(threadId, attr);
		}

		/// <summary>
		/// Removes a thread from the list.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual void removeThread(int threadId)
		{
			mThreadMap.Remove(threadId);
		}

		/// <summary>
		/// Returns the list of threads as <seealso cref="ThreadInfo"/> objects.
		/// <p/>The list is empty until a thread update was requested with
		/// <seealso cref="Client#requestThreadUpdate()"/>.
		/// </summary>
		//[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual ThreadInfo[] threads
		{
			get
			{
			    return mThreadMap.Values.ToArray();
			}
		}

		/// <summary>
		/// Returns the <seealso cref="ThreadInfo"/> by thread id.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual ThreadInfo getThread(int threadId)
		{
			return mThreadMap[threadId];
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual void clearThreads()
		{
			mThreadMap.Clear();
		}

		/// <summary>
		/// Returns the list of <seealso cref="NativeAllocationInfo"/>. </summary>
		/// <seealso cref= Client#requestNativeHeapInformation() </seealso>
		//[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual IList<NativeAllocationInfo> nativeAllocationList
		{
			get
			{
				return mNativeAllocationList.AsReadOnly();
			}
		}

		/// <summary>
		/// adds a new <seealso cref="NativeAllocationInfo"/> to the <seealso cref="Client"/> </summary>
		/// <param name="allocInfo"> The <seealso cref="NativeAllocationInfo"/> to add. </param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual void addNativeAllocation(NativeAllocationInfo allocInfo)
		{
			mNativeAllocationList.Add(allocInfo);
		}

		/// <summary>
		/// Clear the current malloc info.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual void clearNativeAllocationInfo()
		{
			mNativeAllocationList.Clear();
		}

		/// <summary>
		/// Returns the total native memory. </summary>
		/// <seealso cref= Client#requestNativeHeapInformation() </seealso>
		//[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual int totalNativeMemory
		{
			get
			{
				return mNativeTotalMemory;
			}
			set
			{
				mNativeTotalMemory = value;
			}
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]

		//[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual void addNativeLibraryMapInfo(long startAddr, long endAddr, string library)
		{
			mNativeLibMapInfo.Add(new NativeLibraryMapInfo(startAddr, endAddr, library));
		}

		/// <summary>
		/// Returns the list of native libraries mapped in memory for this client.
		/// </summary>
		//[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual IList<NativeLibraryMapInfo> mappedNativeLibraries
		{
			get
			{
				return mNativeLibMapInfo.AsReadOnly();
			}
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual AllocationTrackingStatus allocationStatus
		{
			set
			{
				mAllocationStatus = value;
			}
			get
			{
				return mAllocationStatus;
			}
		}

		/// <summary>
		/// Returns the allocation tracking status. </summary>
		/// <seealso cref= Client#requestAllocationStatus() </seealso>
		//[MethodImpl(MethodImplOptions.Synchronized)]

		//[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual AllocationInfo[] allocations
		{
			set
			{
				mAllocations = value;
			}
			get
			{
				return mAllocations;
			}
		}

		/// <summary>
		/// Returns the list of tracked allocations. </summary>
		/// <seealso cref= Client#requestAllocationDetails() </seealso>
		[MethodImpl(MethodImplOptions.Synchronized)]

		internal virtual void addFeature(string feature)
		{
			mFeatures.Add(feature);
		}

		/// <summary>
		/// Returns true if the <seealso cref="Client"/> supports the given <var>feature</var> </summary>
		/// <param name="feature"> The feature to test. </param>
		/// <returns> true if the feature is supported
		/// </returns>
		/// <seealso cref= ClientData#FEATURE_PROFILING </seealso>
		/// <seealso cref= ClientData#FEATURE_HPROF </seealso>
		public virtual bool hasFeature(string feature)
		{
			return mFeatures.Contains(feature);
		}

		/// <summary>
		/// Sets the device-side path to the hprof file being written </summary>
		/// <param name="pendingHprofDump"> the file to the hprof file </param>
		internal virtual string pendingHprofDump
		{
			set
			{
				mPendingHprofDump = value;
			}
			get
			{
				return mPendingHprofDump;
			}
		}


		public virtual bool hasPendingHprofDump()
		{
			return mPendingHprofDump != null;
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		internal virtual MethodProfilingStatus methodProfilingStatus
		{
			set
			{
				mProfilingStatus = value;
			}
			get
			{
				return mProfilingStatus;
			}
		}

		/// <summary>
		/// Returns the method profiling status. </summary>
		/// <seealso cref= Client#requestMethodProfilingStatus() </seealso>
		//[MethodImpl(MethodImplOptions.Synchronized)]

		/// <summary>
		/// Sets the device-side path to the method profile file being written </summary>
		/// <param name="pendingMethodProfiling"> the file being written </param>
		internal virtual string pendingMethodProfiling
		{
			set
			{
				mPendingMethodProfiling = value;
			}
			get
			{
				return mPendingMethodProfiling;
			}
		}

	}


}