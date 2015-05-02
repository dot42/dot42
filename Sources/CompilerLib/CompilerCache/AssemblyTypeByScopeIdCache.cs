using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Dot42.Mapping;
using Dot42.Utility;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Dot42.CompilerLib.CompilerCache
{
    /// <summary>
    /// Allows to quickly retrieve types and methods of assemblies by
    /// their MetadataToken.
    /// 
    /// Also allows to check if an Assembly was modified since the last
    /// compilation.
    /// </summary>
    public class AssemblyTypeByScopeIdCache
    {
        private readonly Func<AssemblyDefinition, string> _filenameFromAssembly;
        private readonly MapFileLookup _map;
        private readonly ConcurrentDictionary<AssemblyDefinition, bool> _modified = new ConcurrentDictionary<AssemblyDefinition, bool>();
        private readonly ConcurrentDictionary<string, Dictionary<MetadataToken, TypeDefinition>> _typesByMetaDatatoken = new ConcurrentDictionary<string, Dictionary<MetadataToken, TypeDefinition>>();
        private readonly ConcurrentDictionary<string, Dictionary<Tuple<MetadataToken,int>, MethodDefinition>> _methodsByMetaDatatoken = new ConcurrentDictionary<string, Dictionary<Tuple<MetadataToken, int>, MethodDefinition>>();

        public AssemblyTypeByScopeIdCache(Func<AssemblyDefinition, string> filenameFromAssembly, MapFileLookup map)
        {
            _filenameFromAssembly = filenameFromAssembly;
            _map = map;
        }

        public void AddAssembly(AssemblyDefinition assembly)
        {
            if(IsModified(assembly))
                return;

            var scope = assembly.MainModule.Name;
            if (_typesByMetaDatatoken.ContainsKey(scope))
                return;

            var assemblyTypes = assembly.MainModule.GetAllTypes().ToDictionary(t => t.MetadataToken);
            _typesByMetaDatatoken.TryAdd(scope, assemblyTypes);

            // Unfortunately the method's MetadataToken appears to be not neccessarily unique 
            // even in a single type. We therefore use the methods index as ScopeId.

            var assemblyMethods = new Dictionary<Tuple<MetadataToken, int>, MethodDefinition>();
            foreach (var type in assembly.MainModule.GetAllTypes())
            {
                if (!type.HasMethods) continue;
                var token = type.MetadataToken;

                for (int i = 0; i < type.Methods.Count; ++i)
                    assemblyMethods.Add(Tuple.Create(token, i), type.Methods[i]);
            }
            _methodsByMetaDatatoken.TryAdd(scope, assemblyMethods);
        }

        public bool IsModified(AssemblyDefinition assembly)
        {
            bool isModified;
            if (_modified.TryGetValue(assembly, out isModified))
                return isModified;

            var assemblyFileName = _filenameFromAssembly(assembly);
            if (assemblyFileName == null)
            {
                _modified[assembly] = true;
                return true;
            }
                
            var scopeEntry = _map.Scopes.FirstOrDefault(s => s.Filename.Equals(assemblyFileName, StringComparison.Ordinal));

            if (scopeEntry == null)
            {
                _modified[assembly] = true;
                return true;
            }

            isModified = File.GetLastWriteTimeUtc(assemblyFileName) != scopeEntry.Timestamp
                        || Hash.HashFileMD5(assemblyFileName) != scopeEntry.Hashcode;
            _modified[assembly] = isModified;
            return isModified;
        }

        public TypeDefinition FindType(string dexFullName)
        {
            var typeEntry = _map.GetTypeByDexName(dexFullName);
            if (typeEntry == null)
                return null;
            return FindType(typeEntry);
        }

        public TypeDefinition FindType(TypeEntry typeEntry)
        {
            if (string.IsNullOrEmpty(typeEntry.ScopeId))
                return null;

            Dictionary<MetadataToken, TypeDefinition> assemblyTypes;
            if (!_typesByMetaDatatoken.TryGetValue(typeEntry.Scope, out assemblyTypes))
            {
                return null;
            }

            TypeDefinition tdef;

            var token = ScopeIdToMetadataToken(typeEntry.ScopeId);
            assemblyTypes.TryGetValue(token, out tdef);
            return tdef;
        }

        public MethodDefinition FindMethod(MethodEntry methodEntry)
        {
            if (string.IsNullOrEmpty(methodEntry.ScopeId))
                return null;

            var typeEntry = _map.GetTypeByMethodId(methodEntry.Id);
            if (typeEntry == null || string.IsNullOrEmpty(methodEntry.ScopeId))
                return null;

            Dictionary<Tuple<MetadataToken, int>, MethodDefinition> assemblyMethods;
            if (!_methodsByMetaDatatoken.TryGetValue(typeEntry.Scope, out assemblyMethods))
            {
                return null;
            }
            var token = ScopeIdToMetadataToken(typeEntry.ScopeId);
            var idx = int.Parse(methodEntry.ScopeId, CultureInfo.InvariantCulture);

            MethodDefinition def;
            assemblyMethods.TryGetValue(Tuple.Create(token, idx), out def);
            return def;
        }

        public static MetadataToken ScopeIdToMetadataToken(string scopeId)
        {
            return new MetadataToken(UInt32.Parse(scopeId, NumberStyles.AllowHexSpecifier));
        }
    }
}
