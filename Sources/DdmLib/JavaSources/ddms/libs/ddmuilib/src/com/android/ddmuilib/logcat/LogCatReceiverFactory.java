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

import com.android.ddmlib.AndroidDebugBridge;
import com.android.ddmlib.AndroidDebugBridge.IDeviceChangeListener;
import com.android.ddmlib.IDevice;

import org.eclipse.jface.preference.IPreferenceStore;

import java.util.HashMap;
import java.util.Map;

/**
 * A factory for {@link LogCatReceiver} objects. Its primary objective is to cache
 * constructed {@link LogCatReceiver}'s per device and hand them back when requested.
 */
public class LogCatReceiverFactory {
    /** Singleton instance. */
    public static final LogCatReceiverFactory INSTANCE = new LogCatReceiverFactory();

    private Map<String, LogCatReceiver> mReceiverCache = new HashMap<String, LogCatReceiver>();

    /** Private constructor: cannot instantiate. */
    private LogCatReceiverFactory() {
        AndroidDebugBridge.addDeviceChangeListener(new IDeviceChangeListener() {
            @Override
            public void deviceDisconnected(IDevice device) {
                removeReceiverFor(device);
            }

            @Override
            public void deviceConnected(IDevice device) {
            }

            @Override
            public void deviceChanged(IDevice device, int changeMask) {
            }
        });
    }

    private synchronized void removeReceiverFor(IDevice device) {
        mReceiverCache.remove(device.getSerialNumber());
    }

    public synchronized LogCatReceiver newReceiver(IDevice device, IPreferenceStore prefs) {
        LogCatReceiver r = mReceiverCache.get(device.getSerialNumber());
        if (r != null) {
            return r;
        }

        r = new LogCatReceiver(device, prefs);
        mReceiverCache.put(device.getSerialNumber(), r);
        return r;
    }
}
