using System;

/*
 * Copyright (C) 2008 The Android Open Source Project
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

namespace Dot42.DdmLib.log
{


	/// <summary>
	/// Exception thrown when accessing an <seealso cref="EventContainer"/> value with the wrong type.
	/// </summary>
	public sealed class InvalidTypeException : Exception
	{

		/// <summary>
		/// Needed by <seealso cref="Serializable"/>.
		/// </summary>
		private const long serialVersionUID = 1L;

		/// <summary>
		/// Constructs a new exception with the default detail message. </summary>
		/// <seealso cref= java.lang.Exception </seealso>
		public InvalidTypeException() : base("Invalid Type")
		{
		}

		/// <summary>
		/// Constructs a new exception with the specified detail message. </summary>
		/// <param name="message"> the detail message. The detail message is saved for later retrieval
		/// by the <seealso cref="Throwable#getMessage()"/> method. </param>
		/// <seealso cref= java.lang.Exception </seealso>
		public InvalidTypeException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructs a new exception with the specified cause and a detail message of
		/// <code>(cause==null ? null : cause.toString())</code> (which typically contains
		/// the class and detail message of cause). </summary>
		/// <param name="cause"> the cause (which is saved for later retrieval by the
		/// <seealso cref="Throwable#getCause()"/> method). (A <code>null</code> value is permitted,
		/// and indicates that the cause is nonexistent or unknown.) </param>
		/// <seealso cref= java.lang.Exception </seealso>
		public InvalidTypeException(Exception cause) : base(cause.Message, cause)
		{
		}

		/// <summary>
		/// Constructs a new exception with the specified detail message and cause. </summary>
		/// <param name="message"> the detail message. The detail message is saved for later retrieval
		/// by the <seealso cref="Throwable#getMessage()"/> method. </param>
		/// <param name="cause"> the cause (which is saved for later retrieval by the
		/// <seealso cref="Throwable#getCause()"/> method). (A <code>null</code> value is permitted,
		/// and indicates that the cause is nonexistent or unknown.) </param>
		/// <seealso cref= java.lang.Exception </seealso>
		public InvalidTypeException(string message, Exception cause) : base(message, cause)
		{
		}
	}

}