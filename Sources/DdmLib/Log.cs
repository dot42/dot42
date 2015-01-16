using System;
using System.Linq;

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
	/// Log class that mirrors the API in main Android sources.
	/// <p/>Default behavior outputs the log to <seealso cref="System#out"/>. Use
	/// <seealso cref="#setLogOutput(com.android.ddmlib.Log.ILogOutput)"/> to redirect the log somewhere else.
	/// </summary>
	public sealed class Log
	{

		/// <summary>
		/// Log Level enum.
		/// </summary>
		public sealed class LogLevel
		{
		    public static readonly LogLevel VERBOSE = new LogLevel(2, "verbose", 'V'); //$NON-NLS-1$
		    public static readonly LogLevel DEBUG = new LogLevel(3, "debug", 'D'); //$NON-NLS-1$
			public static readonly LogLevel INFO = new LogLevel(4, "info", 'I'); //$NON-NLS-1$
			public static readonly LogLevel WARN = new LogLevel(5, "warn", 'W'); //$NON-NLS-1$
			public static readonly LogLevel ERROR = new LogLevel(6, "error", 'E'); //$NON-NLS-1$
			public static readonly LogLevel ASSERT = new LogLevel(7, "assert", 'A'); //$NON-NLS-1$

            private static readonly LogLevel[] values = new[] { VERBOSE, DEBUG, INFO, WARN, ERROR, ASSERT };

			private readonly int mPriorityLevel;
			private readonly String mStringValue;
			private readonly char mPriorityLetter;

			LogLevel(int intPriority, String stringValue, char priorityChar)
			{
				mPriorityLevel = intPriority;
				mStringValue = stringValue;
				mPriorityLetter = priorityChar;
			}

			public static LogLevel getByString(String value)
			{
			    return values.FirstOrDefault(x => x.mStringValue == value);
			}

			/// <summary>
			/// Returns the <seealso cref="LogLevel"/> enum matching the specified letter. </summary>
			/// <param name="letter"> the letter matching a <code>LogLevel</code> enum </param>
			/// <returns> a <code>LogLevel</code> object or <code>null</code> if no match were found. </returns>
			public static LogLevel getByLetter(char letter)
			{
                return values.FirstOrDefault(x => x.mPriorityLetter == letter);
            }

			/// <summary>
			/// Returns the <seealso cref="LogLevel"/> enum matching the specified letter.
			/// <p/>
			/// The letter is passed as a <seealso cref="String"/> argument, but only the first character
			/// is used. </summary>
			/// <param name="letter"> the letter matching a <code>LogLevel</code> enum </param>
			/// <returns> a <code>LogLevel</code> object or <code>null</code> if no match were found. </returns>
			public static LogLevel getByLetterString(String letter)
			{
				if (letter.Length > 0)
				{
					return getByLetter(letter[0]);
				}
	
				return null;
			}

			/// <summary>
			/// Returns the letter identifying the priority of the <seealso cref="LogLevel"/>.
			/// </summary>
			public char getPriorityLetter()
			{
				return mPriorityLetter;
			}

			/// <summary>
			/// Returns the numerical value of the priority.
			/// </summary>
			public int getPriority()
			{
				return mPriorityLevel;
			}

			/// <summary>
			/// Returns a non translated string representing the LogLevel.
			/// </summary>
			public string getStringValue()
			{
				return mStringValue;
			}
	}

		/// <summary>
		/// Classes which implement this interface provides methods that deal with outputting log
		/// messages.
		/// </summary>
		public interface ILogOutput
		{
			/// <summary>
			/// Sent when a log message needs to be printed. </summary>
			/// <param name="logLevel"> The <seealso cref="LogLevel"/> enum representing the priority of the message. </param>
			/// <param name="tag"> The tag associated with the message. </param>
			/// <param name="message"> The message to display. </param>
			void printLog(LogLevel logLevel, string tag, string message);

			/// <summary>
			/// Sent when a log message needs to be printed, and, if possible, displayed to the user
			/// in a dialog box. </summary>
			/// <param name="logLevel"> The <seealso cref="LogLevel"/> enum representing the priority of the message. </param>
			/// <param name="tag"> The tag associated with the message. </param>
			/// <param name="message"> The message to display. </param>
			void printAndPromptLog(LogLevel logLevel, string tag, string message);
		}

		private static LogLevel mLevel = DdmPreferences.logLevel;

		private static ILogOutput sLogOutput;

		private static readonly char[] mSpaceLine = new char[72];
		private static readonly char[] mHexDigit = new char[] {'0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f'};
		static Log()
		{
			/* prep for hex dump */
			int i = mSpaceLine.Length - 1;
			while (i >= 0)
			{
				mSpaceLine[i--] = ' ';
			}
			mSpaceLine[0] = mSpaceLine[1] = mSpaceLine[2] = mSpaceLine[3] = '0';
			mSpaceLine[4] = '-';
		}

		internal sealed class Config
		{
			internal const bool LOGV = true;
			internal const bool LOGD = true;
		}

		private Log()
		{
		}

		/// <summary>
		/// Outputs a <seealso cref="LogLevel#VERBOSE"/> level message. </summary>
		/// <param name="tag"> The tag associated with the message. </param>
		/// <param name="message"> The message to output. </param>
		public static void v(string tag, string message)
		{
			println(LogLevel.VERBOSE, tag, message);
		}

		/// <summary>
		/// Outputs a <seealso cref="LogLevel#DEBUG"/> level message. </summary>
		/// <param name="tag"> The tag associated with the message. </param>
		/// <param name="message"> The message to output. </param>
		public static void d(string tag, string message)
		{
			println(LogLevel.DEBUG, tag, message);
		}

		/// <summary>
		/// Outputs a <seealso cref="LogLevel#INFO"/> level message. </summary>
		/// <param name="tag"> The tag associated with the message. </param>
		/// <param name="message"> The message to output. </param>
		public static void i(string tag, string message)
		{
			println(LogLevel.INFO, tag, message);
		}

		/// <summary>
		/// Outputs a <seealso cref="LogLevel#WARN"/> level message. </summary>
		/// <param name="tag"> The tag associated with the message. </param>
		/// <param name="message"> The message to output. </param>
		public static void w(string tag, string message)
		{
			println(LogLevel.WARN, tag, message);
		}

		/// <summary>
		/// Outputs a <seealso cref="LogLevel#ERROR"/> level message. </summary>
		/// <param name="tag"> The tag associated with the message. </param>
		/// <param name="message"> The message to output. </param>
		public static void e(string tag, string message)
		{
			println(LogLevel.ERROR, tag, message);
		}

		/// <summary>
		/// Outputs a log message and attempts to display it in a dialog. </summary>
		/// <param name="tag"> The tag associated with the message. </param>
		/// <param name="message"> The message to output. </param>
		public static void logAndDisplay(LogLevel logLevel, string tag, string message)
		{
			if (sLogOutput != null)
			{
				sLogOutput.printAndPromptLog(logLevel, tag, message);
			}
			else
			{
				println(logLevel, tag, message);
			}
		}

		/// <summary>
		/// Outputs a <seealso cref="LogLevel#ERROR"/> level <seealso cref="Throwable"/> information. </summary>
		/// <param name="tag"> The tag associated with the message. </param>
		/// <param name="throwable"> The <seealso cref="Throwable"/> to output. </param>
		public static void e(string tag, Exception throwable)
		{
			if (throwable != null)
			{
				println(LogLevel.ERROR, tag, throwable.Message + '\n' + throwable.StackTrace);
			}
		}

		internal static LogLevel level
		{
			set
			{
				mLevel = value;
			}
		}

		/// <summary>
		/// Sets the <seealso cref="ILogOutput"/> to use to print the logs. If not set, <seealso cref="System#out"/>
		/// will be used. </summary>
		/// <param name="logOutput"> The <seealso cref="ILogOutput"/> to use to print the log. </param>
		public static ILogOutput logOutput
		{
			set
			{
				sLogOutput = value;
			}
		}

		/// <summary>
		/// Show hex dump.
		/// <p/>
		/// Local addition.  Output looks like:
		/// 1230- 00 11 22 33 44 55 66 77 88 99 aa bb cc dd ee ff  0123456789abcdef
		/// <p/>
		/// Uses no string concatenation; creates one String object per line.
		/// </summary>
		internal static void hexDump(string tag, LogLevel level, byte[] data, int offset, int length)
		{

			int kHexOffset = 6;
			int kAscOffset = 55;
			char[] line = new char[mSpaceLine.Length];
			int addr, baseAddr, count;
			int i, ch;
			bool needErase = true;

			//Log.w(tag, "HEX DUMP: off=" + offset + ", length=" + length);

			baseAddr = 0;
			while (length != 0)
			{
				if (length > 16)
				{
					// full line
					count = 16;
				}
				else
				{
					// partial line; re-copy blanks to clear end
					count = length;
					needErase = true;
				}

				if (needErase)
				{
					Array.Copy(mSpaceLine, 0, line, 0, mSpaceLine.Length);
					needErase = false;
				}

				// output the address (currently limited to 4 hex digits)
				addr = baseAddr;
				addr &= 0xffff;
				ch = 3;
				while (addr != 0)
				{
					line[ch] = mHexDigit[addr & 0x0f];
					ch--;
					addr >>= 4;
				}

				// output hex digits and ASCII chars
				ch = kHexOffset;
				for (i = 0; i < count; i++)
				{
					var val = data[offset + i];

					line[ch++] = mHexDigit[((int)((uint)val >> 4)) & 0x0f];
					line[ch++] = mHexDigit[val & 0x0f];
					ch++;

					if (val >= 0x20 && val < 0x7f)
					{
						line[kAscOffset + i] = (char) val;
					}
					else
					{
						line[kAscOffset + i] = '.';
					}
				}

				println(level, tag, new string(line));

				// advance to next chunk of data
				length -= count;
				offset += count;
				baseAddr += count;
			}

		}

		/// <summary>
		/// Dump the entire contents of a byte array with DEBUG priority.
		/// </summary>
		internal static void hexDump(byte[] data)
		{
			hexDump("ddms", LogLevel.DEBUG, data, 0, data.Length);
		}

		/* currently prints to stdout; could write to a log window */
		private static void println(LogLevel logLevel, string tag, string message)
		{
			if (logLevel.getPriority() >= mLevel.getPriority())
			{
				if (sLogOutput != null)
				{
					sLogOutput.printLog(logLevel, tag, message);
				}
				else
				{
					printLog(logLevel, tag, message);
				}
			}
		}

		/// <summary>
		/// Prints a log message. </summary>
		/// <param name="logLevel"> </param>
		/// <param name="tag"> </param>
		/// <param name="message"> </param>
		public static void printLog(LogLevel logLevel, string tag, string message)
		{
			Console.Write(getLogFormatString(logLevel, tag, message));
		}

		/// <summary>
		/// Formats a log message. </summary>
		/// <param name="logLevel"> </param>
		/// <param name="tag"> </param>
		/// <param name="message"> </param>
		public static string getLogFormatString(LogLevel logLevel, string tag, string message)
		{
			return string.Format("{0} {1}/{2}: {3}\n", DateTime.Now, logLevel.getPriorityLetter(), tag, message);
		}
	}



}