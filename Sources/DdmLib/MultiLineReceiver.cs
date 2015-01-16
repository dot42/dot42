using System;
using System.Collections.Generic;
using System.Text;

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
	/// Base implementation of <seealso cref="IShellOutputReceiver"/>, that takes the raw data coming from the
	/// socket, and convert it into <seealso cref="string"/> objects.
	/// <p/>Additionally, it splits the string by lines.
	/// <p/>Classes extending it must implement <seealso cref="#processNewLines(String[])"/> which receives
	/// new parsed lines as they become available.
	/// </summary>
	public abstract class MultiLineReceiver : IShellOutputReceiver
	{

		private bool mTrimLines = true;

	/// <summary>
		/// unfinished message line, stored for next packet </summary>
		private string mUnfinishedLine = null;

		private readonly List<string> mArray = new List<string>();

		/// <summary>
		/// Set the trim lines flag. </summary>
		/// <param name="trim"> hether the lines are trimmed, or not. </param>
		public virtual bool trimLine
		{
            set { mTrimLines = value; }
		}

		/* (non-Javadoc)
		 * @see com.android.ddmlib.adb.IShellOutputReceiver#addOutput(
		 *      byte[], int, int)
		 */
		public virtual void addOutput(byte[] data, int offset, int length)
		{
			if (cancelled == false)
			{
				string s = null;
				try
				{
					s = Encoding.UTF8.GetString(data, offset, length);
				}
				catch (Exception)
				{
					// normal encoding didn't work, try the default one
					s = Encoding.Default.GetString(data, offset,length);
				}

				// ok we've got a string
				if (s != null)
				{
					// if we had an unfinished line we add it.
					if (mUnfinishedLine != null)
					{
						s = mUnfinishedLine + s;
						mUnfinishedLine = null;
					}

					// now we split the lines
					mArray.Clear();
					int start = 0;
					do
					{
						int index = s.IndexOf("\r\n", start); //$NON-NLS-1$

						// if \r\n was not found, this is an unfinished line
						// and we store it to be processed for the next packet
						if (index == -1)
						{
							mUnfinishedLine = s.Substring(start);
							break;
						}

						// so we found a \r\n;
						// extract the line
						string line = s.Substring(start, index - start);
						if (mTrimLines)
						{
							line = line.Trim();
						}
						mArray.Add(line);

						// move start to after the \r\n we found
						start = index + 2;
					} while (true);

					if (mArray.Count > 0)
					{
						// at this point we've split all the lines.
						// make the array
						string[] lines = mArray.ToArray();

						// send it for final processing
						processNewLines(lines);
					}
				}
			}
		}

		/* (non-Javadoc)
		 * @see com.android.ddmlib.adb.IShellOutputReceiver#flush()
		 */
		public virtual void flush()
		{
			if (mUnfinishedLine != null)
			{
				processNewLines(new string[] {mUnfinishedLine});
			}

			done();
		}

		/// <summary>
		/// Terminates the process. This is called after the last lines have been through
		/// <seealso cref="#processNewLines(String[])"/>.
		/// </summary>
		public virtual void done()
		{
			// do nothing.
		}

		/// <summary>
		/// Called when new lines are being received by the remote process.
		/// <p/>It is guaranteed that the lines are complete when they are given to this method. </summary>
		/// <param name="lines"> The array containing the new lines. </param>
		public abstract void processNewLines(string[] lines);

	    /// <summary>
	    /// Cancel method to stop the execution of the remote shell command. </summary>
	    /// <returns> true to cancel the execution of the command. </returns>
        public virtual bool cancelled { get { return false; } }
	}

}