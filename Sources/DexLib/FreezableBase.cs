using System;
using System.Threading;

namespace Dot42.DexLib
{
    public class ObjectFrozenException : Exception
    {
        public ObjectFrozenException() : base("Attempt to modify object that has been frozen.")
        {
        }    
    }

    public class FreezableBase
    {
        private int _freezeCount;
        /// <summary>
        /// returns true if the object is in a frozen state i.e. is unmodifiable.
        /// </summary>
        public bool IsFrozen { get { return _freezeCount > 0; } }

        /// <summary>
        /// Freeze the object. Further modifications are not possible, 
        /// and internal optimizations can occur.
        /// </summary>
        /// <returns>true if the object was unfrozen before.</returns>
        public virtual bool Freeze()
        {
            return Interlocked.Increment(ref _freezeCount) == 1;
        }

        /// <summary>
        /// Unfreezes the object. Further modifications are possible,
        /// if unfreeze has been called the same times than freeze
        /// on this object.
        /// </summary>
        /// <returns>true if the object is now unfrozen.</returns>
        public virtual bool Unfreeze()
        {
            int ret = Interlocked.Decrement(ref _freezeCount);
            if(ret < 0)
                throw new InvalidOperationException("can not unfreeze more than freeze");
            return ret == 0;
        }

        protected void ThrowIfFrozen()
        {
            if(IsFrozen)
                throw new ObjectFrozenException();
        }

    }
}
