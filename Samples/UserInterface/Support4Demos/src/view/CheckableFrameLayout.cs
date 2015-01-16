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

using Android.Content;
using Android.Graphics.Drawable;
using Android.Util;
using Android.Widget;

using Dot42;

namespace com.example.android.supportv4.view
{
	[CustomView]
    public class CheckableFrameLayout : FrameLayout, ICheckable 
    {
        private bool mChecked;

        public CheckableFrameLayout(Context context) : base(context)
        {
        }

        public CheckableFrameLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public void SetChecked(bool @checked) 
        {
            mChecked = @checked;
            SetBackgroundDrawable(@checked ? new ColorDrawable(unchecked((int)0xff0000a0)) : null);
        }

        public bool IsChecked()
        {
            return mChecked;
        }

        public void Toggle() 
        {
            SetChecked(!mChecked);
        }

    }
}
