using System.Diagnostics;
using System.Linq;

namespace Dot42.ImportJarLib.Doxygen
{
    [DebuggerDisplay("{Name}, {__Signature}")]
    public class DocMethod : DocMember<DocClass>
    {
        private readonly DocMemberList<DocParameter, DocMethod> parameters;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DocMethod(string name)
            : base(name)
        {
            parameters = new DocMemberList<DocParameter, DocMethod>(this);
        }

        /// <summary>
        /// Parameters of the method
        /// </summary>
        public DocMemberList<DocParameter, DocMethod> Parameters { get { return parameters; } }

        public string __Signature
        {
            get { return "(" + string.Join(",", Parameters.Select(x => x.ParameterType)) + ")"; }
        }
    }
}
