using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using Mono.Cecil;

namespace Dot42.WcfTools.ProxyBuilder
{
    /// <summary>
    /// Build a serialize method for a specific type.
    /// </summary>
    internal class TypeSerializerBuilder : TypeBuilderBase
    {
        private class SerializerBuilderHelper : BuilderHelper
        {
            internal TypeSerializerBuilder TypeSerializerBuilder { get; set; }
        }

        private readonly Dictionary<string, SerializerBuilderHelper> builders = new Dictionary<string, SerializerBuilderHelper>();

        /// <summary>
        /// Default ctor
        /// </summary>
        internal TypeSerializerBuilder(TypeDefinition type)
            :base(type, "Serialize" + type.FullName.Replace(".", ""))
        {
        }

        /// <summary>
        /// Create all internal structures.
        /// </summary>
        public void Create(ProxySerializationContext context)
        {
            foreach (var property in type.Properties)
            {
                if (!Ignore(property))
                {
                    var builder = context.GetSerializer(property.PropertyType.Resolve());
                    var builderHelper = new SerializerBuilderHelper
                        {
                            TypeSerializerBuilder = builder,
                            PropertyName = property.Name
                        };

                    FillBuilderHelper(type, property, builderHelper);

                    builders.Add(property.Name, builderHelper);
                }
            }

            foreach (var field in type.Fields)
            {
                //Data...
            }
        }

        /// <summary>
        /// Create the entire serialization method as C# code.
        /// </summary>
        public void Generate(CodeTypeDeclaration declaringType, CodeGenerator generator)
        {
            // Add generate method to declaring type
            if (type.FullName == "System.IO.Stream")
            {
                declaringType.Members.Add(GeneratePassthroughStream());
            }
            else if (IsComplexType() || IsEnum())
            {
                declaringType.Members.AddRange(GenerateComplexType());
            }
            //else: this should be a known value type, which are implemented in the base-class.
        }

        private CodeTypeMemberCollection GenerateComplexType()
        {
            var result = new CodeTypeMemberCollection();

            result.Add(GenerateSwitchComplexType(false));
            result.Add(GenerateSwitchComplexType(true));
            //XmlXml
            result.Add(GenerateTypeXmlXml());
            result.Add(GenerateTypeArrayXmlXml());
            result.Add(type.IsEnum ? GenerateEnumTypeXElement() : GenerateTypeXElement());
            //DataXml
            result.Add(GenerateTypeDataXml());
            result.Add(GenerateTypeArrayDataXml());
            //DataJson
            result.Add(GenerateTypeDataJson());
            result.Add(GenerateTypeArrayDataJson());
            if (!type.IsEnum) result.Add(GenerateTypeJSONObject());
            result.Add(GenerateTypeArrayJSONArray());

            return result;
        }

        #region Passthrough stream
        private CodeMemberMethod GeneratePassthroughStream()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName,
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(typeof(Stream));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName), "stream"));
            methodDecl.Parameters.Add(
               new CodeParameterDeclarationExpression(new CodeTypeReference("SerializationFormat"), "serializationFormat"));
            methodDecl.Parameters.Add(
               new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(System.ServiceModel.Web.WebMessageFormat)), "webMessageFormat"));

            methodDecl.Statements.Add(
                new CodeMethodReturnStatement(new CodeVariableReferenceExpression("stream")));

            return methodDecl;
        }
        #endregion

        #region Format switch
        private CodeMemberMethod GenerateSwitchComplexType(bool objIsArray)
        {
            var methodDecl = new CodeMemberMethod
                {
                    Name = methodName + (objIsArray ? "Array" : "" ),
                    Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
                };
            methodDecl.ReturnType = new CodeTypeReference(typeof (Stream));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName + (objIsArray ? "[]" : "")), "obj"));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference("SerializationFormat"), "serializationFormat"));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof (System.ServiceModel.Web.WebMessageFormat)), "webMessageFormat"));

            CodeConditionStatement _if1 = new CodeConditionStatement();
            CodeMethodInvokeExpression _invoke1 = new CodeMethodInvokeExpression();
            CodePropertyReferenceExpression _prop1 = new CodePropertyReferenceExpression();
            _prop1.PropertyName = "DataContract";
            CodeVariableReferenceExpression _arg2 = new CodeVariableReferenceExpression();
            _arg2.VariableName = "SerializationFormat";
            _prop1.TargetObject = _arg2;
            _invoke1.Parameters.Add(_prop1);

            CodeMethodReferenceExpression _Equals_method1 = new CodeMethodReferenceExpression();
            _Equals_method1.MethodName = "Equals";
            CodeVariableReferenceExpression _arg3 = new CodeVariableReferenceExpression();
            _arg3.VariableName = "serializationFormat";
            _Equals_method1.TargetObject = _arg3;
            _invoke1.Method = _Equals_method1;
            _if1.Condition = _invoke1;
            
            CodeConditionStatement _if2 = new CodeConditionStatement();
            CodeMethodInvokeExpression _invoke2 = new CodeMethodInvokeExpression();
            CodePropertyReferenceExpression _prop2 = new CodePropertyReferenceExpression();
            _prop2.PropertyName = "Xml";
            CodeVariableReferenceExpression _arg5 = new CodeVariableReferenceExpression();
            _arg5.VariableName = "System.ServiceModel.Web.WebMessageFormat";
            _prop2.TargetObject = _arg5;
            _invoke2.Parameters.Add(_prop2);

            CodeMethodReferenceExpression _Equals_method2 = new CodeMethodReferenceExpression();
            _Equals_method2.MethodName = "Equals";
            CodeVariableReferenceExpression _arg6 = new CodeVariableReferenceExpression();
            _arg6.VariableName = "webMessageFormat";
            _Equals_method2.TargetObject = _arg6;
            _invoke2.Method = _Equals_method2;
            _if2.Condition = _invoke2;

            _if2.TrueStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, methodName + "__DataXml", new CodeVariableReferenceExpression("obj"))));

            CodeConditionStatement _if3 = new CodeConditionStatement();
            CodeMethodInvokeExpression _invoke3 = new CodeMethodInvokeExpression();
            CodePropertyReferenceExpression _prop3 = new CodePropertyReferenceExpression();
            _prop3.PropertyName = "Json";
            CodeVariableReferenceExpression _arg7 = new CodeVariableReferenceExpression();
            _arg7.VariableName = "System.ServiceModel.Web.WebMessageFormat";
            _prop3.TargetObject = _arg7;
            _invoke3.Parameters.Add(_prop3);

            CodeMethodReferenceExpression _Equals_method3 = new CodeMethodReferenceExpression();
            _Equals_method3.MethodName = "Equals";
            CodeVariableReferenceExpression _arg8 = new CodeVariableReferenceExpression();
            _arg8.VariableName = "webMessageFormat";
            _Equals_method3.TargetObject = _arg8;
            _invoke3.Method = _Equals_method3;
            _if3.Condition = _invoke3;

            _if3.TrueStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, methodName + "__DataJson", new CodeVariableReferenceExpression("obj"))));
            
            CodeThrowExceptionStatement _throw1 = new CodeThrowExceptionStatement();
            CodeObjectCreateExpression _new1 = new CodeObjectCreateExpression();
            CodeTypeReference _NotSupportedException_type1 = new CodeTypeReference("NotSupportedException");
            _new1.CreateType = _NotSupportedException_type1;
            CodeBinaryOperatorExpression _binop1 = new CodeBinaryOperatorExpression();
            CodePrimitiveExpression _value4 = new CodePrimitiveExpression();
            _value4.Value = "Unknown WebMessageFormat found: ";
            _binop1.Left = _value4;
            _binop1.Operator = CodeBinaryOperatorType.Add;
            CodeVariableReferenceExpression _arg9 = new CodeVariableReferenceExpression();
            _arg9.VariableName = "webMessageFormat";
            _binop1.Right = _arg9;
            _new1.Parameters.Add(_binop1);

            _throw1.ToThrow = _new1;
            _if3.FalseStatements.Add(_throw1);

            _if2.FalseStatements.Add(_if3);

            _if1.TrueStatements.Add(_if2);

            CodeConditionStatement _if4 = new CodeConditionStatement();
            CodeMethodInvokeExpression _invoke4 = new CodeMethodInvokeExpression();
            CodePropertyReferenceExpression _prop4 = new CodePropertyReferenceExpression();
            _prop4.PropertyName = "XmlSerializer";
            CodeVariableReferenceExpression _arg10 = new CodeVariableReferenceExpression();
            _arg10.VariableName = "SerializationFormat";
            _prop4.TargetObject = _arg10;
            _invoke4.Parameters.Add(_prop4);

            CodeMethodReferenceExpression _Equals_method4 = new CodeMethodReferenceExpression();
            _Equals_method4.MethodName = "Equals";
            CodeVariableReferenceExpression _arg11 = new CodeVariableReferenceExpression();
            _arg11.VariableName = "serializationFormat";
            _Equals_method4.TargetObject = _arg11;
            _invoke4.Method = _Equals_method4;
            _if4.Condition = _invoke4;
           
            CodeConditionStatement _if5 = new CodeConditionStatement();
            CodeMethodInvokeExpression _invoke5 = new CodeMethodInvokeExpression();
            CodePropertyReferenceExpression _prop5 = new CodePropertyReferenceExpression();
            _prop5.PropertyName = "Xml";
            CodeVariableReferenceExpression _arg13 = new CodeVariableReferenceExpression();
            _arg13.VariableName = "System.ServiceModel.Web.WebMessageFormat";
            _prop5.TargetObject = _arg13;
            _invoke5.Parameters.Add(_prop5);

            CodeMethodReferenceExpression _Equals_method5 = new CodeMethodReferenceExpression();
            _Equals_method5.MethodName = "Equals";
            CodeVariableReferenceExpression _arg14 = new CodeVariableReferenceExpression();
            _arg14.VariableName = "webMessageFormat";
            _Equals_method5.TargetObject = _arg14;
            _invoke5.Method = _Equals_method5;
            _if5.Condition = _invoke5;

            _if5.TrueStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, methodName + "__XmlXml", new CodeVariableReferenceExpression("obj"))));

            CodeConditionStatement _if6 = new CodeConditionStatement();
            CodeMethodInvokeExpression _invoke6 = new CodeMethodInvokeExpression();
            CodePropertyReferenceExpression _prop6 = new CodePropertyReferenceExpression();
            _prop6.PropertyName = "Json";
            CodeVariableReferenceExpression _arg15 = new CodeVariableReferenceExpression();
            _arg15.VariableName = "System.ServiceModel.Web.WebMessageFormat";
            _prop6.TargetObject = _arg15;
            _invoke6.Parameters.Add(_prop6);

            CodeMethodReferenceExpression _Equals_method6 = new CodeMethodReferenceExpression();
            _Equals_method6.MethodName = "Equals";
            CodeVariableReferenceExpression _arg16 = new CodeVariableReferenceExpression();
            _arg16.VariableName = "webMessageFormat";
            _Equals_method6.TargetObject = _arg16;
            _invoke6.Method = _Equals_method6;
            _if6.Condition = _invoke6;
            
            CodeThrowExceptionStatement _throw2 = new CodeThrowExceptionStatement();
            CodeObjectCreateExpression _new2 = new CodeObjectCreateExpression();
            CodeTypeReference _NotSupportedException_type2 = new CodeTypeReference("NotSupportedException");
            _new2.CreateType = _NotSupportedException_type2;
            CodePrimitiveExpression _value6 = new CodePrimitiveExpression();
            _value6.Value = "WebMessageFormat.Json is not supported in relation with XmlSerializerFormatAttribute";
            _new2.Parameters.Add(_value6);

            _throw2.ToThrow = _new2;
            _if6.TrueStatements.Add(_throw2);

            CodeThrowExceptionStatement _throw3 = new CodeThrowExceptionStatement();
            CodeObjectCreateExpression _new3 = new CodeObjectCreateExpression();
            CodeTypeReference _NotSupportedException_type3 = new CodeTypeReference("NotSupportedException");
            _new3.CreateType = _NotSupportedException_type3;
            CodeBinaryOperatorExpression _binop2 = new CodeBinaryOperatorExpression();
            CodePrimitiveExpression _value7 = new CodePrimitiveExpression();
            _value7.Value = "Unknown WebMessageFormat found: ";
            _binop2.Left = _value7;
            _binop2.Operator = CodeBinaryOperatorType.Add;
            CodeVariableReferenceExpression _arg17 = new CodeVariableReferenceExpression();
            _arg17.VariableName = "webMessageFormat";
            _binop2.Right = _arg17;
            _new3.Parameters.Add(_binop2);

            _throw3.ToThrow = _new3;
            _if6.FalseStatements.Add(_throw3);

            _if5.FalseStatements.Add(_if6);

            _if4.TrueStatements.Add(_if5);

            CodeThrowExceptionStatement _throw4 = new CodeThrowExceptionStatement();
            CodeObjectCreateExpression _new4 = new CodeObjectCreateExpression();
            CodeTypeReference _NotSupportedException_type4 = new CodeTypeReference("NotSupportedException");
            _new4.CreateType = _NotSupportedException_type4;
            CodeBinaryOperatorExpression _binop3 = new CodeBinaryOperatorExpression();
            CodePrimitiveExpression _value8 = new CodePrimitiveExpression();
            _value8.Value = "Unknown SerializationFormat found: ";
            _binop3.Left = _value8;
            _binop3.Operator = CodeBinaryOperatorType.Add;
            CodeVariableReferenceExpression _arg18 = new CodeVariableReferenceExpression();
            _arg18.VariableName = "serializationFormat";
            _binop3.Right = _arg18;
            _new4.Parameters.Add(_binop3);

            _throw4.ToThrow = _new4;
            _if4.FalseStatements.Add(_throw4);

            _if1.FalseStatements.Add(_if4);

            methodDecl.Statements.Add(_if1);

            return methodDecl;
        }
        #endregion

        #region XmlXml
        private CodeMemberMethod GenerateTypeXmlXml()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName + "__XmlXml",
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(typeof(Stream));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName), "obj"));
           
            methodDecl.Statements.Add(
               new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(XElement)), "root",new CodeObjectCreateExpression(typeof(XElement),                                  
                   new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(XName)), "Get", new CodePrimitiveExpression(type.Name)))));

            methodDecl.Statements.Add(
                new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(XDocument)), "xDocument",
                    new CodeObjectCreateExpression(typeof(XDocument), new CodeTypeReferenceExpression("root"))));

            methodDecl.Statements.Add(
               new CodeMethodInvokeExpression(null, methodName, new CodeVariableReferenceExpression("obj"), new CodeVariableReferenceExpression("root")));

            methodDecl.Statements.Add(
               new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Stream)), "stream",
                   new CodeMethodInvokeExpression(null,"GetStream", new CodeVariableReferenceExpression("xDocument"))));

            methodDecl.Statements.Add(
                new CodeMethodReturnStatement( new CodeVariableReferenceExpression("stream") ));

            return methodDecl;
        }

        private CodeMemberMethod GenerateTypeArrayXmlXml()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName + "__XmlXml",
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(typeof(Stream));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName + "[]"), "objs"));

            methodDecl.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotSupportedException))));

            return methodDecl;

        }

        private CodeMemberMethod GenerateTypeXElement()
        {
            var methodDecl = new CodeMemberMethod
                {
                    Name = methodName,
                    Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
                };
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName), "obj"));
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(XElement)), "xElement"));

            FillXElementMembers(methodDecl.Statements);
   
            return methodDecl;
        }

        private void FillXElementMembers(CodeStatementCollection statements)
        {
            foreach (var keyValue in builders)
            {
                statements.Add(new CodeCommentStatement(keyValue.Key));

                var emitInitial = true;
                CodeExpression addCodeExpression;
                CodeExpression valueCodeExpression;
                CodeStatement additionalStatement = null;

                if (!keyValue.Value.IsArray && !keyValue.Value.IsComplexType)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value.Xml.DataType))
                    {
                        switch (keyValue.Value.Xml.DataType)
                        {
                            case "base64Binary":
                                valueCodeExpression = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(Convert)), "ToBase64String",
                                                                                 new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("obj"), keyValue.Key));
                                break;

                            default:
                                throw new NotSupportedException("The provided DataType is not supported");
                        }
                    }
                    else
                    {
                        if (keyValue.Value.TypeSerializerBuilder.type.IsEnum)
                        {
                            valueCodeExpression = new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("obj"), keyValue.Key), "ToString");
                        }
                        else if (keyValue.Value.TypeSerializerBuilder.type.FullName == "System.String")
                        {
                            valueCodeExpression = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("obj"), keyValue.Key);
                        }
                        else
                        {
                            valueCodeExpression = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof (XmlConvert)), "ToString",
                                                                                 new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("obj"), keyValue.Key));
                        }
                    }
                }
                else
                {
                    valueCodeExpression = null;
                    
                    if (keyValue.Value.IsArray)
                    {
                        if (string.IsNullOrEmpty(keyValue.Value.Xml.ArrayItemName))
                        {
                            emitInitial = false;
                        }

                        additionalStatement = new CodeIterationStatement(new CodeVariableDeclarationStatement(typeof (int), "i", new CodeSnippetExpression("0")),
                                                                         new CodeSnippetExpression("i < obj." + keyValue.Key + ".Length"), new CodeSnippetStatement("i++"),
                            new CodeSnippetStatement(emitInitial ? "System.Xml.Linq.XElement xParentElement = (System.Xml.Linq.XElement)xElement.LastNode;" : "System.Xml.Linq.XElement xParentElement = xElement;"),
                            new CodeSnippetStatement("xParentElement.Add(new System.Xml.Linq.XElement(System.Xml.Linq.XName.Get(\"" + (emitInitial ? keyValue.Value.Xml.ArrayItemName:keyValue.Key) + "\", \"" + keyValue.Value.Xml.Namespace + "\")));"),
                            new CodeSnippetStatement(keyValue.Value.TypeSerializerBuilder.MethodName + "( obj." + keyValue.Key + "[i], (System.Xml.Linq.XElement)xParentElement.LastNode);"));
                    }
                    else
                    {
                        additionalStatement = new CodeExpressionStatement(new CodeMethodInvokeExpression(null, keyValue.Value.TypeSerializerBuilder.MethodName, 
                            new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("obj"), keyValue.Key),
                            new CodeCastExpression( typeof(XElement), new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("xElement"), "LastNode"))));
                    }
                }

                if (keyValue.Value.Xml.UsesAttribute)
                {
                    addCodeExpression = new CodeObjectCreateExpression(
                        typeof(XAttribute), 
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(typeof(XName)),
                            "Get",
                            new CodePrimitiveExpression(keyValue.Value.Xml.Name)), valueCodeExpression
                        );
                }
                else
                {
                    if (valueCodeExpression == null)
                    {
                        addCodeExpression = new CodeObjectCreateExpression(
                        typeof(XElement),
                         new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(typeof(XName)),
                            "Get",
                            new CodePrimitiveExpression(keyValue.Value.Xml.Name), new CodePrimitiveExpression(keyValue.Value.Xml.Namespace)));
                    }
                    else
                    {
                        addCodeExpression = new CodeObjectCreateExpression(
                            typeof (XElement),
                            new CodeMethodInvokeExpression(
                                new CodeTypeReferenceExpression(typeof(XName)),
                                "Get",
                                new CodePrimitiveExpression(keyValue.Value.Xml.Name), new CodePrimitiveExpression(keyValue.Value.Xml.Namespace)),
                            valueCodeExpression);
                    }
                }

                var propertyStatements = new List<CodeStatement>();

                if (emitInitial)
                    propertyStatements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("xElement"), "Add", addCodeExpression)));
                
                if (additionalStatement != null) propertyStatements.Add(additionalStatement);

                if (keyValue.Value.Xml.IsOptional)
                {
                    statements.Add(
                        new CodeConditionStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("obj"),keyValue.Key + "Specified"), propertyStatements.ToArray()));
                }
                else
                {
                    propertyStatements.ForEach(s => statements.Add(s));
                }
                
            }
        }

        private CodeMemberMethod GenerateEnumTypeXElement()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName,
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName), "obj"));
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(XElement)), "xElement"));

            methodDecl.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotSupportedException))));

            return methodDecl;
        }

        #endregion

        #region DataXml
        private CodeMemberMethod GenerateTypeDataXml()
        {
            var methodDecl = new CodeMemberMethod
                {
                    Name = methodName + "__DataXml",
                    Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
                };
            methodDecl.ReturnType = new CodeTypeReference(typeof (Stream));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName), "obj"));

            methodDecl.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotSupportedException))));

            return methodDecl;
        }

        private CodeMemberMethod GenerateTypeArrayDataXml()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName + "__DataXml",
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(typeof(Stream));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName + "[]"), "objs"));

            methodDecl.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotSupportedException))));

            return methodDecl;
        }
        #endregion

        #region DataJson
        private CodeMemberMethod GenerateTypeDataJson()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName + "__DataJson",
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(typeof(Stream));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName), "obj"));

            if (type.IsEnum)
            {
                methodDecl.Statements.Add(
                   new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Stream)), "stream",
                                                        new CodeMethodInvokeExpression(null, "GetStream",
                                                            new CodeMethodInvokeExpression( new CodeCastExpression(type.Fields[0].FieldType.FullName, new CodeVariableReferenceExpression("obj")), "ToJsonString" ))));

                methodDecl.Statements.Add(
                   new CodeMethodReturnStatement(new CodeVariableReferenceExpression("stream")));
            }
            else
            {
                methodDecl.Statements.Add(
                    new CodeVariableDeclarationStatement(new CodeTypeReference("Org.Json.JSONObject"), "jsonObject", new CodeObjectCreateExpression("Org.Json.JSONObject")));

                methodDecl.Statements.Add(
                    new CodeMethodInvokeExpression(null, methodName, new CodeVariableReferenceExpression("obj"), new CodeVariableReferenceExpression("jsonObject")));

                methodDecl.Statements.Add(
                    new CodeVariableDeclarationStatement(new CodeTypeReference(typeof (Stream)), "stream",
                                                         new CodeMethodInvokeExpression(null, "GetStream", new CodeVariableReferenceExpression("jsonObject"))));

                methodDecl.Statements.Add(
                    new CodeMethodReturnStatement(new CodeVariableReferenceExpression("stream")));
            }

            return methodDecl;
        }

        private CodeMemberMethod GenerateTypeArrayDataJson()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName + "__DataJson",
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(typeof(Stream));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName + "[]"), "objs"));

            methodDecl.Statements.Add(
               new CodeVariableDeclarationStatement(new CodeTypeReference("Org.Json.JSONArray"), "jsonArray", new CodeObjectCreateExpression("Org.Json.JSONArray")));

            methodDecl.Statements.Add(
               new CodeMethodInvokeExpression(null, methodName, new CodeVariableReferenceExpression("objs"), new CodeVariableReferenceExpression("jsonArray")));

            methodDecl.Statements.Add(
               new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Stream)), "stream",
                   new CodeMethodInvokeExpression(null, "GetStream", new CodeVariableReferenceExpression("jsonArray"))));

            methodDecl.Statements.Add(
                new CodeMethodReturnStatement(new CodeVariableReferenceExpression("stream")));

            return methodDecl;
        }

        private CodeMemberMethod GenerateTypeJSONObject()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName,
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName), "obj"));
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("Org.Json.JSONObject"), "jsonObject"));

           FillJSONObjectMembers(methodDecl.Statements);

            return methodDecl;
        }

        private void FillJSONObjectMembers(CodeStatementCollection statements)
        {
            foreach (var keyValue in builders)
            {
                statements.Add(new CodeCommentStatement(keyValue.Key));

                if (!keyValue.Value.IsArray)
                {
                    FillJSONObjectMembersNonArray(keyValue, statements);
                }
                else
                {
                    FillJSONObjectMembersArray(keyValue, statements);    
                }
            }
        }

        private void FillJSONObjectMembersNonArray(KeyValuePair<string, SerializerBuilderHelper> keyValue, CodeStatementCollection statements)
        {
            if (keyValue.Value.IsComplexType)
            {
                var complexName = "complexType_" + keyValue.Key;

                statements.Add(
                    new CodeVariableDeclarationStatement(new CodeTypeReference("Org.Json.JSONObject"), complexName, new CodeObjectCreateExpression("Org.Json.JSONObject")));

                statements.Add(
                    new CodeMethodInvokeExpression(null, "AddToJsonObject", new CodeVariableReferenceExpression("jsonObject"),
                                               new CodePrimitiveExpression(keyValue.Value.Data.Name),
                                               new CodeVariableReferenceExpression(complexName)));

                statements.Add(
                    new CodeMethodInvokeExpression(null, keyValue.Value.TypeSerializerBuilder.MethodName,
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("obj"), keyValue.Key), new CodeVariableReferenceExpression(complexName)));

            }
            else
            {
                if (keyValue.Value.TypeSerializerBuilder.type.IsEnum)
                {
                    statements.Add(
                        new CodeMethodInvokeExpression(null, "AddToJsonObject", new CodeVariableReferenceExpression("jsonObject"),
                                                       new CodePrimitiveExpression(keyValue.Value.Data.Name),
                                                        new CodeCastExpression(keyValue.Value.TypeSerializerBuilder.type.Fields[0].FieldType.FullName, 
                                                           new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("obj"), keyValue.Key ))));
                }
                else
                {
                    statements.Add(
                        new CodeMethodInvokeExpression(null, "AddToJsonObject", new CodeVariableReferenceExpression("jsonObject"),
                                                       new CodePrimitiveExpression(keyValue.Value.Data.Name),
                                                       new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("obj"), keyValue.Key)));
                }
            }
        }

        private void FillJSONObjectMembersArray(KeyValuePair<string,SerializerBuilderHelper> keyValue, CodeStatementCollection statements)
        {
            var arrayName = "array_" + keyValue.Key;

            var condition = new CodeConditionStatement(new CodeSnippetExpression("obj." + keyValue.Key + " != null"));
            statements.Add(condition);

            condition.TrueStatements.Add(
                   new CodeVariableDeclarationStatement(new CodeTypeReference("Org.Json.JSONArray"), arrayName, new CodeObjectCreateExpression("Org.Json.JSONArray")));

            condition.TrueStatements.Add(
                new CodeMethodInvokeExpression(null, "AddToJsonObject", new CodeVariableReferenceExpression("jsonObject"),
                                           new CodePrimitiveExpression(keyValue.Value.Data.Name),
                                           new CodeVariableReferenceExpression(arrayName)));

            condition.TrueStatements.Add(
                new CodeMethodInvokeExpression(null, keyValue.Value.TypeSerializerBuilder.MethodName, 
                    new CodeVariableReferenceExpression( "obj." + keyValue.Key ), new CodeVariableReferenceExpression(arrayName)));

            //TODO: FalseStatements: should we explicit set the keyValue.Value.Data.Name to null?
        }

        private CodeMemberMethod GenerateTypeArrayJSONArray()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName,
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(type.FullName + "[]"), "objs"));
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("Org.Json.JSONArray"), "jsonArray"));

            var innerStatements = new CodeStatementCollection();
            FillJSONArrayMembers(innerStatements);

            var innerStatementsArray = new CodeStatement[innerStatements.Count];
            innerStatements.CopyTo(innerStatementsArray, 0);

            methodDecl.Statements.Add(
                new CodeIterationStatement(new CodeVariableDeclarationStatement(typeof(int), "i", new CodeSnippetExpression("0")),
                                           new CodeSnippetExpression("i < objs.Length"), new CodeSnippetStatement("i++"),
                                           innerStatementsArray));

            return methodDecl;
        }

        private void FillJSONArrayMembers(CodeStatementCollection statements)
        {
            if (IsComplexType())
            {
                statements.Add(
                    new CodeVariableDeclarationStatement(new CodeTypeReference("Org.Json.JSONObject"), "jsonObject", new CodeObjectCreateExpression("Org.Json.JSONObject")));

                statements.Add(
                    new CodeMethodInvokeExpression(null, "AddToJsonArray", new CodeVariableReferenceExpression("jsonArray"), new CodeVariableReferenceExpression("jsonObject")));

                statements.Add(
                    new CodeMethodInvokeExpression(null, methodName, new CodeVariableReferenceExpression("objs[i]"), new CodeVariableReferenceExpression("jsonObject")));

            }
            else
            {
                if (IsEnum())
                {
                    statements.Add(
                        new CodeMethodInvokeExpression(null, "AddToJsonArray", new CodeVariableReferenceExpression("jsonArray"), new CodeCastExpression(type.Fields[0].FieldType.FullName, new CodeVariableReferenceExpression("objs[i]"))));
                }
                else
                {
                    statements.Add(
                        new CodeMethodInvokeExpression(null, "AddToJsonArray", new CodeVariableReferenceExpression("jsonArray"), new CodeVariableReferenceExpression("objs[i]")));
                }
            }
        }

        #endregion
    }
}
