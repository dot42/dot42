using System;
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
	/// Data representing an image taken from a device frame buffer.
	/// </summary>
	public sealed class RawImage
	{
		public int version;
		public int bpp;
		public int size;
		public int width;
		public int height;
		public int red_offset;
		public int red_length;
		public int blue_offset;
		public int blue_length;
		public int green_offset;
		public int green_length;
		public int alpha_offset;
		public int alpha_length;

		public byte[] data;

		/// <summary>
		/// Reads the header of a RawImage from a <seealso cref="ByteBuffer"/>.
		/// <p/>The way the data is sent over adb is defined in system/core/adb/framebuffer_service.c </summary>
		/// <param name="version"> the version of the protocol. </param>
		/// <param name="buf"> the buffer to read from. </param>
		/// <returns> true if success </returns>
		public bool readHeader(int version, ByteBuffer buf)
		{
			this.version = version;

			if (version == 16)
			{
				// compatibility mode with original protocol
				this.bpp = 16;

				// read actual values.
				this.size = buf.getInt();
                this.width = buf.getInt();
                this.height = buf.getInt();

				// create default values for the rest. Format is 565
				this.red_offset = 11;
				this.red_length = 5;
				this.green_offset = 5;
				this.green_length = 6;
				this.blue_offset = 0;
				this.blue_length = 5;
				this.alpha_offset = 0;
				this.alpha_length = 0;
			}
			else if (version == 1)
			{
                this.bpp = buf.getInt();
                this.size = buf.getInt();
                this.width = buf.getInt();
                this.height = buf.getInt();
                this.red_offset = buf.getInt();
                this.red_length = buf.getInt();
                this.blue_offset = buf.getInt();
                this.blue_length = buf.getInt();
                this.green_offset = buf.getInt();
                this.green_length = buf.getInt();
                this.alpha_offset = buf.getInt();
                this.alpha_length = buf.getInt();
			}
			else
			{
				// unsupported protocol!
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns the mask value for the red color.
		/// <p/>This value is compatible with org.eclipse.swt.graphics.PaletteData
		/// </summary>
		public int redMask
		{
			get
			{
				return getMask(red_length, red_offset);
			}
		}

		/// <summary>
		/// Returns the mask value for the green color.
		/// <p/>This value is compatible with org.eclipse.swt.graphics.PaletteData
		/// </summary>
		public int greenMask
		{
			get
			{
				return getMask(green_length, green_offset);
			}
		}

		/// <summary>
		/// Returns the mask value for the blue color.
		/// <p/>This value is compatible with org.eclipse.swt.graphics.PaletteData
		/// </summary>
		public int blueMask
		{
			get
			{
				return getMask(blue_length, blue_offset);
			}
		}

		/// <summary>
		/// Returns the size of the header for a specific version of the framebuffer adb protocol. </summary>
		/// <param name="version"> the version of the protocol </param>
		/// <returns> the number of int that makes up the header. </returns>
		public static int getHeaderSize(int version)
		{
			switch (version)
			{
				case 16: // compatibility mode
					return 3; // size, width, height
				case 1:
					return 12; // bpp, size, width, height, 4*(length, offset)
			}

			return 0;
		}

		/// <summary>
		/// Returns a rotated version of the image
		/// The image is rotated counter-clockwise.
		/// </summary>
		public RawImage rotated
		{
			get
			{
				RawImage rotated = new RawImage();
				rotated.version = this.version;
				rotated.bpp = this.bpp;
				rotated.size = this.size;
				rotated.red_offset = this.red_offset;
				rotated.red_length = this.red_length;
				rotated.blue_offset = this.blue_offset;
				rotated.blue_length = this.blue_length;
				rotated.green_offset = this.green_offset;
				rotated.green_length = this.green_length;
				rotated.alpha_offset = this.alpha_offset;
				rotated.alpha_length = this.alpha_length;
    
				rotated.width = this.height;
				rotated.height = this.width;
    
				int count = this.data.Length;
				rotated.data = new byte[count];
    
				int byteCount = this.bpp >> 3; // bpp is in bits, we want bytes to match our array
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final int w = this.width;
				int w = this.width;
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final int h = this.height;
				int h = this.height;
				for (int y = 0 ; y < h ; y++)
				{
					for (int x = 0 ; x < w ; x++)
					{
						Array.Copy(this.data, (y * w + x) * byteCount, rotated.data, ((w - x - 1) * h + y) * byteCount, byteCount);
					}
				}
    
				return rotated;
			}
		}

		/// <summary>
		/// Returns an ARGB integer value for the pixel at <var>index</var> in <seealso cref="#data"/>.
		/// </summary>
		public int getARGB(int index)
		{
			int value;
			if (bpp == 16)
			{
				value = data[index] & 0x00FF;
				value |= (data[index + 1] << 8) & 0x0FF00;
			}
			else if (bpp == 32)
			{
				value = data[index] & 0x00FF;
				value |= (data[index + 1] & 0x00FF) << 8;
				value |= (data[index + 2] & 0x00FF) << 16;
				value |= (data[index + 3] & 0x00FF) << 24;
			}
			else
			{
				throw new InvalidOperationException("RawImage.getARGB(int) only works in 16 and 32 bit mode.");
			}

			int r = (((int)((uint)value >> red_offset)) & getMask(red_length)) << (8 - red_length);
			int g = (((int)((uint)value >> green_offset)) & getMask(green_length)) << (8 - green_length);
			int b = (((int)((uint)value >> blue_offset)) & getMask(blue_length)) << (8 - blue_length);
			int a;
			if (alpha_length == 0)
			{
				a = 0xFF; // force alpha to opaque if there's no alpha value in the framebuffer.
			}
			else
			{
				a = (((int)((uint)value >> alpha_offset)) & getMask(alpha_length)) << (8 - alpha_length);
			}

			return a << 24 | r << 16 | g << 8 | b;
		}

		/// <summary>
		/// creates a mask value based on a length and offset.
		/// <p/>This value is compatible with org.eclipse.swt.graphics.PaletteData
		/// </summary>
		private int getMask(int length, int offset)
		{
			int res = getMask(length) << offset;

			// if the bpp is 32 bits then we need to invert it because the buffer is in little endian
			if (bpp == 32)
			{
				return res.reverseBytes();
			}

			return res;
		}

		/// <summary>
		/// Creates a mask value based on a length. </summary>
		/// <param name="length">
		/// @return </param>
		private static int getMask(int length)
		{
			return (1 << length) - 1;
		}
	}

}