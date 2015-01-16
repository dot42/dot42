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
package com.android.ddmlib;


import java.io.UnsupportedEncodingException;

/**
 * A {@link IShellOutputReceiver} which collects the whole shell output into one
 * {@link String}.
 */
public class CollectingOutputReceiver implements IShellOutputReceiver {

    private StringBuffer mOutputBuffer = new StringBuffer();
    private boolean mIsCanceled = false;

    public String getOutput() {
        return mOutputBuffer.toString();
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public boolean isCancelled() {
        return mIsCanceled;
    }

    /**
     * Cancel the output collection
     */
    public void cancel() {
        mIsCanceled = true;
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public void addOutput(byte[] data, int offset, int length) {
        if (!isCancelled()) {
            String s = null;
            try {
                s = new String(data, offset, length, "UTF-8"); //$NON-NLS-1$
            } catch (UnsupportedEncodingException e) {
                // normal encoding didn't work, try the default one
                s = new String(data, offset,length);
            }
            mOutputBuffer.append(s);
        }
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public void flush() {
        // ignore
    }
}
