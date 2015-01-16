using System;
using System.Text;

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
	/// A <seealso cref="IShellOutputReceiver"/> which collects the whole shell output into one
	/// <seealso cref="string"/>.
	/// </summary>
	public class CollectingOutputReceiver : IShellOutputReceiver
	{

		private StringBuilder mOutputBuffer = new StringBuilder();
		private bool mIsCanceled = false;

		public virtual string output
		{
			get
			{
				return mOutputBuffer.ToString();
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public  bool cancelled
		{
			get
			{
				return mIsCanceled;
			}
		}

		/// <summary>
		/// Cancel the output collection
		/// </summary>
		public virtual void cancel()
		{
			mIsCanceled = true;
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public void addOutput(byte[] data, int offset, int length)
		{
			if (!cancelled)
			{
				string s = null;
				try
				{
				    s = Encoding.UTF8.GetString(data, offset, length);
				}
				catch (Exception)
				{
					// normal encoding didn't work, try the default one
                    s = Encoding.Default.GetString(data, offset, length);
                }
				mOutputBuffer.Append(s);
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public void flush()
		{
			// ignore
		}
	}

}