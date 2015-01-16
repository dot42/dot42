namespace Dot42.DebuggerLib
{
    public class JdwpException : DebuggerException
    {
        private readonly int errorCode;

        /// <summary>
        /// Default ctor
        /// </summary>
        public JdwpException(int errorCode)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public JdwpException(int errorCode, string message) : base(message)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// See <see cref="Jdwp.ErrorCodes"/>
        /// </summary>
        public int ErrorCode
        {
            get { return errorCode; }
        }
    }
}
