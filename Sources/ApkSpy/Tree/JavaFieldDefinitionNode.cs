using System;
using System.Text;
using Dot42.JvmClassLib;

namespace Dot42.ApkSpy.Tree
{
    internal class JavaFieldDefinitionNode : TextNode
    {
        private readonly FieldDefinition fieldDef;

        /// <summary>
        /// Default ctor
        /// </summary>
        public JavaFieldDefinitionNode(FieldDefinition fieldDef)
        {
            this.fieldDef = fieldDef;
            Text = fieldDef.Name;
            ImageIndex = 4;
        }

        /// <summary>
        /// Create all child nodes
        /// </summary>
        protected override void CreateChildNodes()
        {
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText(ISpyContext settings)
        {
            var nl = Environment.NewLine;
            var sb = new StringBuilder();
            sb.AppendFormat("AccessFlags: {0}{1}", AccessFlagsAsString(fieldDef.AccessFlags), nl);
            sb.AppendFormat("Type:        {0}{1}", fieldDef.FieldType.ClassName, nl);
            sb.AppendFormat("Value:       {0}{1}", fieldDef.ConstantValue, nl);
            sb.AppendFormat("Annotations: {0}{1}", TextNode.LoadAnnotations(fieldDef), nl);
            return sb.ToString();
        }
     
        private string AccessFlagsAsString(FieldAccessFlags accessFlags)
        {
            string result = accessFlags.HasFlag(FieldAccessFlags.Public) ? "public " : string.Empty;
            result += accessFlags.HasFlag(FieldAccessFlags.Private) ? "private " : string.Empty;
            result += accessFlags.HasFlag(FieldAccessFlags.Protected) ? "protected " : string.Empty;
            result += accessFlags.HasFlag(FieldAccessFlags.Static) ? "static " : string.Empty;
            result += accessFlags.HasFlag(FieldAccessFlags.Final) ? "final " : string.Empty;
            result += accessFlags.HasFlag(FieldAccessFlags.Volatile) ? "volatile " : string.Empty;
            result += accessFlags.HasFlag(FieldAccessFlags.Transient) ? "transient " : string.Empty;
            result += accessFlags.HasFlag(FieldAccessFlags.Synthetic) ? "synthetic " : string.Empty;
            result += accessFlags.HasFlag(FieldAccessFlags.Enum) ? "enum " : string.Empty;

            return result.Trim();
        }
    }
}
