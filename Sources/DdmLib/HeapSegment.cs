using System;
using System.Text;
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
	/// Describes the types and locations of objects in a segment of a heap.
	/// </summary>
	public sealed class HeapSegment : IComparable<HeapSegment>
	{

		/// <summary>
		/// Describes an object/region encoded in the HPSG data.
		/// </summary>
		public class HeapSegmentElement : IComparable<HeapSegmentElement>
		{

			/*
			 * Solidity values, which must match the values in
			 * the HPSG data.
			 */

		/// <summary>
			/// The element describes a free block. </summary>
			public static int SOLIDITY_FREE = 0;

		/// <summary>
			/// The element is strongly-reachable. </summary>
			public static int SOLIDITY_HARD = 1;

		/// <summary>
			/// The element is softly-reachable. </summary>
			public static int SOLIDITY_SOFT = 2;

		/// <summary>
			/// The element is weakly-reachable. </summary>
			public static int SOLIDITY_WEAK = 3;

		/// <summary>
			/// The element is phantom-reachable. </summary>
			public static int SOLIDITY_PHANTOM = 4;

		/// <summary>
			/// The element is pending finalization. </summary>
			public static int SOLIDITY_FINALIZABLE = 5;

		/// <summary>
			/// The element is not reachable, and is about to be swept/freed. </summary>
			public static int SOLIDITY_SWEEP = 6;

		/// <summary>
			/// The reachability of the object is unknown. </summary>
			public static int SOLIDITY_INVALID = -1;


			/*
			 * Kind values, which must match the values in
			 * the HPSG data.
			 */

		/// <summary>
			/// The element describes a data object. </summary>
			public static int KIND_OBJECT = 0;

		/// <summary>
			/// The element describes a class object. </summary>
			public static int KIND_CLASS_OBJECT = 1;

		/// <summary>
			/// The element describes an array of 1-byte elements. </summary>
			public static int KIND_ARRAY_1 = 2;

		/// <summary>
			/// The element describes an array of 2-byte elements. </summary>
			public static int KIND_ARRAY_2 = 3;

		/// <summary>
			/// The element describes an array of 4-byte elements. </summary>
			public static int KIND_ARRAY_4 = 4;

		/// <summary>
			/// The element describes an array of 8-byte elements. </summary>
			public static int KIND_ARRAY_8 = 5;

		/// <summary>
			/// The element describes an unknown type of object. </summary>
			public static int KIND_UNKNOWN = 6;

		/// <summary>
			/// The element describes a native object. </summary>
			public static int KIND_NATIVE = 7;

		/// <summary>
			/// The object kind is unknown or unspecified. </summary>
			public static int KIND_INVALID = -1;


			/// <summary>
			/// A bit in the HPSG data that indicates that an element should
			/// be combined with the element that follows, typically because
			/// an element is too large to be described by a single element.
			/// </summary>
			private static int PARTIAL_MASK = 1 << 7;


			/// <summary>
			/// Describes the reachability/solidity of the element.  Must
			/// be set to one of the SOLIDITY_* values.
			/// </summary>
			private int mSolidity;

			/// <summary>
			/// Describes the type/kind of the element.  Must be set to one
			/// of the KIND_* values.
			/// </summary>
			private int mKind;

			/// <summary>
			/// Describes the length of the element, in bytes.
			/// </summary>
			private int mLength;


			/// <summary>
			/// Creates an uninitialized element.
			/// </summary>
			public HeapSegmentElement()
			{
				solidity = SOLIDITY_INVALID;
				kind = KIND_INVALID;
				length = -1;
			}

			/// <summary>
			/// Create an element describing the entry at the current
			/// position of hpsgData.
			/// </summary>
			/// <param name="hs"> The heap segment to pull the entry from. </param>
			/// <exception cref="BufferUnderflowException"> if there is not a whole entry
			///                                  following the current position
			///                                  of hpsgData. </exception>
			/// <exception cref="ParseException">           if the provided data is malformed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HeapSegmentElement(HeapSegment hs) throws java.nio.BufferUnderflowException, java.text.ParseException
			public HeapSegmentElement(HeapSegment hs)
			{
				set(hs);
			}

			/// <summary>
			/// Replace the element with the entry at the current position of
			/// hpsgData.
			/// </summary>
			/// <param name="hs"> The heap segment to pull the entry from. </param>
			/// <returns> this object. </returns>
			/// <exception cref="BufferUnderflowException"> if there is not a whole entry
			///                                  following the current position of
			///                                  hpsgData. </exception>
			/// <exception cref="ParseException">           if the provided data is malformed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HeapSegmentElement set(HeapSegment hs) throws java.nio.BufferUnderflowException, java.text.ParseException
			public virtual HeapSegmentElement set(HeapSegment hs)
			{

				/* TODO: Maybe keep track of the virtual address of each element
				 *       so that they can be examined independently.
				 */
				ByteBuffer data = hs.mUsageData;
				int eState = data.get() & 0x000000ff;
				int eLen = (data.get() & 0x000000ff) + 1;

				while ((eState & PARTIAL_MASK) != 0)
				{

					/* If the partial bit was set, the next byte should describe
					 * the same object as the current one.
					 */
					int nextState = data.get() & 0x000000ff;
					if ((nextState & ~PARTIAL_MASK) != (eState & ~PARTIAL_MASK))
					{
						throw new ArgumentException("State mismatch at " + data.position);
					}
					eState = nextState;
					eLen += (data.get() & 0x000000ff) + 1;
				}

				solidity = eState & 0x7;
				kind = (eState >> 3) & 0x7;
				length = eLen * hs.mAllocationUnitSize;

				return this;
			}

			public virtual int solidity
			{
				get
				{
					return mSolidity;
				}
				set
				{
					this.mSolidity = value;
				}
			}


			public virtual int kind
			{
				get
				{
					return mKind;
				}
				set
				{
					this.mKind = value;
				}
			}


			public virtual int length
			{
				get
				{
					return mLength;
				}
				set
				{
					this.mLength = value;
				}
			}


			public int CompareTo(HeapSegmentElement other)
			{
				if (mLength != other.mLength)
				{
					return mLength < other.mLength ? - 1 : 1;
				}
				return 0;
			}
		}

		//* The ID of the heap that this segment belongs to.
		internal int mHeapId;

		//* The size of an allocation unit, in bytes. (e.g., 8 bytes)
		internal int mAllocationUnitSize;

		//* The virtual address of the start of this segment.
		internal long mStartAddress;

		//* The offset of this pices from mStartAddress, in bytes.
		internal int mOffset;

		//* The number of allocation units described in this segment.
		internal int mAllocationUnitCount;

		//* The raw data that describes the contents of this segment.
		internal ByteBuffer mUsageData;

		//* mStartAddress is set to this value when the segment becomes invalid.
		private const long INVALID_START_ADDRESS = -1;

		/// <summary>
		/// Create a new HeapSegment based on the raw contents
		/// of an HPSG chunk.
		/// </summary>
		/// <param name="hpsgData"> The raw data from an HPSG chunk. </param>
		/// <exception cref="BufferUnderflowException"> if hpsgData is too small
		///                                  to hold the HPSG chunk header data. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HeapSegment(java.nio.ByteBuffer hpsgData) throws java.nio.BufferUnderflowException
		public HeapSegment(ByteBuffer hpsgData)
		{
			/* Read the HPSG chunk header.
			 * These get*() calls may throw a BufferUnderflowException
			 * if the underlying data isn't big enough.
			 */
			hpsgData.order = ByteOrder.BIG_ENDIAN;
            mHeapId = hpsgData.getInt();
			mAllocationUnitSize = hpsgData.get();
            mStartAddress = hpsgData.getInt() & 0x00000000ffffffffL;
            mOffset = hpsgData.getInt();
            mAllocationUnitCount = hpsgData.getInt();

			// Hold onto the remainder of the data.
			mUsageData = hpsgData.slice();
			mUsageData.order = ByteOrder.BIG_ENDIAN; // doesn't actually matter

			// Validate the data.
	//xxx do it
	//xxx make sure the number of elements matches mAllocationUnitCount.
	//xxx make sure the last element doesn't have P set
		}

		/// <summary>
		/// See if this segment still contains data, and has not been
		/// appended to another segment.
		/// </summary>
		/// <returns> true if this segment has not been appended to
		///         another segment. </returns>
		public bool valid
		{
			get
			{
				return mStartAddress != INVALID_START_ADDRESS;
			}
		}

		/// <summary>
		/// See if <code>other</code> comes immediately after this segment.
		/// </summary>
		/// <param name="other"> The HeapSegment to check. </param>
		/// <returns> true if <code>other</code> comes immediately after this
		///         segment. </returns>
		public bool canAppend(HeapSegment other)
		{
			return valid && other.valid && mHeapId == other.mHeapId && mAllocationUnitSize == other.mAllocationUnitSize && endAddress == other.startAddress;
		}

		/// <summary>
		/// Append the contents of <code>other</code> to this segment
		/// if it describes the segment immediately after this one.
		/// </summary>
		/// <param name="other"> The segment to append to this segment, if possible.
		///              If appended, <code>other</code> will be invalid
		///              when this method returns. </param>
		/// <returns> true if <code>other</code> was successfully appended to
		///         this segment. </returns>
		public bool append(HeapSegment other)
		{
			if (canAppend(other))
			{
				/* Preserve the position.  The mark is not preserved,
				 * but we don't use it anyway.
				 */
				int pos = mUsageData.position;

				// Guarantee that we have enough room for the new data.
				if (mUsageData.capacity - mUsageData.limit < other.mUsageData.limit)
				{
					/* Grow more than necessary in case another append()
					 * is about to happen.
					 */
					int newSize = mUsageData.limit + other.mUsageData.limit;
					ByteBuffer newData = ByteBuffer.allocate(newSize * 2);

					mUsageData.rewind();
					newData.put(mUsageData);
					mUsageData = newData;
				}

				// Copy the data from the other segment and restore the position.
				other.mUsageData.rewind();
				mUsageData.put(other.mUsageData);
				mUsageData.position = pos;

				// Fix this segment's header to cover the new data.
				mAllocationUnitCount += other.mAllocationUnitCount;

				// Mark the other segment as invalid.
				other.mStartAddress = INVALID_START_ADDRESS;
				other.mUsageData = null;

				return true;
			}
			else
			{
				return false;
			}
		}

		public long startAddress
		{
			get
			{
				return mStartAddress + mOffset;
			}
		}

		public int length
		{
			get
			{
				return mAllocationUnitSize * mAllocationUnitCount;
			}
		}

		public long endAddress
		{
			get
			{
				return startAddress + length;
			}
		}

		public void rewindElements()
		{
			if (mUsageData != null)
			{
				mUsageData.rewind();
			}
		}

		public HeapSegmentElement getNextElement(HeapSegmentElement reuse)
		{
			try
			{
				if (reuse != null)
				{
					return reuse.set(this);
				}
				else
				{
					return new HeapSegmentElement(this);
				}
			}
			catch (BufferUnderflowException)
			{
				/* Normal "end of buffer" situation.
				 */
			}
			catch (ArgumentException)
			{
				/* Malformed data.
				 */
	//TODO: we should catch this in the constructor
			}
			return null;
		}

		/*
		 * Method overrides for Comparable
		 */
		public override bool Equals(object o)
		{
			if (o is HeapSegment)
			{
				return CompareTo((HeapSegment) o) == 0;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return mHeapId * 31 + mAllocationUnitSize * 31 + (int) mStartAddress * 31 + mOffset * 31 + mAllocationUnitCount * 31 + mUsageData.GetHashCode();
		}

		public override string ToString()
		{
			StringBuilder str = new StringBuilder();

			str.Append("HeapSegment { heap ").Append(mHeapId).Append(", start 0x").Append(((int) startAddress).toHexString()).Append(", length ").Append(length).Append(" }");

			return str.ToString();
		}

		public int CompareTo(HeapSegment other)
		{
			if (mHeapId != other.mHeapId)
			{
				return mHeapId < other.mHeapId ? - 1 : 1;
			}
			if (startAddress != other.startAddress)
			{
				return startAddress < other.startAddress ? - 1 : 1;
			}

			/* If two segments have the same start address, the rest of
			 * the fields should be equal.  Go through the motions, though.
			 * Note that we re-check the components of getStartAddress()
			 * (mStartAddress and mOffset) to make sure that all fields in
			 * an equal segment are equal.
			 */

			if (mAllocationUnitSize != other.mAllocationUnitSize)
			{
				return mAllocationUnitSize < other.mAllocationUnitSize ? - 1 : 1;
			}
			if (mStartAddress != other.mStartAddress)
			{
				return mStartAddress < other.mStartAddress ? - 1 : 1;
			}
			if (mOffset != other.mOffset)
			{
				return mOffset < other.mOffset ? - 1 : 1;
			}
			if (mAllocationUnitCount != other.mAllocationUnitCount)
			{
				return mAllocationUnitCount < other.mAllocationUnitCount ? - 1 : 1;
			}
			if (mUsageData != other.mUsageData)
			{
				return mUsageData.CompareTo(other.mUsageData);
			}
			return 0;
		}
	}

}