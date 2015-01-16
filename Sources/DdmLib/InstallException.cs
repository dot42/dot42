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
	/// Thrown if installation or uninstallation of application fails.
	/// </summary>
	public class InstallException : CanceledException
	{
		public InstallException(Exception cause) : base(cause.Message, cause)
		{
		}

		public InstallException(string message, Exception cause) : base(message, cause)
		{
		}

		/// <summary>
		/// Returns true if the installation was canceled by user input. This can typically only
		/// happen in the sync phase.
		/// </summary>
		public override bool wasCanceled()
		{
			Exception cause = InnerException;
			return cause is SyncException && ((SyncException)cause).wasCanceled();
		}
	}

}