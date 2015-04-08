using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dot42.FrameworkDefinitions;
using Dot42.ImportJarLib.Doxygen;
using Dot42.ImportJarLib.Model;
using Microsoft.CSharp;

namespace Dot42.ImportJarLib
{
    public sealed class CodeGenerator
    {
        private readonly TextWriter writer;
        private readonly IDocTypeNameResolver resolver;
        private readonly ICodeGeneratorContext context;
        private readonly TargetFramework target;
        private string indent = "\t\t";

        /// <summary>
        /// Default ctor
        /// </summary>
        private CodeGenerator(TextWriter writer, IDocTypeNameResolver resolver, ICodeGeneratorContext context, TargetFramework target)
        {
            this.writer = writer;
            this.resolver = resolver;
            this.context = context;
            this.target = target;
        }

        /// <summary>
        /// Generate code for the given types.
        /// </summary>
        public static void Generate(string folder, List<NetTypeDefinition> types, List<NetCustomAttribute> assemblyAttributes, IDocTypeNameResolver resolver, ICodeGeneratorContext context, TargetFramework target)
        {
            // Clean
            foreach (var iterator in context.PossibleNamespaceRoots)
            {
                var root = iterator;
                var path = Path.Combine(folder, root + ".cs");
                File.Delete(path);
            }

            // Select namespace roots
            var roots = types.Select(context.GetNamespaceRoot).Distinct().ToList();

            // Save
            var addAssemblyAttributes = true;
            foreach (var iterator in roots)
            {
                var root = iterator;
                var path = Path.Combine(folder, root + ".cs");
                using (var writer = new StreamWriter(path, false, Encoding.UTF8, 128 * 1024))
                {
                    GenerateHeader(writer, context);
                    var selectedTypes = types.Where(x => context.GetNamespaceRoot(x) == root)
                                             .OrderBy(p=>p.FullName);  // generate a stable output.
                    var generator = new CodeGenerator(writer, resolver, context, target);
                    if (addAssemblyAttributes)
                    {
                        generator.GenerateAssemblyAttributes(assemblyAttributes);
                        addAssemblyAttributes = false;
                    }
                    generator.Generate(selectedTypes);
                }
            }
        }

        /// <summary>
        /// Create a header for the source file.
        /// </summary>
        private static void GenerateHeader(TextWriter writer, ICodeGeneratorContext context)
        {
            context.CreateSourceFileHeader(writer);            
        }

        /// <summary>
        /// Generate code for the given assembly attributes.
        /// </summary>
        private void GenerateAssemblyAttributes(IEnumerable<NetCustomAttribute> attributes)
        {
            var saveIndent = indent;
            indent = "";
            CreateAttributes(attributes, null, "assembly:");
            indent = saveIndent;
        }

        /// <summary>
        /// Generate code for the given types.
        /// </summary>
        private void Generate(IEnumerable<NetTypeDefinition> types)
        {
            string lastNamespace = null;
            foreach (var type in types.OrderBy(x => x.Namespace))
            {
                if (type.Namespace != lastNamespace)
                {
                    if (lastNamespace != null)
                        CloseNamespace();
                    writer.WriteLine("namespace {0}", type.Namespace);
                    writer.WriteLine("{");
                    lastNamespace = type.Namespace;
                }

                CreateTypeCode(type);
            }
            if (lastNamespace != null)
            {
                CloseNamespace();
            }
        }

        /// <summary>
        /// Close a namespace
        /// </summary>
        private void CloseNamespace()
        {
            writer.WriteLine("}");
            writer.WriteLine();            
        }

        /// <summary>
        /// Create a type declaration
        /// </summary>
        private void CreateTypeCode(NetTypeDefinition type)
        {
            CreateComments(type.Description, type.OriginalJavaClassName);
            CreateAttributes(type.CustomAttributes, type);
            // Header
            writer.Write(indent);
            writer.Write(Convert(type, type.Attributes));
            if (type.IsEnum) writer.Write("class ");
            //if (type.IsEnum) writer.Write("enum ");
            else if (type.IsInterface) writer.Write("interface ");
            else if (type.IsStruct) writer.Write("struct ");
            else writer.Write("class ");

            writer.Write(type.Name);

            if (type.GenericParameters.Any())
            {
                writer.Write("<");
                var needComma = false;
                foreach (var gp in type.GenericParameters)
                {
                    if (needComma) writer.Write(", ");
                    writer.Write(gp.Name);
                    needComma = true;
                }
                writer.Write(">");
            }

            var outputBaseType = (type.BaseType != null) && (!type.IsEnum) && !type.BaseType.IsObject();
            if (outputBaseType || (type.Interfaces.Any()))
            {
                writer.Write(" : ");
                var needComma = false;
                if (outputBaseType)
                {
                    writer.Write(CreateRef(type.BaseType, false, false, false, false, type, target));
                    needComma = true;
                }
                foreach (var intf in type.Interfaces)
                {
                    if (needComma) writer.Write(", ");
                    writer.Write(CreateRef(intf, false, true, false, false, type, target));
                    needComma = true;
                }
            }


            // Write creation reason as comment
            if (context.GenerateDebugComments)
            {
                writer.WriteLine();
                writer.Write(" /* scope: ");
                writer.Write(type.Scope);
                writer.Write(" */ ");
            }

            writer.WriteLine();
            writer.Write(indent);
            writer.WriteLine("{");

            var oldIndent = indent;
            try
            {
                indent += "\t\t";

                if (type.IsEnum)
                {
                    
                }
                if (!type.IsInterface)
                {
                    foreach (var member in type.Fields)
                    {
                        CreateFieldCode(member);
                    }
                }
                foreach (var member in type.Methods)
                {
                    if(member.Property == null) // TODO: this check clearly belongs somewhere else, but where?
                        CreateMethodCode(member);
                }
                foreach (var member in type.Properties)
                {
                    CreatePropertyCode(member);
                }
                foreach (var member in type.NestedTypes)
                {
                    CreateTypeCode(member);
                }
            }
            finally
            {
                indent = oldIndent;
            }

            writer.Write(indent);
            writer.WriteLine("}");
            writer.WriteLine();
        }

        /// <summary>
        /// Create code for the given field.
        /// </summary>
        private void CreateFieldCode(NetFieldDefinition field)
        {
            CreateComments(field.Description, field.OriginalJavaName);
            CreateAttributes(field.CustomAttributes, field);
            writer.Write(indent);
            var isEnum = field.DeclaringType.IsEnum;
            //if (!isEnum)
            {
                writer.Write(Convert(field, field.Attributes));
                writer.Write(CreateRef(field.FieldType, false, true, true, false, field.DeclaringType, target));
                writer.Write(" ");
            }
            writer.Write(field.Name);

            if (field.DefaultValue != null)
            {
                writer.Write(" = ");
                writer.Write(ConvertPrimitive(field.DefaultValue, field.FieldType));
            }

            //writer.Write(isEnum ? "," : ";");
            writer.Write(";");

            writer.WriteLine();
        }

        /// <summary>
        /// Create code for the given method
        /// </summary>
        private void CreateMethodCode(NetMethodDefinition method)
        {
            var noBody = false;
            CreateComments(method.Description, method.IsConstructor ? null : method.OriginalJavaName);
            CreateAttributes(method.CustomAttributes, method);
            writer.Write(indent);
            if ((method.InterfaceType == null) && !method.IsDeconstructor)
            {
                writer.Write(Convert(method, method.Attributes, context.GenerateExternalMethods, false));
            } 
            else if (((method.InterfaceType != null) && context.GenerateExternalMethods) || method.IsDeconstructor)
            {
                noBody = true;
                writer.Write("extern ");
            } 
            if (!(method.IsConstructor || method.IsDeconstructor))
            {
                writer.Write(CreateRef(method.ReturnType, true, true, true, false, method.DeclaringType, target));
                writer.Write(" ");
                if (method.InterfaceType != null)
                {
                    writer.Write(CreateRef(method.InterfaceType, false, true, true, false, method.DeclaringType, target));
                    writer.Write(".");
                }
            }            
            if (method.IsDeconstructor)
            {
                writer.Write('~');
            }
            writer.Write(method.IsConstructor || method.IsDeconstructor ? method.DeclaringType.Name : method.Name);

            if (method.GenericParameters.Any())
            {
                writer.Write("<");
                var needComma = false;
                foreach (var gp in method.GenericParameters)
                {
                    if (needComma) writer.Write(", ");
                    writer.Write(gp.Name);
                    needComma = true;
                }
                writer.Write(">");
            }

            {
                writer.Write("(");
                var needComma = false;
                foreach (var p in method.Parameters)
                {
                    if (needComma) writer.Write(", ");
                    if (p.IsParams) writer.Write("params ");
                    writer.Write(CreateRef(p.ParameterType, false, true, true, false, method.DeclaringType, target));
                    writer.Write(" {0}", FixIdentifier(p.Name));
                    needComma = true;
                }
                writer.Write(")");
            }

            // Write creation reason as comment
            if (context.GenerateDebugComments)
            {
                writer.Write(" /* ");
                writer.Write(method.CreateReason);
                writer.Write(" */ ");
            }

            // Create method body
            if (!method.IsAbstract && !method.DeclaringType.IsInterface && !context.GenerateExternalMethods && !noBody)
            {
                writer.WriteLine();

                writer.Write(indent);
                writer.WriteLine("{");

                var returnType = method.ReturnType;
                if (!returnType.IsVoid())
                {
                    writer.Write(indent);
                    writer.Write("\t\treturn default(");
                    writer.Write(CreateRef(returnType, false, true, true, false, method.DeclaringType, target));
                    writer.Write(");");
                    writer.WriteLine();
                }

                writer.Write(indent);
                writer.WriteLine("}");
            }
            else
            {
                writer.WriteLine(";");
            }

            writer.WriteLine();
        }

        /// <summary>
        /// Create code for the given property
        /// </summary>
        private void CreatePropertyCode(NetPropertyDefinition prop)
        {
            var mainMethod = (prop.Getter??prop.Setter);
            CreateComments(prop.Description, mainMethod.OriginalJavaName);
            CreateAttributes(prop.CustomAttributes, prop);
            writer.Write(indent);
            writer.Write(Convert(mainMethod, mainMethod.Attributes, context.GenerateExternalMethods, false));
            writer.Write(CreateRef(prop.PropertyType, true, true, true, false, prop.DeclaringType, target));
            writer.Write(" ");
            writer.Write(prop.Name);

            if (prop.Parameters.Any())
            {
                writer.Write("[");
                var needComma = false;
                var pIndex = 0;
                foreach (var p in prop.Parameters)
                {
                    if (needComma) writer.Write(", ");
                    writer.Write(CreateRef(p.ParameterType, false, true, true, false, prop.DeclaringType, target));
                    writer.Write(pIndex == 0 ? " index" : string.Format(" p{0}", pIndex));
                    pIndex++;
                    needComma = true;
                }
                writer.Write("]");
            }

            // Create method body
            var isAbstract = mainMethod.IsAbstract;
            var isInterface = mainMethod.DeclaringType.IsInterface;

            writer.WriteLine();

            writer.Write(indent);
            writer.WriteLine("{");

            // increase indent
            var oldIndent = indent;
            indent += "\t\t";

            try
            {
                if (prop.Getter != null)
                {
                    CreateAttributes(mainMethod.CustomAttributes, prop.Getter);
                    writer.Write(indent);
                    writer.Write("get");

                    bool isEmptyMethod = isAbstract || isInterface || context.GenerateExternalMethods;

                    if (!isEmptyMethod)
                    {
                        writer.Write("{ return default(");
                        writer.Write(CreateRef(prop.PropertyType, false, true, true, false, prop.Getter.DeclaringType,
                            target));
                        writer.Write("); }");
                        //writer.Write("{{ return {0}({1}); }}", prop.Getter.Name, string.Join(", ", prop.Getter.Parameters.Select((x, i) => (i == 0) ? "index" : x.Name)));
                    }
                    else
                    {
                        writer.Write(";");
                    }
                    writer.WriteLine();
                }

                if (prop.Setter != null)
                {
                    CreateAttributes(prop.Setter.CustomAttributes, prop.Setter);
                    writer.Write(indent);
                    writer.Write("set");
                    //if (!isEmptyMethod)
                    //{
                    //    writer.Write("{{ {0}({1}); }}", prop.Setter.Name,
                    //        string.Join(", ", prop.Setter.Parameters.Select((x, i) => (i == 0) ? "value" : x.Name)));
                    //}
                    //else
                    //{
                    //    writer.Write(";");
                    //}
                    writer.Write(!isAbstract && !context.GenerateExternalMethods ? "{ }" : ";");
                    writer.WriteLine();
                }
            }
            finally
            {
                indent = indent = oldIndent;

            }
            // restore indent
            writer.Write(indent);
            writer.WriteLine("}");

            writer.WriteLine();
        }

        /// <summary>
        /// Write custom attributes
        /// </summary>
        private void CreateAttributes(IEnumerable<NetCustomAttribute> attributes, INetMemberDefinition memberDefinition, string attributePrefix = null)
        {
            const string AttributePostfix = "Attribute";
            var importAttributeName = target.ImportAsStubs
                                          ? AttributeConstants.DexImportAttributeName
                                          : AttributeConstants.JavaImportAttributeName;

            foreach (var ca in attributes)
            {
                writer.Write(indent);
                writer.Write('[');
                if (attributePrefix != null)
                {
                    writer.Write(attributePrefix);
                }
                var typeRef = (ca.AttributeType == null)
                    ? (AttributeConstants.Dot42AttributeNamespace + "." + importAttributeName)
                    : CreateRef(ca.AttributeType, false, false, false, true, null, target);
                if (typeRef.EndsWith(AttributePostfix))
                    typeRef = typeRef.Substring(0, typeRef.Length - AttributePostfix.Length);
                writer.Write(typeRef);
                if (ca.ConstructorArguments.Any() || ca.Properties.Any())
                {
                    writer.Write('(');
                    var needComma = false;
                    foreach (var value in ca.ConstructorArguments)
                    {
                        if (needComma) writer.Write(", ");
                        writer.Write(ConvertPrimitive(value, null));
                        needComma = true;
                    }
                    foreach (var pair in ca.Properties)
                    {
                        if (needComma) writer.Write(", ");
                        writer.Write(pair.Key);
                        writer.Write(" = ");
                        writer.Write(ConvertPrimitive(pair.Value, null));
                        needComma = true;                        
                    }
                    writer.Write(')');
                }
                writer.WriteLine(']');
            }
            if ((memberDefinition != null) && (memberDefinition.EditorBrowsableState != EditorBrowsableState.Always))
            {
                writer.Write(indent);
                writer.WriteLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.{0})]", memberDefinition.EditorBrowsableState);
            }
        }

        /// <summary>
        /// Write comments if not empty
        /// </summary>
        private void CreateComments(DocDescription description, string originalJavaName)
        {
            if (description == null)
            {
                if (!string.IsNullOrEmpty(originalJavaName))
                {
                    var builder = new CommentBuilder();
                    builder.JavaName.Write(originalJavaName);
                    builder.WriteTo(writer, indent);
                }
                return;
            }
            description.WriteAsCode(writer, indent, resolver, originalJavaName);
        }

        /// <summary>
        /// Create a type reference
        /// </summary>
        private static string CreateRef(NetTypeReference typeRef, bool allowVoid, bool useShortTypeNames, bool useShortNestedTypeNames, bool useGlobalPrefix, NetTypeDefinition context, TargetFramework target)
        {
            if (typeRef == null)
            {
                if (allowVoid)
                    return "void";
                throw new ArgumentNullException("typeRef");
            }
            var result = typeRef.Accept(new CodeTypeReferenceBuilder(context, useShortTypeNames, useShortNestedTypeNames, allowVoid, target.TypeNameMap), true);
            if (context != null)
            {
                switch (result)
                {
                    case "global::Android.Widget.AdapterView<object>.IOnItemClickListener":
                        switch (context.FullName)
                        {
                            case "Android.Widget.Spinner":
                                return result.Replace("<object>", "<global::Android.Widget.ISpinnerAdapter>");
                            case "Android.Widget.ExpandableListView":
                                return result.Replace("<object>", "<global::Android.Widget.IListAdapter>");
                        }
                        break;
                }
            }
            if (useGlobalPrefix)
            {
                if (!result.StartsWith("global::"))
                {
                    result = "global::" + result;
                }
            }
            return result;
        }

        /// <summary>
        /// Convert type attributes to string
        /// </summary>
        private static String Convert(NetTypeDefinition type, TypeAttributes source)
        {
            string result;
            switch (source & TypeAttributes.VisibilityMask)
            {
                case TypeAttributes.NotPublic:
                case TypeAttributes.NestedAssembly:
                    result = "internal ";
                    break;
                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                    result = "public ";
                    break;
                case TypeAttributes.NestedPrivate:
                    result = "private ";
                    break;
                case TypeAttributes.NestedFamily:
                case TypeAttributes.NestedFamANDAssem:
                    result = "protected ";
                    break;
                case TypeAttributes.NestedFamORAssem:
                    result = "protected internal ";
                    break;
                default:
                    throw new ArgumentException(string.Format("Unknown visibility 0x{0:X4}", (int)(source & TypeAttributes.VisibilityMask)));
            }
            if (!(type.IsInterface || type.IsStruct || type.IsEnum || type.IsStatic))
            {
                if (source.HasFlag(TypeAttributes.Abstract)) result += "abstract ";
                if (source.HasFlag(TypeAttributes.Sealed)) result += "sealed ";
            }
            if (type.IsStatic)
            {
                result += "static ";
            }
            if (type.IsEnum)
            {
                result += "sealed ";
            }
            if (!type.IsEnum)
            {
                result += "partial ";
            }
            return result;
        }

        /// <summary>
        /// Convert field attributes to member attributes
        /// </summary>
        private static string Convert(NetFieldDefinition field, FieldAttributes source)
        {
            var result = string.Empty;
            switch (source & FieldAttributes.FieldAccessMask)
            {
                case FieldAttributes.Private:
                    result = "private ";
                    break;
                case FieldAttributes.Assembly:
                    result = "internal ";
                    break;
                case FieldAttributes.Family:
                case FieldAttributes.FamANDAssem:
                    result = "protected ";
                    break;
                case FieldAttributes.FamORAssem:
                    result = "protected internal ";
                    break;
                default:
                    result = "public ";
                    break;
            }

            if (field.DefaultValue != null)
            {
                var type = field.FieldType;
                if (type.IsBoolean() || type.IsChar() || type.IsByte() || type.IsSByte() || 
                    type.IsInt16() || type.IsInt32() || type.IsInt64() || type.IsSingle() || type.IsDouble() || type.IsString())
                {
                    const FieldAttributes mask = FieldAttributes.Static | FieldAttributes.InitOnly;
                    if ((source & mask) == mask)
                    {
                        return result + "const ";
                    }
                }
            }

            if ((source & FieldAttributes.Static) != 0) result += "static ";
            if ((source & FieldAttributes.Literal) != 0) result += "const ";
            if ((source & FieldAttributes.InitOnly) != 0) result += "readonly ";
            return result;
        }

        /// <summary>
        /// Convert method attributes to member attributes
        /// </summary>
        private static string Convert(NetMethodDefinition method, MethodAttributes source, bool useExtern, bool accessOnly)
        {
            var result = string.Empty;
            if (method.DeclaringType.IsInterface)
                return result;

            switch (source & MethodAttributes.MemberAccessMask)
            {
                case MethodAttributes.Private:
                    result ="private ";
                    break;
                case MethodAttributes.Assembly:
                    result = "internal ";
                    break;
                case MethodAttributes.Family:
                case MethodAttributes.FamANDAssem:
                    result = "protected ";
                    break;
                case MethodAttributes.FamORAssem:
                    result = "protected internal ";
                    break;
                default:
                    result = "public ";
                    break;
            }
            if ((source & MethodAttributes.Static) != 0)
            {
                result += "static ";
            }
            else
            {
                if (!accessOnly)
                {
                    if (method.IsAbstract)
                    {
                        result += "abstract ";
                    }
                    else
                    {
                        if (method.IsNewSlot)
                        {
                            if (method.IsVirtual)
                                result += "new virtual ";
                            else
                                result += "new ";
                        }
                        else
                        {
                            if (method.NeedsOverrideKeyword)
                            {
                                result += "override ";
                            }
                            else
                            {
                                if ((0 != (source & MethodAttributes.Virtual)) && !method.DeclaringType.IsEnum)
                                    result += "virtual ";
                            }
                        }
                    }
                }
            }
            if (!accessOnly)
            {
                if (useExtern && !method.IsAbstract) result += "extern ";
            }
            return result;
        }

        private static string ConvertPrimitive(object value, NetTypeReference expectedType)
        {
            if (expectedType != null)
            {
                if (expectedType.IsBoolean())
                {
                    value = System.Convert.ToBoolean(value);
                }
                else if (expectedType.IsChar())
                {
                    value = System.Convert.ToChar(value);
                }
            }

            if (value is byte[])
            {
                var arr = (byte[]) value;
                var seq = arr.Select(x => string.Format("{0}", x));
                return "new byte[] { " + string.Join(",", seq) + " } ";
            }
            if (value is string[])
            {
                var arr = (string[])value;
                var seq = arr.Select(x => ConvertPrimitive(x, null));
                return "new string[] { " + string.Join("," + Environment.NewLine + "\t", seq) + " } ";
            }
            
            if (value is double)
            {
                var dvalue = (double) value;
                if (double.IsNaN(dvalue)) return "0.0d / 0.0d";
                if (double.IsNegativeInfinity(dvalue)) return "-1.0d / 0.0d";
                if (double.IsPositiveInfinity(dvalue)) return "1.0d / 0.0d";
            }

            if (value is float)
            {
                var fvalue = (float)value;
                if (float.IsNaN(fvalue)) return "0.0f / 0.0f";
                if (float.IsNegativeInfinity(fvalue)) return "-1.0f / 0.0f";
                if (float.IsPositiveInfinity(fvalue)) return "1.0f / 0.0f";
            }

            var compiler = new CSharpCodeProvider();
            var writer = new StringWriter();
            compiler.GenerateCodeFromExpression(new CodePrimitiveExpression(value), writer,
                                                new CodeGeneratorOptions());
            return writer.ToString();
        }

        /// <summary>
        /// Avoid using keywords as identifier
        /// </summary>
        private static string FixIdentifier(string value)
        {
            switch (value)
            {
                case "object":
                case "void":
                case "int":
                    return "@" + value;
                default:
                    return value;
            }
        }

        /// <summary>
        /// Helper used to generate code type references.
        /// </summary>
        private sealed class CodeTypeReferenceBuilder : INetTypeVisitor<string, bool>
        {
            private readonly TypeNameMap typeNameMap;
            private readonly NetTypeDefinition context;
            private readonly bool useShortTypeNames;
            private readonly bool useShortNestedTypeNames;
            private readonly bool allowVoid;

            private static readonly Dictionary<string, string> shortTypeNameMap = new Dictionary<string, string>() {
                { "System.Void", "void" },
                { "System.Object", "object" },
                { "System.String", "string" },
                { "System.Int64", "long" },
                { "System.Int32", "int" },
                { "System.Int16", "short" },
                { "System.Double", "double" },
                { "System.Single", "float" },
                { "System.Char", "char" },
                { "System.Boolean", "bool" },
                { "System.Byte", "byte" },
                { "System.SByte", "sbyte" },
            };

            /// <summary>
            /// Default ctor
            /// </summary>
            public CodeTypeReferenceBuilder(NetTypeDefinition context, bool useShortTypeNames, bool useShortNestedTypeNames, bool allowVoid, TypeNameMap typeNameMap)
            {
                this.context = context;
                this.useShortTypeNames = useShortTypeNames;
                this.useShortNestedTypeNames = useShortNestedTypeNames;
                this.allowVoid = allowVoid;
                this.typeNameMap = typeNameMap;
            }

            public string Visit(NetTypeDefinition type, bool addDummyGeneric)
            {
                string result;
                if (type.DeclaringType != null)
                {
                    // Nested
                    if (type == context)
                    {
                        result = type.Name;
                    }
                    else
                    {
                        var declType = type.DeclaringType.Accept(this, true);
                        result = declType + "." + type.Name;
                    }
                }
                else
                {
                    result = type.FullName;
                    string mapped;
                    if (useShortTypeNames && shortTypeNameMap.TryGetValue(result, out mapped))
                    {
                        if ((mapped == "void") && !allowVoid)
                            mapped = "object";
                        result = mapped;
                    }
                    else
                    {
                        //if (/*(item.Namespace != "System") ||*/ (item.Name == "Exception") || (item.Name == "Type"))
                        {
                            result = "global::" + result;
                        }
                    }
                }
                if (addDummyGeneric && (type.GenericParameters.Count > 0))
                {
                    var sb = new StringBuilder();
                    sb.Append('<');
                    var index = 0;
                    foreach (var gp in type.GenericParameters)
                    {
                        if (index > 0) sb.Append(", ");
                        NetTypeReference  resolvedGp;
                        var name = GenericParameters.TryResolve(gp, context, typeNameMap, out resolvedGp) ? resolvedGp.Accept(this, true) : "object";
                        sb.Append(name);
                        index++;
                    }
                    sb.Append('>');
                    result += sb;

                    // Add dummy type arguments
                    //var args = string.Join(", ", type.GenericParameters.Select(x => "object").ToArray());
                    //result = result + "<" + args + ">";
                }
                return result;
            }

            private static string FormatType(CodeTypeReference typeRef)
            {
                var sb = new StringBuilder();
                sb.Append(typeRef.BaseType);
                if (typeRef.TypeArguments.Count > 0)
                {
                    sb.Append('<');
                    for (var i = 0; i < typeRef.TypeArguments.Count; i++ )
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append(FormatType(typeRef.TypeArguments[i]));
                    }
                    sb.Append('>');
                }
                var result = sb.ToString();
                return result;
            }

            public string Visit(NetGenericParameter item, bool addDummyGeneric)
            {
                return item.Name;
            }

            public string Visit(NetGenericInstanceType item, bool addDummyGeneric)
            {
                var prefix = (item.DeclaringType != null) ? item.DeclaringType.Accept(this, true) + "." + item.ElementType.Name : item.ElementType.Accept(this, false);
                var args = string.Join(", ", item.GenericArguments.Select(x => x.Accept(this, true)).ToArray());
                return prefix + "<" + args + ">";
            }

            public string Visit(NetArrayType item, bool addDummyGeneric)
            {
                return item.ElementType.Accept(this, true) + "[]";
            }

            public string Visit(NetNullableType item, bool addDummyGeneric)
            {
                return item.ElementType.Accept(this, addDummyGeneric) + "?";
            }
        }
    }
}
