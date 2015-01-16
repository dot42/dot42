using System;

namespace Dot42.Utility
{
    public class EventArgs<T> : EventArgs
    {
        private readonly T data;

        /// <summary>
        /// Default ctor
        /// </summary>
        public EventArgs(T data)
        {
            this.data = data;
        }

        /// <summary>
        /// Payload.
        /// </summary>
        public T Data
        {
            get { return data; }
        }
    }
}
