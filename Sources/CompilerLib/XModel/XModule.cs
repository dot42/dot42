using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.Cecil;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Unit of an application
    /// </summary>
    public sealed class XModule
    {
        private HashSet<AssemblyNameDefinition> loadedAssemblies = new HashSet<AssemblyNameDefinition>();

        private readonly Dictionary<Type, object> caches = new Dictionary<Type, object>();

        private readonly Dictionary<string, FullNameCacheEntry> fullNameCache = new Dictionary<string, FullNameCacheEntry>();
        private readonly Dictionary<string, XTypeDefinition> scopeIdCache = new Dictionary<string, XTypeDefinition>();
        private readonly List<XTypeDefinition> types = new List<XTypeDefinition>();
        private readonly XTypeSystem typeSystem;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XModule()
        {
            typeSystem = new XTypeSystem(this);
        }

        /// <summary>
        /// Low level types
        /// </summary>
        public XTypeSystem TypeSystem
        {
            get { return typeSystem; }
        }

        /// <summary>
        /// Gets the type with the given namespace and name.
        /// </summary>
        public bool TryGetType(string fullName, out XTypeDefinition type)
        {
            FullNameCacheEntry e;
            if (fullNameCache.TryGetValue(fullName, out e))
            {
                type = e.Type;
                return true;
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Gets the type with the given namespace and name.
        /// </summary>
        internal XTypeDefinition GetTypeByScopeID(string scopeId)
        {
            return scopeIdCache[scopeId];
        }

        internal ReadOnlyCollection<XTypeDefinition> Types { get { return types.AsReadOnly(); } }

        /// <summary>
        /// Add the given type to my list.
        /// </summary>
        internal void Register(XTypeDefinition type, string overrideFullName = null)
        {

            Register(type, null, false);

            string className;
            if (type.TryGetDexImportNames(out className))
            {
                var typeRef = Java.XBuilder.AsTypeReference(this, className, XTypeUsageFlags.DeclaringType);
                Register(type, typeRef.FullName, true);
            }

            if (type.TryGetJavaImportNames(out className))
            {
                var typeRef = Java.XBuilder.AsTypeReference(this, className, XTypeUsageFlags.DeclaringType);
                Register(type, typeRef.FullName, true);
            }
        }

        private void Register(XTypeDefinition type, string overridenName, bool isImport)
        {
            // scopeId must be unique
            if (scopeIdCache.ContainsKey(type.ScopeId) && scopeIdCache[type.ScopeId] != type)
                throw new Exception("scopeId not unique.");
            scopeIdCache[type.ScopeId] = type;

            var fullname = overridenName ?? type.FullName;

            FullNameCacheEntry e;
            if (fullNameCache.TryGetValue(fullname, out e))
            {
                if (e.Priority < type.Priority)
                    return;
                // new priority is higher or equal, 
                // but we must not override a non-import type.
                if (isImport && !e.IsImport)
                    return;
            }

            fullNameCache[fullname] = new FullNameCacheEntry(type, type.Priority, isImport);
        }

        /// <summary>
        /// Callback to call when an assembly was loaded.
        /// All types of the assembly are converted to XType's.
        /// </summary>
        public void OnAssemblyLoaded(AssemblyDefinition assembly)
        {
            // TODO: this is a hack. fix the loading of assemblies so that each 
            //       only gets send to us exactly once. then remove this line.
            if (loadedAssemblies.Contains(assembly.Name))
                return;
            loadedAssemblies.Add(assembly.Name);

            foreach (var type in assembly.MainModule.Types)
            {
                var typeDef = new DotNet.XBuilder.ILTypeDefinition(this, null, type);
                types.Add(typeDef);
                Register(typeDef);
            }
        }

        /// <summary>
        /// Callback to call when a java class was loaded.
        /// The class is converted to XType's.
        /// </summary>
        public void OnClassLoaded(JvmClassLib.ClassFile cf)
        {
            if (!cf.IsCreatedByLoader && !cf.IsNested)
            {
                var typeDef = new Java.XBuilder.JavaTypeDefinition(this, null, cf);
                types.Add(typeDef);
                Register(typeDef);
            }
        }

        /// <summary>
        /// Gets a cache by type.
        /// </summary>
        internal T GetCache<T>()
            where T : new()
        {
            object entry;
            var key = typeof (T);
            if (!caches.TryGetValue(key, out entry))
            {
                entry = new T();
                caches[key] = entry;
            }
            return (T)entry;
        }


        private struct FullNameCacheEntry
        {
            public readonly XTypeDefinition Type;
            public readonly int Priority;
            public readonly bool IsImport;

            public FullNameCacheEntry(XTypeDefinition type, int priority, bool isImport)
            {
                Type = type;
                Priority = priority;
                IsImport = isImport;
            }
        }
    }
}
