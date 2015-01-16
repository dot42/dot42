using System.Xml.Linq;

namespace Dot42.Mapping
{
    public sealed class VariableList : RegisterEntryList<Variable>
    {
        public VariableList()
            : base("v")
        {
        }

        internal VariableList(XElement e)
            : base(e, "v", x => new Variable(x))
        {
        }
    }
}
