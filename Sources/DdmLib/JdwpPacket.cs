using System.Diagnostics;
using Dot42.DdmLib.support;

/* //device/tools/ddms/libs/ddmlib/src/com/android/ddmlib/JdwpPacket.java
**
** Copyright 2007, The Android Open Source Project
**
** Licensed under the Apache License, Version 2.0 (the "License");
** you may not use this file except in compliance with the License.
** You may obtain a copy of the License at
**
**     http://www.apache.org/licenses/LICENSE-2.0
**
** Unless required by applicable law or agreed to in writing, software
** distributed under the License is distributed on an "AS IS" BASIS,
** WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
** See the License for the specific language governing permissions and
** limitations under the License.
*/

namespace Dot42.DdmLib
{


	/// <summary>
	/// A JDWP packet, sitting at the start of a ByteBuffer somewhere.
	/// 
	/// This allows us to wrap a "pointer" to the data with the results of
	/// decoding the packet.
	/// 
	/// None of the operations here are synchronized.  If multiple threads will
	/// be accessing the same ByteBuffers, external sync will be required.
	/// 
	/// Use the constructor to create an empty packet, or "findPacket()" to
	/// wrap a JdwpPacket around existing data.
	/// </summary>
	internal sealed class JdwpPacket
	{
		// header len
		public const int JDWP_HEADER_LEN = 11;

		// results from findHandshake
		public const int HANDSHAKE_GOOD = 1;
		public const int HANDSHAKE_NOTYET = 2;
		public const int HANDSHAKE_BAD = 3;

		// our cmdSet/cmd
		private const int DDMS_CMD_SET = 0xc7; // 'G' + 128
		private const int DDMS_CMD = 0x01;

		// "flags" field
		private const int REPLY_PACKET = 0x80;

		// this is sent and expected at the start of a JDWP connection
		private static readonly byte[] mHandshake = {(byte) 'J', (byte) 'D', (byte) 'W', (byte) 'P', (byte) '-', (byte) 'H', (byte) 'a', (byte) 'n', (byte) 'd', (byte) 's', (byte) 'h', (byte) 'a', (byte) 'k', (byte) 'e'};

		public static readonly int HANDSHAKE_LEN = mHandshake.Length;

		private ByteBuffer mBuffer;
		private int mLength, mId, mFlags, mCmdSet, mCmd, mErrCode;
		private bool mIsNew;

		private static int mSerialId = 0x40000000;


		/// <summary>
		/// Create a new, empty packet, in "buf".
		/// </summary>
		internal JdwpPacket(ByteBuffer buf)
		{
			mBuffer = buf;
			mIsNew = true;
		}

		/// <summary>
		/// Finish a packet created with newPacket().
		/// 
		/// This always creates a command packet, with the next serial number
		/// in sequence.
		/// 
		/// We have to take "payloadLength" as an argument because we can't
		/// see the position in the "slice" returned by getPayload().  We could
		/// fish it out of the chunk header, but it's legal for there to be
		/// more than one chunk in a JDWP packet.
		/// 
		/// On exit, "position" points to the end of the data.
		/// </summary>
		internal void finishPacket(int payloadLength)
		{
			Debug.Assert(mIsNew);

			ByteOrder oldOrder = mBuffer.order;
			mBuffer.order = ChunkHandler.CHUNK_ORDER;

			mLength = JDWP_HEADER_LEN + payloadLength;
			mId = nextSerial;
			mFlags = 0;
			mCmdSet = DDMS_CMD_SET;
			mCmd = DDMS_CMD;

			mBuffer.putInt(0x00, mLength);
			mBuffer.putInt(0x04, mId);
			mBuffer.put(0x08, (byte) mFlags);
			mBuffer.put(0x09, (byte) mCmdSet);
			mBuffer.put(0x0a, (byte) mCmd);

			mBuffer.order = oldOrder;
			mBuffer.position = mLength;
		}

		/// <summary>
		/// Get the next serial number.  This creates a unique serial number
		/// across all connections, not just for the current connection.  This
		/// is a useful property when debugging, but isn't necessary.
		/// 
		/// We can't synchronize on an int, so we use a sync method.
		/// </summary>
		//[MethodImpl(MethodImplOptions.Synchronized)]
		private static int nextSerial
		{
			get
			{
				return mSerialId++;
			}
		}

		/// <summary>
		/// Return a slice of the byte buffer, positioned past the JDWP header
		/// to the start of the chunk header.  The buffer's limit will be set
		/// to the size of the payload if the size is known; if this is a
		/// packet under construction the limit will be set to the end of the
		/// buffer.
		/// 
		/// Doesn't examine the packet at all -- works on empty buffers.
		/// </summary>
		internal ByteBuffer payload
		{
			get
			{
				ByteBuffer buf;
				int oldPosn = mBuffer.position;
    
				mBuffer.position = JDWP_HEADER_LEN;
				buf = mBuffer.slice(); // goes from position to limit
				mBuffer.position = oldPosn;
    
				if (mLength > 0)
				{
					buf.limit =(mLength - JDWP_HEADER_LEN);
				}
				else
				{
					Debug.Assert(mIsNew);
				}
				buf.order = ChunkHandler.CHUNK_ORDER;
				return buf;
			}
		}

		/// <summary>
		/// Returns "true" if this JDWP packet has a JDWP command type.
		/// 
		/// This never returns "true" for reply packets.
		/// </summary>
		internal bool ddmPacket
		{
			get
			{
				return (mFlags & REPLY_PACKET) == 0 && mCmdSet == DDMS_CMD_SET && mCmd == DDMS_CMD;
			}
		}

		/// <summary>
		/// Returns "true" if this JDWP packet is tagged as a reply.
		/// </summary>
		internal bool reply
		{
			get
			{
				return (mFlags & REPLY_PACKET) != 0;
			}
		}

		/// <summary>
		/// Returns "true" if this JDWP packet is a reply with a nonzero
		/// error code.
		/// </summary>
		internal bool error
		{
			get
			{
				return reply && mErrCode != 0;
			}
		}

		/// <summary>
		/// Returns "true" if this JDWP packet has no data.
		/// </summary>
		internal bool empty
		{
			get
			{
				return (mLength == JDWP_HEADER_LEN);
			}
		}

		/// <summary>
		/// Return the packet's ID.  For a reply packet, this allows us to
		/// match the reply with the original request.
		/// </summary>
		internal int id
		{
			get
			{
				return mId;
			}
		}

		/// <summary>
		/// Return the length of a packet.  This includes the header, so an
		/// empty packet is 11 bytes long.
		/// </summary>
		internal int length
		{
			get
			{
				return mLength;
			}
		}

		/// <summary>
		/// Write our packet to "chan".  Consumes the packet as part of the
		/// write.
		/// 
		/// The JDWP packet starts at offset 0 and ends at mBuffer.position().
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void writeAndConsume(java.nio.channels.SocketChannel chan) throws java.io.IOException
		internal void writeAndConsume(SocketChannel chan)
		{
			int oldLimit;

			//Log.i("ddms", "writeAndConsume: pos=" + mBuffer.position()
			//    + ", limit=" + mBuffer.limit());

			Debug.Assert(mLength > 0);

			mBuffer.flip(); // limit<-posn, posn<-0
			oldLimit = mBuffer.limit;
			mBuffer.limit = (mLength);
			while (mBuffer.position != mBuffer.limit)
			{
				chan.write(mBuffer);
			}
			// position should now be at end of packet
			Debug.Assert(mBuffer.position == mLength);

			mBuffer.limit= (oldLimit);
			mBuffer.compact(); // shift posn...limit, posn<-pending data

			//Log.i("ddms", "               : pos=" + mBuffer.position()
			//    + ", limit=" + mBuffer.limit());
		}

		/// <summary>
		/// "Move" the packet data out of the buffer we're sitting on and into
		/// buf at the current position.
		/// </summary>
		internal void movePacket(ByteBuffer buf)
		{
			Log.v("ddms", "moving " + mLength + " bytes");
			int oldPosn = mBuffer.position;

			mBuffer.position = (0);
			mBuffer.limit =(mLength);
			buf.put(mBuffer);
			mBuffer.position=(mLength);
			mBuffer.limit=(oldPosn);
			mBuffer.compact(); // shift posn...limit, posn<-pending data
		}

		/// <summary>
		/// Consume the JDWP packet.
		/// 
		/// On entry and exit, "position" is the #of bytes in the buffer.
		/// </summary>
		internal void consume()
		{
			//Log.d("ddms", "consuming " + mLength + " bytes");
			//Log.d("ddms", "  posn=" + mBuffer.position()
			//    + ", limit=" + mBuffer.limit());

			/*
			 * The "flip" call sets "limit" equal to the position (usually the
			 * end of data) and "position" equal to zero.
			 *
			 * compact() copies everything from "position" and "limit" to the
			 * start of the buffer, sets "position" to the end of data, and
			 * sets "limit" to the capacity.
			 *
			 * On entry, "position" is set to the amount of data in the buffer
			 * and "limit" is set to the capacity.  We want to call flip()
			 * so that position..limit spans our data, advance "position" past
			 * the current packet, then compact.
			 */
			mBuffer.flip(); // limit<-posn, posn<-0
			mBuffer.position = (mLength);
			mBuffer.compact(); // shift posn...limit, posn<-pending data
			mLength = 0;
			//Log.d("ddms", "  after compact, posn=" + mBuffer.position()
			//    + ", limit=" + mBuffer.limit());
		}

		/// <summary>
		/// Find the JDWP packet at the start of "buf".  The start is known,
		/// but the length has to be parsed out.
		/// 
		/// On entry, the packet data in "buf" must start at offset 0 and end
		/// at "position".  "limit" should be set to the buffer capacity.  This
		/// method does not alter "buf"s attributes.
		/// 
		/// Returns a new JdwpPacket if a full one is found in the buffer.  If
		/// not, returns null.  Throws an exception if the data doesn't look like
		/// a valid JDWP packet.
		/// </summary>
		internal static JdwpPacket findPacket(ByteBuffer buf)
		{
			int count = buf.position;
			int length, id, flags, cmdSet, cmd;

			if (count < JDWP_HEADER_LEN)
			{
				return null;
			}

			ByteOrder oldOrder = buf.order;
			buf.order =(ChunkHandler.CHUNK_ORDER);

			length = buf.getInt(0x00);
			id = buf.getInt(0x04);
			flags = buf.get(0x08) & 0xff;
			cmdSet = buf.get(0x09) & 0xff;
			cmd = buf.get(0x0a) & 0xff;

			buf.order = oldOrder;

			if (length < JDWP_HEADER_LEN)
			{
				throw new BadPacketException();
			}
			if (count < length)
			{
				return null;
			}

			JdwpPacket pkt = new JdwpPacket(buf);
			//pkt.mBuffer = buf;
			pkt.mLength = length;
			pkt.mId = id;
			pkt.mFlags = flags;

			if ((flags & REPLY_PACKET) == 0)
			{
				pkt.mCmdSet = cmdSet;
				pkt.mCmd = cmd;
				pkt.mErrCode = -1;
			}
			else
			{
				pkt.mCmdSet = -1;
				pkt.mCmd = -1;
				pkt.mErrCode = cmdSet | (cmd << 8);
			}

			return pkt;
		}

		/// <summary>
		/// Like findPacket(), but when we're expecting the JDWP handshake.
		/// 
		/// Returns one of:
		///   HANDSHAKE_GOOD   - found handshake, looks good
		///   HANDSHAKE_BAD    - found enough data, but it's wrong
		///   HANDSHAKE_NOTYET - not enough data has been read yet
		/// </summary>
		internal static int findHandshake(ByteBuffer buf)
		{
			int count = buf.position;
			int i;

			if (count < mHandshake.Length)
			{
				return HANDSHAKE_NOTYET;
			}

			for (i = mHandshake.Length - 1; i >= 0; --i)
			{
				if (buf.get(i) != mHandshake[i])
				{
					return HANDSHAKE_BAD;
				}
			}

			return HANDSHAKE_GOOD;
		}

		/// <summary>
		/// Remove the handshake string from the buffer.
		/// 
		/// On entry and exit, "position" is the #of bytes in the buffer.
		/// </summary>
		internal static void consumeHandshake(ByteBuffer buf)
		{
			// in theory, nothing else can have arrived, so this is overkill
			buf.flip(); // limit<-posn, posn<-0
			buf.position = mHandshake.Length;
			buf.compact(); // shift posn...limit, posn<-pending data
		}

		/// <summary>
		/// Copy the handshake string into the output buffer.
		/// 
		/// On exit, "buf"s position will be advanced.
		/// </summary>
		internal static void putHandshake(ByteBuffer buf)
		{
			buf.put(mHandshake);
		}
	}


}