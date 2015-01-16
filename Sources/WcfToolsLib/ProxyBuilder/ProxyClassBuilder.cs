using System;
using System.Collections.Generic;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace Dot42.WcfTools.ProxyBuilder
{
    /// <summary>
    /// Build a proxy class for a specific ServiceContract interface.
    /// </summary>
    internal class ProxyClassBuilder
    {
        private readonly TypeDefinition interfaceType;
        private List<ProxyMethodBuilder> methodBuilders;
        private string proxyTypeName;
        private SerializationFormat serializationFormat;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ProxyClassBuilder(TypeDefinition interfaceType)
        {
            this.interfaceType = interfaceType;
        }

        /// <summary>
        /// Create all internal structures.
        /// </summary>
        public void Create(ProxySerializationContext context)
        {
            ProcessSerializationAttribute();

            // Create type name of proxy class
            proxyTypeName = interfaceType.Name + "__WcfProxy";

            // Collect all methods for which we have to build a proxy
            var operationMethods = new List<MethodDefinition>();
            var otherMethods = new List<MethodDefinition>();
            var processed = new HashSet<TypeDefinition>();
            CollectOperationMethods(operationMethods, otherMethods, interfaceType, processed);

            // Prepare method proxy builders
            methodBuilders = new List<ProxyMethodBuilder>();
            methodBuilders.AddRange(operationMethods.Select(x => new ProxyOperationContractMethodBuilder(this, x)));
            methodBuilders.AddRange(otherMethods.Select(x => new ProxyNotSupportedMethodBuilder(this, x)));

            // Prepare internal structures of method builders
            methodBuilders.ForEach(x => x.Create(context));
        }

        private void ProcessSerializationAttribute()
        {
            // Find all format attributes
            var xmlSerializerFormatAttribute = interfaceType.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == WcfAttributeConstants.XmlSerializerFormatAttribute);
            var dataContractFormatAttribute = interfaceType.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == WcfAttributeConstants.DataContractFormatAttribute);

            if (xmlSerializerFormatAttribute != null)
            {
                if(dataContractFormatAttribute!=null) throw new NotSupportedException(
                        string.Format("Both XmlSerializerFormat and DataContractFormat attributes are specified on the same interface '{0}'",
                                      interfaceType.FullName));

                serializationFormat = SerializationFormat.XmlSerializer;
            }
            else
            {
                //DataConteract is the default when no attribute is specified as well.
                serializationFormat = SerializationFormat.DataContract;
            }

        }

        /// <summary>
        /// Create the entire proxy class as C# code.
        /// </summary>
        public void Generate(CodeGenerator generator)
        {
            // Prepare type
            var typeDecl = new CodeTypeDeclaration(proxyTypeName);
            typeDecl.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            typeDecl.BaseTypes.Add("Dot42.Internal.WcfProxyBase");
            typeDecl.BaseTypes.Add(interfaceType.FullName);
            typeDecl.CustomAttributes.Add(new CodeAttributeDeclaration("Dot42.Include", new CodeAttributeArgument("ApplyToMembers", new CodePrimitiveExpression(true))));

            var methodDecl = new CodeConstructor();
            methodDecl.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(typeof(System.Uri), "baseAddress"));
            typeDecl.Members.Add(methodDecl);

            methodDecl.Statements.Add( new CodeAssignStatement( 
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "baseAddress"), new CodeTypeReferenceExpression("baseAddress")));

            // Generate method code
            methodBuilders.ForEach(x => x.Generate(typeDecl, generator));

            generator.Add(typeDecl);
        }

        /// <summary>
        /// Collect all operation methods in the given interface and all it's implemented interfaces.
        /// </summary>
        private static void CollectOperationMethods(List<MethodDefinition> operationMethods, List<MethodDefinition> otherMethods, TypeDefinition interfaceType, HashSet<TypeDefinition> processed)
        {
            // Avoid visiting interfaces more than once
            if (!processed.Add(interfaceType))
                return;

            // Find all OperationContract methods
            operationMethods.AddRange(interfaceType.Methods.Where(m => m.HasCustomAttributes && m.CustomAttributes.Any(x => x.AttributeType.FullName == WcfAttributeConstants.OperationContractAttribute)));

            // Find all other methods
            otherMethods.AddRange(interfaceType.Methods.Except(operationMethods));

            // Recurve into all implemented interfaces
            foreach (var intfRef in interfaceType.Interfaces)
            {
                var intf = intfRef.Interface.GetElementType().Resolve();
                if (intf != null)
                {
                    CollectOperationMethods(operationMethods, otherMethods, intf, processed);
                }
            }
        }

        public SerializationFormat SerializationFormat
        {
            get { return serializationFormat; }
        }
    }
}
