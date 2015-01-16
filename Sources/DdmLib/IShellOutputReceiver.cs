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
	/// Classes which implement this interface provide methods that deal with out from a remote shell
	/// command on a device/emulator.
	/// </summary>
	public interface IShellOutputReceiver
	{
		/// <summary>
		/// Called every time some new data is available. </summary>
		/// <param name="data"> The new data. </param>
		/// <param name="offset"> The offset at which the new data starts. </param>
		/// <param name="length"> The length of the new data. </param>
		void addOutput(byte[] data, int offset, int length);

		/// <summary>
		/// Called at the end of the process execution (unless the process was
		/// canceled). This allows the receiver to terminate and flush whatever
		/// data was not yet processed.
		/// </summary>
		void flush();

		/// <summary>
		/// Cancel method to stop the execution of the remote shell command. </summary>
		/// <returns> true to cancel the execution of the command. </returns>
		bool cancelled {get;}
	}

}