namespace Dot42.ImportJarLib.Doxygen
{
    internal class DocTypeRef : IDocTypeRef
    {
        private readonly string name;
        private readonly string refId;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DocTypeRef(string name, string refId)
        {
            this.refId = refId;
            this.name = name;
        }

        /// <summary>
        /// Resolve this reference into an XmlClass.
        /// </summary>
        public IDocResolvedTypeRef Resolve(DocModel model)
        {
            if (!string.IsNullOrEmpty(refId))
            {
                DocClass @class;
                if (model.TryGetClassById(refId, out @class))
                    return @class;
            }
            if (!string.IsNullOrEmpty(name))
            {
                return ResolveByName(model, name);
            }
            return null;
        }

        /// <summary>
        /// Try to resolve a name into an XmlClass.
        /// </summary>
        private static IDocResolvedTypeRef ResolveByName(DocModel model, string name)
        {
            if (name.EndsWith("[]"))
            {
                var compType = ResolveByName(model, name.Substring(0, name.Length - 2));
                return (compType != null) ? new DocArrayType(compType) : null;
            }

            DocClass @class;
            if (model.TryGetClassByName(name, out @class))
                return @class;

            var primitiveType = DocPrimitiveType.Find(name);
            if (primitiveType != null)
                return primitiveType;

            return null;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
