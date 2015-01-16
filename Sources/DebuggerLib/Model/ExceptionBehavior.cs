using System;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Control how the debugger behaves on a specific exception.
    /// </summary>
    public sealed class ExceptionBehavior
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ExceptionBehavior(string exceptionName, bool stopOnThrow, bool stopUncaught)
        {
            if (string.IsNullOrEmpty(exceptionName))
                throw new ArgumentNullException("exceptionName");
            StopUncaught = stopUncaught;
            StopOnThrow = stopOnThrow;
            ExceptionName = exceptionName;
        }

        /// <summary>
        /// Name of the exception this behavior is about.
        /// </summary>
        public string ExceptionName { get; private set; }

        /// <summary>
        /// If set, the process will stop when the exception is thrown.
        /// </summary>
        public bool StopOnThrow { get; private set; }

        /// <summary>
        /// If set, the process will stop when the exception is not caught.
        /// </summary>
        public bool StopUncaught { get; private set; }
    }
}
