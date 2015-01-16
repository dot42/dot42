using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Dot42.DdmLib.support;

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
    /// Parses the 'raw output mode' results of an instrumentation test run from shell and informs a
    /// ITestRunListener of the results.
    /// 
    /// <p>Expects the following output:
    /// 
    /// <p>If fatal error occurred when attempted to run the tests:
    /// <pre>
    /// INSTRUMENTATION_STATUS: Error=error Message
    /// INSTRUMENTATION_FAILED:
    /// </pre>
    /// <p>or
    /// <pre>
    /// INSTRUMENTATION_RESULT: shortMsg=error Message
    /// </pre>
    /// 
    /// <p>Otherwise, expect a series of test results, each one containing a set of status key/value
    /// pairs, delimited by a start(1)/pass(0)/fail(-2)/error(-1) status code result. At end of test
    /// run, expects that the elapsed test time in seconds will be displayed
    /// 
    /// <p>For example:
    /// <pre>
    /// INSTRUMENTATION_STATUS_CODE: 1
    /// INSTRUMENTATION_STATUS: class=com.foo.FooTest
    /// INSTRUMENTATION_STATUS: test=testFoo
    /// INSTRUMENTATION_STATUS: numtests=2
    /// INSTRUMENTATION_STATUS: stack=com.foo.FooTest#testFoo:312
    ///    com.foo.X
    /// INSTRUMENTATION_STATUS_CODE: -2
    /// ...
    /// 
    /// Time: X
    /// </pre>
    /// <p>Note that the "value" portion of the key-value pair may wrap over several text lines
    /// </summary>
    public class InstrumentationResultParser : MultiLineReceiver
    {

        /// <summary>
        /// Relevant test status keys. </summary>
        private class StatusKeys
        {
            internal const string TEST = "test";
            internal const string CLASS = "class";
            internal const string STACK = "stack";
            internal const string NUMTESTS = "numtests";
            internal const string ERROR = "Error";
            internal const string SHORTMSG = "shortMsg";
        }

        /// <summary>
        /// The set of expected status keys. Used to filter which keys should be stored as metrics </summary>
        private static readonly ISet<string> KNOWN_KEYS = new HashSet<string>();
        static InstrumentationResultParser()
        {
            KNOWN_KEYS.Add(StatusKeys.TEST);
            KNOWN_KEYS.Add(StatusKeys.CLASS);
            KNOWN_KEYS.Add(StatusKeys.STACK);
            KNOWN_KEYS.Add(StatusKeys.NUMTESTS);
            KNOWN_KEYS.Add(StatusKeys.ERROR);
            KNOWN_KEYS.Add(StatusKeys.SHORTMSG);
            // unused, but regularly occurring status keys.
            KNOWN_KEYS.Add("stream");
            KNOWN_KEYS.Add("id");
            KNOWN_KEYS.Add("current");
        }

        /// <summary>
        /// Test result status codes. </summary>
        private class StatusCodes
        {
            internal const int FAILURE = -2;
            internal const int START = 1;
            internal const int ERROR = -1;
            internal const int OK = 0;
            internal const int IN_PROGRESS = 2;
        }

        /// <summary>
        /// Prefixes used to identify output. </summary>
        private class Prefixes
        {
            internal const string STATUS = "INSTRUMENTATION_STATUS: ";
            internal const string STATUS_CODE = "INSTRUMENTATION_STATUS_CODE: ";
            internal const string STATUS_FAILED = "INSTRUMENTATION_FAILED: ";
            internal const string CODE = "INSTRUMENTATION_CODE: ";
            internal const string RESULT = "INSTRUMENTATION_RESULT: ";
            internal const string TIME_REPORT = "Time: ";
        }

        private readonly ICollection<ITestRunListener> mTestListeners;

        /// <summary>
        /// Test result data
        /// </summary>
        private class TestResult
        {
            internal int? mCode = null;
            internal string mTestName = null;
            internal string mTestClass = null;
            internal string mStackTrace = null;
            internal int? mNumTests = null;

            /// <summary>
            /// Returns true if all expected values have been parsed </summary>
            internal virtual bool complete
            {
                get
                {
                    return mCode != null && mTestName != null && mTestClass != null;
                }
            }

            /// <summary>
            /// Provides a more user readable string for TestResult, if possible </summary>
            public override string ToString()
            {
                StringBuilder output = new StringBuilder();
                if (mTestClass != null)
                {
                    output.Append(mTestClass);
                    output.Append('#');
                }
                if (mTestName != null)
                {
                    output.Append(mTestName);
                }
                if (output.Length > 0)
                {
                    return output.ToString();
                }
                return "unknown result";
            }
        }

        /// <summary>
        /// the name to provide to <seealso cref="ITestRunListener#testRunStarted(String, int)"/> </summary>
        private readonly string mTestRunName;

        /// <summary>
        /// Stores the status values for the test result currently being parsed </summary>
        private TestResult mCurrentTestResult = null;

        /// <summary>
        /// Stores the status values for the test result last parsed </summary>
        private TestResult mLastTestResult = null;

        /// <summary>
        /// Stores the current "key" portion of the status key-value being parsed. </summary>
        private string mCurrentKey = null;

        /// <summary>
        /// Stores the current "value" portion of the status key-value being parsed. </summary>
        private StringBuilder mCurrentValue = null;

        /// <summary>
        /// True if start of test has already been reported to listener. </summary>
        private bool mTestStartReported = false;

        /// <summary>
        /// True if the completion of the test run has been detected. </summary>
        private bool mTestRunFinished = false;

        /// <summary>
        /// True if test run failure has already been reported to listener. </summary>
        private bool mTestRunFailReported = false;

        /// <summary>
        /// The elapsed time of the test run, in milliseconds. </summary>
        private long mTestTime = 0;

        /// <summary>
        /// True if current test run has been canceled by user. </summary>
        private bool mIsCancelled = false;

        /// <summary>
        /// The number of tests currently run </summary>
        private int mNumTestsRun = 0;

        /// <summary>
        /// The number of tests expected to run </summary>
        private int mNumTestsExpected = 0;

        /// <summary>
        /// True if the parser is parsing a line beginning with "INSTRUMENTATION_RESULT" </summary>
        private bool mInInstrumentationResultKey = false;

        /// <summary>
        /// Stores key-value pairs under INSTRUMENTATION_RESULT header, these are printed at the
        /// end of a test run, if applicable
        /// </summary>
        private IDictionary<string, string> mInstrumentationResultBundle = new Dictionary<string, string>();

        /// <summary>
        /// Stores key-value pairs of metrics emitted during the execution of each test case.  Note that
        /// standard keys that are stored in the TestResults class are filtered out of this Map.
        /// </summary>
        private IDictionary<string, string> mTestMetrics = new Dictionary<string, string>();

        private const string LOG_TAG = "InstrumentationResultParser";

        /// <summary>
        /// Error message supplied when no parseable test results are received from test run. </summary>
        internal const string NO_TEST_RESULTS_MSG = "No test results";

        /// <summary>
        /// Error message supplied when a test start bundle is parsed, but not the test end bundle. </summary>
        internal const string INCOMPLETE_TEST_ERR_MSG_PREFIX = "Test failed to run to completion";
        internal const string INCOMPLETE_TEST_ERR_MSG_POSTFIX = "Check device logcat for details";

        /// <summary>
        /// Error message supplied when the test run is incomplete. </summary>
        internal const string INCOMPLETE_RUN_ERR_MSG_PREFIX = "Test run failed to complete";

        /// <summary>
        /// Creates the InstrumentationResultParser.
        /// </summary>
        /// <param name="runName"> the test run name to provide to
        ///            <seealso cref="ITestRunListener#testRunStarted(String, int)"/> </param>
        /// <param name="listeners"> informed of test results as the tests are executing </param>
        public InstrumentationResultParser(string runName, ICollection<ITestRunListener> listeners)
        {
            mTestRunName = runName;
            mTestListeners = new List<ITestRunListener>(listeners);
        }

        /// <summary>
        /// Creates the InstrumentationResultParser for a single listener.
        /// </summary>
        /// <param name="runName"> the test run name to provide to
        ///            <seealso cref="ITestRunListener#testRunStarted(String, int)"/> </param>
        /// <param name="listener"> informed of test results as the tests are executing </param>
        public InstrumentationResultParser(string runName, ITestRunListener listener)
        {
            mTestRunName = runName;
            mTestListeners = new List<ITestRunListener>(1);
            mTestListeners.Add(listener);
        }

        /// <summary>
        /// Processes the instrumentation test output from shell.
        /// </summary>
        /// <seealso cref= MultiLineReceiver#processNewLines </seealso>
        public override void processNewLines(string[] lines)
        {
            foreach (string line in lines)
            {
                parse(line);
                // in verbose mode, dump all adb output to log
                Log.v(LOG_TAG, line);
            }
        }

        /// <summary>
        /// Parse an individual output line. Expects a line that is one of:
        /// <ul>
        /// <li>
        /// The start of a new status line (starts with Prefixes.STATUS or Prefixes.STATUS_CODE),
        /// and thus there is a new key=value pair to parse, and the previous key-value pair is
        /// finished.
        /// </li>
        /// <li>
        /// A continuation of the previous status (the "value" portion of the key has wrapped
        /// to the next line).
        /// </li>
        /// <li> A line reporting a fatal error in the test run (Prefixes.STATUS_FAILED) </li>
        /// <li> A line reporting the total elapsed time of the test run. (Prefixes.TIME_REPORT) </li>
        /// </ul>
        /// </summary>
        /// <param name="line">  Text output line </param>
        private void parse(string line)
        {
            if (line.StartsWith(Prefixes.STATUS_CODE))
            {
                // Previous status key-value has been collected. Store it.
                submitCurrentKeyValue();
                mInInstrumentationResultKey = false;
                parseStatusCode(line);
            }
            else if (line.StartsWith(Prefixes.STATUS))
            {
                // Previous status key-value has been collected. Store it.
                submitCurrentKeyValue();
                mInInstrumentationResultKey = false;
                parseKey(line, Prefixes.STATUS.Length);
            }
            else if (line.StartsWith(Prefixes.RESULT))
            {
                // Previous status key-value has been collected. Store it.
                submitCurrentKeyValue();
                mInInstrumentationResultKey = true;
                parseKey(line, Prefixes.RESULT.Length);
            }
            else if (line.StartsWith(Prefixes.STATUS_FAILED) || line.StartsWith(Prefixes.CODE))
            {
                // Previous status key-value has been collected. Store it.
                submitCurrentKeyValue();
                mInInstrumentationResultKey = false;
                // these codes signal the end of the instrumentation run
                mTestRunFinished = true;
                // just ignore the remaining data on this line
            }
            else if (line.StartsWith(Prefixes.TIME_REPORT))
            {
                parseTime(line);
            }
            else
            {
                if (mCurrentValue != null)
                {
                    // this is a value that has wrapped to next line.
                    mCurrentValue.Append("\r\n");
                    mCurrentValue.Append(line);
                }
                else if (line.Trim().Length > 0)
                {
                    Log.d(LOG_TAG, "unrecognized line " + line);
                }
            }
        }

        /// <summary>
        /// Stores the currently parsed key-value pair in the appropriate place.
        /// </summary>
        private void submitCurrentKeyValue()
        {
            if (mCurrentKey != null && mCurrentValue != null)
            {
                string statusValue = mCurrentValue.ToString();
                if (mInInstrumentationResultKey)
                {
                    if (!KNOWN_KEYS.Contains(mCurrentKey))
                    {
                        mInstrumentationResultBundle.Add(mCurrentKey, statusValue);
                    }
                    else if (mCurrentKey.Equals(StatusKeys.SHORTMSG))
                    {
                        // test run must have failed
                        handleTestRunFailed(string.Format("Instrumentation run failed due to '{0}'", statusValue));
                    }
                }
                else
                {
                    TestResult testInfo = currentTestInfo;

                    if (mCurrentKey.Equals(StatusKeys.CLASS))
                    {
                        testInfo.mTestClass = statusValue.Trim();
                    }
                    else if (mCurrentKey.Equals(StatusKeys.TEST))
                    {
                        testInfo.mTestName = statusValue.Trim();
                    }
                    else if (mCurrentKey.Equals(StatusKeys.NUMTESTS))
                    {
                        try
                        {
                            testInfo.mNumTests = Convert.ToInt32(statusValue);
                        }
                        catch (SystemException)
                        {
                            Log.w(LOG_TAG, "Unexpected integer number of tests, received " + statusValue);
                        }
                    }
                    else if (mCurrentKey.Equals(StatusKeys.ERROR))
                    {
                        // test run must have failed
                        handleTestRunFailed(statusValue);
                    }
                    else if (mCurrentKey.Equals(StatusKeys.STACK))
                    {
                        testInfo.mStackTrace = statusValue;
                    }
                    else if (!KNOWN_KEYS.Contains(mCurrentKey))
                    {
                        // Not one of the recognized key/value pairs, so dump it in mTestMetrics
                        mTestMetrics.Add(mCurrentKey, statusValue);
                    }
                }

                mCurrentKey = null;
                mCurrentValue = null;
            }
        }

        /// <summary>
        /// A utility method to return the test metrics from the current test case execution and get
        /// ready for the next one.
        /// </summary>
        private IDictionary<string, string> andResetTestMetrics
        {
            get
            {
                IDictionary<string, string> retVal = mTestMetrics;
                mTestMetrics = new Dictionary<string, string>();
                return retVal;
            }
        }

        private TestResult currentTestInfo
        {
            get
            {
                if (mCurrentTestResult == null)
                {
                    mCurrentTestResult = new TestResult();
                }
                return mCurrentTestResult;
            }
        }

        private void clearCurrentTestInfo()
        {
            mLastTestResult = mCurrentTestResult;
            mCurrentTestResult = null;
        }

        /// <summary>
        /// Parses the key from the current line.
        /// Expects format of "key=value".
        /// </summary>
        /// <param name="line"> full line of text to parse </param>
        /// <param name="keyStartPos"> the starting position of the key in the given line </param>
        private void parseKey(string line, int keyStartPos)
        {
            int endKeyPos = line.IndexOf('=', keyStartPos);
            if (endKeyPos != -1)
            {
                mCurrentKey = line.Substring(keyStartPos, endKeyPos - keyStartPos).Trim();
                parseValue(line, endKeyPos + 1);
            }
        }

        /// <summary>
        /// Parses the start of a key=value pair.
        /// </summary>
        /// <param name="line"> - full line of text to parse </param>
        /// <param name="valueStartPos"> - the starting position of the value in the given line </param>
        private void parseValue(string line, int valueStartPos)
        {
            mCurrentValue = new StringBuilder();
            mCurrentValue.Append(line.Substring(valueStartPos));
        }

        /// <summary>
        /// Parses out a status code result.
        /// </summary>
        private void parseStatusCode(string line)
        {
            string value = line.Substring(Prefixes.STATUS_CODE.Length).Trim();
            TestResult testInfo = currentTestInfo;
            testInfo.mCode = StatusCodes.ERROR;
            try
            {
                testInfo.mCode = Convert.ToInt32(value);
            }
            catch (SystemException)
            {
                Log.w(LOG_TAG, "Expected integer status code, received: " + value);
                testInfo.mCode = StatusCodes.ERROR;
            }
            if (testInfo.mCode != StatusCodes.IN_PROGRESS)
            {
                // this means we're done with current test result bundle
                reportResult(testInfo);
                clearCurrentTestInfo();
            }
        }

        /// <summary>
        /// Returns true if test run canceled.
        /// </summary>
        /// <seealso cref= IShellOutputReceiver#isCancelled() </seealso>
        public override bool cancelled
        {
            get
            {
                return mIsCancelled;
            }
        }

        /// <summary>
        /// Requests cancellation of test run.
        /// </summary>
        public virtual void cancel()
        {
            mIsCancelled = true;
        }

        /// <summary>
        /// Reports a test result to the test run listener. Must be called when a individual test
        /// result has been fully parsed.
        /// </summary>
        /// <param name="statusMap"> key-value status pairs of test result </param>
        private void reportResult(TestResult testInfo)
        {
            if (!testInfo.complete)
            {
                Log.w(LOG_TAG, "invalid instrumentation status bundle " + testInfo.ToString());
                return;
            }
            reportTestRunStarted(testInfo);
            TestIdentifier testId = new TestIdentifier(testInfo.mTestClass, testInfo.mTestName);
            IDictionary<string, string> metrics;

            switch (testInfo.mCode)
            {
                case StatusCodes.START:
                    foreach (ITestRunListener listener in mTestListeners)
                    {
                        listener.testStarted(testId);
                    }
                    break;
                case StatusCodes.FAILURE:
                    metrics = andResetTestMetrics;
                    foreach (ITestRunListener listener in mTestListeners)
                    {
                        listener.testFailed(TestFailure.FAILURE, testId, getTrace(testInfo));

                        listener.testEnded(testId, metrics);
                    }
                    mNumTestsRun++;
                    break;
                case StatusCodes.ERROR:
                    metrics = andResetTestMetrics;
                    foreach (ITestRunListener listener in mTestListeners)
                    {
                        listener.testFailed(TestFailure.ERROR, testId, getTrace(testInfo));
                        listener.testEnded(testId, metrics);
                    }
                    mNumTestsRun++;
                    break;
                case StatusCodes.OK:
                    metrics = andResetTestMetrics;
                    foreach (ITestRunListener listener in mTestListeners)
                    {
                        listener.testEnded(testId, metrics);
                    }
                    mNumTestsRun++;
                    break;
                default:
                    metrics = andResetTestMetrics;
                    Log.e(LOG_TAG, "Unknown status code received: " + testInfo.mCode);
                    foreach (ITestRunListener listener in mTestListeners)
                    {
                        listener.testEnded(testId, metrics);
                    }
                    mNumTestsRun++;
                    break;
            }

        }

        /// <summary>
        /// Reports the start of a test run, and the total test count, if it has not been previously
        /// reported.
        /// </summary>
        /// <param name="testInfo"> current test status values </param>
        private void reportTestRunStarted(TestResult testInfo)
        {
            // if start test run not reported yet
            if (!mTestStartReported && testInfo.mNumTests != null)
            {
                foreach (ITestRunListener listener in mTestListeners)
                {
                    listener.testRunStarted(mTestRunName, testInfo.mNumTests.Value);
                }
                mNumTestsExpected = testInfo.mNumTests.Value;
                mTestStartReported = true;
            }
        }

        /// <summary>
        /// Returns the stack trace of the current failed test, from the provided testInfo.
        /// </summary>
        private string getTrace(TestResult testInfo)
        {
            if (testInfo.mStackTrace != null)
            {
                return testInfo.mStackTrace;
            }
            else
            {
                Log.e(LOG_TAG, "Could not find stack trace for failed test ");
                return (new Exception("Unknown failure")).ToString();
            }
        }

        /// <summary>
        /// Parses out and store the elapsed time.
        /// </summary>
        private void parseTime(string line)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.regex.Pattern timePattern = java.util.regex.Pattern.compile(String.format("%s\\s*([\\d\\.]+)", Prefixes.TIME_REPORT));
            var timePattern = new Regex(string.Format("{0}\\s*([\\d\\.]+)", Prefixes.TIME_REPORT));
            var timeMatcher = timePattern.Match(line);
            if (timeMatcher.Success)
            {
                string timeString = timeMatcher.@group(1);
                try
                {
                    float timeSeconds = Convert.ToSingle(timeString);
                    mTestTime = (long)(timeSeconds * 1000);
                }
                catch (SystemException)
                {
                    Log.w(LOG_TAG, string.Format("Unexpected time format {0}", line));
                }
            }
            else
            {
                Log.w(LOG_TAG, string.Format("Unexpected time format {0}", line));
            }
        }

        /// <summary>
        /// Process a instrumentation run failure
        /// </summary>
        internal virtual void handleTestRunFailed(string errorMsg)
        {
            errorMsg = (errorMsg == null ? "Unknown error" : errorMsg);
            Log.i(LOG_TAG, string.Format("test run failed: '{0}'", errorMsg));
            if (mLastTestResult != null && mLastTestResult.complete && StatusCodes.START == mLastTestResult.mCode)
            {

                // received test start msg, but not test complete
                // assume test caused this, report as test failure
                TestIdentifier testId = new TestIdentifier(mLastTestResult.mTestClass, mLastTestResult.mTestName);
                foreach (ITestRunListener listener in mTestListeners)
                {
                    listener.testFailed(TestFailure.ERROR, testId, string.Format("{0}. Reason: '{1}'. {2}", INCOMPLETE_TEST_ERR_MSG_PREFIX, errorMsg, INCOMPLETE_TEST_ERR_MSG_POSTFIX));
                    listener.testEnded(testId, andResetTestMetrics);
                }
            }
            foreach (ITestRunListener listener in mTestListeners)
            {
                if (!mTestStartReported)
                {
                    // test run wasn't started - must have crashed before it started
                    listener.testRunStarted(mTestRunName, 0);
                }
                listener.testRunFailed(errorMsg);
                listener.testRunEnded(mTestTime, mInstrumentationResultBundle);
            }
            mTestStartReported = true;
            mTestRunFailReported = true;
        }

        /// <summary>
        /// Called by parent when adb session is complete.
        /// </summary>
        public override void done()
        {
            base.done();
            if (!mTestRunFailReported)
            {
                handleOutputDone();
            }
        }

        /// <summary>
        /// Handles the end of the adb session when a test run failure has not been reported yet
        /// </summary>
        private void handleOutputDone()
        {
            if (!mTestStartReported && !mTestRunFinished)
            {
                // no results
                handleTestRunFailed(NO_TEST_RESULTS_MSG);
            }
            else if (mNumTestsExpected > mNumTestsRun)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final String message = String.format("%1$s. Expected %2$d tests, received %3$d", INCOMPLETE_RUN_ERR_MSG_PREFIX, mNumTestsExpected, mNumTestsRun);
                string message = string.Format("{0}. Expected {1:D} tests, received {2:D}", INCOMPLETE_RUN_ERR_MSG_PREFIX, mNumTestsExpected, mNumTestsRun);
                handleTestRunFailed(message);
            }
            else
            {
                foreach (ITestRunListener listener in mTestListeners)
                {
                    if (!mTestStartReported)
                    {
                        // test run wasn't started, but it finished successfully. Must be a run with
                        // no tests
                        listener.testRunStarted(mTestRunName, 0);
                    }
                    listener.testRunEnded(mTestTime, mInstrumentationResultBundle);
                }
            }
        }
    }

}