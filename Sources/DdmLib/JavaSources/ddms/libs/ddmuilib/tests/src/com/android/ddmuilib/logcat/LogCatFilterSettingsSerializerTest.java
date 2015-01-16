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

import com.android.ddmlib.Log.LogLevel;

import java.util.Arrays;
import java.util.List;

import junit.framework.TestCase;

public class LogCatFilterSettingsSerializerTest extends TestCase {
    /* test that decode(encode(f)) = f */
    public void testSerializer() {
        LogCatFilter fs = new LogCatFilter(
                "TestFilter",               //$NON-NLS-1$
                "Tag'.*Regex",              //$NON-NLS-1$
                "regexForTextField..''",    //$NON-NLS-1$
                "123",                      //$NON-NLS-1$
                "TestAppName.*",            //$NON-NLS-1$
                LogLevel.ERROR);

        LogCatFilterSettingsSerializer serializer = new LogCatFilterSettingsSerializer();
        String s = serializer.encodeToPreferenceString(Arrays.asList(fs));
        List<LogCatFilter> decodedFiltersList = serializer.decodeFromPreferenceString(s);

        assertEquals(1, decodedFiltersList.size());

        LogCatFilter dfs = decodedFiltersList.get(0);
        assertEquals(fs.getName(), dfs.getName());
        assertEquals(fs.getTag(), dfs.getTag());
        assertEquals(fs.getText(), dfs.getText());
        assertEquals(fs.getPid(), dfs.getPid());
        assertEquals(fs.getAppName(), dfs.getAppName());
        assertEquals(fs.getLogLevel(), dfs.getLogLevel());
    }

    /* test that transient filters are not persisted */
    public void testTransientFilters() {
        LogCatFilter fs = new LogCatFilter(
                "TestFilter",               //$NON-NLS-1$
                "Tag'.*Regex",              //$NON-NLS-1$
                "regexForTextField..''",    //$NON-NLS-1$
                "123",                      //$NON-NLS-1$
                "TestAppName.*",            //$NON-NLS-1$
                LogLevel.ERROR);
        fs.setTransient();

        LogCatFilterSettingsSerializer serializer = new LogCatFilterSettingsSerializer();
        String s = serializer.encodeToPreferenceString(Arrays.asList(fs));
        List<LogCatFilter> decodedFiltersList = serializer.decodeFromPreferenceString(s);

        assertEquals(0, decodedFiltersList.size());
    }
}
