using System;

namespace TallComponents.Common.Extensions
{
    public static class EventHandlerExtensions
    {
        /// <summary>
        /// Fire the given handler if not null.
        /// </summary>
        public static void Fire(this EventHandler handler, object sender, EventArgs e)
        {
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Fire the given handler if not null using EventArgs.Empty
        /// </summary>
        public static void Fire(this EventHandler handler, object sender)
        {
            if (handler != null)
            {
                handler(sender, EventArgs.Empty);
            }
        }
        /// <summary>
        /// Fire the given handler if not null.
        /// </summary>
        public static void Fire<T>(this EventHandler<T> handler, object sender, T e)
            where T : EventArgs
        {
            if (handler != null)
            {
                handler(sender, e);
            }
        }
    }
}
