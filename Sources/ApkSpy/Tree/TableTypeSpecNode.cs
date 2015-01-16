using System.Linq;
using System.Text;
using Dot42.ApkLib.Resources;

namespace Dot42.ApkSpy.Tree
{
    internal class TableTypeSpecNode: TextNode
    {
        private readonly Table.TypeSpec spec;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TableTypeSpecNode(Table.TypeSpec typeSpec)
        {
            spec = typeSpec;
            Text = typeSpec.Name;
        }

        protected override void CreateChildNodes()
        {
            var entryIndex = 0;
            Nodes.AddRange(spec.Entries.Select(x => new TableEntryNode(entryIndex++, spec, x)).ToArray());
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText(ISpyContext settings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("TypeSpec.ID   : " + spec.Id.ToString());
            sb.AppendLine("TypeSpec.Name : " + spec.Name);
            return sb.ToString();
        }
    }
}
