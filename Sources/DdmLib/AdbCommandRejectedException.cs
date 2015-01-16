using System;

/*
 * Copyright (C) 2010 The Android Open Source Project
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
	/// Exception thrown when adb refuses a command.
	/// </summary>
	public class AdbCommandRejectedException : Exception
	{
		private const long serialVersionUID = 1L;
		private readonly bool mIsDeviceOffline;
		private readonly bool mErrorDuringDeviceSelection;

		internal AdbCommandRejectedException(string message) : base(message)
		{
			mIsDeviceOffline = "device offline".Equals(message);
			mErrorDuringDeviceSelection = false;
		}

		internal AdbCommandRejectedException(string message, bool errorDuringDeviceSelection) : base(message)
		{
			mErrorDuringDeviceSelection = errorDuringDeviceSelection;
			mIsDeviceOffline = "device offline".Equals(message);
		}

		/// <summary>
		/// Returns true if the error is due to the device being offline.
		/// </summary>
		public virtual bool deviceOffline
		{
			get
			{
				return mIsDeviceOffline;
			}
		}

		/// <summary>
		/// Returns whether adb refused to target a given device for the command.
		/// <p/>If false, adb refused the command itself, if true, it refused to target the given
		/// device.
		/// </summary>
		public virtual bool wasErrorDuringDeviceSelection()
		{
			return mErrorDuringDeviceSelection;
		}
	}

}