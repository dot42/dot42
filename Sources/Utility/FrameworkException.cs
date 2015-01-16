namespace Dot42.Utility
{
    public class FrameworkException : Dot42Exception 
    {
        public FrameworkException(string message)
            : base(string.Format("Error in framework: {0}", message))
        {            
        }
    }
}
