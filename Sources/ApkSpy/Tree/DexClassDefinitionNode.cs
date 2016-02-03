using System;
using System.Text;
using Dot42.ApkSpy.Disassembly;
using Dot42.DexLib;

namespace Dot42.ApkSpy.Tree
{
    internal class DexClassDefinitionNode : TextNode
    {
        private readonly ClassDefinition classDef;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DexClassDefinitionNode(ClassDefinition classDef)
        {
            this.classDef = classDef;
            Text = classDef.Name;
            ImageIndex = classDef.IsInterface ? 5 : 2;
        }

        /// <summary>
        /// Create all child nodes
        /// </summary>
        protected override void CreateChildNodes()
        {
            foreach (var child in classDef.InnerClasses)
            {
                Nodes.Add(new DexClassDefinitionNode(child));
            }
            foreach (var field in classDef.Fields)
            {
                Nodes.Add(new DexFieldDefinitionNode(field));
            }
            foreach (var method in classDef.Methods)
            {
                Nodes.Add(new DexMethodDefinitionNode(method));
            }
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText(ISpyContext settings)
        {
            if (!settings.EnableBaksmali)
            {
                var nl = Environment.NewLine;
                var sb = new StringBuilder();
                sb.AppendFormat("FullName: {0}{1}", classDef.Fullname, nl);
                sb.AppendFormat("SuperClass: {0}{1}",
                    (classDef.SuperClass != null) ? classDef.SuperClass.Fullname : "<none>", nl);

                foreach (var intf in classDef.Interfaces)
                {
                    sb.AppendFormat("Implements: {0}{1}", intf.Fullname, nl);
                }

                sb.AppendFormat("AccessFlags: {0}{1}", AccessFlagsAsString(classDef.AccessFlags), nl);
                
                if (classDef.SourceFile != null)
                    sb.AppendFormat("Source file: {0}{1}", classDef.SourceFile, nl);

                sb.AppendFormat("Annotations: {0}{1}", LoadAnnotations(classDef), nl);
                return sb.ToString();
            }
            else
            {
                return new BaksmaliDisassembler(settings).Disassemble(classDef);
            }

        }

        private string AccessFlagsAsString(AccessFlags accessFlags)
        {
            string result = accessFlags.HasFlag(AccessFlags.Public) ? "public " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Private) ? "private " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Protected) ? "protected " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Static) ? "static " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Final) ? "final " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Interface) ? "interface " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Abstract) ? "abstract " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Synthetic) ? "synthetic " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Annotation) ? "annotation " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Enum) ? "enum " : string.Empty;

            return result.Trim();
        }
    }
}
