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
	/// Abstract exception for exception that can be thrown when a user input cancels the action.
	/// <p/>
	/// <seealso cref="#wasCanceled()"/> returns whether the action was canceled because of user input.
	/// 
	/// </summary>
	public abstract class CanceledException : Exception
	{
		private const long serialVersionUID = 1L;

		internal CanceledException(string message) : base(message)
		{
		}

		internal CanceledException(string message, Exception cause) : base(message, cause)
		{
		}

		/// <summary>
		/// Returns true if the action was canceled by user input.
		/// </summary>
		public abstract bool wasCanceled();
	}

}