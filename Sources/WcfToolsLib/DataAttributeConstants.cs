namespace Dot42.WcfTools
{
    /// <summary>
    /// Custom attribute constants used in data format
    /// </summary>
    internal static class DataAttributeConstants
    {
        public const string DataContractAttribute = "System.Runtime.Serialization.DataContractAttribute";
        public const string DataMemberAttribute = "System.Runtime.Serialization.DataMemberAttribute";

        public const string Name = "Name";
        public const string Namespace = "Namespace";

        public const string IsReference = "IsReference";

        public const string EmitDefaultValue = "EmitDefaultValue";
        public const string IsRequired = "IsRequired";
        public const string Order = "Order";
    }
}
