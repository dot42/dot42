using System.Xml.Linq;

namespace Dot42.Mapping
{
    public sealed class ParameterList : RegisterEntryList<Parameter>
    {
        public ParameterList()
            : base("p")
        {
        }

        internal ParameterList(XElement e)
            : base(e, "p", x => new Parameter(x))
        {
        }
    }
}
