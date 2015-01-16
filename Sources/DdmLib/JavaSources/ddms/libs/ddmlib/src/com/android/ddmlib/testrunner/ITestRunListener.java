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

package com.android.ddmlib.testrunner;

import java.util.Map;

/**
 * Receives event notifications during instrumentation test runs.
 * <p/>
 * Patterned after {@link junit.runner.TestRunListener}.
 * <p/>
 * The sequence of calls will be:
 * <ul>
 * <li> testRunStarted
 * <li> testStarted
 * <li> [testFailed]
 * <li> testEnded
 * <li> ....
 * <li> [testRunFailed]
 * <li> testRunEnded
 * </ul>
 */
public interface ITestRunListener {

    /**
     *  Types of test failures.
     */
    enum TestFailure {
        /** Test failed due to unanticipated uncaught exception. */
        ERROR,
        /** Test failed due to a false assertion. */
        FAILURE
    }

    /**
     * Reports the start of a test run.
     *
     * @param runName the test run name
     * @param testCount total number of tests in test run
     */
    public void testRunStarted(String runName, int testCount);

    /**
     * Reports the start of an individual test case.
     *
     * @param test identifies the test
     */
    public void testStarted(TestIdentifier test);

    /**
     * Reports the failure of a individual test case.
     * <p/>
     * Will be called between testStarted and testEnded.
     *
     * @param status failure type
     * @param test identifies the test
     * @param trace stack trace of failure
     */
    public void testFailed(TestFailure status, TestIdentifier test, String trace);

    /**
     * Reports the execution end of an individual test case.
     * <p/>
     * If {@link #testFailed} was not invoked, this test passed.  Also returns any key/value
     * metrics which may have been emitted during the test case's execution.
     *
     * @param test identifies the test
     * @param testMetrics a {@link Map} of the metrics emitted
     */
    public void testEnded(TestIdentifier test, Map<String, String> testMetrics);

    /**
     * Reports test run failed to complete due to a fatal error.
     *
     * @param errorMessage {@link String} describing reason for run failure.
     */
    public void testRunFailed(String errorMessage);

    /**
     * Reports test run stopped before completion due to a user request.
     * <p/>
     * TODO: currently unused, consider removing
     *
     * @param elapsedTime device reported elapsed time, in milliseconds
     */
    public void testRunStopped(long elapsedTime);

    /**
     * Reports end of test run.
     *
     * @param elapsedTime device reported elapsed time, in milliseconds
     * @param runMetrics key-value pairs reported at the end of a test run
     */
    public void testRunEnded(long elapsedTime, Map<String, String> runMetrics);
}
