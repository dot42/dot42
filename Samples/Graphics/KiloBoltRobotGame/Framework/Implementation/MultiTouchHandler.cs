using Android.View;
using Java.Util;

namespace KiloBoltRobotGame.Framework
{
    public class MultiTouchHandler : ITouchHandler
    {
        private const int MAX_TOUCHPOINTS = 10;

        private readonly bool[] isTouched = new bool[MAX_TOUCHPOINTS];
        private readonly int[] touchX = new int[MAX_TOUCHPOINTS];
        private readonly int[] touchY = new int[MAX_TOUCHPOINTS];
        private readonly int[] id = new int[MAX_TOUCHPOINTS];
        private readonly Pool<TouchEvent> touchEventPool;
        private readonly IList<TouchEvent> touchEvents = new ArrayList<TouchEvent>();
        private readonly IList<TouchEvent> touchEventsBuffer = new ArrayList<TouchEvent>();
        private readonly float scaleX;
        private readonly float scaleY;

        public MultiTouchHandler(View view, float scaleX, float scaleY)
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
                int action = @event.GetAction() & MotionEvent.ACTION_MASK;
                int pointerIndex = (@event.GetAction() & MotionEvent.ACTION_POINTER_ID_MASK) >>
                                   MotionEvent.ACTION_POINTER_ID_SHIFT;
                int pointerCount = @event.GetPointerCount();
                TouchEvent touchEvent;
                for (int i = 0; i < MAX_TOUCHPOINTS; i++)
                {
                    if (i >= pointerCount)
                    {
                        isTouched[i] = false;
                        id[i] = -1;
                        continue;
                    }
                    int pointerId = @event.GetPointerId(i);
                    if (@event.GetAction() != MotionEvent.ACTION_MOVE && i != pointerIndex)
                    {
                        // if it's an up/down/cancel/out event, mask the id to see if we should process it for this touch
                        // point
                        continue;
                    }
                    switch (action)
                    {
                        case MotionEvent.ACTION_DOWN:
                        case MotionEvent.ACTION_POINTER_DOWN:
                            touchEvent = touchEventPool.newObject();
                            touchEvent.type = TouchEvent.TOUCH_DOWN;
                            touchEvent.pointer = pointerId;
                            touchEvent.x = touchX[i] = (int) (@event.GetX(i)*scaleX);
                            touchEvent.y = touchY[i] = (int) (@event.GetY(i)*scaleY);
                            isTouched[i] = true;
                            id[i] = pointerId;
                            touchEventsBuffer.Add(touchEvent);
                            break;

                        case MotionEvent.ACTION_UP:
                        case MotionEvent.ACTION_POINTER_UP:
                        case MotionEvent.ACTION_CANCEL:
                            touchEvent = touchEventPool.newObject();
                            touchEvent.type = TouchEvent.TOUCH_UP;
                            touchEvent.pointer = pointerId;
                            touchEvent.x = touchX[i] = (int) (@event.GetX(i)*scaleX);
                            touchEvent.y = touchY[i] = (int) (@event.GetY(i)*scaleY);
                            isTouched[i] = false;
                            id[i] = -1;
                            touchEventsBuffer.Add(touchEvent);
                            break;

                        case MotionEvent.ACTION_MOVE:
                            touchEvent = touchEventPool.newObject();
                            touchEvent.type = TouchEvent.TOUCH_DRAGGED;
                            touchEvent.pointer = pointerId;
                            touchEvent.x = touchX[i] = (int) (@event.GetX(i)*scaleX);
                            touchEvent.y = touchY[i] = (int) (@event.GetY(i)*scaleY);
                            isTouched[i] = true;
                            id[i] = pointerId;
                            touchEventsBuffer.Add(touchEvent);
                            break;
                    }
                }
                return true;
            }
        }

        public bool isTouchDown(int pointer)
        {
            lock (this)
            {
                int index = getIndex(pointer);
                if (index < 0 || index >= MAX_TOUCHPOINTS)
                    return false;
                else
                    return isTouched[index];
            }
        }

        public int getTouchX(int pointer)
        {
            lock (this)
            {
                int index = getIndex(pointer);
                if (index < 0 || index >= MAX_TOUCHPOINTS)
                    return 0;
                else
                    return touchX[index];
            }
        }

        public int getTouchY(int pointer)
        {
            lock (this)
            {
                int index = getIndex(pointer);
                if (index < 0 || index >= MAX_TOUCHPOINTS)
                    return 0;
                else
                    return touchY[index];
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

        // returns the index for a given pointerId or -1 if no index.
        private int getIndex(int pointerId)
        {
            for (int i = 0; i < MAX_TOUCHPOINTS; i++)
            {
                if (id[i] == pointerId)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}