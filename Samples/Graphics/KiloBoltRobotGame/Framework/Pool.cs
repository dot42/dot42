using Java.Util;

namespace KiloBoltRobotGame.Framework
{
    public class Pool<T>
    {
        public interface IPoolObjectFactory<T>
        {
            T createObject();
        }

        private readonly IList<T> freeObjects;
        private readonly IPoolObjectFactory<T> factory;
        private readonly int maxSize;

        public Pool(IPoolObjectFactory<T> factory, int maxSize)
        {
            this.factory = factory;
            this.maxSize = maxSize;
            this.freeObjects = new ArrayList<T>(maxSize);
        }

        public T newObject()
        {
            T @object = default(T);

            if (freeObjects.Size() == 0)
                @object = factory.createObject();
            else
                @object = freeObjects.Remove(freeObjects.Size() - 1);

            return @object;
        }

        public void free(T @object)
        {
            if (freeObjects.Size() < maxSize)
                freeObjects.Add(@object);
        }
    }
}