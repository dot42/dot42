using System;
using System.Collections.Generic;
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
    public sealed class MapFile : IEnumerable<TypeEntry>
    {
        private readonly Dictionary<string, TypeEntry> typesByDexName;
        private readonly Dictionary<string, Document> documents = new Dictionary<string, Document>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public MapFile()
        {
            typesByDexName = new Dictionary<string, TypeEntry>();
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

            typesByDexName = new Dictionary<string, TypeEntry>();
            foreach (var type in map.Root.Elements("type"))
            {
                Add(new TypeEntry(type));
            }

            foreach (var element in map.Root.Elements("document"))
            {
                var doc = new Document(element);
                documents.Add(doc.Path.ToLowerInvariant(), doc);
            }
        }

        /// <summary>
        /// Convert to XML.
        /// </summary>
        public XDocument ToXml()
        {
            var root = new XElement("dmap");
            root.Add(typesByDexName.Values.Select(x => x.ToXml("type")));
            root.Add(documents.Values.Select(x => x.ToXml("document")));
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
            if (!string.IsNullOrEmpty(entry.DexName))
            {
                typesByDexName[entry.DexName] = entry;
            }
            else if (!string.IsNullOrEmpty(entry.Name))
            {
                typesByDexName[entry.Name] = entry;
            }            
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
        /// Gets a type by its map file id.
        /// </summary>
        /// <returns>Null if not found</returns>
        public TypeEntry GetTypeById(int id)
        {
            return typesByDexName.Values.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Gets a document with given path or create it.
        /// </summary>
        public Document GetOrCreateDocument(string path, bool create)
        {
            Document result;
            path = path.ToLowerInvariant();
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

        /// <summary>
        /// Try to find the document location that belongs to the given type, method + offset.
        /// </summary>
        public bool TryFindLocation(TypeEntry type, MethodEntry method, int methodOffset, out Document document, out DocumentPosition position)
        {
            document = null;
            position = null;

            foreach (var doc in documents.Values)
            {
                foreach (var docPos in doc.Positions)
                {
                    if ((docPos.TypeId != type.Id) || (docPos.MethodId != method.Id))
                        continue;

                    // type and method matches
                    if ((docPos.MethodOffset <= methodOffset) || (position == null))
                    {
                        // Found possible result
                        if ((position == null) || (docPos.MethodOffset > position.MethodOffset))
                        {
                            // Found better result
                            document = doc;
                            position = docPos;
                        }
                    }

                }
            }

            return (position != null);
        }

        /// <summary>
        /// Get all locations for the given type and method.
        /// </summary>
        public IEnumerable<Tuple<Document, DocumentPosition>> GetLocations(TypeEntry type, MethodEntry method)
        {
            foreach (var doc in documents.Values)
            {
                foreach (var docPos in doc.Positions)
                {
                    if ((docPos.TypeId != type.Id) || (docPos.MethodId != method.Id))
                        continue;
                    yield return Tuple.Create(doc, docPos);
                }
            }
        }

        /// <summary>
        /// Enumerate all types
        /// </summary>
        public IEnumerator<TypeEntry> GetEnumerator()
        {
            return typesByDexName.Values.GetEnumerator();
        }

        /// <summary>
        /// Enumerate all types
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
