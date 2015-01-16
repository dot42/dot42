using System;
using System.CodeDom;
using System.Collections.Generic;
using Mono.Cecil;
using System.IO;
using System.Xml.Linq;

namespace Dot42.WcfTools.ProxyBuilder
{
    /// <summary>
    /// Build a de-serialize method for a specific type.
    /// </summary>
    internal class TypeDeserializerBuilder : TypeBuilderBase
    {
        private class DeserializerBuilderHelper : BuilderHelper
        {
            internal TypeDeserializerBuilder TypeDeserializerBuilder { get; set; }
        }

        private readonly Dictionary<string, DeserializerBuilderHelper> builders = new Dictionary<string, DeserializerBuilderHelper>();

        /// <summary>
        /// Default ctor
        /// </summary>
        internal TypeDeserializerBuilder(TypeDefinition type)
            :base(type, "Deserialize" + type.FullName.Replace(".", ""))
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
                    var builder = context.GetDeserializer(property.PropertyType.Resolve());
                    var builderHelper = new DeserializerBuilderHelper
                        {
                            TypeDeserializerBuilder = builder,
                            PropertyName = property.Name
                        };

                    FillBuilderHelper(type, property, builderHelper);

                    builders.Add(property.Name, builderHelper);
                }
            }
        }

        /// <summary>
        /// Create the entire deserialization method as C# code.
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
            result.Add(type.IsEnum ? GenerateXElementEnum() : GenerateXElement());
            //DataXml
            result.Add(GenerateTypeDataXml());
            result.Add(GenerateTypeArrayDataXml());
            //DataJson
            result.Add(GenerateTypeDataJson());
            result.Add(GenerateTypeArrayDataJson());
            result.Add(type.IsEnum ? GenerateEnumTypeJSON() : GenerateTypeJSONObject());
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
            methodDecl.ReturnType = new CodeTypeReference(type.FullName);
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Stream)), "stream"));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference("SerializationFormat"), "serializationFormat"));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(System.ServiceModel.Web.WebMessageFormat)), "webMessageFormat"));

            methodDecl.Statements.Add(new CodeMethodReturnStatement(new CodeTypeReferenceExpression("stream")));

            return methodDecl;
        }
        #endregion

        #region Format switch

        private CodeMemberMethod GenerateSwitchComplexType(bool objIsArray)
        {
            var name = methodName + (objIsArray ? "Array" : "");

            var methodDecl = new CodeMemberMethod
            {
                Name = name,
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(type.FullName + (objIsArray ? "[]" : ""));

            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Stream)), "stream"));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference("SerializationFormat"), "serializationFormat"));
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(System.ServiceModel.Web.WebMessageFormat)), "webMessageFormat"));

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

            _if2.TrueStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, name + "__DataXml", new CodeVariableReferenceExpression("stream"))));

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

            _if3.TrueStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, name + "__DataJson", new CodeVariableReferenceExpression("stream"))));

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

            _if5.TrueStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, name + "__XmlXml", new CodeVariableReferenceExpression("stream"))));

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
            methodDecl.ReturnType = new CodeTypeReference(type.FullName);
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof (Stream)), "stream"));
         
            methodDecl.Statements.Add(
                new CodeVariableDeclarationStatement(new CodeTypeReference(typeof (XDocument)), "doc",                                                     
                                                     new CodeMethodInvokeExpression(null, "GetXDocument", new CodeTypeReferenceExpression("stream"))));

            methodDecl.Statements.Add(
                new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, methodName, new CodeTypeReferenceExpression("doc.Root"))));

            return methodDecl;
        }

        private CodeMemberMethod GenerateTypeArrayXmlXml()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName + "Array__XmlXml",
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(type.FullName + "[]");
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Stream)), "stream"));

            methodDecl.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotSupportedException))));

            return methodDecl;
        }

        private CodeMemberMethod GenerateXElement()
        {
            var methodDecl = new CodeMemberMethod
                {
                    Name = methodName,
                    Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
                };
            methodDecl.ReturnType = new CodeTypeReference(type.FullName);
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(XElement)), "xElement"));

            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(type.FullName), "result", new CodeObjectCreateExpression(type.FullName)));

            FillMembersXml(methodDecl.Statements);

            methodDecl.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result")));

            return methodDecl;
        }

        private void FillMembersXml(CodeStatementCollection statements)
        {
            foreach (var keyValue in builders)
            {
                statements.Add(new CodeCommentStatement(keyValue.Key));

                var typeAsString = keyValue.Value.Xml.UsesAttribute ? "Attribute" : "Element";
                var xmlType = keyValue.Value.Xml.UsesAttribute ? typeof (XAttribute) : typeof (XElement);

                CodeExpression nameSpace = new CodePrimitiveExpression(keyValue.Value.Xml.Namespace);
                if (keyValue.Value.Xml.UsesAttribute)
                {
                    nameSpace = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("xElement"), "Name.NamespaceName");
                }

                if (keyValue.Value.IsArray)
                {
                    typeAsString += "s";
                    xmlType = typeof(List<XElement>);
                }


                if (keyValue.Value.IsArray)
                {
                    statements.Add(
                        new CodeVariableDeclarationStatement(new CodeTypeReference(xmlType),
                                                             keyValue.Key + "_" + typeAsString, new CodeObjectCreateExpression(xmlType, 
                                                             new CodeMethodInvokeExpression(
                                                                 new CodeTypeReferenceExpression("xElement"),
                                                                 typeAsString,
                                                                 new CodeMethodInvokeExpression(
                                                                     new CodeTypeReferenceExpression(
                                                                         typeof(XName)), "Get",
                                                                     new CodePrimitiveExpression(keyValue.Value.Xml.Name),
                                                                     nameSpace)))));
                    
                    if (!string.IsNullOrEmpty(keyValue.Value.Xml.ArrayItemName))
                    {
                        statements.Add(new CodeCommentStatement("Array support in XmlDeserializer should be improved here..."));
                    }

                }
                else
                {
                    statements.Add(
                       new CodeVariableDeclarationStatement(new CodeTypeReference(xmlType),
                                                            keyValue.Key + "_" + typeAsString,
                                                            new CodeMethodInvokeExpression(
                                                                new CodeTypeReferenceExpression("xElement"),
                                                                typeAsString,
                                                                new CodeMethodInvokeExpression(
                                                                    new CodeTypeReferenceExpression(
                                                                        typeof(XName)), "Get",
                                                                    new CodePrimitiveExpression(keyValue.Value.Xml.Name),
                                                                    nameSpace))));
                }

                var trueStatements = new List<CodeStatement>();
                if (!keyValue.Value.IsArray)
                {
                    trueStatements.Add(new CodeAssignStatement(
                                           new CodeTypeReferenceExpression("result." + keyValue.Key),
                                           new CodeMethodInvokeExpression( null,
                                                                          keyValue.Value.TypeDeserializerBuilder
                                                                                  .MethodName,
                                                                          new CodeVariableReferenceExpression(
                                                                              keyValue.Key + "_" + typeAsString))));
                }
                else
                {
                    trueStatements.Add(
                        new CodeSnippetStatement("var list = new System.Collections.Generic.List<" + keyValue.Value.TypeDeserializerBuilder.type.FullName + ">();"));

                    trueStatements.Add(new CodeIterationStatement(new CodeVariableDeclarationStatement(typeof(int), "i", new CodeSnippetExpression("0")), new CodeSnippetExpression("i < " + keyValue.Key + "_" + typeAsString + ".Count"), new CodeSnippetStatement("i++"),
                        new CodeSnippetStatement("list.Add(" + keyValue.Value.TypeDeserializerBuilder.MethodName + "(" + keyValue.Key + "_" + typeAsString + "[i]));")));

                    trueStatements.Add(
                        new CodeSnippetStatement("result." + keyValue.Key + " = list.ToArray();"));
                }

                if (keyValue.Value.Xml.IsOptional)
                {
                    trueStatements.Add(
                        new CodeAssignStatement(
                            new CodeTypeReferenceExpression("result." + keyValue.Key + "Specified"), new CodePrimitiveExpression(true)));
                }
                var falseStatements = new List<CodeStatement>();
                if (keyValue.Value.Xml.IsOptional)
                {
                    falseStatements.Add(
                        new CodeAssignStatement(
                            new CodeTypeReferenceExpression("result." + keyValue.Key + "Specified"), new CodePrimitiveExpression(false)));
                }

                statements.Add(new CodeConditionStatement(
                                   new CodeSnippetExpression(keyValue.Key + "_" + typeAsString + " != null"), trueStatements.ToArray(), falseStatements.ToArray()));
            }

        }

        private CodeMemberMethod GenerateXElementEnum()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName,
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(type.FullName);
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(XElement)), "xElement"));

            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(type.FullName), "result"));

            methodDecl.Statements.Add(
                new CodeConditionStatement(new CodeMethodInvokeExpression(null, type.FullName + ".TryParse",
                                                                          new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("xElement"),"Value"),
                                                                          new CodeTypeReferenceExpression("out result")), new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result"))));

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
            methodDecl.ReturnType = new CodeTypeReference(type.FullName);
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Stream)), "stream"));

            methodDecl.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotSupportedException))));

            return methodDecl;
        }

        private CodeMemberMethod GenerateTypeArrayDataXml()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName + "Array__DataXml",
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(type.FullName + "[]");
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Stream)), "stream"));

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
            methodDecl.ReturnType = new CodeTypeReference(type.FullName);
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Stream)), "stream"));

            if (type.IsEnum)
            {
                methodDecl.Statements.Add(
                    new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(long)), "value",
                                                         new CodeMethodInvokeExpression(null, "GetLong", new CodeTypeReferenceExpression("stream"))));

                methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(type.FullName), "result", new CodeCastExpression(type.FullName, new CodeTypeReferenceExpression("value"))));

                methodDecl.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result")));
            }
            else
            {
                methodDecl.Statements.Add(
                    new CodeVariableDeclarationStatement(new CodeTypeReference("Org.Json.JSONObject"), "jsonObject",
                                                         new CodeMethodInvokeExpression(null, "GetJSONObject", new CodeTypeReferenceExpression("stream"))));

                methodDecl.Statements.Add(
                    new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, methodName, new CodeTypeReferenceExpression("jsonObject"))));
            }

            return methodDecl;
        }

        private CodeMemberMethod GenerateTypeArrayDataJson()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName + "Array__DataJson",
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(type.FullName + "[]");
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Stream)), "stream"));

            methodDecl.Statements.Add(
               new CodeVariableDeclarationStatement(new CodeTypeReference("Org.Json.JSONArray"), "jsonArray",
                                                     new CodeMethodInvokeExpression(null, "GetJSONArray", new CodeTypeReferenceExpression("stream"))));

            methodDecl.Statements.Add(
                new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, methodName + "Array", new CodeTypeReferenceExpression("jsonArray"))));

            return methodDecl;
        }

        private CodeMemberMethod GenerateTypeJSONObject()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName,
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(type.FullName);
            methodDecl.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference("Org.Json.JSONObject"), "jsonObject"));

            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(type.FullName), "result", new CodeObjectCreateExpression(type.FullName)));

            FillMembersDataJson(methodDecl.Statements);

            methodDecl.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result")));

            return methodDecl;
        }

        private void FillMembersDataJson(CodeStatementCollection statements)
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

        private void FillJSONObjectMembersNonArray(KeyValuePair<string, DeserializerBuilderHelper> keyValue, CodeStatementCollection statements)
        {
            if (keyValue.Value.IsComplexType)
            {
                var complexName = "complexType_" + keyValue.Key;

                statements.Add(
                    new CodeVariableDeclarationStatement(new CodeTypeReference("Org.Json.JSONObject"), complexName,
                                                         new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("jsonObject"), "GetJSONObject",
                                                                                        new CodePrimitiveExpression(keyValue.Value.Data.Name))));

                statements.Add(new CodeAssignStatement(
                                   new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), keyValue.Key),
                                   new CodeMethodInvokeExpression(null, keyValue.Value.TypeDeserializerBuilder.MethodName, new CodeVariableReferenceExpression(complexName))));
            }
            else
            {
                statements.Add(new CodeAssignStatement(
                                   new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), keyValue.Key),
                                   new CodeMethodInvokeExpression(null, keyValue.Value.TypeDeserializerBuilder.MethodName, new CodeVariableReferenceExpression("jsonObject"),
                                                                  new CodePrimitiveExpression(keyValue.Value.Data.Name))));

            }
        }

        private void FillJSONObjectMembersArray(KeyValuePair<string, DeserializerBuilderHelper> keyValue, CodeStatementCollection statements)
        {
            var arrayName = "array_" + keyValue.Key;

            statements.Add(
                new CodeVariableDeclarationStatement(new CodeTypeReference("Org.Json.JSONArray"), arrayName,
                                                     new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("jsonObject"), "OptJSONArray", new CodePrimitiveExpression(keyValue.Value.Data.Name))));

            var condition = new CodeConditionStatement(new CodeSnippetExpression(arrayName + " != null"));
            statements.Add(condition);

            condition.TrueStatements.Add(new CodeAssignStatement(
                               new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), keyValue.Key),
                               new CodeMethodInvokeExpression(null, keyValue.Value.TypeDeserializerBuilder.MethodName + "Array", new CodeVariableReferenceExpression(arrayName))));
        }

        private CodeMemberMethod GenerateTypeArrayJSONArray()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName + "Array",
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(type.FullName + "[]");
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("Org.Json.JSONArray"), "jsonArray"));

            var innerStatements = new CodeStatementCollection();
            FillJSONArrayMembers(innerStatements);

            var innerStatementsArray = new CodeStatement[innerStatements.Count];
            innerStatements.CopyTo(innerStatementsArray, 0);

            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.Collections.Generic.List<" + type.FullName + ">"), "result", new CodeObjectCreateExpression("System.Collections.Generic.List<" + type.FullName + ">")));

            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(typeof(int), "length",
                                                                          new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("jsonArray"), "Length")));

            methodDecl.Statements.Add(
                new CodeIterationStatement(new CodeVariableDeclarationStatement(typeof(int), "i", new CodeSnippetExpression("0")),
                                           new CodeSnippetExpression("i < length"), new CodeSnippetStatement("i++"),
                                           innerStatementsArray));

            methodDecl.Statements.Add(new CodeMethodReturnStatement( new CodeMethodInvokeExpression( new CodeVariableReferenceExpression("result"), "ToArray")));

            return methodDecl;
        }

        private void FillJSONArrayMembers(CodeStatementCollection statements)
        {
            if (IsComplexType())
            {
                statements.Add(
                    new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("result"), "Add",
                                                   new CodeMethodInvokeExpression(null, methodName,
                                                      new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("jsonArray"), "GetJSONObject", new CodeSnippetExpression("i")))));
            }
            else if (IsEnum())
            {
                statements.Add(
                    new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("result"), "Add",
                        new CodeCastExpression(type.FullName,  
                          new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("jsonArray"), "OptLong", new CodeSnippetExpression("i")))));
            }
            else
            {
                //should not happens
                throw new NotImplementedException();
            }
        }

        private CodeMemberMethod GenerateEnumTypeJSON()
        {
            var methodDecl = new CodeMemberMethod
            {
                Name = methodName,
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
            };
            methodDecl.ReturnType = new CodeTypeReference(type.FullName);
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("Org.Json.JSONObject"), "jsonObject"));
            methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.String"), "name"));

            methodDecl.Statements.Add(
                   new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(long)), "value",
                                                        new CodeMethodInvokeExpression( new CodeTypeReferenceExpression("jsonObject"), "OptLong", new CodeTypeReferenceExpression("name"))));

            methodDecl.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(type.FullName), "result", new CodeCastExpression(type.FullName, new CodeTypeReferenceExpression("value"))));

 

            methodDecl.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result")));

            return methodDecl;
        }

        #endregion
    }
}
