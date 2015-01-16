using Android.Content;
using Android.Hardware;
using Android.View;

namespace SimpleCamera
{
    /// <summary>
    /// Camera preview control
    /// </summary>
    public class CameraPreview : SurfaceView, ISurfaceHolder_ICallback
    {
        private readonly Camera camera;
        private readonly ISurfaceHolder surfaceHolder;

        /// <summary>
        /// Default ctor
        /// </summary>
        public CameraPreview(Context context, Camera camera)
            : base(context)
        {
            this.camera = camera;
            surfaceHolder = GetHolder();
            surfaceHolder.AddCallback(this);
            surfaceHolder.SetType(ISurfaceHolderConstants.SURFACE_TYPE_PUSH_BUFFERS);
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                camera.SetPreviewDisplay(surfaceHolder);
                camera.StartPreview();
            }
            catch 
            {
                // Ignore for now
            }
        }

        public void SurfaceChanged(ISurfaceHolder holder, int format, int width, int height)
        {
            // Start preview
            try
            {
                camera.SetPreviewDisplay(surfaceHolder);
                camera.StartPreview();
            }
            catch 
            {
                // Ignore for now
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            camera.StopPreview();
            camera.Release();
        }
    }
}
