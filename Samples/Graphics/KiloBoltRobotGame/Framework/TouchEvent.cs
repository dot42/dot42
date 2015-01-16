namespace KiloBoltRobotGame.Framework
{
    public class TouchEvent
    {
        public const int TOUCH_DOWN = 0;
        public const int TOUCH_UP = 1;
        public const int TOUCH_DRAGGED = 2;
        public const int TOUCH_HOLD = 3;

        public int type;
        public int x, y;
        public int pointer;
    }
}