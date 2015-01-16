using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Web;
using Dot42.Cecil;
using Dot42.WcfTools.Extensions;
using Mono.Cecil;

namespace Dot42.WcfTools.ProxyBuilder
{
    /// <summary>
    /// Build a proxy method for a specific OperationContract interface method.
    /// </summary>
    internal sealed class ProxyOperationContractMethodBuilder : ProxyMethodBuilder
    {
        private enum WebMode
        {
            [Obfuscation]
            None,
            [Obfuscation]
            Get,
            [Obfuscation]
            Put,
            [Obfuscation]
            Delete,
            [Obfuscation]
            Post
        }

        private WebMode webMode;
        private string uriTemplate;
        private readonly HashSet<string> inputArguments = new HashSet<string>();
        private string toSerializeArgument;

        private TypeSerializerBuilder typeSerializerBuilder;
        private bool serializerTypeIsArray;
        private TypeDeserializerBuilder typeDeserializerBuilder;
        private bool deserializerTypeIsArray;

        private SerializationFormat serializationFormat;
        private WebMessageFormat requestFormat = WebMessageFormat.Xml;
        private WebMessageFormat responseFormat = WebMessageFormat.Xml;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ProxyOperationContractMethodBuilder(ProxyClassBuilder parent, MethodDefinition interfaceMethod)
            : base(parent, interfaceMethod)
        {
        }

        /// <summary>
        /// Create all internal structures.
        /// </summary>
        public override void Create(ProxySerializationContext context)
        {
            ProcessSerializationAttribute();
            ProcessWebAttribute();
            ProcessInputArguments();
            ProcessSerializers(context);
        }

        #region web attributes
       
        private void ProcessWebAttribute()
        {
            // Find all web attributess
            var webGetAttribute = InterfaceMethod.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == WcfAttributeConstants.WebGetAttribute);
            var webInvokeAttribute = InterfaceMethod.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == WcfAttributeConstants.WebInvokeAttribute);

            if (webGetAttribute == null)
            {
                if (webInvokeAttribute == null)
                {
                    webMode = WebMode.None;
                }
                else
                {
                    webMode = GetWebMode(InterfaceMethod, webInvokeAttribute);
                    requestFormat = GetRequestFormat(InterfaceMethod, webInvokeAttribute);
                    responseFormat = GetResponseFormat(InterfaceMethod, webInvokeAttribute);
                    uriTemplate = GetUriTemplate(InterfaceMethod, webInvokeAttribute);
                }
            }
            else
            {
                if (webInvokeAttribute == null)
                {
                    webMode = WebMode.Get;
                    responseFormat = GetResponseFormat(InterfaceMethod, webGetAttribute);
                    uriTemplate = GetUriTemplate(InterfaceMethod, webGetAttribute);
                }
                else
                {
                    throw new NotSupportedException(
                        string.Format("Both WebGet and WebInvoke attributes are specified on the same method '{0}'",
                                      InterfaceMethod.FullName));
                }
            }
        }

        private void ProcessSerializationAttribute()
        {
            // Find all format attributes
            var xmlSerializerFormatAttribute = InterfaceMethod.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == WcfAttributeConstants.XmlSerializerFormatAttribute);
            var dataContractFormatAttribute = InterfaceMethod.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == WcfAttributeConstants.DataContractFormatAttribute);

            if (xmlSerializerFormatAttribute != null)
            {
                if (dataContractFormatAttribute != null) throw new NotSupportedException(
                           string.Format("Both XmlSerializerFormat and DataContractFormat attributes are specified on the same method '{0}'",
                                         InterfaceMethod.FullName));

                serializationFormat = SerializationFormat.XmlSerializer;
            }
            else
            {
                if (dataContractFormatAttribute != null)
                {
                    serializationFormat = SerializationFormat.DataContract;
                }
                else
                {
                    serializationFormat = Parent.SerializationFormat;
                }
            }

        }

        private static WebMode GetWebMode(MethodDefinition interfaceMethod, CustomAttribute customAttribute)
        {
            WebMode webMode;

            var methodProperty =customAttribute.Properties.FirstOrDefault(x => x.Name == WcfAttributeConstants.Method);
            if (methodProperty.Name == WcfAttributeConstants.Method)
            {
                var argument = (string) methodProperty.Argument.Value;
                switch (argument.ToUpper())
                {
                    case WcfAttributeConstants.Method_Post:
                        webMode = WebMode.Post;
                        break;

                    case WcfAttributeConstants.Method_Put:
                        webMode = WebMode.Put;
                        break;

                    case WcfAttributeConstants.Method_Delete:
                        webMode = WebMode.Delete;
                        break;

                    default:
                        throw new NotSupportedException(
                            string.Format("WebInvoke Method '{0}' is not supported on '{1}'", argument,
                                            interfaceMethod.FullName));
                }
            }
            else
            {
                //default 
                webMode = WebMode.Post;
            }

            return webMode;
        }

        private static WebMessageFormat GetRequestFormat(MethodDefinition interfaceMethod, CustomAttribute customAttribute)
        {
            return customAttribute.GetWebMessageFormatProperty(WcfAttributeConstants.RequestFormat);
        }

        private static WebMessageFormat GetResponseFormat(MethodDefinition interfaceMethod, CustomAttribute customAttribute)
        {
            return customAttribute.GetWebMessageFormatProperty(WcfAttributeConstants.ResponseFormat);
        }

        private static string GetUriTemplate(MethodDefinition interfaceMethod, CustomAttribute customAttribute)
        {
            var uriTemplate = customAttribute.GetStringProperty(WcfAttributeConstants.UriTemplate);

            if(string.IsNullOrWhiteSpace(uriTemplate))
                throw new NotSupportedException(string.Format("WebGet/WebInvoke UriTemplate on '{0}' is not specified.", interfaceMethod.FullName));

            return uriTemplate;
        }
        #endregion

        #region arguments

        private void ProcessInputArguments()
        {
            var variableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var builder = new UriTemplate(uriTemplate);
            foreach (var variableName in builder.PathSegmentVariableNames) variableNames.Add(variableName);
            foreach (var variableName in builder.QueryValueVariableNames) variableNames.Add(variableName);

            var usedParameters = InterfaceMethod.Parameters.Where(p => variableNames.Contains(p.Name)).ToArray();
            if (usedParameters.Any())
            {
                foreach (var usedParameter in usedParameters)
                    inputArguments.Add(usedParameter.Name);
            }

            var nonUsedParameters = InterfaceMethod.Parameters.Where(p => !variableNames.Contains(p.Name)).ToArray();
            if (nonUsedParameters.Any())
            {
                if (nonUsedParameters.Count() > 1) throw new NotSupportedException(string.Format("WebGet/WebInvoke on '{0}' have multiple parameters to serialize, only one is allowed (no support for Wrap).", InterfaceMethod.FullName));

                toSerializeArgument = nonUsedParameters[0].Name;
            }
        }

        #endregion

        #region serializers

        private void ProcessSerializers(ProxySerializationContext context)
        {
            if (InterfaceMethod.ReturnType != null)
            {
                var typeDefinition = InterfaceMethod.ReturnType.Resolve();
                if (typeDefinition.FullName != "System.Void")
                {
                    typeDeserializerBuilder = context.GetDeserializer(typeDefinition);
                    deserializerTypeIsArray = InterfaceMethod.ReturnType.IsArray;
                }
            }

            if (!string.IsNullOrEmpty(toSerializeArgument))
            {
                var parameter = InterfaceMethod.Parameters.First(p => p.Name == toSerializeArgument);
                var typeDefinition = parameter.ParameterType.Resolve();
                typeSerializerBuilder = context.GetSerializer(typeDefinition);
                serializerTypeIsArray = parameter.ParameterType.IsArray;
            }
            
        }

        #endregion

        /// <summary>
        /// Create the entire proxy method as C# code.
        /// </summary>
        public override void Generate(CodeTypeDeclaration declaringType, CodeGenerator generator)
        {
            // Prepare method
            var methodDecl = CreateProxyMethodDeclaration();

            // Add code
            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(UriTemplate)), "uriTemplate", new CodeObjectCreateExpression(typeof(UriTemplate),
                                                                           new CodePrimitiveExpression(uriTemplate))));

            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(NameValueCollection)), "nameValues", new CodeObjectCreateExpression(typeof(NameValueCollection))));

            foreach (var inputArgument in inputArguments)
            {
                methodDecl.Statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("nameValues"), "Add", new CodePrimitiveExpression(inputArgument), new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(inputArgument), "ToString()")));
            }

            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Uri)), "uri", new CodeMethodInvokeExpression( new CodeTypeReferenceExpression("uriTemplate"), "BindByName", new CodeTypeReferenceExpression("baseAddress"), new CodeTypeReferenceExpression("nameValues"))));

            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Stream)), "inStream", new CodeSnippetExpression("null")));
            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Stream)), "outStream", new CodeSnippetExpression("null")));

            var codeTryCatchFinallyStatement = new CodeTryCatchFinallyStatement();
            methodDecl.Statements.Add(codeTryCatchFinallyStatement);

            if (typeSerializerBuilder!=null)
            {
                var arrayPostfix = serializerTypeIsArray ? "Array" : "";
                var serializeCode = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("SerializationHelper"), typeSerializerBuilder.MethodName + arrayPostfix,
                    new CodeTypeReferenceExpression(toSerializeArgument), new CodeTypeReferenceExpression("SerializationHelper.SerializationFormat." + serializationFormat.ToString()), new CodeTypeReferenceExpression("System.ServiceModel.Web.WebMessageFormat." + requestFormat.ToString()));
                codeTryCatchFinallyStatement.TryStatements.Add(new CodeAssignStatement(new CodeTypeReferenceExpression("inStream"), serializeCode));     
            }

            var webCode = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("WebHelper"), "WebInvoke",
                new CodeTypeReferenceExpression("uri"), new CodeTypeReferenceExpression("WebHelper.Verb." + webMode.ToString()), new CodeTypeReferenceExpression("inStream"), new CodeTypeReferenceExpression("System.ServiceModel.Web.WebMessageFormat." + requestFormat.ToString()), new CodeSnippetExpression(typeDeserializerBuilder != null ? "true" : "false"));
            codeTryCatchFinallyStatement.TryStatements.Add(new CodeAssignStatement(new CodeTypeReferenceExpression("outStream"), webCode));

            if (typeDeserializerBuilder != null)
            {
                var arrayPostfix = deserializerTypeIsArray ? "Array" : "";
                codeTryCatchFinallyStatement.TryStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("SerializationHelper"), typeDeserializerBuilder.MethodName + arrayPostfix, new CodeTypeReferenceExpression("outStream"),
                    new CodeTypeReferenceExpression("SerializationHelper.SerializationFormat." + serializationFormat.ToString()), new CodeTypeReferenceExpression("System.ServiceModel.Web.WebMessageFormat." + responseFormat.ToString()))));
            }

            codeTryCatchFinallyStatement.FinallyStatements.Add(new CodeConditionStatement( new CodeSnippetExpression("inStream != null"),new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("inStream"),"Dispose"))));
            codeTryCatchFinallyStatement.FinallyStatements.Add(new CodeConditionStatement(new CodeSnippetExpression("outStream != null"), new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("outStream"), "Dispose"))));


            // Add method to declaring type
            declaringType.Members.Add(methodDecl);
        }
    }
}
