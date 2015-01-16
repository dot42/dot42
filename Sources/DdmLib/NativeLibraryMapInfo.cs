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
	/// Memory address to library mapping for native libraries.
	/// <p/>
	/// Each instance represents a single native library and its start and end memory addresses. 
	/// </summary>
	public sealed class NativeLibraryMapInfo
	{
		private long mStartAddr;
		private long mEndAddr;

		private string mLibrary;

		/// <summary>
		/// Constructs a new native library map info. </summary>
		/// <param name="startAddr"> The start address of the library. </param>
		/// <param name="endAddr"> The end address of the library. </param>
		/// <param name="library"> The name of the library. </param>
		internal NativeLibraryMapInfo(long startAddr, long endAddr, string library)
		{
			this.mStartAddr = startAddr;
			this.mEndAddr = endAddr;
			this.mLibrary = library;
		}

		/// <summary>
		/// Returns the name of the library.
		/// </summary>
		public string libraryName
		{
			get
			{
				return mLibrary;
			}
		}

		/// <summary>
		/// Returns the start address of the library.
		/// </summary>
		public long startAddress
		{
			get
			{
				return mStartAddr;
			}
		}

		/// <summary>
		/// Returns the end address of the library.
		/// </summary>
		public long endAddress
		{
			get
			{
				return mEndAddr;
			}
		}

		/// <summary>
		/// Returns whether the specified address is inside the library. </summary>
		/// <param name="address"> The address to test. </param>
		/// <returns> <code>true</code> if the address is between the start and end address of the library. </returns>
		/// <seealso cref= #getStartAddress() </seealso>
		/// <seealso cref= #getEndAddress() </seealso>
		public bool isWithinLibrary(long address)
		{
			return address >= mStartAddr && address <= mEndAddr;
		}
	}

}