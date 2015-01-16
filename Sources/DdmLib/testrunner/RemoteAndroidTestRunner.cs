using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

namespace Dot42.DdmLib.testrunner
{
    /// <summary>
	/// Runs a Android test command remotely and reports results.
	/// </summary>
	public class RemoteAndroidTestRunner : IRemoteAndroidTestRunner
	{

		private readonly string mPackageName;
		private readonly string mRunnerName;
		private IDevice mRemoteDevice;
		// default to no timeout
		private int mMaxTimeToOutputResponse = 0;
		private string mRunName = null;

	/// <summary>
		/// map of name-value instrumentation argument pairs </summary>
		private IDictionary<string, string> mArgMap;
		private InstrumentationResultParser mParser;

		private const string LOG_TAG = "RemoteAndroidTest";
		private const string DEFAULT_RUNNER_NAME = "android.test.InstrumentationTestRunner";

		private const char CLASS_SEPARATOR = ',';
		private const char METHOD_SEPARATOR = '#';
		private const char RUNNER_SEPARATOR = '/';

		// defined instrumentation argument names
		private const string CLASS_ARG_NAME = "class";
		private const string LOG_ARG_NAME = "log";
		private const string DEBUG_ARG_NAME = "debug";
		private const string COVERAGE_ARG_NAME = "coverage";
		private const string PACKAGE_ARG_NAME = "package";
		private const string SIZE_ARG_NAME = "size";

		/// <summary>
		/// Creates a remote Android test runner.
		/// </summary>
		/// <param name="packageName"> the Android application package that contains the tests to run </param>
		/// <param name="runnerName"> the instrumentation test runner to execute. If null, will use default
		///   runner </param>
		/// <param name="remoteDevice"> the Android device to execute tests on </param>
		public RemoteAndroidTestRunner(string packageName, string runnerName, IDevice remoteDevice)
		{

			mPackageName = packageName;
			mRunnerName = runnerName;
			mRemoteDevice = remoteDevice;
			mArgMap = new Dictionary<string, string>();
		}

		/// <summary>
		/// Alternate constructor. Uses default instrumentation runner.
		/// </summary>
		/// <param name="packageName"> the Android application package that contains the tests to run </param>
		/// <param name="remoteDevice"> the Android device to execute tests on </param>
		public RemoteAndroidTestRunner(string packageName, IDevice remoteDevice) : this(packageName, null, remoteDevice)
		{
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public string packageName
		{
			get
			{
				return mPackageName;
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public string runnerName
		{
			get
			{
				if (mRunnerName == null)
				{
					return DEFAULT_RUNNER_NAME;
				}
				return mRunnerName;
			}
		}

		/// <summary>
		/// Returns the complete instrumentation component path.
		/// </summary>
		private string runnerPath
		{
			get
			{
				return packageName + RUNNER_SEPARATOR + runnerName;
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public string className
		{
			set
			{
				addInstrumentationArg(CLASS_ARG_NAME, value);
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public string[] classNames
		{
			set
			{
				StringBuilder classArgBuilder = new StringBuilder();
    
				for (int i = 0; i < value.Length; i++)
				{
					if (i != 0)
					{
						classArgBuilder.Append(CLASS_SEPARATOR);
					}
					classArgBuilder.Append(value[i]);
				}
				className = classArgBuilder.ToString();
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public void setMethodName(string className, string testName)
		{
			className = className + METHOD_SEPARATOR + testName;
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public string testPackageName
		{
			set
			{
				addInstrumentationArg(PACKAGE_ARG_NAME, value);
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public void addInstrumentationArg(string name, string value)
		{
			if (name == null || value == null)
			{
				throw new System.ArgumentException("name or value arguments cannot be null");
			}
			mArgMap.Add(name, value);
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public void removeInstrumentationArg(string name)
		{
			if (name == null)
			{
				throw new System.ArgumentException("name argument cannot be null");
			}
			mArgMap.Remove(name);
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public void addBooleanArg(string name, bool value)
		{
			addInstrumentationArg(name, Convert.ToString(value));
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public bool logOnly
		{
			set
			{
				addBooleanArg(LOG_ARG_NAME, value);
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public  bool debug
		{
			set
			{
				addBooleanArg(DEBUG_ARG_NAME, value);
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public  bool coverage
		{
			set
			{
				addBooleanArg(COVERAGE_ARG_NAME, value);
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public  TestSize testSize
		{
			set
			{
				addInstrumentationArg(SIZE_ARG_NAME, value.getValue() /*.runnerValue*/);
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public  int maxtimeToOutputResponse
		{
			set
			{
				mMaxTimeToOutputResponse = value;
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public  string runName
		{
			set
			{
				mRunName = value;
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void run(ITestRunListener... listeners) throws com.android.ddmlib.TimeoutException, com.android.ddmlib.AdbCommandRejectedException, com.android.ddmlib.ShellCommandUnresponsiveException, java.io.IOException
		public  void run(params ITestRunListener[] listeners)
		{
			run(listeners.ToList());
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void run(java.util.Collection<ITestRunListener> listeners) throws com.android.ddmlib.TimeoutException, com.android.ddmlib.AdbCommandRejectedException, com.android.ddmlib.ShellCommandUnresponsiveException, java.io.IOException
		public  void run(ICollection<ITestRunListener> listeners)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String runCaseCommandStr = String.format("am instrument -w -r %1$s %2$s", getArgsCommand(), getRunnerPath());
			string runCaseCommandStr = string.Format("am instrument -w -r {0} {1}", argsCommand, runnerPath);
			Log.i(LOG_TAG, string.Format("Running {0} on {1}", runCaseCommandStr, mRemoteDevice.serialNumber));
			string runName = mRunName == null ? mPackageName : mRunName;
			mParser = new InstrumentationResultParser(runName, listeners);

			try
			{
				mRemoteDevice.executeShellCommand(runCaseCommandStr, mParser, mMaxTimeToOutputResponse);
			}
			catch (IOException e)
			{
				Log.w(LOG_TAG, string.Format("IOException {0} when running tests {1} on {2}", e.ToString(), packageName, mRemoteDevice.serialNumber));
				// rely on parser to communicate results to listeners
				mParser.handleTestRunFailed(e.ToString());
				throw e;
			}
			catch (ShellCommandUnresponsiveException e)
			{
				Log.w(LOG_TAG, string.Format("ShellCommandUnresponsiveException {0} when running tests {1} on {2}", e.ToString(), packageName, mRemoteDevice.serialNumber));
				mParser.handleTestRunFailed(string.Format("Failed to receive adb shell test output within {0:D} ms. " + "Test may have timed out, or adb connection to device became unresponsive", mMaxTimeToOutputResponse));
				throw e;
			}
			catch (TimeoutException e)
			{
				Log.w(LOG_TAG, string.Format("TimeoutException when running tests {0} on {1}", packageName, mRemoteDevice.serialNumber));
				mParser.handleTestRunFailed(e.ToString());
				throw e;
			}
			catch (AdbCommandRejectedException e)
			{
				Log.w(LOG_TAG, string.Format("AdbCommandRejectedException {0} when running tests {1} on {2}", e.ToString(), packageName, mRemoteDevice.serialNumber));
				mParser.handleTestRunFailed(e.ToString());
				throw e;
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public  void cancel()
		{
			if (mParser != null)
			{
				mParser.cancel();
			}
		}

		/// <summary>
		/// Returns the full instrumentation command line syntax for the provided instrumentation
		/// arguments.
		/// Returns an empty string if no arguments were specified.
		/// </summary>
		private string argsCommand
		{
			get
			{
				StringBuilder commandBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> argPair in mArgMap)
				{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final String argCmd = String.format(" -e %1$s %2$s", argPair.getKey(), argPair.getValue());
					string argCmd = string.Format(" -e {0} {1}", argPair.Key, argPair.Value);
					commandBuilder.Append(argCmd);
				}
				return commandBuilder.ToString();
			}
		}
	}

}