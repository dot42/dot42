using System;
using System.Collections.Generic;

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

namespace Dot42.DdmLib.testrunner
{
    public enum TestSize
    {
        /// <summary>
        /// Run tests annotated with SmallTest </summary>
        SMALL,

        /// <summary>
        /// Run tests annotated with MediumTest </summary>
        MEDIUM,

        /// <summary>
        /// Run tests annotated with LargeTest </summary>
        LARGE
    }

    public static partial class EnumExtensionMethods
    {
        /// <summary>
        /// Return the <seealso cref="TestSize"/> corresponding to the given Android platform defined value.
        /// </summary>
        /// <exception cref="IllegalArgumentException"> if <seealso cref="TestSize"/> cannot be found. </exception>
        public static TestSize getTestSize(String value)
        {
            switch (value)
            {
                case "small":
                    return TestSize.SMALL;
                case "medium":
                    return TestSize.MEDIUM;
                case "large":
                    return TestSize.LARGE;                    
            }
            throw new ArgumentException("Unknown TestSize: " + value);
        }

        /// <summary>
        /// Convert a test size to string
        /// </summary>
        public static string getValue(this TestSize value)
        {
            switch (value)
            {
                case TestSize.SMALL:
                    return "small";
                case TestSize.MEDIUM:
                    return "medium";
                case TestSize.LARGE:
                    return "large";
                default:
                    throw new ArgumentException("Invalid TestSize " + (int)value);
            }
        }
    }

    /// <summary>
	/// Interface for running a Android test command remotely and reporting result to a listener.
	/// </summary>
	public interface IRemoteAndroidTestRunner
	{


		/// <summary>
		/// Returns the application package name.
		/// </summary>
		string packageName {get;}

		/// <summary>
		/// Returns the runnerName.
		/// </summary>
		string runnerName {get;}

		/// <summary>
		/// Sets to run only tests in this class
		/// Must be called before 'run'.
		/// </summary>
		/// <param name="className"> fully qualified class name (eg x.y.z) </param>
		string className {set;}

		/// <summary>
		/// Sets to run only tests in the provided classes
		/// Must be called before 'run'.
		/// <p>
		/// If providing more than one class, requires a InstrumentationTestRunner that supports
		/// the multiple class argument syntax.
		/// </summary>
		/// <param name="classNames"> array of fully qualified class names (eg x.y.z) </param>
		string[] classNames {set;}

		/// <summary>
		/// Sets to run only specified test method
		/// Must be called before 'run'.
		/// </summary>
		/// <param name="className"> fully qualified class name (eg x.y.z) </param>
		/// <param name="testName"> method name </param>
		void setMethodName(string className, string testName);

		/// <summary>
		/// Sets to run all tests in specified package
		/// Must be called before 'run'.
		/// </summary>
		/// <param name="packageName"> fully qualified package name (eg x.y.z) </param>
		string testPackageName {set;}

		/// <summary>
		/// Sets to run only tests of given size.
		/// Must be called before 'run'.
		/// </summary>
		/// <param name="size"> the <seealso cref="TestSize"/> to run. </param>
		TestSize testSize {set;}

		/// <summary>
		/// Adds a argument to include in instrumentation command.
		/// <p/>
		/// Must be called before 'run'. If an argument with given name has already been provided, it's
		/// value will be overridden.
		/// </summary>
		/// <param name="name"> the name of the instrumentation bundle argument </param>
		/// <param name="value"> the value of the argument </param>
		void addInstrumentationArg(string name, string value);

		/// <summary>
		/// Removes a previously added argument.
		/// </summary>
		/// <param name="name"> the name of the instrumentation bundle argument to remove </param>
		void removeInstrumentationArg(string name);

		/// <summary>
		/// Adds a boolean argument to include in instrumentation command.
		/// <p/> </summary>
		/// <seealso cref= RemoteAndroidTestRunner#addInstrumentationArg
		/// </seealso>
		/// <param name="name"> the name of the instrumentation bundle argument </param>
		/// <param name="value"> the value of the argument </param>
		void addBooleanArg(string name, bool value);

		/// <summary>
		/// Sets this test run to log only mode - skips test execution.
		/// </summary>
		bool logOnly {set;}

		/// <summary>
		/// Sets this debug mode of this test run. If true, the Android test runner will wait for a
		/// debugger to attach before proceeding with test execution.
		/// </summary>
		bool debug {set;}

		/// <summary>
		/// Sets this code coverage mode of this test run.
		/// </summary>
		bool coverage {set;}

		/// <summary>
		/// Sets the maximum time allowed between output of the shell command running the tests on
		/// the devices.
		/// <p/>
		/// This allows setting a timeout in case the tests can become stuck and never finish. This is
		/// different from the normal timeout on the connection.
		/// <p/>
		/// By default no timeout will be specified.
		/// </summary>
		/// <seealso cref= <seealso cref="IDevice#executeShellCommand(String, com.android.ddmlib.IShellOutputReceiver, int)"/> </seealso>
		int maxtimeToOutputResponse {set;}

		/// <summary>
		/// Set a custom run name to be reported to the <seealso cref="ITestRunListener"/> on <seealso cref="#run"/>
		/// <p/>
		/// If unspecified, will use package name
		/// </summary>
		/// <param name="runName"> </param>
		string runName {set;}

		/// <summary>
		/// Execute this test run.
		/// <p/>
		/// Convenience method for <seealso cref="#run(Collection)"/>.
		/// </summary>
		/// <param name="listeners"> listens for test results </param>
		/// <exception cref="TimeoutException"> in case of a timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="ShellCommandUnresponsiveException"> if the device did not output any test result for
		/// a period longer than the max time to output. </exception>
		/// <exception cref="IOException"> if connection to device was lost.
		/// </exception>
		/// <seealso cref= #setMaxtimeToOutputResponse(int) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void run(ITestRunListener... listeners) throws com.android.ddmlib.TimeoutException, com.android.ddmlib.AdbCommandRejectedException, com.android.ddmlib.ShellCommandUnresponsiveException, java.io.IOException;
		void run(params ITestRunListener[] listeners);

		/// <summary>
		/// Execute this test run.
		/// </summary>
		/// <param name="listeners"> collection of listeners for test results </param>
		/// <exception cref="TimeoutException"> in case of a timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="ShellCommandUnresponsiveException"> if the device did not output any test result for
		/// a period longer than the max time to output. </exception>
		/// <exception cref="IOException"> if connection to device was lost.
		/// </exception>
		/// <seealso cref= #setMaxtimeToOutputResponse(int) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void run(java.util.Collection<ITestRunListener> listeners) throws com.android.ddmlib.TimeoutException, com.android.ddmlib.AdbCommandRejectedException, com.android.ddmlib.ShellCommandUnresponsiveException, java.io.IOException;
		void run(ICollection<ITestRunListener> listeners);

		/// <summary>
		/// Requests cancellation of this test run.
		/// </summary>
		void cancel();

	}

}