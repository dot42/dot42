using System.Collections.Generic;

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
    ///<summary>
    /// Types of test failures.
    ///</summary>
    public enum TestFailure
    {
        /// <summary>
        /// Test failed due to unanticipated uncaught exception. </summary>
        ERROR,

        /// <summary>
        /// Test failed due to a false assertion. </summary>
        FAILURE
    }



    /// <summary>
    /// Receives event notifications during instrumentation test runs.
    /// <p/>
    /// Patterned after <seealso cref="junit.runner.TestRunListener"/>.
    /// <p/>
    /// The sequence of calls will be:
    /// <ul>
    /// <li> testRunStarted
    /// <li> testStarted
    /// <li> [testFailed]
    /// <li> testEnded
    /// <li> ....
    /// <li> [testRunFailed]
    /// <li> testRunEnded
    /// </ul>
    /// </summary>
    public interface ITestRunListener
    {

        /// <summary>
        /// Reports the start of a test run.
        /// </summary>
        /// <param name="runName"> the test run name </param>
        /// <param name="testCount"> total number of tests in test run </param>
        void testRunStarted(string runName, int testCount);

        /// <summary>
        /// Reports the start of an individual test case.
        /// </summary>
        /// <param name="test"> identifies the test </param>
        void testStarted(TestIdentifier test);

        /// <summary>
        /// Reports the failure of a individual test case.
        /// <p/>
        /// Will be called between testStarted and testEnded.
        /// </summary>
        /// <param name="status"> failure type </param>
        /// <param name="test"> identifies the test </param>
        /// <param name="trace"> stack trace of failure </param>
        void testFailed(TestFailure status, TestIdentifier test, string trace);

        /// <summary>
        /// Reports the execution end of an individual test case.
        /// <p/>
        /// If <seealso cref="#testFailed"/> was not invoked, this test passed.  Also returns any key/value
        /// metrics which may have been emitted during the test case's execution.
        /// </summary>
        /// <param name="test"> identifies the test </param>
        /// <param name="testMetrics"> a <seealso cref="Map"/> of the metrics emitted </param>
        void testEnded(TestIdentifier test, IDictionary<string, string> testMetrics);

        /// <summary>
        /// Reports test run failed to complete due to a fatal error.
        /// </summary>
        /// <param name="errorMessage"> <seealso cref="string"/> describing reason for run failure. </param>
        void testRunFailed(string errorMessage);

        /// <summary>
        /// Reports test run stopped before completion due to a user request.
        /// <p/>
        /// TODO: currently unused, consider removing
        /// </summary>
        /// <param name="elapsedTime"> device reported elapsed time, in milliseconds </param>
        void testRunStopped(long elapsedTime);

        /// <summary>
        /// Reports end of test run.
        /// </summary>
        /// <param name="elapsedTime"> device reported elapsed time, in milliseconds </param>
        /// <param name="runMetrics"> key-value pairs reported at the end of a test run </param>
        void testRunEnded(long elapsedTime, IDictionary<string, string> runMetrics);
    }

}