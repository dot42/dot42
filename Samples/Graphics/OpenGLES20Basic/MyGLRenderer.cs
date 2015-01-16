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

using Java.Nio;

using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;

using Android.Opengl;

namespace com.example.android.opengl
{
    public class MyGLRenderer : GLSurfaceView.IRenderer
    {
        private Triangle mTriangle;

        public void OnSurfaceCreated(IGL10 unused, EGLConfig config)
        {

            // Set the background frame color
            GLES20.GlClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            mTriangle = new Triangle();
        }

        public void OnDrawFrame(IGL10 unused)
        {

            // Draw background color
            GLES20.GlClear(GLES20.GL_COLOR_BUFFER_BIT);

            // Draw triangle
            mTriangle.Draw();
        }

        public void OnSurfaceChanged(IGL10 unused, int width, int height)
        {
            // Adjust the viewport based on geometry changes,
            // such as screen rotation
            GLES20.GlViewport(0, 0, width, height);
        }

        public static int LoadShader(int type, string shaderCode)
        {

            // create a vertex shader type (GLES20.GL_VERTEX_SHADER)
            // or a fragment shader type (GLES20.GL_FRAGMENT_SHADER)
            int shader = GLES20.GlCreateShader(type);

            // add the source code to the shader and compile it
            GLES20.GlShaderSource(shader, shaderCode);
            GLES20.GlCompileShader(shader);

            return shader;
        }

    }

    internal class Triangle
    {
        private const string vertexShaderCode =
            "attribute vec4 vPosition;" +
            "void main() {" +
            "  gl_Position = vPosition;" +
            "}";

        private const string fragmentShaderCode =
            "precision mediump float;" +
            "uniform vec4 vColor;" +
            "void main() {" +
            "  gl_FragColor = vColor;" +
            "}";

        private readonly FloatBuffer vertexBuffer;
        private readonly int mProgram;
        private int mPositionHandle;
        private int mColorHandle;

        // number of coordinates per vertex in this array
        private static readonly int COORDS_PER_VERTEX = 3;

        private static float[] triangleCoords =
            {
                // in counterclockwise order:
                0.0f, 0.622008459f, 0.0f, // top
                -0.5f, -0.311004243f, 0.0f, // bottom left
                0.5f, -0.311004243f, 0.0f // bottom right
            };

        private int vertexCount = triangleCoords.Length/COORDS_PER_VERTEX;
        private int vertexStride = COORDS_PER_VERTEX*4; // bytes per vertex

        // Set color with red, green, blue and alpha (opacity) values
        private float[] color = {0.63671875f, 0.76953125f, 0.22265625f, 1.0f};

        public Triangle()
        {
            // initialize vertex byte buffer for shape coordinates
            ByteBuffer bb = ByteBuffer.AllocateDirect(
                // (number of coordinate values * 4 bytes per float)
                triangleCoords.Length*4);
            // use the device hardware's native byte order
            bb.Order(ByteOrder.NativeOrder());

            // create a floating point buffer from the ByteBuffer
            vertexBuffer = bb.AsFloatBuffer();
            // add the coordinates to the FloatBuffer
            vertexBuffer.Put(triangleCoords);
            // set the buffer to read the first coordinate
            vertexBuffer.Position(0);

            // prepare shaders and OpenGL program
            int vertexShader = MyGLRenderer.LoadShader(GLES20.GL_VERTEX_SHADER,
                                                       vertexShaderCode);
            int fragmentShader = MyGLRenderer.LoadShader(GLES20.GL_FRAGMENT_SHADER,
                                                         fragmentShaderCode);

            mProgram = GLES20.GlCreateProgram(); // create empty OpenGL Program
            GLES20.GlAttachShader(mProgram, vertexShader); // add the vertex shader to program
            GLES20.GlAttachShader(mProgram, fragmentShader); // add the fragment shader to program
            GLES20.GlLinkProgram(mProgram); // create OpenGL program executables
        }

        public void Draw()
        {
            // Add program to OpenGL environment
            GLES20.GlUseProgram(mProgram);

            // get handle to vertex shader's vPosition member
            mPositionHandle = GLES20.GlGetAttribLocation(mProgram, "vPosition");

            // Enable a handle to the triangle vertices
            GLES20.GlEnableVertexAttribArray(mPositionHandle);

            // Prepare the triangle coordinate data
            GLES20.GlVertexAttribPointer(mPositionHandle, COORDS_PER_VERTEX,
                                         GLES20.GL_FLOAT, false,
                                         vertexStride, vertexBuffer);

            // get handle to fragment shader's vColor member
            mColorHandle = GLES20.GlGetUniformLocation(mProgram, "vColor");

            // Set color for drawing the triangle
            GLES20.GlUniform4fv(mColorHandle, 1, color, 0);

            // Draw the triangle
            GLES20.GlDrawArrays(GLES20.GL_TRIANGLES, 0, vertexCount);

            // Disable vertex array
            GLES20.GlDisableVertexAttribArray(mPositionHandle);
        }
    }
}
