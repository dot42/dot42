using Android.Content;
using Android.Os;
using Android.View;
using Java.Util;

namespace KiloBoltRobotGame.Framework
{
    public class AndroidInput : IInput
    {
        private ITouchHandler touchHandler;

        public AndroidInput(Context context, View view, float scaleX, float scaleY)
        {
            if (int.Parse(Build.VERSION.SDK) < 5)
                touchHandler = new SingleTouchHandler(view, scaleX, scaleY);
            else
                touchHandler = new MultiTouchHandler(view, scaleX, scaleY);
        }


        public bool isTouchDown(int pointer)
        {
            return touchHandler.isTouchDown(pointer);
        }

        public int getTouchX(int pointer)
        {
            return touchHandler.getTouchX(pointer);
        }

        public int getTouchY(int pointer)
        {
            return touchHandler.getTouchY(pointer);
        }

        public IList<TouchEvent> getTouchEvents()
        {
            return touchHandler.getTouchEvents();
        }

    }
}