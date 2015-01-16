using Android.View;
using Java.Util;

namespace KiloBoltRobotGame.Framework
{
    public class SingleTouchHandler : ITouchHandler
    {
        private bool isTouched;
        private int touchX;
        private int touchY;
        private readonly Pool<TouchEvent> touchEventPool;
        private readonly IList<TouchEvent> touchEvents = new ArrayList<TouchEvent>();
        private readonly IList<TouchEvent> touchEventsBuffer = new ArrayList<TouchEvent>();
        private readonly float scaleX;
        private readonly float scaleY;

        public SingleTouchHandler(View view, float scaleX, float scaleY)
        {
            touchEventPool = new Pool<TouchEvent>(new TouchEventFactory(), 100);
            view.SetOnTouchListener(this);

            this.scaleX = scaleX;
            this.scaleY = scaleY;
        }

        public bool OnTouch(View v, MotionEvent @event)
        {
            lock (this)
            {
                TouchEvent touchEvent = touchEventPool.newObject();
                switch (@event.GetAction())
                {
                    case MotionEvent.ACTION_DOWN:
                        touchEvent.type = TouchEvent.TOUCH_DOWN;
                        isTouched = true;
                        break;
                    case MotionEvent.ACTION_MOVE:
                        touchEvent.type = TouchEvent.TOUCH_DRAGGED;
                        isTouched = true;
                        break;
                    case MotionEvent.ACTION_CANCEL:
                    case MotionEvent.ACTION_UP:
                        touchEvent.type = TouchEvent.TOUCH_UP;
                        isTouched = false;
                        break;
                }

                touchEvent.x = touchX = (int) (@event.GetX()*scaleX);
                touchEvent.y = touchY = (int) (@event.GetY()*scaleY);
                touchEventsBuffer.Add(touchEvent);

                return true;
            }
        }

        public bool isTouchDown(int pointer)
        {
            lock (this)
            {
                if (pointer == 0)
                    return isTouched;
                else
                    return false;
            }
        }

        public int getTouchX(int pointer)
        {
            lock (this)
            {
                return touchX;
            }
        }

        public int getTouchY(int pointer)
        {
            lock (this)
            {
                return touchY;
            }
        }

        public IList<TouchEvent> getTouchEvents()
        {
            lock (this)
            {
                int len = touchEvents.Size();
                for (int i = 0; i < len; i++)
                    touchEventPool.free(touchEvents.Get(i));
                touchEvents.Clear();
                touchEvents.AddAll(touchEventsBuffer);
                touchEventsBuffer.Clear();
                return touchEvents;
            }
        }
    }
}