/*
 * Copyright (C) 2011 The Android Open Source Project
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
package com.android.ddmuilib.logcat;

import junit.framework.TestCase;

public class LogCatStackTraceParserTest extends TestCase {
    private LogCatStackTraceParser mTranslator;

    private static final String SAMPLE_METHOD = "com.foo.Class.method"; //$NON-NLS-1$
    private static final String SAMPLE_FNAME = "FileName";              //$NON-NLS-1$
    private static final int SAMPLE_LINENUM = 20;
    private static final String SAMPLE_TRACE =
            String.format("  at %s(%s.groovy:%d)",                      //$NON-NLS-1$
                    SAMPLE_METHOD, SAMPLE_FNAME, SAMPLE_LINENUM);

    @Override
    protected void setUp() throws Exception {
        mTranslator = new LogCatStackTraceParser();
    }

    public void testIsValidExceptionTrace() {
        assertTrue(mTranslator.isValidExceptionTrace(SAMPLE_TRACE));
        assertFalse(mTranslator.isValidExceptionTrace(
                "java.lang.RuntimeException: message"));  //$NON-NLS-1$
        assertFalse(mTranslator.isValidExceptionTrace(
                "at com.foo.test(Ins.java:unknown)"));    //$NON-NLS-1$
    }

    public void testGetMethodName() {
        assertEquals(SAMPLE_METHOD, mTranslator.getMethodName(SAMPLE_TRACE));
    }

    public void testGetFileName() {
        assertEquals(SAMPLE_FNAME, mTranslator.getFileName(SAMPLE_TRACE));
    }

    public void testGetLineNumber() {
        assertEquals(SAMPLE_LINENUM, mTranslator.getLineNumber(SAMPLE_TRACE));
    }
}
