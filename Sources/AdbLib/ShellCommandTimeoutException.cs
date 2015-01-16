namespace Dot42.AdbLib
{
    public class ShellCommandTimeoutException : AdbException
    {
        public ShellCommandTimeoutException() : base(string.Empty)
        {
        }
    }
}
