using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;
using NinjaTools.Collections;

namespace Dot42.Mapping
{
    /// <summary>
    /// Allows quick lookups of entries in MapFile
    /// 
    /// Assumes the the MapFile is not changed.
    /// Note: think about putting this into a SQLite database.
    /// </summary>
    public class MapFileLookup
    {
        private readonly MapFile _map;

        private readonly Dictionary<string, TypeEntry> _typesByDexName = new Dictionary<string, TypeEntry>();
        private readonly Dictionary<string, TypeEntry> _typesByClrName = new Dictionary<string, TypeEntry>();
        private readonly Dictionary<string, TypeEntry> _typesBySignature = new Dictionary<string, TypeEntry>();
        private readonly Dictionary<int, TypeEntry> _typesById = new Dictionary<int, TypeEntry>();
        private readonly Dictionary<int, List<SourceCodePosition>> _positionsByMethodId;
        private readonly Dictionary<int, TypeEntry> _typesByMethodId = new Dictionary<int, TypeEntry>();
        private readonly Dictionary<Tuple<string, string, string>, MethodEntry> _methodsByFullSignature = new Dictionary<Tuple<string, string, string>, MethodEntry>();

        public MapFileLookup(MapFile map)
        {
            _map = map;

            // note the OrderBy(MethodOffset), which allows us to perform a binary search later on. 
            _positionsByMethodId = map.Documents.SelectMany(d => d.Positions, SourceCodePosition.Create)
                                                .GroupBy(p=>p.Position.MethodId)
                                                .ToDictionary(p=>p.Key, p=>p.OrderBy(dp=>dp.Position.MethodOffset).ToList());

            foreach (var entry in map.TypeEntries)
            {
                Add(entry);
            }
        }

        public ICollection<TypeEntry> TypeEntries { get { return _map.TypeEntries; } }
        public ICollection<ScopeEntry> Scopes { get { return _map.Scopes; } }

        public TypeEntry GeneratedType { get { return _map.GeneratedType; } }

        private void Add(TypeEntry entry)
        {
            // if there are duplicates in any field, assume the more "important"
            // definition comes first.

            if (!string.IsNullOrEmpty(entry.DexName))
            {
                if (!_typesByDexName.ContainsKey(entry.DexName))
                    _typesByDexName[entry.DexName] = entry;
            }

            if (!string.IsNullOrEmpty(entry.Name))
            {
                if(!_typesByClrName.ContainsKey(entry.Name))
                    _typesByClrName[entry.Name] = entry;
            }

            if (!string.IsNullOrEmpty(entry.DexSignature))
            {
                if(!_typesBySignature.ContainsKey(entry.DexSignature))
                    _typesBySignature[entry.DexSignature] = entry;
            }

            if (entry.Id != 0)
            {
                _typesById[entry.Id] = entry;
            }

            if (entry.Methods.Count > 0)
            {
                string typeDexName = entry.DexName;
                string typeDexSignature = entry.DexSignature;

                if (entry.Id == 0)
                {
                    // all methods are in the generated class
                    if (_map.GeneratedType != null)
                    {
                        typeDexName = _map.GeneratedType.DexName;
                        typeDexSignature = _map.GeneratedType.DexSignature;
                    }
                }

                if (typeDexName != null) // should not happen, but guard against anyways.
                {
                    foreach (var m in entry.Methods)
                    {
                        if (m.Id != 0)
                            _typesByMethodId[m.Id] = entry;

                        _methodsByFullSignature[Tuple.Create(typeDexName, m.DexName, m.DexSignature)] = m;
                        _methodsByFullSignature[Tuple.Create(typeDexSignature, m.DexName, m.DexSignature)] = m;
                    }
                }
            }
        }


        /// <summary>
        /// Gets a type by its map file id.
        /// </summary>
        /// <returns>Null if not found</returns>
        public TypeEntry GetTypeById(int id)
        {
            TypeEntry e;
            _typesById.TryGetValue(id, out e);
            return e;
        }

        /// <summary>
        /// Gets the type the method belongs to.
        /// </summary>
        /// <returns>Null if not found</returns>
        public TypeEntry GetTypeByMethodId(int methodId)
        {
            TypeEntry e;
            _typesByMethodId.TryGetValue(methodId, out e);
            return e;
        }

        /// <summary>
        /// Gets a type by its signature
        /// </summary>
        /// <returns>null if not found</returns>
        public TypeEntry GetTypeBySignature(string signature)
        {
            TypeEntry e;
            _typesBySignature.TryGetValue(signature, out e);
            return e;
        }

        /// <summary>
        /// Gets a type by its CLR Name
        /// </summary>
        /// <returns>null if not found</returns>
        public TypeEntry GetTypeByClrName(string clrFullName)
        {
            TypeEntry e;
            _typesByClrName.TryGetValue(clrFullName, out e);
            return e;
        }

        public TypeEntry GetTypeByDexName(string dexFullName)
        {
            TypeEntry e;
            _typesByDexName.TryGetValue(dexFullName, out e);
            return e;
        }

        public MethodEntry GetMethodByDexSignature(string dexClassNameOrSignature, string dexMethodName, string dexSignature)
        {
            MethodEntry m;
            var key = Tuple.Create(dexClassNameOrSignature, dexMethodName, dexSignature);
            if(_methodsByFullSignature.TryGetValue(key, out m))
                return m;

            // Does dexMethodName have a '[' ... ']' postfix?
            var openIndex = dexMethodName.IndexOf('[');
            if (openIndex < 0)
                return null;
            var closeIndex = dexMethodName.LastIndexOf(']');
            if (closeIndex < 0)
                return null;

            // Try name without generics
            return GetMethodByDexSignature(dexClassNameOrSignature, dexMethodName.Substring(0, openIndex), dexSignature);
        }

        /// <summary>
        /// Get all source code positions for the given type and method. The returned list will be
        /// ordered by MethodOffset.
        /// </summary>
        public IList<SourceCodePosition> GetSourceCodePositions(MethodEntry method)
        {
            List<SourceCodePosition> posList;
            if (_positionsByMethodId.TryGetValue(method.Id, out posList))
            {
                return posList.AsReadOnly();
            }
            return new SourceCodePosition[0];
        }

        /// <summary>
        /// Try to find the document location that belongs to the given method + offset.
        /// </summary>
        /// <returns>null, if not found. If we know the document, but the offset is marked to
        /// have no source code attached (compiler generated), the returned position will
        /// be marked as IsSpecial.</returns>
        public SourceCodePosition FindSourceCode(MethodEntry method, int methodOffset, bool allowSpecial = true)
        {
            var locs = GetSourceCodePositions(method);

            // perform a binary search
            int idx = locs.FindLastIndexSmallerThanOrEqualTo(methodOffset, p => p.Position.MethodOffset);

            if (idx != -1 && (allowSpecial || !locs[idx].IsSpecial))
                return locs[idx];

            if (allowSpecial && locs.Count > 0)
            {
                // this can only happen at the beginning of the method.
                // forge a special location.
                var pos = locs[0].Position;
                var forgedPos = new DocumentPosition(pos.Start.Line,pos.Start.Column, pos.End.Line, pos.End.Column, pos.TypeId, pos.MethodId, DocumentPosition.SpecialOffset);
                return new SourceCodePosition(locs[0].Document, forgedPos);
            }

            return null;
        }

        /// <summary>
        /// Beginning at offset, returns the next available source code position.
        /// Will not return positions with "IsSpecial" flag set.
        /// </summary>
        /// <returns>null, if not found</returns>
        public SourceCodePosition FindNextSourceCode(MethodEntry method, int methodOffset)
        {
            var loc = GetSourceCodePositions(method);

            int idx = loc.FindFirstIndexGreaterThanOrEqualTo(methodOffset, p => p.Position.MethodOffset);

            for (;idx != -1 && idx < loc.Count; ++idx)
            {
                var ret = loc[idx];
                if (ret.IsSpecial)
                    continue;
                return ret;
            }
            return null;
        }

        /// <summary>
        /// Gets a document with the given path, or null, if not found.
        /// </summary>
        public Document FindDocument(string document)
        {
            return _map.GetOrCreateDocument(document, false);
        }

        /// <summary>
        /// TODO: not sure what this method actually does. Is is quite, but 
        ///       not entirely, similar to GetTypeByDexName
        /// </summary>
        /// <returns>Null if not found</returns>
        public TypeEntry GetTypeByNewName(string fullname)
        {
            return _map.GetTypeByNewName(fullname);
        }
    }
}
