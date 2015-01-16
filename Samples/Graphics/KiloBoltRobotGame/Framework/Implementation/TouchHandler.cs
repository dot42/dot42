using Android.View;
using Java.Util;

namespace KiloBoltRobotGame.Framework
{
    public interface ITouchHandler : View.IOnTouchListener
    {
        bool isTouchDown(int pointer);

        int getTouchX(int pointer);

        int getTouchY(int pointer);

        IList<TouchEvent> getTouchEvents();
    }
}