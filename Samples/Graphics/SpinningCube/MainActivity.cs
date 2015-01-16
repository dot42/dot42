using Android.App;
using Android.Opengl;
using Android.Os;
using Dot42.Manifest;

[assembly: Application("Spinning Cube", Icon = "Icon")]

namespace SpinningCube
{
    [Activity]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);          
        
            var glView = new GLSurfaceView(this);
            glView.SetRenderer(new MyRenderer());
            SetContentView(glView);
        }     
    }
}
