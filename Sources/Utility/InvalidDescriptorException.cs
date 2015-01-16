namespace Dot42.Utility
{
    public class InvalidDescriptorException : Dot42Exception 
    {
        public InvalidDescriptorException(string descriptor)
            : base(string.Format("Invalid descriptor: {0}", descriptor))
        {            
        }
    }
}
