using Java.Util;

namespace KiloBoltRobotGame.Framework
{
    public interface IInput
    { 
        bool isTouchDown(int pointer);

        int getTouchX(int pointer);

        int getTouchY(int pointer);

        IList<TouchEvent> getTouchEvents();
    }
}