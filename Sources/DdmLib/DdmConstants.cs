/*
 * Copyright (C) 2009 The Android Open Source Project
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

using System;

namespace Dot42.DdmLib
{

	public sealed class DdmConstants
	{

		public const int PLATFORM_UNKNOWN = 0;
		public const int PLATFORM_LINUX = 1;
		public const int PLATFORM_WINDOWS = 2;
		public const int PLATFORM_DARWIN = 3;

		/// <summary>
		/// Returns current platform, one of <seealso cref="#PLATFORM_WINDOWS"/>, <seealso cref="#PLATFORM_DARWIN"/>,
		/// <seealso cref="#PLATFORM_LINUX"/> or <seealso cref="#PLATFORM_UNKNOWN"/>.
		/// </summary>
		public static readonly int CURRENT_PLATFORM = currentPlatform();

		/// <summary>
		/// Extension for Traceview files.
		/// </summary>
		public const string DOT_TRACE = ".trace";

		/// <summary>
		/// hprof-conv executable (with extension for the current OS) </summary>
		public static readonly string FN_HPROF_CONVERTER = (CURRENT_PLATFORM == PLATFORM_WINDOWS) ? "hprof-conv.exe" : "hprof-conv"; //$NON-NLS-1$ //$NON-NLS-2$

		/// <summary>
		/// traceview executable (with extension for the current OS) </summary>
		public static readonly string FN_TRACEVIEW = (CURRENT_PLATFORM == PLATFORM_WINDOWS) ? "traceview.bat" : "traceview"; //$NON-NLS-1$ //$NON-NLS-2$

		/// <summary>
		/// Returns current platform
		/// </summary>
		/// <returns> one of <seealso cref="#PLATFORM_WINDOWS"/>, <seealso cref="#PLATFORM_DARWIN"/>,
		/// <seealso cref="#PLATFORM_LINUX"/> or <seealso cref="#PLATFORM_UNKNOWN"/>. </returns>
		public static int currentPlatform()
		{
			var os = Environment.OSVersion.Platform; //$NON-NLS-1$
			if (os == PlatformID.MacOSX) //$NON-NLS-1$
			{
				return PLATFORM_DARWIN;
			} //$NON-NLS-1$
			else if (os == PlatformID.Win32NT)
			{
				return PLATFORM_WINDOWS;
			} //$NON-NLS-1$
			else if (os == PlatformID.Unix)
			{
				return PLATFORM_LINUX;
			}

			return PLATFORM_UNKNOWN;
		}

	}

}