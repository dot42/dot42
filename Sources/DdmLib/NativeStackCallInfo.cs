using System;
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
	/// Represents a stack call. This is used to return all of the call
	/// information as one object.
	/// </summary>
	public sealed class NativeStackCallInfo
	{
		private static readonly Regex SOURCE_NAME_PATTERN = new Regex("^(.+):(\\d+)$");

	/// <summary>
		/// address of this stack frame </summary>
		private long mAddress;

	/// <summary>
		/// name of the library </summary>
		private string mLibrary;

	/// <summary>
		/// name of the method </summary>
		private string mMethod;

		/// <summary>
		/// name of the source file + line number in the format<br>
		/// &lt;sourcefile&gt;:&lt;linenumber&gt;
		/// </summary>
		private string mSourceFile;

		private int mLineNumber = -1;

		/// <summary>
		/// Basic constructor with library, method, and sourcefile information
		/// </summary>
		/// <param name="address"> address of this stack frame </param>
		/// <param name="lib"> The name of the library </param>
		/// <param name="method"> the name of the method </param>
		/// <param name="sourceFile"> the name of the source file and the line number
		/// as "[sourcefile]:[fileNumber]" </param>
		public NativeStackCallInfo(long address, string lib, string method, string sourceFile)
		{
			mAddress = address;
			mLibrary = lib;
			mMethod = method;

			var m = SOURCE_NAME_PATTERN.Match(sourceFile);
			if (m.Success)
			{
				mSourceFile = m.Groups[1].Value;
				try
				{
					mLineNumber = int.Parse(m.Groups[2].Value);
				}
				catch (Exception)
				{
					// do nothing, the line number will stay at -1
				}
			}
			else
			{
				mSourceFile = sourceFile;
			}
		}

		/// <summary>
		/// Returns the address of this stack frame.
		/// </summary>
		public long address
		{
			get
			{
				return mAddress;
			}
		}

		/// <summary>
		/// Returns the name of the library name.
		/// </summary>
		public string libraryName
		{
			get
			{
				return mLibrary;
			}
		}

		/// <summary>
		/// Returns the name of the method.
		/// </summary>
		public string methodName
		{
			get
			{
				return mMethod;
			}
		}

		/// <summary>
		/// Returns the name of the source file.
		/// </summary>
		public string sourceFile
		{
			get
			{
				return mSourceFile;
			}
		}

		/// <summary>
		/// Returns the line number, or -1 if unknown.
		/// </summary>
		public int lineNumber
		{
			get
			{
				return mLineNumber;
			}
		}

		public override string ToString()
		{
			return string.Format("\t{0:x8}\t{1} --- {2} --- {3}:{4:D}", address, libraryName, methodName, sourceFile, lineNumber);
		}
	}

}