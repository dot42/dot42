using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Dot42.JvmClassLib;

namespace Dot42.ApkSpy.Tree
{
    internal class JavaClassFileNode : TextFileNode
    {
        private ClassFile classDef;

        /// <summary>
        /// Default ctor
        /// </summary>
        public JavaClassFileNode(SourceFile source, string fileName)
            : base(source, fileName)
        {
            /*classDef = Source.Jar.OpenClass(fileName);
            Text = classDef.ClassName;
            ImageIndex = classDef.IsInterface ? 5 : 2;*/
        }

        /// <summary>
        /// Create all child nodes
        /// </summary>
        protected override void CreateChildNodes()
        {
            classDef = Source.OpenClass(FileName);
            foreach (var field in classDef.Fields.OrderBy(x => x.Name))
            {
                Nodes.Add(new JavaFieldDefinitionNode(field));
            }
            foreach (var method in classDef.Methods.OrderBy(x => x.Name))
            {
                Nodes.Add(new JavaMethodDefinitionNode(method));
            }
        }

        protected override string LoadText()
        {
            EnsureChildNodesCreated();

            var nl = Environment.NewLine;
            var sb = new StringBuilder();
            sb.AppendFormat("FullName:\t\t{0}{1}", classDef.ClassName, nl);
            sb.AppendFormat("Name:\t\t{0}{1}", classDef.Name, nl);
            sb.AppendFormat("Package:\t\t{0}{1}", classDef.Package, nl);
            sb.AppendFormat("DeclaringClass:\t{0}{1}", (classDef.DeclaringClass != null) ? classDef.DeclaringClass.ClassName : "<none>", nl);
            sb.AppendFormat("SuperClass:\t{0}{1}", (classDef.SuperClass != null) ? classDef.SuperClass.ClassName : "<none>", nl);
            sb.AppendFormat("Signature:\t{0}{1}", (classDef.Signature != null) ? classDef.Signature.Original : "<none>", nl);
            foreach (var intf in classDef.Interfaces)
            {
                sb.AppendFormat("Implements:\t{0}{1}", intf.ClassName, nl);
            }
            sb.AppendFormat("AccessFlags:\t{0}{1}", AccessFlagsAsString(classDef.AccessFlags), nl);
            sb.AppendFormat("Annotations: {0}{1}", TextNode.LoadAnnotations(classDef), nl);

            var attr = classDef.InnerClassesAttribute;
            if (attr != null)
            {
                sb.AppendLine();
                sb.AppendLine("Inner classes:");
                var index = 0;
                foreach (var innerClass in classDef.InnerClassesAttribute.Classes)
                {
                    sb.AppendFormat("\t[{0}]{1}", index++, nl);
                    sb.AppendFormat("\tinner:\t\t{0}{1}", innerClass.Inner, nl);
                    sb.AppendFormat("\touter:\t\t{0}{1}", innerClass.Outer, nl);
                    sb.AppendFormat("\tname:\t\t{0}{1}", innerClass.Name, nl);
                    sb.AppendFormat("\taccess flags:\t{0}{1}", AccessFlagsAsString(innerClass.AccessFlags), nl);
                }
            }

            return sb.ToString();
        }

        private string AccessFlagsAsString(ClassAccessFlags accessFlags)
        {
            string result = accessFlags.HasFlag(ClassAccessFlags.Public) ? "public " : string.Empty;
            result += accessFlags.HasFlag(ClassAccessFlags.Final) ? "final " : string.Empty;
            result += accessFlags.HasFlag(ClassAccessFlags.Super) ? "super " : string.Empty;
            result += accessFlags.HasFlag(ClassAccessFlags.Interface) ? "interface " : string.Empty;
            result += accessFlags.HasFlag(ClassAccessFlags.Abstract) ? "abstract " : string.Empty;
            result += accessFlags.HasFlag(ClassAccessFlags.Synthetic) ? "synthetic " : string.Empty;
            result += accessFlags.HasFlag(ClassAccessFlags.Annotation) ? "annotation " : string.Empty;
            result += accessFlags.HasFlag(ClassAccessFlags.Enum) ? "enum " : string.Empty;

            return result.Trim();
        }

        private string AccessFlagsAsString(NestedClassAccessFlags accessFlags)
        {
            string result = accessFlags.HasFlag(NestedClassAccessFlags.Public) ? "public " : string.Empty;
            result += accessFlags.HasFlag(NestedClassAccessFlags.Private) ? "private " : string.Empty;
            result += accessFlags.HasFlag(NestedClassAccessFlags.Protected) ? "protected " : string.Empty;
            result += accessFlags.HasFlag(NestedClassAccessFlags.Static) ? "static " : string.Empty;
            result += accessFlags.HasFlag(NestedClassAccessFlags.Final) ? "final " : string.Empty;
            result += accessFlags.HasFlag(NestedClassAccessFlags.Interface) ? "interface " : string.Empty;
            result += accessFlags.HasFlag(NestedClassAccessFlags.Abstract) ? "abstract " : string.Empty;
            result += accessFlags.HasFlag(NestedClassAccessFlags.Synthetic) ? "synthetic " : string.Empty;
            result += accessFlags.HasFlag(NestedClassAccessFlags.Annotation) ? "annotation " : string.Empty;
            result += accessFlags.HasFlag(NestedClassAccessFlags.Enum) ? "enum " : string.Empty;

            return result.Trim();
        }
    }

    [Export(typeof(INodeBuilder))]
    public class JavaClassFileNodeBuilder : INodeBuilder
    {
        /// <summary>
        /// Can this builder create a node for the given filename in the given APK?
        /// </summary>
        public bool Supports(SourceFile source, string fileName)
        {
            return source.IsJar && fileName.EndsWith(".class", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Create a node for the given filename in the given APK?
        /// </summary>
        public Node Create(SourceFile source, string fileName)
        {
            return new JavaClassFileNode(source, fileName);
        }
    }
}
