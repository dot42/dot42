namespace Dot42.WcfTools
{
    /// <summary>
    /// Custom attribute constants used in WCF
    /// </summary>
    internal static class WcfAttributeConstants
    {
        public const string OperationContractAttribute = "System.ServiceModel.OperationContractAttribute";
        public const string ServiceContractAttribute = "System.ServiceModel.ServiceContractAttribute";
        public const string XmlSerializerFormatAttribute = "System.ServiceModel.XmlSerializerFormatAttribute";
        public const string DataContractFormatAttribute = "System.ServiceModel.DataContractFormatAttribute";

        public const string WebGetAttribute = "System.ServiceModel.Web.WebGetAttribute";
        public const string WebInvokeAttribute = "System.ServiceModel.Web.WebInvokeAttribute";

        public const string Method = "Method";
        public const string Method_Post = "POST";
        public const string Method_Put = "PUT";
        public const string Method_Delete = "DELETE";

        public const string UriTemplate = "UriTemplate";

        public const string RequestFormat = "RequestFormat";
        public const string ResponseFormat = "ResponseFormat";
    }
}
