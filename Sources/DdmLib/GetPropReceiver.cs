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

using System.Text.RegularExpressions;

namespace Dot42.DdmLib
{


	/// <summary>
	/// A receiver able to parse the result of the execution of
	/// <seealso cref="#GETPROP_COMMAND"/> on a device.
	/// </summary>
	internal sealed class GetPropReceiver : MultiLineReceiver
	{
		internal const string GETPROP_COMMAND = "getprop"; //$NON-NLS-1$

		private static readonly Regex GETPROP_PATTERN = new Regex("^\\[([^]]+)\\]\\:\\s*\\[(.*)\\]$"); //$NON-NLS-1$

	/// <summary>
		/// indicates if we need to read the first </summary>
		private Device mDevice = null;

		/// <summary>
		/// Creates the receiver with the device the receiver will modify. </summary>
		/// <param name="device"> The device to modify </param>
		public GetPropReceiver(Device device)
		{
			mDevice = device;
		}

		public override void processNewLines(string[] lines)
		{
			// We receive an array of lines. We're expecting
			// to have the build info in the first line, and the build
			// date in the 2nd line. There seems to be an empty line
			// after all that.

			foreach (string line in lines)
			{
				if (line.Length == 0 || line.StartsWith("#"))
				{
					continue;
				}

				var m = GETPROP_PATTERN.Match(line);
				if (m.Success)
				{
					string label = m.Groups[0].Value;
					string value = m.Groups[1].Value;

					if (label.Length > 0)
					{
						mDevice.addProperty(label, value);
					}
				}
			}
		}

		public override bool cancelled
		{
			get
			{
				return false;
			}
		}

		public override void done()
		{
			mDevice.update(DeviceConstants.CHANGE_BUILD_INFO);
		}
	}

}