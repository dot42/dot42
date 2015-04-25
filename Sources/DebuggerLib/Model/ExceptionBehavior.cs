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

        #region Equality
        private bool Equals(ExceptionBehavior other)
        {
            return string.Equals(ExceptionName, other.ExceptionName) && StopOnThrow == other.StopOnThrow && StopUncaught == other.StopUncaught;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ExceptionBehavior && Equals((ExceptionBehavior) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ExceptionName != null ? ExceptionName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ StopOnThrow.GetHashCode();
                hashCode = (hashCode*397) ^ StopUncaught.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}
