using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Dot42.ApkLib.Resources;
using Dot42.FrameworkDefinitions;
using Dot42.ResourcesLib;

namespace Dot42.Compiler.Resources
{
    internal sealed class CreateResourceIdsCode
    {
        private const string androidPrefix = "android:";
        private readonly Table resourceTable;
        private readonly List<Tuple<string, ResourceType>> resources;
        private readonly List<StyleableDeclaration> styleableDeclarations;

        /// <summary>
        /// Default ctor
        /// </summary>
        public CreateResourceIdsCode(Table resourceTable, List<Tuple<string, ResourceType>> resources, List<StyleableDeclaration> styleableDeclarations)
        {
            this.resourceTable = resourceTable;
            this.resources = resources;
            this.styleableDeclarations = styleableDeclarations;
        }

        /// <summary>
        /// Generate the resource ID's
        /// </summary>
        public void Generate(string outputFolder, string packageName, string generatedCodeNamespace, string generatedCodeLanguage, DateTime timeStamp)
        {
            var compileUnit = CreateCompileUnit(packageName, generatedCodeNamespace);

            // Write code
            Directory.CreateDirectory(outputFolder);
            switch (generatedCodeLanguage)
            {
                case "C#":
                    GenerateCSharpCode(outputFolder, compileUnit, timeStamp);
                    break;
                case "VB":
                    GenerateVBNetCode(outputFolder, compileUnit, timeStamp);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unknown code language {0}", generatedCodeLanguage));
            }
        }

        /// <summary>
        /// Create the code as codedom structure.
        /// </summary>
        private CodeCompileUnit CreateCompileUnit(string packageName, string generatedCodeNamespace)
        {
            if (string.IsNullOrEmpty(generatedCodeNamespace))
                generatedCodeNamespace = packageName;

            var compileUnit = new CodeCompileUnit();
            var pkgNamespace = new CodeNamespace(generatedCodeNamespace);
            compileUnit.Namespaces.Add(pkgNamespace);
            pkgNamespace.Imports.Add(new CodeNamespaceImport("System"));

            var rType = new CodeTypeDeclaration("R");
            rType.IsClass = true;
            rType.TypeAttributes = TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Sealed |
                                   TypeAttributes.Public;
            pkgNamespace.Types.Add(rType);

            if (resourceTable != null)
            {
                foreach (var typeSpec in resourceTable.Packages[0].TypeSpecs)
                {
                    if (typeSpec.EntryCount == 0)
                        continue;

                    var typeDef = new CodeTypeDeclaration(CreateTypeSpecName(typeSpec));
                    typeDef.IsClass = true;
                    typeDef.TypeAttributes = TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Sealed |
                                             TypeAttributes.NestedPublic;
                    rType.Members.Add(typeDef);

                    var index = 0;
                    foreach (var entry in typeSpec.Entries)
                    {
                        // Skip code generation for system ID's
                        if (!SystemIdConstants.Ids.Contains(entry.Key))
                        {
                            var resId = 0x7F000000 | ((typeSpec.Id) << 16) | index;
                            var field = new CodeMemberField(typeof (int), MakeValueNetIdentifier(UnfixResourceName(entry.Key)));
                            field.Attributes = MemberAttributes.Public | MemberAttributes.Const;
                            field.InitExpression = new CodeSnippetExpression(string.Format("0x{0:x8}", resId));
                            typeDef.Members.Add(field);
                        }
                        index++;
                    }
                }

                if (styleableDeclarations.Any())
                {
                    var styleableTypeDef = new CodeTypeDeclaration("Styleable");
                    styleableTypeDef.IsClass = true;
                    styleableTypeDef.TypeAttributes = TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.NestedPublic;
                    rType.Members.Add(styleableTypeDef);

                    foreach (var declaration in styleableDeclarations)
                    {
                        // Type
                        var typeDef = new CodeTypeDeclaration(declaration.Name);
                        typeDef.IsClass = true;
                        typeDef.TypeAttributes = TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.NestedPublic;
                        styleableTypeDef.Members.Add(typeDef);

                        // Array
                        var list = string.Join(", ", declaration.AttributeNames.Select(FormatAttributeName));
                        var arrayField = new CodeSnippetTypeMember("                public static readonly int[] AllIds = new[] { " + list + " };");
                        typeDef.Members.Add(arrayField);

                        int idx = 0;
                        foreach (var attr in declaration.AttributeNames)
                        {
                            var field = new CodeMemberField(typeof(int), UnfixResourceName(attr));
                            field.Attributes = MemberAttributes.Public | MemberAttributes.Const;
                            field.InitExpression = new CodeSnippetExpression( (idx++).ToString());
                            typeDef.Members.Add(field);
                        }
                    }                    
                }
            }
            return compileUnit;
        }

        private static string FormatAttributeName(string x)
        {
            string result;
            if (x.StartsWith(androidPrefix))
            {
                x = x.Substring(androidPrefix.Length);
                x = x.Substring(0, 1).ToUpper() + x.Substring(1);
                return string.Format("Android.R.Attr.{0}", x); // Never a mask
            }
            result = string.Format("R.Attr.{0}", x);
           
            return result;
        }

        private static string CreateTypeSpecName(Table.TypeSpec typeSpec)
        {
            var name = Utility.NameConverter.UpperCamelCase(typeSpec.Name);
            //if (!name.EndsWith("s"))
            //    name += "s";
            return name;
        }

        /// <summary>
        /// Use the original resource name (if available).
        /// </summary>
        private string UnfixResourceName(string key)
        {
            if (key.StartsWith(androidPrefix))
            {
                key = key.Substring(androidPrefix.Length);
            }
            var pair = resources.FirstOrDefault(x => ResourceExtensions.GetNormalizedResourceName(x.Item1, ResourceType.Unknown) == key);
            return (pair != null) ? ConfigurationQualifiers.StripQualifiers(pair.Item1, true, false) : key.Replace(' ', '_');
        }

        /// <summary>
        /// Create a valid .NET identifier from the given value.
        /// </summary>
        private static string MakeValueNetIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            value = value.Replace('.', '_');
            return value;
        }

        /// <summary>
        /// Generate C# code.
        /// </summary>
        private static void GenerateCSharpCode(string outputFolder, CodeCompileUnit compileUnit, DateTime timeStamp)
        {
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var options = new CodeGeneratorOptions {BracingStyle = "C"};
            var filename = Path.Combine(outputFolder, "R.cs");
            GenerateCode(filename, compileUnit, provider, options, timeStamp);
        }

        /// <summary>
        /// Generate VB.NET code.
        /// </summary>
        private static void GenerateVBNetCode(string outputFolder, CodeCompileUnit compileUnit, DateTime timeStamp)
        {
            var provider = CodeDomProvider.CreateProvider("VisualBasic");
            var options = new CodeGeneratorOptions { BracingStyle = "C" };
            var filename = Path.Combine(outputFolder, "R.vb");
            GenerateCode(filename, compileUnit, provider, options, timeStamp);
        }

        /// <summary>
        /// Generate VB.NET code.
        /// </summary>
        private static void GenerateCode(string outputPath, CodeCompileUnit compileUnit, CodeDomProvider provider, CodeGeneratorOptions options, DateTime timeStamp)
        {
            string contents;
            using (var stringWriter = new StringWriter())
            {
                provider.GenerateCodeFromCompileUnit(compileUnit, stringWriter, options);
                stringWriter.Flush();                
                contents = stringWriter.ToString();

                const string prefixMarker = "<auto-generated>";
                const string postfixMarker = "</auto-generated>";

                var startIndex = contents.IndexOf(prefixMarker, StringComparison.Ordinal);
                var endIndex = contents.IndexOf(postfixMarker, StringComparison.Ordinal);
                if ((startIndex >= 0) && (endIndex >= 0))
                {
                    // Replace auto-generated comment created by .NET.
                    var prefix = contents.Substring(0, startIndex);
                    var postfix = contents.Substring(endIndex + postfixMarker.Length);
                    contents = prefix + "This file is automatically generated by dot42" + postfix;
                }

            }

            // Update file only when it has changed
            var existingContents = File.Exists(outputPath) ? File.ReadAllText(outputPath) : "-";
            if (contents.Trim() != existingContents.Trim())
            {
                File.WriteAllText(outputPath, contents);
            }
            File.SetLastWriteTime(outputPath, timeStamp);
        }
    }
}
