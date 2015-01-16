using System.Text;
using Dot42.ApkLib.Resources;

namespace Dot42.ApkSpy.Tree
{
    internal class TableEntryInstanceNode: TextNode
    {
        private readonly Table.Type type;
        private readonly Table.EntryInstance instance;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TableEntryInstanceNode(Table.Type type, Table.EntryInstance instance)
        {
            this.type = type;
            this.instance = instance;
            Text = type.Configuration.ToString();
        }

        public bool IsAnyConfiguration { get { return type.Configuration.IsAny; } }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText(ISpyContext settings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Flags:" + instance.Flags);
            if (!instance.IsComplex)
            {
                sb.AppendLine("Value:" + ((Table.SimpleEntryInstance) instance).StringValue);
            }
            else
            {
                sb.AppendLine("Value:" + instance);
            }
            return sb.ToString();
        }

        internal string GetText(ISpyContext settings)
        {
            return LoadText(settings);
        }
    }
}
