using System;
using System.Text;
using Dot42.DexLib;

namespace Dot42.ApkSpy.Tree
{
    internal class DexFieldDefinitionNode : TextNode
    {
        private readonly FieldDefinition fieldDef;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DexFieldDefinitionNode(FieldDefinition fieldDef)
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
            sb.AppendFormat("Type:        {0}{1}", fieldDef.Type, nl);
            sb.AppendFormat("Value:       {0}{1}", fieldDef.Value, nl);
            return sb.ToString();
        }
     
        private string AccessFlagsAsString(AccessFlags accessFlags)
        {
            string result = accessFlags.HasFlag(AccessFlags.Public) ? "public " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Private) ? "private " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Protected) ? "protected " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Static) ? "static " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Final) ? "final " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Volatile) ? "volatile " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Transient) ? "transient " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Synthetic) ? "synthetic " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Enum) ? "enum " : string.Empty;

            return result.Trim();
        }
    }
}
