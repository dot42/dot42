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

namespace Dot42.DdmLib.utils
{

	/// <summary>
	/// Utility class providing array to int/long conversion for data received from devices through adb. 
	/// </summary>
	public sealed class ArrayHelper
	{

		/// <summary>
		/// Swaps an unsigned value around, and puts the result in an array that can be sent to a device. </summary>
		/// <param name="value"> The value to swap. </param>
		/// <param name="dest"> the destination array </param>
		/// <param name="offset"> the offset in the array where to put the swapped value.
		///      Array length must be at least offset + 4 </param>
		public static void swap32bitsToArray(int value, byte[] dest, int offset)
		{
			dest[offset] = (byte)(value & 0x000000FF);
			dest[offset + 1] = (byte)((value & 0x0000FF00) >> 8);
			dest[offset + 2] = (byte)((value & 0x00FF0000) >> 16);
			dest[offset + 3] = (byte)((value & 0xFF000000) >> 24);
		}

		/// <summary>
		/// Reads a signed 32 bit integer from an array coming from a device. </summary>
		/// <param name="value"> the array containing the int </param>
		/// <param name="offset"> the offset in the array at which the int starts </param>
		/// <returns> the integer read from the array </returns>
		public static int swap32bitFromArray(byte[] value, int offset)
		{
			int v = 0;
			v |= ((int)value[offset]) & 0x000000FF;
			v |= (((int)value[offset + 1]) & 0x000000FF) << 8;
			v |= (((int)value[offset + 2]) & 0x000000FF) << 16;
			v |= (((int)value[offset + 3]) & 0x000000FF) << 24;

			return v;
		}

		/// <summary>
		/// Reads an unsigned 16 bit integer from an array coming from a device,
		/// and returns it as an 'int' </summary>
		/// <param name="value"> the array containing the 16 bit int (2 byte). </param>
		/// <param name="offset"> the offset in the array at which the int starts
		///      Array length must be at least offset + 2 </param>
		/// <returns> the integer read from the array. </returns>
		public static int swapU16bitFromArray(byte[] value, int offset)
		{
			int v = 0;
			v |= ((int)value[offset]) & 0x000000FF;
			v |= (((int)value[offset + 1]) & 0x000000FF) << 8;

			return v;
		}

		/// <summary>
		/// Reads a signed 64 bit integer from an array coming from a device. </summary>
		/// <param name="value"> the array containing the int </param>
		/// <param name="offset"> the offset in the array at which the int starts
		///      Array length must be at least offset + 8 </param>
		/// <returns> the integer read from the array </returns>
		public static long swap64bitFromArray(byte[] value, int offset)
		{
			long v = 0;
			v |= ((long)value[offset]) & 0x00000000000000FFL;
			v |= (((long)value[offset + 1]) & 0x00000000000000FFL) << 8;
			v |= (((long)value[offset + 2]) & 0x00000000000000FFL) << 16;
			v |= (((long)value[offset + 3]) & 0x00000000000000FFL) << 24;
			v |= (((long)value[offset + 4]) & 0x00000000000000FFL) << 32;
			v |= (((long)value[offset + 5]) & 0x00000000000000FFL) << 40;
			v |= (((long)value[offset + 6]) & 0x00000000000000FFL) << 48;
			v |= (((long)value[offset + 7]) & 0x00000000000000FFL) << 56;

			return v;
		}
	}

}