namespace Dot42.ImportJarLib.Doxygen
{
    public class DocParameter : DocMember<DocMethod>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public DocParameter(string name)
            : base(name)
        {
        }

        public IDocTypeRef ParameterType { get; set; }
    }
}
