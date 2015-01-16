namespace KiloBoltRobotGame.Framework
{
    internal class TouchEventFactory : Pool<TouchEvent>.IPoolObjectFactory<TouchEvent>
    {
        public TouchEvent createObject()
        {
            return new TouchEvent();
        }
    }
}