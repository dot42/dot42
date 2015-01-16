using System;
using System.Diagnostics;
using Dot42.DdmLib.support;

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
	/// Custom Event Container for the Gc event since this event doesn't simply output data in
	/// int or long format, but encodes several values on 4 longs.
	/// <p/>
	/// The array of <seealso cref="EventValueDescription"/>s parsed from the "event-log-tags" file must
	/// be ignored, and instead, the array returned from <seealso cref="#getValueDescriptions()"/> must be used. 
	/// </summary>
	internal sealed class GcEventContainer : EventContainer
	{

		public const int GC_EVENT_TAG = 20001;

		private string processId;
		private long gcTime;
		private long bytesFreed;
		private long objectsFreed;
		private long actualSize;
		private long allowedSize;
		private long softLimit;
		private long objectsAllocated;
		private long bytesAllocated;
		private long zActualSize;
		private long zAllowedSize;
		private long zObjectsAllocated;
		private long zBytesAllocated;
		private long dlmallocFootprint;
		private long mallinfoTotalAllocatedSpace;
		private long externalLimit;
		private long externalBytesAllocated;

		internal GcEventContainer(LogReceiver.LogEntry entry, int tag, object data) : base(entry, tag, data)
		{
			init(data);
		}

		internal GcEventContainer(int tag, int pid, int tid, int sec, int nsec, object data) : base(tag, pid, tid, sec, nsec, data)
		{
			init(data);
		}

		/// <param name="data"> </param>
		private void init(object data)
		{
			if (data is object[])
			{
				var values = (object[])data;
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i] is long?)
					{
						parseDvmHeapInfo((long)values[i], i);
					}
				}
			}
		}

		public override EventValueTypes type
		{
			get
			{
				return EventValueTypes.LIST;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean testValue(int index, Object value, CompareMethod compareMethod) throws InvalidTypeException
		public override bool testValue(int index, object value, CompareMethod compareMethod)
		{
			// do a quick easy check on the type.
			if (index == 0)
			{
				if ((value is string) == false)
				{
					throw new InvalidTypeException();
				}
			}
			else if ((value is long?) == false)
			{
				throw new InvalidTypeException();
			}

			switch (compareMethod.Method)
			{
				case EventContainer.CompareMethods.EQUAL_TO:
					if (index == 0)
					{
						return processId.Equals(value);
					}
					else
					{
						return getValueAsLong(index) == (long)((long?)value);
					}
				case EventContainer.CompareMethods.LESSER_THAN:
					return getValueAsLong(index) <= (long)((long?)value);
				case EventContainer.CompareMethods.LESSER_THAN_STRICT:
					return getValueAsLong(index) < (long)((long?)value);
				case EventContainer.CompareMethods.GREATER_THAN:
					return getValueAsLong(index) >= (long)((long?)value);
				case EventContainer.CompareMethods.GREATER_THAN_STRICT:
					return getValueAsLong(index) > (long) ((long?)value);
				case EventContainer.CompareMethods.BIT_CHECK:
					return (getValueAsLong(index) & (long)((long?)value)) != 0;
			}

			throw new System.IndexOutOfRangeException();
		}

		public override object getValue(int valueIndex)
		{
			if (valueIndex == 0)
			{
				return processId;
			}

			try
			{
				return new long?(getValueAsLong(valueIndex));
			}
			catch (InvalidTypeException)
			{
				// this would only happened if valueIndex was 0, which we test above.
			}

			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double getValueAsDouble(int valueIndex) throws InvalidTypeException
		public override double getValueAsDouble(int valueIndex)
		{
			return (double)getValueAsLong(valueIndex);
		}

		public override string getValueAsString(int valueIndex)
		{
			switch (valueIndex)
			{
				case 0:
					return processId;
				default:
					try
					{
						return Convert.ToString(getValueAsLong(valueIndex));
					}
					catch (InvalidTypeException)
					{
						// we shouldn't stop there since we test, in this method first.
					}
                    break;
			}

			throw new System.IndexOutOfRangeException();
		}

		/// <summary>
		/// Returns a custom array of <seealso cref="EventValueDescription"/> since the actual content of this
		/// event (list of (long, long) does not match the values encoded into those longs.
		/// </summary>
		internal static EventValueDescription[] valueDescriptions
		{
			get
			{
				try
				{
					return new EventValueDescription[] {new EventValueDescription("Process Name", EventValueTypes.STRING), new EventValueDescription("GC Time", EventValueTypes.LONG, EventValueDescription.ValueTypes.MILLISECONDS), new EventValueDescription("Freed Objects", EventValueTypes.LONG, EventValueDescription.ValueTypes.OBJECTS), new EventValueDescription("Freed Bytes", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Soft Limit", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Actual Size (aggregate)", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Allowed Size (aggregate)", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Allocated Objects (aggregate)", EventValueTypes.LONG, EventValueDescription.ValueTypes.OBJECTS), new EventValueDescription("Allocated Bytes (aggregate)", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Actual Size", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Allowed Size", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Allocated Objects", EventValueTypes.LONG, EventValueDescription.ValueTypes.OBJECTS), new EventValueDescription("Allocated Bytes", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Actual Size (zygote)", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Allowed Size (zygote)", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Allocated Objects (zygote)", EventValueTypes.LONG, EventValueDescription.ValueTypes.OBJECTS), new EventValueDescription("Allocated Bytes (zygote)", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("External Allocation Limit", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("External Bytes Allocated", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("dlmalloc Footprint", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES), new EventValueDescription("Malloc Info: Total Allocated Space", EventValueTypes.LONG, EventValueDescription.ValueTypes.BYTES)};
				}
				catch (InvalidValueTypeException)
				{
					// this shouldn't happen since we control manual the EventValueType and the ValueType
					// values. For development purpose, we assert if this happens.
					Debug.Assert(false);
				}
    
				// this shouldn't happen, but the compiler complains otherwise.
				return null;
			}
		}

		private void parseDvmHeapInfo(long data, int index)
		{
			switch (index)
			{
				case 0:
					//    [63   ] Must be zero
					//    [62-24] ASCII process identifier
					//    [23-12] GC time in ms
					//    [11- 0] Bytes freed

					gcTime = float12ToInt((int)((data >> 12) & 0xFFFL));
					bytesFreed = float12ToInt((int)(data & 0xFFFL));

					// convert the long into an array, in the proper order so that we can convert the
					// first 5 char into a string.
					var dataArray = new byte[8];
					put64bitsToArray((ulong) data, dataArray, 0);

					// get the name from the string
					processId = dataArray.getString(0, 5, null);
					break;
				case 1:
					//    [63-62] 10
					//    [61-60] Reserved; must be zero
					//    [59-48] Objects freed
					//    [47-36] Actual size (current footprint)
					//    [35-24] Allowed size (current hard max)
					//    [23-12] Objects allocated
					//    [11- 0] Bytes allocated
					objectsFreed = float12ToInt((int)((data >> 48) & 0xFFFL));
					actualSize = float12ToInt((int)((data >> 36) & 0xFFFL));
					allowedSize = float12ToInt((int)((data >> 24) & 0xFFFL));
					objectsAllocated = float12ToInt((int)((data >> 12) & 0xFFFL));
					bytesAllocated = float12ToInt((int)(data & 0xFFFL));
					break;
				case 2:
					//    [63-62] 11
					//    [61-60] Reserved; must be zero
					//    [59-48] Soft limit (current soft max)
					//    [47-36] Actual size (current footprint)
					//    [35-24] Allowed size (current hard max)
					//    [23-12] Objects allocated
					//    [11- 0] Bytes allocated
					softLimit = float12ToInt((int)((data >> 48) & 0xFFFL));
					zActualSize = float12ToInt((int)((data >> 36) & 0xFFFL));
					zAllowedSize = float12ToInt((int)((data >> 24) & 0xFFFL));
					zObjectsAllocated = float12ToInt((int)((data >> 12) & 0xFFFL));
					zBytesAllocated = float12ToInt((int)(data & 0xFFFL));
					break;
				case 3:
					//    [63-48] Reserved; must be zero
					//    [47-36] dlmallocFootprint
					//    [35-24] mallinfo: total allocated space
					//    [23-12] External byte limit
					//    [11- 0] External bytes allocated
					dlmallocFootprint = float12ToInt((int)((data >> 36) & 0xFFFL));
					mallinfoTotalAllocatedSpace = float12ToInt((int)((data >> 24) & 0xFFFL));
					externalLimit = float12ToInt((int)((data >> 12) & 0xFFFL));
					externalBytesAllocated = float12ToInt((int)(data & 0xFFFL));
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Converts a 12 bit float representation into an unsigned int (returned as a long) </summary>
		/// <param name="f12"> </param>
		private static long float12ToInt(int f12)
		{
			return (f12 & 0x1FF) << (((int)((uint)f12 >> 9)) * 4);
		}

		/// <summary>
		/// puts an unsigned value in an array. </summary>
		/// <param name="value"> The value to put. </param>
		/// <param name="dest"> the destination array </param>
		/// <param name="offset"> the offset in the array where to put the value.
		///      Array length must be at least offset + 8 </param>
		private static void put64bitsToArray(ulong value, byte[] dest, int offset)
		{
			dest[offset + 7] = (byte)(value & 0x00000000000000FFL);
			dest[offset + 6] = (byte)((value & 0x000000000000FF00L) >> 8);
			dest[offset + 5] = (byte)((value & 0x0000000000FF0000L) >> 16);
			dest[offset + 4] = (byte)((value & 0x00000000FF000000L) >> 24);
			dest[offset + 3] = (byte)((value & 0x000000FF00000000L) >> 32);
			dest[offset + 2] = (byte)((value & 0x0000FF0000000000L) >> 40);
			dest[offset + 1] = (byte)((value & 0x00FF000000000000L) >> 48);
			dest[offset + 0] = (byte)((value & 0xFF00000000000000L) >> 56);
		}

		/// <summary>
		/// Returns the long value of the <code>valueIndex</code>-th value. </summary>
		/// <param name="valueIndex"> the index of the value. </param>
		/// <exception cref="InvalidTypeException"> if index is 0 as it is a string value. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private final long getValueAsLong(int valueIndex) throws InvalidTypeException
		private long getValueAsLong(int valueIndex)
		{
			switch (valueIndex)
			{
				case 0:
					throw new InvalidTypeException();
				case 1:
					return gcTime;
				case 2:
					return objectsFreed;
				case 3:
					return bytesFreed;
				case 4:
					return softLimit;
				case 5:
					return actualSize;
				case 6:
					return allowedSize;
				case 7:
					return objectsAllocated;
				case 8:
					return bytesAllocated;
				case 9:
					return actualSize - zActualSize;
				case 10:
					return allowedSize - zAllowedSize;
				case 11:
					return objectsAllocated - zObjectsAllocated;
				case 12:
					return bytesAllocated - zBytesAllocated;
				case 13:
				   return zActualSize;
				case 14:
					return zAllowedSize;
				case 15:
					return zObjectsAllocated;
				case 16:
					return zBytesAllocated;
				case 17:
					return externalLimit;
				case 18:
					return externalBytesAllocated;
				case 19:
					return dlmallocFootprint;
				case 20:
					return mallinfoTotalAllocatedSpace;
			}

			throw new System.IndexOutOfRangeException();
		}
	}

}