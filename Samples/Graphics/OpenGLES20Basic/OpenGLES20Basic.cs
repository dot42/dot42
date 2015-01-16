/*
 * Copyright (C) 2012 The Android Open Source Project
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

using Android.App;
using Android.Content;
using Android.Opengl;
using Android.Os;
using Dot42.Manifest;

[assembly: Application("OpenGLES20Basic")]

namespace com.example.android.opengl
{
    [Activity]
    public class OpenGLES20Basic : Activity
    {
        private GLSurfaceView mGLView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create a GLSurfaceView instance and set it
            // as the ContentView for this Activity
            mGLView = new MyGLSurfaceView(this);
            SetContentView(mGLView);
        }

        protected override void OnPause()
        {
            base.OnPause();
            // The following call pauses the rendering thread.
            // If your OpenGL application is memory intensive,
            // you should consider de-allocating objects that
            // consume significant memory here.
            mGLView.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            // The following call resumes a paused rendering thread.
            // If you de-allocated graphic objects for onPause()
            // this is a good place to re-allocate them.
            mGLView.OnResume();
        }
    }

    internal class MyGLSurfaceView : GLSurfaceView
    {

        public MyGLSurfaceView(Context context)
            : base(context)
        {

            // Create an OpenGL ES 2.0 context.
            SetEGLContextClientVersion(2);

            // Set the Renderer for drawing on the GLSurfaceView
            SetRenderer(new MyGLRenderer());

            // Render the view only when there is a change in the drawing data
            SetRenderMode(GLSurfaceView.RENDERMODE_WHEN_DIRTY);
        }
    }
}
