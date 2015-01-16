namespace Dot42.AdbLib
{
    public class AdbCommandRejectedException : AdbException
    {
        private readonly bool errorDuringDeviceSelection;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AdbCommandRejectedException(string message)
            : this(message, false)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public AdbCommandRejectedException(string message, bool errorDuringDeviceSelection)
            : base(message)
        {
            this.errorDuringDeviceSelection = errorDuringDeviceSelection;
        }

        public bool ErrorDuringDeviceSelection
        {
            get { return errorDuringDeviceSelection; }
        }
    }
}
