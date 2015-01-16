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
	/// Centralized point to provide a <seealso cref="IDebugPortProvider"/> to ddmlib.
	/// 
	/// <p/>When <seealso cref="Client"/> objects are created, they start listening for debuggers on a specific
	/// port. The default behavior is to start with <seealso cref="DdmPreferences#getDebugPortBase()"/> and
	/// increment this value for each new <code>Client</code>.
	/// 
	/// <p/>This <seealso cref="DebugPortManager"/> allows applications using ddmlib to provide a custom
	/// port provider on a per-<code>Client</code> basis, depending on the device/emulator they are
	/// running on, and/or their names.
	/// </summary>
	public class DebugPortManager
	{
        public class DebugPortProvider
        {
            public const int NO_STATIC_PORT = -1;            
        }

		/// <summary>
		/// Classes which implement this interface provide a method that provides a non random
		/// debugger port for a newly created <seealso cref="Client"/>.
		/// </summary>
		public interface IDebugPortProvider
		{

			/// <summary>
			/// Returns a non-random debugger port for the specified application running on the
			/// specified <seealso cref="Device"/>. </summary>
			/// <param name="device"> The device the application is running on. </param>
			/// <param name="appName"> The application name, as defined in the <code>AndroidManifest.xml</code>
			/// <var>package</var> attribute of the <var>manifest</var> node. </param>
			/// <returns> The non-random debugger port or <seealso cref="#NO_STATIC_PORT"/> if the <seealso cref="Client"/>
			/// should use the automatic debugger port provider. </returns>
			int getPort(IDevice device, string appName);
		}

		private static IDebugPortProvider sProvider = null;

		/// <summary>
		/// Sets the <seealso cref="IDebugPortProvider"/> that will be used when a new <seealso cref="Client"/> requests
		/// a debugger port. </summary>
		/// <param name="provider"> the <code>IDebugPortProvider</code> to use. </param>
		public static IDebugPortProvider provider
		{
			set
			{
				sProvider = value;
			}
			get
			{
				return sProvider;
			}
		}

	}

}