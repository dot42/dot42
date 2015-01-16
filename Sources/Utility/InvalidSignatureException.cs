namespace Dot42.Utility
{
    public class InvalidSignatureException : Dot42Exception 
    {
        public InvalidSignatureException(string signature)
            : base(string.Format("Invalid signature: {0}", signature))
        {            
        }
    }
}
