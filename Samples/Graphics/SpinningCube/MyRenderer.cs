using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;

namespace SpinningCube
{
    /// <summary>
    /// Own <see cref="GLSurfaceView.IRenderer"/> implementation.
    /// </summary>
    public class MyRenderer : GLSurfaceView.IRenderer
    {
        private readonly Cube mCube = new Cube();
        private float mCubeRotation;

        public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
            gl.GlClearColor(0.0f, 0.0f, 0.0f, 0.5f);

            gl.GlClearDepthf(1.0f);
            gl.GlEnable(IGL10Constants.GL_DEPTH_TEST);
            gl.GlDepthFunc(IGL10Constants.GL_LEQUAL);

            gl.GlHint(IGL10Constants.GL_PERSPECTIVE_CORRECTION_HINT,
                      IGL10Constants.GL_NICEST);
        }

        public void OnDrawFrame(IGL10 gl)
        {
            gl.GlClear(IGL10Constants.GL_COLOR_BUFFER_BIT | IGL10Constants.GL_DEPTH_BUFFER_BIT);
            gl.GlLoadIdentity();

            gl.GlTranslatef(0.0f, 0.0f, -10.0f);
            gl.GlRotatef(mCubeRotation, 1.0f, 1.0f, 1.0f);

            mCube.Draw(gl);

            gl.GlLoadIdentity();

            mCubeRotation -= 0.7f;
        }

        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            gl.GlViewport(0, 0, width, height);
            gl.GlMatrixMode(IGL10Constants.GL_PROJECTION);
            gl.GlLoadIdentity();
            GLU.GluPerspective(gl, 45.0f, (float)width / (float)height, 0.1f, 100.0f);
            gl.GlViewport(0, 0, width, height);

            gl.GlMatrixMode(IGL10Constants.GL_MODELVIEW);
            gl.GlLoadIdentity();
        }
    }
}