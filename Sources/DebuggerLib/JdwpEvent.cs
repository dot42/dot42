using System;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Base class for JDWP originating events
    /// </summary>
    public abstract class JdwpEvent : EventArgs
    {
        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public abstract TResult Accept<TResult, TData>(EventVisitor<TResult, TData> visitor, TData data);
    }
}
