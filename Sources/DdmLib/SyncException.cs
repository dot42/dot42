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
	/// Exception thrown when a transfer using <seealso cref="SyncService"/> doesn't complete.
	/// <p/>This is different from an <seealso cref="IOException"/> because it's not the underlying connection
	/// that triggered the error, but the adb transfer protocol that didn't work somehow, or that the
	/// targets (local and/or remote) were wrong.
	/// </summary>
	public class SyncException : CanceledException
	{
		private const long serialVersionUID = 1L;

		public enum SyncError
		{
	/// <summary>
			/// canceled transfer </summary>
			CANCELED,
	/// <summary>
			/// Transfer error </summary>
			TRANSFER_PROTOCOL_ERROR,
	/// <summary>
			/// unknown remote object during a pull </summary>
			NO_REMOTE_OBJECT,
	/// <summary>
			/// Result code when attempting to pull multiple files into a file </summary>
			TARGET_IS_FILE,
	/// <summary>
			/// Result code when attempting to pull multiple into a directory that does not exist. </summary>
			NO_DIR_TARGET,
	/// <summary>
			/// wrong encoding on the remote path. </summary>
			REMOTE_PATH_ENCODING,
	/// <summary>
			/// remote path that is too long. </summary>
			REMOTE_PATH_LENGTH,
	/// <summary>
			/// error while reading local file. </summary>
			FILE_READ_ERROR,
	/// <summary>
			/// error while writing local file. </summary>
			FILE_WRITE_ERROR,
	/// <summary>
			/// attempting to push a directory. </summary>
			LOCAL_IS_DIRECTORY,
	/// <summary>
			/// attempting to push a non-existent file. </summary>
			NO_LOCAL_FILE,
	/// <summary>
			/// when the target path of a multi file push is a file. </summary>
			REMOTE_IS_FILE,
	/// <summary>
			/// receiving too much data from the remove device at once </summary>
			BUFFER_OVERRUN
		}

	    private readonly SyncError mError;

		public SyncException(SyncError error) : base(error.getMessage())
		{
			mError = error;
		}

		public SyncException(SyncError error, string message) : base(message)
		{
			mError = error;
		}

		public SyncException(SyncError error, Exception cause) : base(error.getMessage(), cause)
		{
			mError = error;
		}

		public virtual SyncError errorCode
		{
			get
			{
				return mError;
			}
		}

		/// <summary>
		/// Returns true if the sync was canceled by user input.
		/// </summary>
	   public override bool wasCanceled()
	   {
			return mError == SyncError.CANCELED;
	   }
	}

    public static partial class EnumExtensionMethods
    {
        public static string getMessage(this SyncException.SyncError error)
        {
            switch (error)
            {
                case SyncException.SyncError.CANCELED:
                    return "Operation was canceled by the user.";
                case SyncException.SyncError.TRANSFER_PROTOCOL_ERROR:
                    return "Adb Transfer Protocol Error.";
                case SyncException.SyncError.NO_REMOTE_OBJECT:
                    return ("Remote object doesn't exist!");
                case SyncException.SyncError.TARGET_IS_FILE:
                    return ("Target object is a file.");
                case SyncException.SyncError.NO_DIR_TARGET:
                    return ("Target directory doesn't exist.");
                case SyncException.SyncError.REMOTE_PATH_ENCODING:
                    return ("Remote Path encoding is not supported.");
                case SyncException.SyncError.REMOTE_PATH_LENGTH:
                    return ("Remote path is too long.");
                case SyncException.SyncError.FILE_READ_ERROR:
                    return ("Reading local file failed!");
                case SyncException.SyncError.FILE_WRITE_ERROR:
                    return ("Writing local file failed!");
                case SyncException.SyncError.LOCAL_IS_DIRECTORY:
                    return ("Local path is a directory.");
                case SyncException.SyncError.NO_LOCAL_FILE:
                    return ("Local path doesn't exist.");
                case SyncException.SyncError.REMOTE_IS_FILE:
                    return ("Remote path is a file.");
                case SyncException.SyncError.BUFFER_OVERRUN:
                    return ("Receiving too much data.");                    
            }
            return null;
        }
    }
}