namespace Dot42.BarDeployLib
{
    public class LoginAuthException: BarDeployException
    {
        public LoginAuthException(int failedAttempts, int retriesRemaining)
            : base(string.Format("Authentication failure: {0} failed attempts, {1} retries remaining", failedAttempts, retriesRemaining))
        {
        }
    }
}
