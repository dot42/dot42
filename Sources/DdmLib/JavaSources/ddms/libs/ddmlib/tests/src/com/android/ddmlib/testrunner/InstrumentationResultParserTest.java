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

import com.android.ddmlib.testrunner.ITestRunListener.TestFailure;

import org.easymock.Capture;
import org.easymock.EasyMock;

import junit.framework.TestCase;

import java.util.Collections;
import java.util.Map;

/**
 * Unit tests for {@link @InstrumentationResultParser}.
 */
@SuppressWarnings("unchecked")
public class InstrumentationResultParserTest extends TestCase {

    private InstrumentationResultParser mParser;
    private ITestRunListener mMockListener;

    // static dummy test names to use for validation
    private static final String RUN_NAME = "foo";
    private static final String CLASS_NAME = "com.test.FooTest";
    private static final String TEST_NAME = "testFoo";
    private static final String STACK_TRACE = "java.lang.AssertionFailedException";
    private static final TestIdentifier TEST_ID = new TestIdentifier(CLASS_NAME, TEST_NAME);

    /**
     * @param name - test name
     */
    public InstrumentationResultParserTest(String name) {
        super(name);
    }

    /**
     * @see junit.framework.TestCase#setUp()
     */
    @Override
    protected void setUp() throws Exception {
        super.setUp();
        // use a strict mock to verify order of method calls
        mMockListener = EasyMock.createStrictMock(ITestRunListener.class);
        mParser = new InstrumentationResultParser(RUN_NAME, mMockListener);
    }

    /**
     * Tests parsing empty output.
     */
    public void testParse_empty() {
        mMockListener.testRunStarted(RUN_NAME, 0);
        mMockListener.testRunFailed(InstrumentationResultParser.NO_TEST_RESULTS_MSG);
        mMockListener.testRunEnded(0, Collections.EMPTY_MAP);

        injectAndVerifyTestString("");
    }

    /**
     * Tests parsing output for a successful test run with no tests.
     */
    public void testParse_noTests() {
        StringBuilder output = new StringBuilder();
        addLine(output, "INSTRUMENTATION_RESULT: stream=");
        addLine(output, "Test results for InstrumentationTestRunner=");
        addLine(output, "Time: 0.001");
        addLine(output, "OK (0 tests)");
        addLine(output, "INSTRUMENTATION_CODE: -1");

        mMockListener.testRunStarted(RUN_NAME, 0);
        mMockListener.testRunEnded(1, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());
    }

    /**
     * Tests parsing output for a single successful test execution.
     */
    public void testParse_singleTest() {
        StringBuilder output = createSuccessTest();

        mMockListener.testRunStarted(RUN_NAME, 1);
        mMockListener.testStarted(TEST_ID);
        mMockListener.testEnded(TEST_ID, Collections.EMPTY_MAP);
        mMockListener.testRunEnded(0, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());
    }

    /**
     * Tests parsing output for a successful test execution with metrics.
     */
    public void testParse_testMetrics() {
        StringBuilder output = buildCommonResult();

        addStatusKey(output, "randomKey", "randomValue");
        addSuccessCode(output);

        final Capture<Map<String, String>> captureMetrics = new Capture<Map<String, String>>();
        mMockListener.testRunStarted(RUN_NAME, 1);
        mMockListener.testStarted(TEST_ID);
        mMockListener.testEnded(EasyMock.eq(TEST_ID), EasyMock.capture(captureMetrics));
        mMockListener.testRunEnded(0, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());

        assertEquals("randomValue", captureMetrics.getValue().get("randomKey"));
    }

    /**
     * Test parsing output for a test that produces repeated metrics values
     * <p/>
     * This mimics launch performance test output.
     */
    public void testParse_repeatedTestMetrics() {
        StringBuilder output = new StringBuilder();
        // add test start output
        addCommonStatus(output);
        addStartCode(output);

        addStatusKey(output, "currentiterations", "1");
        addStatusCode(output, "2");
        addStatusKey(output, "currentiterations", "2");
        addStatusCode(output, "2");
        addStatusKey(output, "currentiterations", "3");
        addStatusCode(output, "2");

        // add test end
        addCommonStatus(output);
        addStatusKey(output, "numiterations", "3");
        addSuccessCode(output);

        final Capture<Map<String, String>> captureMetrics = new Capture<Map<String, String>>();
        mMockListener.testRunStarted(RUN_NAME, 1);
        mMockListener.testStarted(TEST_ID);
        mMockListener.testEnded(EasyMock.eq(TEST_ID), EasyMock.capture(captureMetrics));
        mMockListener.testRunEnded(0, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());

        assertEquals("3", captureMetrics.getValue().get("currentiterations"));
        assertEquals("3", captureMetrics.getValue().get("numiterations"));
    }

    /**
     * Test parsing output for a test failure.
     */
    public void testParse_testFailed() {
        StringBuilder output = buildCommonResult();
        addStackTrace(output);
        addFailureCode(output);

        mMockListener.testRunStarted(RUN_NAME, 1);
        mMockListener.testStarted(TEST_ID);
        mMockListener.testFailed(TestFailure.FAILURE, TEST_ID, STACK_TRACE);
        mMockListener.testEnded(TEST_ID, Collections.EMPTY_MAP);
        mMockListener.testRunEnded(0, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());
    }

    /**
     * Test parsing and conversion of time output that contains extra chars.
     */
    public void testParse_timeBracket() {
        StringBuilder output = createSuccessTest();
        output.append("Time: 0.001)");

        mMockListener.testRunStarted(RUN_NAME, 1);
        mMockListener.testStarted(TEST_ID);
        mMockListener.testEnded(TEST_ID, Collections.EMPTY_MAP);
        mMockListener.testRunEnded(1, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());
    }

    /**
     * Test parsing output for a test run failure.
     */
    public void testParse_runFailed() {
        StringBuilder output = new StringBuilder();
        final String errorMessage = "Unable to find instrumentation info";
        addStatusKey(output, "Error", errorMessage);
        addStatusCode(output, "-1");
        output.append("INSTRUMENTATION_FAILED: com.dummy/android.test.InstrumentationTestRunner");
        addLineBreak(output);

        mMockListener.testRunStarted(RUN_NAME, 0);
        mMockListener.testRunFailed(errorMessage);
        mMockListener.testRunEnded(0, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());
    }

    /**
     * Test parsing output when a status code cannot be parsed
     */
    public void testParse_invalidCode() {
        StringBuilder output = new StringBuilder();
        addLine(output, "android.util.AndroidException: INSTRUMENTATION_FAILED: foo/foo");
        addLine(output, "INSTRUMENTATION_STATUS: id=ActivityManagerService");
        addLine(output, "INSTRUMENTATION_STATUS: Error=Unable to find instrumentation target package: foo");
        addLine(output, "INSTRUMENTATION_STATUS_CODE: -1at com.android.commands.am.Am.runInstrument(Am.java:532)");
        addLine(output, "");
        addLine(output, "        at com.android.commands.am.Am.run(Am.java:111)");
        addLineBreak(output);

        mMockListener.testRunStarted(RUN_NAME, 0);
        mMockListener.testRunFailed((String)EasyMock.anyObject());
        mMockListener.testRunEnded(0, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());
    }

    /**
     * Test parsing output for a test run failure, where an instrumentation component failed to
     * load.
     * <p/>
     * Parsing input takes the from of INSTRUMENTATION_RESULT: fff
     */
    public void testParse_failedResult() {
        StringBuilder output = new StringBuilder();
        final String errorMessage = "Unable to instantiate instrumentation";
        output.append("INSTRUMENTATION_RESULT: shortMsg=");
        output.append(errorMessage);
        addLineBreak(output);
        output.append("INSTRUMENTATION_CODE: 0");
        addLineBreak(output);

        mMockListener.testRunStarted(RUN_NAME, 0);
        mMockListener.testRunFailed(EasyMock.contains(errorMessage));
        mMockListener.testRunEnded(0, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());
    }

    /**
     * Test parsing output for a test run that did not complete.
     * <p/>
     * This can occur if device spontaneously reboots, or if test method could not be found.
     */
    public void testParse_incomplete() {
        StringBuilder output = new StringBuilder();
        // add a start test sequence, but without an end test sequence
        addCommonStatus(output);
        addStartCode(output);

        mMockListener.testRunStarted(RUN_NAME, 1);
        mMockListener.testStarted(TEST_ID);
        mMockListener.testFailed(EasyMock.eq(TestFailure.ERROR), EasyMock.eq(TEST_ID),
                EasyMock.startsWith(InstrumentationResultParser.INCOMPLETE_TEST_ERR_MSG_PREFIX));
        mMockListener.testEnded(TEST_ID, Collections.EMPTY_MAP);
        mMockListener.testRunFailed(EasyMock.startsWith(
                InstrumentationResultParser.INCOMPLETE_RUN_ERR_MSG_PREFIX));
        mMockListener.testRunEnded(0, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());
    }

    /**
     * Test parsing output for a test run that did not start due to incorrect syntax supplied to am.
     */
    public void testParse_amFailed() {
        StringBuilder output = new StringBuilder();
        addLine(output, "usage: am [subcommand] [options]");
        addLine(output, "start an Activity: am start [-D] [-W] <INTENT>");
        addLine(output, "-D: enable debugging");
        addLine(output, "-W: wait for launch to complete");
        addLine(output, "start a Service: am startservice <INTENT>");
        addLine(output, "Error: Bad component name: wfsdafddfasasdf");

        mMockListener.testRunStarted(RUN_NAME, 0);
        mMockListener.testRunFailed(InstrumentationResultParser.NO_TEST_RESULTS_MSG);
        mMockListener.testRunEnded(0, Collections.EMPTY_MAP);

        injectAndVerifyTestString(output.toString());
    }

    /**
     * Test parsing output for a test run that produces INSTRUMENTATION_RESULT output.
     * <p/>
     * This mimics launch performance test output.
     */
    public void testParse_instrumentationResults() {
        StringBuilder output = new StringBuilder();
        addResultKey(output, "other_pss", "2390");
        addResultKey(output, "java_allocated", "2539");
        addResultKey(output, "foo", "bar");
        addResultKey(output, "stream", "should not be captured");
        addLine(output, "INSTRUMENTATION_CODE: -1");

        Capture<Map<String, String>> captureMetrics = new Capture<Map<String, String>>();
        mMockListener.testRunStarted(RUN_NAME, 0);
        mMockListener.testRunEnded(EasyMock.anyLong(), EasyMock.capture(captureMetrics));

        injectAndVerifyTestString(output.toString());

        assertEquals("2390", captureMetrics.getValue().get("other_pss"));
        assertEquals("2539", captureMetrics.getValue().get("java_allocated"));
        assertEquals("bar", captureMetrics.getValue().get("foo"));
        assertEquals(3, captureMetrics.getValue().size());
    }

    /**
     * Builds a common test result using TEST_NAME and TEST_CLASS.
     */
    private StringBuilder buildCommonResult() {
        StringBuilder output = new StringBuilder();
        // add test start bundle
        addCommonStatus(output);
        addStartCode(output);
        // add end test bundle, without status
        addCommonStatus(output);
        return output;
    }

    /**
     * Create instrumentation output for a successful single test case execution.
     */
    private StringBuilder createSuccessTest() {
        StringBuilder output = buildCommonResult();
        addSuccessCode(output);
        return output;
    }

    /**
     * Adds common status results to the provided output.
     */
    private void addCommonStatus(StringBuilder output) {
        addStatusKey(output, "stream", "\r\n" + CLASS_NAME);
        addStatusKey(output, "test", TEST_NAME);
        addStatusKey(output, "class", CLASS_NAME);
        addStatusKey(output, "current", "1");
        addStatusKey(output, "numtests", "1");
        addStatusKey(output, "id", "InstrumentationTestRunner");
    }

    /**
     * Adds a stack trace status bundle to output.
     */
    private void addStackTrace(StringBuilder output) {
        addStatusKey(output, "stack", STACK_TRACE);
    }

    /**
     * Helper method to add a status key-value bundle.
     */
    private void addStatusKey(StringBuilder outputBuilder, String key,
            String value) {
        outputBuilder.append("INSTRUMENTATION_STATUS: ");
        outputBuilder.append(key);
        outputBuilder.append('=');
        outputBuilder.append(value);
        addLineBreak(outputBuilder);
    }

    /**
     * Helper method to add a result key value bundle.
     */
    private void addResultKey(StringBuilder outputBuilder, String key,
          String value) {
      outputBuilder.append("INSTRUMENTATION_RESULT: ");
      outputBuilder.append(key);
      outputBuilder.append('=');
      outputBuilder.append(value);
      addLineBreak(outputBuilder);
    }

    /**
     * Append a line to output.
     */
    private void addLine(StringBuilder outputBuilder, String lineContent) {
        outputBuilder.append(lineContent);
        addLineBreak(outputBuilder);
    }

    /**
     * Append line break characters to output
     */
    private void addLineBreak(StringBuilder outputBuilder) {
        outputBuilder.append("\r\n");
    }

    private void addStartCode(StringBuilder outputBuilder) {
        addStatusCode(outputBuilder, "1");
    }

    private void addSuccessCode(StringBuilder outputBuilder) {
        addStatusCode(outputBuilder, "0");
    }

    private void addFailureCode(StringBuilder outputBuilder) {
        addStatusCode(outputBuilder, "-2");
    }

    private void addStatusCode(StringBuilder outputBuilder, String value) {
        outputBuilder.append("INSTRUMENTATION_STATUS_CODE: ");
        outputBuilder.append(value);
        addLineBreak(outputBuilder);
    }

    /**
     * Inject a test string into the result parser, and verify the mock listener.
     *
     * @param result the string to inject into parser under test.
     */
    private void injectAndVerifyTestString(String result) {
        EasyMock.replay(mMockListener);
        byte[] data = result.getBytes();
        mParser.addOutput(data, 0, data.length);
        mParser.flush();
        EasyMock.verify(mMockListener);
    }
}
