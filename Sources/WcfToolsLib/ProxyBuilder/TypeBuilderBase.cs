using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dot42.WcfTools.Extensions;
using Mono.Cecil;

namespace Dot42.WcfTools.ProxyBuilder
{
    internal class TypeBuilderBase
    {
        internal readonly TypeDefinition type;
        internal readonly string methodName;

        internal XmlBaseInfo Xml { get; private set; }
        internal DataBaseInfo Data { get; private set; }

        internal class XmlBaseInfo
        {
            internal string Name { get; set; }
            internal string Namespace { get; set; }
        }

        internal class DataBaseInfo
        {
            internal bool IsDataContract { get; set; }
            internal string Name { get; set; }
            internal string Namespace { get; set; }
            internal bool IsReference { get; set; }
        }

        public TypeBuilderBase(TypeDefinition type, string methodName)
        {
            this.type = type;
            this.methodName = methodName;

            Xml = new XmlBaseInfo();
            Data = new DataBaseInfo();

            FillBuilderBase(type);
        }

        protected class BuilderHelper
        {
            internal class XmlInfo
            {
                internal bool IsOptional { get; set; }
                internal string ArrayItemName { get; set; }
                internal bool UsesAttribute { get; set; }
                internal string Name { get; set; }
                internal string Namespace { get; set; }
                internal string DataType { get; set; }
            }

            internal class DataInfo
            {
                public DataInfo()
                {
                    Namespace = null;
                    IsRequired = false;
                    Order = int.MinValue;
                    EmitDefaultValue = true;
                }

                internal string Name { get; set; }
                internal string Namespace { get; set; }
                internal bool IsRequired { get; set; }
                internal int Order { get; set; }
                internal bool EmitDefaultValue { get; set; }
            }

            public BuilderHelper()
            {
                Xml = new XmlInfo();
                Data = new DataInfo();
            }

            //Set default name for both xml and data.
            internal string PropertyName 
            { 
                set
                {
                    Xml.Name = value;
                    Data.Name = value;

                }
            }
            internal bool IsArray { get; set; }
            internal bool IsComplexType { get; set; }
            internal XmlInfo Xml { get; private set; }
            internal DataInfo Data { get; private set; }  
        }

        private void FillBuilderBase(TypeDefinition type)
        {
            var dataContractAttribute = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == DataAttributeConstants.DataContractAttribute);
            if (dataContractAttribute != null)
            {
                Data.IsDataContract = true;
                Data.Name = GetName(dataContractAttribute, Data.Name);
                Data.Namespace = GetNamespaceData(dataContractAttribute, Data.Namespace);
                Data.IsReference = GetIsReference(dataContractAttribute, Data.IsReference);
            }
        }

        /// <summary>
        /// Gets the name of the [de-]serializer method.
        /// </summary>
        public string MethodName { get { return methodName; } }

        protected static bool Ignore(PropertyDefinition property)
        {
            if (property.GetMethod != null && property.SetMethod != null)
            {
                var xmlIgnoreAttribute = property.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == XmlAttributeConstants.XmlIgnoreAttribute);
                if (xmlIgnoreAttribute == null)
                {
                    return false;
                }
            }

            return true;
        }

        protected static void FillBuilderHelper(TypeDefinition type, PropertyDefinition property, BuilderHelper builderHelper)
        {
            builderHelper.IsArray = property.PropertyType.IsArray;
            builderHelper.IsComplexType = IsComplexType(property.PropertyType.Resolve());

            FillBuilderHelperXml(type, property, builderHelper);
            FillBuilderHelperData(type, property, builderHelper);
        }

        protected bool IsEnum()
        {
            return type.IsEnum;
        }

        protected bool IsComplexType()
        {
            return IsComplexType(type);
        }

        private static bool IsComplexType(TypeDefinition typeDefinition)
        {
            if (typeDefinition.IsPrimitive) return false;
            if (typeDefinition.IsEnum) return false;
            if (typeDefinition.FullName == "System.String") return false;
            if (typeDefinition.FullName == "System.DateTime") return false;
            if (typeDefinition.FullName == "System.TimeSpan") return false;
            if (typeDefinition.FullName == "System.Guid") return false;

            return true;
        }

        private static void FillBuilderHelperXml(TypeDefinition type, PropertyDefinition property, BuilderHelper builderHelper)
        {
            if (type.Properties.Count(p => p.Name == property.Name + XmlAttributeConstants.SpecifiedPostFix) == 1)
            {
                //optional field
                builderHelper.Xml.IsOptional = true;
            }

            var xmlTypeAttribute = property.DeclaringType.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == XmlAttributeConstants.XmlTypeAttribute);
            if (xmlTypeAttribute != null)
            {
                builderHelper.Xml.Namespace = GetNamespaceXml(xmlTypeAttribute, builderHelper.Xml.Namespace);
            }

            var xmlAttributeAttribute = property.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == XmlAttributeConstants.XmlAttributeAttribute);
            if (xmlAttributeAttribute != null)
            {
                builderHelper.Xml.UsesAttribute = true;

                builderHelper.Xml.Name = GetAttributeName(xmlAttributeAttribute, builderHelper.Xml.Name);
            }

            var xmlElementAttribute = property.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == XmlAttributeConstants.XmlElementAttribute);
            if (xmlElementAttribute != null)
            {
                builderHelper.Xml.Name = GetElementName(xmlElementAttribute, builderHelper.Xml.Name);
                builderHelper.Xml.DataType = GetDataType(xmlElementAttribute, builderHelper.Xml.DataType);
            }

            var xmlArrayItemAttribute = property.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == XmlAttributeConstants.XmlArrayItemAttribute);
            if (xmlArrayItemAttribute != null)
            {
                builderHelper.Xml.ArrayItemName = GetElementName(xmlArrayItemAttribute, builderHelper.Xml.ArrayItemName);
            }
        }

        private static void FillBuilderHelperData(TypeDefinition type, PropertyDefinition property, BuilderHelper builderHelper)
        {
            var dataMemberAttribute = property.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == DataAttributeConstants.DataMemberAttribute);
            if (dataMemberAttribute != null)
            {
                builderHelper.Data.Name = GetName(dataMemberAttribute, builderHelper.Data.Name);
                builderHelper.Data.Namespace = GetNamespaceData(dataMemberAttribute, builderHelper.Data.Namespace);
                builderHelper.Data.EmitDefaultValue = GetEmitDefaultValue(dataMemberAttribute, builderHelper.Data.EmitDefaultValue);
                builderHelper.Data.IsRequired = GetIsRequired(dataMemberAttribute, builderHelper.Data.IsRequired);
                builderHelper.Data.Order = GetOrder(dataMemberAttribute, builderHelper.Data.Order);
            }
        }

        private static string GetNamespaceXml(CustomAttribute customAttribute, string defaultName)
        {
            var namespaceProperty = customAttribute.GetStringProperty(XmlAttributeConstants.Namespace);
            if (namespaceProperty != null)
            {
                return namespaceProperty;
            }

            return defaultName;
        }

        private static string GetAttributeName(CustomAttribute customAttribute, string defaultName)
        {
            var nameProperty = customAttribute.GetStringProperty(XmlAttributeConstants.AttributeName);
            if (nameProperty != null)
            {
                return nameProperty;
            }

            return defaultName;
        }

        private static string GetDataType(CustomAttribute customAttribute, string defaultName)
        {
            var nameProperty = customAttribute.GetStringProperty(XmlAttributeConstants.DataType);
            if (nameProperty != null)
            {
                return nameProperty;
            }

            return defaultName;
        }

        private static string GetElementName(CustomAttribute customAttribute, string defaultName)
        {
            var nameProperty = customAttribute.GetStringProperty(XmlAttributeConstants.ElementName);
            if (nameProperty != null)
            {
                return nameProperty;
            }

            switch (customAttribute.Constructor.ToString())
            {
                case "System.Void System.Xml.Serialization.XmlElementAttribute::.ctor(System.String)":
                    return customAttribute.ConstructorArguments[0].Value as string;

                case "System.Void System.Xml.Serialization.XmlElementAttribute::.ctor(System.String,System.Type)":
                    return customAttribute.ConstructorArguments[0].Value as string;

                case "System.Void System.Xml.Serialization.XmlArrayItemAttribute::.ctor(System.String)":
                    return customAttribute.ConstructorArguments[0].Value as string;

                case "System.Void System.Xml.Serialization.XmlArrayItemAttribute::.ctor(System.String,System.Type)":
                    return customAttribute.ConstructorArguments[0].Value as string;

            }

            return defaultName;
        }

        private static string GetName(CustomAttribute customAttribute, string defaultValue)
        {
            var namespaceProperty = customAttribute.GetStringProperty(DataAttributeConstants.Name);
            if (namespaceProperty != null)
            {
                return namespaceProperty;
            }

            return defaultValue;
        }

        private static string GetNamespaceData(CustomAttribute customAttribute, string defaultValue)
        {
            var namespaceProperty = customAttribute.GetStringProperty(DataAttributeConstants.Namespace);
            if (namespaceProperty != null)
            {
                return namespaceProperty;
            }

            return defaultValue;
        }

        private static bool GetIsRequired(CustomAttribute customAttribute, bool defaultValue)
        {
            var isRequiredProperty = customAttribute.GetBooleanProperty(DataAttributeConstants.IsRequired);
            if (isRequiredProperty.HasValue)
            {
                return isRequiredProperty.Value;
            }

            return defaultValue;
        }

        private static bool GetEmitDefaultValue(CustomAttribute customAttribute, bool defaultValue)
        {
            var emitDefaultValueProperty = customAttribute.GetBooleanProperty(DataAttributeConstants.EmitDefaultValue);
            if (emitDefaultValueProperty.HasValue)
            {
                return emitDefaultValueProperty.Value;
            }

            return defaultValue;
        }

        private static int GetOrder(CustomAttribute customAttribute, int defaultValue)
        {
            var orderProperty = customAttribute.GetIntProperty(DataAttributeConstants.Order);
            if (orderProperty.HasValue)
            {
                return orderProperty.Value;
            }

            return defaultValue;
        }

        private static bool GetIsReference(CustomAttribute customAttribute, bool defaultValue)
        {
            var isReferenceProperty = customAttribute.GetBooleanProperty(DataAttributeConstants.IsReference);
            if (isReferenceProperty.HasValue)
            {
                return isReferenceProperty.Value;
            }

            return defaultValue;
        }
        
    }

}
