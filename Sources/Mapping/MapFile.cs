using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.Mapping
{
    /// <summary>
    /// Map between CLR and DEX names
    /// </summary>
    public sealed class MapFile 
    {
        private readonly Dictionary<string, TypeEntry> typesByDexName = new Dictionary<string, TypeEntry>();
        private readonly Dictionary<string, Document> documents = new Dictionary<string, Document>(StringComparer.InvariantCultureIgnoreCase);
        private readonly List<ScopeEntry> scopes = new List<ScopeEntry>();
        private TypeEntry generatedType;

        public ICollection<ScopeEntry> Scopes { get { return new ReadOnlyCollection<ScopeEntry>(scopes); } }
        public ICollection<Document> Documents { get { return documents.Values; } }
        public ICollection<TypeEntry> TypeEntries { get { return typesByDexName.Values; } }

        /// <summary>
        /// this TypeEntry contains the dexName of the compiler generated class, typically named
        /// [package].__generated
        /// </summary>
        public TypeEntry GeneratedType { get { return generatedType; } }

        

        /// <summary>
        /// Default ctor
        /// </summary>
        public MapFile()
        {
        }

        /// <summary>
        /// Read mapfile from disk
        /// </summary>
        public MapFile(string path)
        {
            XDocument map;
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                map = CompressedXml.Load(fileStream);
            }

            foreach (var element in map.Root.Elements("scope"))
            {
                scopes.Add(new ScopeEntry(element));
            }

            foreach (var type in map.Root.Elements("type"))
            {
                Add(new TypeEntry(type));
            }

            foreach (var element in map.Root.Elements("document"))
            {
                var doc = new Document(element);
                documents.Add(doc.Path, doc);
            }
        }

        /// <summary>
        /// Convert to XML.
        /// </summary>
        public XDocument ToXml()
        {
            var root = new XElement("dmap");
            root.Add(typesByDexName.Values.OrderBy(o=>o.Id)
                                          .Select(x => x.ToXml("type")));
            root.Add(documents.Values.Select(x => x.ToXml("document")));
            root.Add(scopes.Select(x => x.ToXml("scope")));
            return new XDocument(root);
        }

        /// <summary>
        /// Save to the given path.
        /// </summary>
        public void Save(string path)
        {
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Create xml
            var document = ToXml();

            // Save to disk
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                CompressedXml.WriteTo(document, fileStream, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Add the given type.
        /// </summary>
        public void Add(TypeEntry entry)
        {
            if (entry.Id == -1)
            {
                generatedType = entry;
            }

            if (!string.IsNullOrEmpty(entry.DexName))
            {
                typesByDexName[entry.DexName] = entry;
            }
            else if (!string.IsNullOrEmpty(entry.Name))
            {
                typesByDexName[entry.Name] = entry;
            }
        }

        public void Add(ScopeEntry scopeEntry)
        {
            scopes.Add(scopeEntry);
        }

        /// <summary>
        /// Look for a type by its new name.
        /// </summary>
        /// <param name="newName"></param>
        /// <returns>Null if not found</returns>
        public TypeEntry GetTypeByNewName(string newName)
        {
            TypeEntry entry;
            // Try name directly.
            if (typesByDexName.TryGetValue(newName, out entry))
            {
                return entry;
            }

            // Mayby a nested type
            var index = newName.LastIndexOfAny(new[] { '.', '+' });
            if (index < 0)
                return null;

            var baseTypeEntry = GetTypeByNewName(newName.Substring(0, index));
            if (baseTypeEntry == null)
                return null;

            var prefix = baseTypeEntry.Name + '+';
            var nestedNewName = newName.Substring(index + 1);
            foreach (var nestedEntry in typesByDexName.Values)
            {
                if ((nestedEntry.DexName == nestedNewName) && (nestedEntry.Name.StartsWith(prefix)))
                {
                    var nestedName = nestedEntry.Name.Substring(prefix.Length);
                    // More nesting levels?
                    if (nestedName.IndexOf('+') < 0)
                    {
                        // No more nesting levels, we found it
                        return nestedEntry;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a document with given path or create it.
        /// </summary>
        public Document GetOrCreateDocument(string path, bool create)
        {
            Document result;
            if (documents.TryGetValue(path, out result))
                return result;
            result = new Document(path);
            documents[path] = result;
            return result;
        }

        /// <summary>
        /// Perform size and speed optimizations.
        /// </summary>
        public void Optimize()
        {
            foreach (var doc in documents.Values) doc.Optimize();            
        }
    }
}
