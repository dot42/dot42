using System.Linq;
using System.Text;
using Dot42.ApkLib.Resources;

namespace Dot42.ApkSpy.Tree
{
    internal class TableEntryNode: TextNode
    {
        private readonly int entryIndex;
        private readonly Table.TypeSpec typeSpec;
        private readonly Table.Entry entry;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TableEntryNode(int entryIndex, Table.TypeSpec typeSpec, Table.Entry entry)
        {
            this.entryIndex = entryIndex;
            this.typeSpec = typeSpec;
            this.entry = entry;
            Text = string.Format("[{0:X4}] {1}", entryIndex, entry.Key);
        }

        /// <summary>
        /// Create all child nodes
        /// </summary>
        protected override void CreateChildNodes()
        {
            foreach (var type in typeSpec.Types)
            {
                Table.EntryInstance entryInstance;
                if (entry.TryGetInstance(type, out entryInstance))
                {
                    Nodes.Add(new TableEntryInstanceNode(type, entryInstance));
                }
            }
            if (Nodes.Count > 1)
            {
                Text = string.Format("{0} ({1})", Text, Nodes.Count);
            }
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText(ISpyContext settings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Entry: [" + entryIndex + "]");
            sb.AppendLine("Key  :" + entry.Key);
            var anyInstanceNode = Nodes.OfType<TableEntryInstanceNode>().FirstOrDefault(x => x.IsAnyConfiguration);
            if (anyInstanceNode != null)
            {
                sb.Append(anyInstanceNode.GetText(settings));
            }
            return sb.ToString();
        }
    }
}
