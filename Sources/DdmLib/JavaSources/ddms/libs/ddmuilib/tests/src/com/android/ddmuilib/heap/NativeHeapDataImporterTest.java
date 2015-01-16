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

package com.android.ddmuilib.heap;

import com.android.ddmlib.NativeAllocationInfo;
import com.android.ddmlib.NativeStackCallInfo;

import junit.framework.TestCase;

import org.eclipse.core.runtime.NullProgressMonitor;

import java.io.StringReader;
import java.lang.reflect.InvocationTargetException;
import java.util.List;

public class NativeHeapDataImporterTest extends TestCase {
    private static final String BASIC_TEXT =
            "Allocations: 1\n" +
            "Size: 524292\n" +
            "TotalSize: 524292\n" +
            "BeginStacktrace:\n" +
            "   40170bd8    /libc_malloc_leak.so --- getbacktrace --- /b/malloc_leak.c:258\n" +
            "   400910d6    /lib/libc.so --- ca110c --- /bionic/malloc_debug_common.c:227\n" +
            "   5dd6abfe    /lib/libcgdrv.so --- 5dd6abfe ---\n" +
            "   5dd98a8e    /lib/libcgdrv.so --- 5dd98a8e ---\n" +
            "EndStacktrace\n";

    private NativeHeapDataImporter mImporter;

    public void testImportValidAllocation() {
        mImporter = createImporter(BASIC_TEXT);
        try {
            mImporter.run(new NullProgressMonitor());
        } catch (InvocationTargetException e) {
            fail("Unexpected exception while parsing text: " + e.getTargetException().getMessage());
        } catch (InterruptedException e) {
            fail("Tests are not interrupted!");
        }

        NativeHeapSnapshot snapshot = mImporter.getImportedSnapshot();
        assertNotNull(snapshot);

        // check whether all details have been parsed correctly
        assertEquals(1, snapshot.getAllocations().size());

        NativeAllocationInfo info = snapshot.getAllocations().get(0);

        assertEquals(1, info.getAllocationCount());
        assertEquals(524292, info.getSize());
        assertEquals(true, info.isStackCallResolved());

        List<NativeStackCallInfo> stack = info.getResolvedStackCall();
        assertEquals(4, stack.size());
    }

    private NativeHeapDataImporter createImporter(String contentsToParse) {
        StringReader r = new StringReader(contentsToParse);
        return new NativeHeapDataImporter(r);
    }
}
