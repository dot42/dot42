using System.Collections.Generic;
using System.Linq;

namespace Dot42.BuildTools.Documentation
{
    internal abstract class DocumentationSet
    {
        private readonly Dictionary<string, NamespaceDocumentation> namespaceTypes = new Dictionary<string, NamespaceDocumentation>();

        /// <summary>
        /// Add the given type to this set.
        /// </summary>
        internal void Add(TypeDocumentation type)
        {
            NamespaceDocumentation ns;
            if (!namespaceTypes.TryGetValue(type.Namespace, out ns))
            {
                ns = new NamespaceDocumentation(type.Namespace);
                namespaceTypes.Add(type.Namespace, ns);
            }
            ns.Add(type);
        }

        /// <summary>
        /// Add the given namespace.
        /// </summary>
        protected void Add(NamespaceDocumentation ns)
        {
            namespaceTypes.Add(ns.Name, ns);
        }

        /// <summary>
        /// Gets all namespaces
        /// </summary>
        public IEnumerable<NamespaceDocumentation> Namespaces
        {
            get { return namespaceTypes.Values.OrderBy(x => x.Name); }
        }

        /// <summary>
        /// Try to get a namespace by name.
        /// Returns true if found, false otherwise.
        /// </summary>
        public bool TryGetNamespace(string name, out NamespaceDocumentation ns)
        {
            return namespaceTypes.TryGetValue(name, out ns);
        }

        /// <summary>
        /// Gets all types in the given namespace.
        /// </summary>
        public IEnumerable<TypeDocumentation> GetTypesInNamespace(string @namespace)
        {
            NamespaceDocumentation ns;
            return namespaceTypes.TryGetValue(@namespace, out ns) ? ns.Types : Enumerable.Empty<TypeDocumentation>();
        }

        /// <summary>
        /// Does this set contain a matching namespace?
        /// </summary>
        public bool ContainsMatching(NamespaceDocumentation @namespace)
        {
            return namespaceTypes.ContainsKey(@namespace.Name);
        }

        /// <summary>
        /// Try to find a type in this set that matches the given type.
        /// </summary>
        private bool TryGetMatchingType(TypeDocumentation type, out TypeDocumentation result)
        {
            result = null;
            NamespaceDocumentation ns;
            if (!TryGetNamespace(type.Namespace, out ns))
                return false;
            return ns.TryGetMatchingType(type, out result);
        }

        /// <summary>
        /// Does this set contain a matching type?
        /// </summary>
        public bool ContainsMatching(TypeDocumentation type)
        {
            TypeDocumentation mine;
            return TryGetMatchingType(type, out mine);
        }

        /// <summary>
        /// Does this set contain a matching event?
        /// </summary>
        public bool ContainsMatching(EventDocumentation @event)
        {
            TypeDocumentation mine;
            return TryGetMatchingType(@event.DeclaringType, out mine) && mine.ContainsMatching(@event);
        }

        /// <summary>
        /// Does this set contain a matching field?
        /// </summary>
        public bool ContainsMatching(FieldDocumentation field)
        {
            TypeDocumentation mine;
            return TryGetMatchingType(field.DeclaringType, out mine) && mine.ContainsMatching(field);
        }

        /// <summary>
        /// Does this set contain a matching method?
        /// </summary>
        public bool ContainsMatching(MethodDocumentation method)
        {
            TypeDocumentation mine;
            return TryGetMatchingType(method.DeclaringType, out mine) && mine.ContainsMatching(method);
        }

        /// <summary>
        /// Does this set contain a matching property?
        /// </summary>
        public bool ContainsMatching(PropertyDocumentation property)
        {
            TypeDocumentation mine;
            return TryGetMatchingType(property.DeclaringType, out mine) && mine.ContainsMatching(property);
        }
    }
}
