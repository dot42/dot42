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
	/// Preferences for the ddm library.
	/// <p/>This class does not handle storing the preferences. It is merely a central point for
	/// applications using the ddmlib to override the default values.
	/// <p/>Various components of the ddmlib query this class to get their values.
	/// <p/>Calls to some <code>set##()</code> methods will update the components using the values
	/// right away, while other methods will have no effect once <seealso cref="AndroidDebugBridge#init(boolean)"/>
	/// has been called.
	/// <p/>Check the documentation of each method.
	/// </summary>
	public sealed class DdmPreferences
	{

	/// <summary>
		/// Default value for thread update flag upon client connection. </summary>
		public const bool DEFAULT_INITIAL_THREAD_UPDATE = false;
	/// <summary>
		/// Default value for heap update flag upon client connection. </summary>
		public const bool DEFAULT_INITIAL_HEAP_UPDATE = false;
	/// <summary>
		/// Default value for the selected client debug port </summary>
		public const int DEFAULT_SELECTED_DEBUG_PORT = 8700;
	/// <summary>
		/// Default value for the debug port base </summary>
		public const int DEFAULT_DEBUG_PORT_BASE = 8600;
	/// <summary>
		/// Default value for the logcat <seealso cref="Log.LogLevel"/> </summary>
		public static readonly Log.LogLevel DEFAULT_LOG_LEVEL = Log.LogLevel.ERROR;
	/// <summary>
		/// Default timeout values for adb connection (milliseconds) </summary>
		public const int DEFAULT_TIMEOUT = 5000; // standard delay, in ms
	/// <summary>
		/// Default profiler buffer size (megabytes) </summary>
		public const int DEFAULT_PROFILER_BUFFER_SIZE_MB = 8;
	/// <summary>
		/// Default values for the use of the ADBHOST environment variable. </summary>
		public const bool DEFAULT_USE_ADBHOST = false;
		public const string DEFAULT_ADBHOST_VALUE = "127.0.0.1";

		private static bool sThreadUpdate = DEFAULT_INITIAL_THREAD_UPDATE;
		private static bool sInitialHeapUpdate = DEFAULT_INITIAL_HEAP_UPDATE;

		private static int sSelectedDebugPort = DEFAULT_SELECTED_DEBUG_PORT;
		private static int sDebugPortBase = DEFAULT_DEBUG_PORT_BASE;
		private static Log.LogLevel sLogLevel = DEFAULT_LOG_LEVEL;
		private static int sTimeOut = DEFAULT_TIMEOUT;
		private static int sProfilerBufferSizeMb = DEFAULT_PROFILER_BUFFER_SIZE_MB;

		private static bool sUseAdbHost = DEFAULT_USE_ADBHOST;
		private static string sAdbHostValue = DEFAULT_ADBHOST_VALUE;

		/// <summary>
		/// Returns the initial <seealso cref="Client"/> flag for thread updates. </summary>
		/// <seealso cref= #setInitialThreadUpdate(boolean) </seealso>
		public static bool initialThreadUpdate
		{
			get
			{
				return sThreadUpdate;
			}
			set
			{
				sThreadUpdate = value;
			}
		}


		/// <summary>
		/// Returns the initial <seealso cref="Client"/> flag for heap updates. </summary>
		/// <seealso cref= #setInitialHeapUpdate(boolean) </seealso>
		public static bool initialHeapUpdate
		{
			get
			{
				return sInitialHeapUpdate;
			}
			set
			{
				sInitialHeapUpdate = value;
			}
		}


		/// <summary>
		/// Returns the debug port used by the selected <seealso cref="Client"/>.
		/// </summary>
		public static int selectedDebugPort
		{
			get
			{
				return sSelectedDebugPort;
			}
			set
			{
				sSelectedDebugPort = value;
    
				MonitorThread monitorThread = MonitorThread.instance;
				if (monitorThread != null)
				{
					monitorThread.debugSelectedPort = value;
				}
			}
		}


		/// <summary>
		/// Returns the debug port used by the first <seealso cref="Client"/>. Following clients, will use the
		/// next port.
		/// </summary>
		public static int debugPortBase
		{
			get
			{
				return sDebugPortBase;
			}
			set
			{
				sDebugPortBase = value;
			}
		}


		/// <summary>
		/// Returns the minimum <seealso cref="Log.LogLevel"/> being displayed.
		/// </summary>
		public static Log.LogLevel logLevel
		{
			get
			{
				return sLogLevel;
			}
			set
			{
				sLogLevel = value;    
				Log.level = sLogLevel;
			}
		}


		/// <summary>
		/// Returns the timeout to be used in adb connections (milliseconds).
		/// </summary>
		public static int timeOut
		{
			get
			{
				return sTimeOut;
			}
			set
			{
				sTimeOut = value;
			}
		}


		/// <summary>
		/// Returns the profiler buffer size (megabytes).
		/// </summary>
		public static int profilerBufferSizeMb
		{
			get
			{
				return sProfilerBufferSizeMb;
			}
			set
			{
				sProfilerBufferSizeMb = value;
			}
		}


		/// <summary>
		/// Returns a boolean indicating that the user uses or not the variable ADBHOST.
		/// </summary>
		public static bool useAdbHost
		{
			get
			{
				return sUseAdbHost;
			}
			set
			{
				sUseAdbHost = value;
			}
		}


		/// <summary>
		/// Returns the value of the ADBHOST variable set by the user.
		/// </summary>
		public static string adbHostValue
		{
			get
			{
				return sAdbHostValue;
			}
			set
			{
				sAdbHostValue = value;
			}
		}


		/// <summary>
		/// Non accessible constructor.
		/// </summary>
		private DdmPreferences()
		{
			// pass, only static methods in the class.
		}
	}

}