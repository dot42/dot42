using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.XModel.Java;
using Dot42.JvmClassLib;
using Mono.Cecil;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Unit of an application
    /// </summary>
    public sealed class XModule
    {
        private readonly Dictionary<Type, object> caches = new Dictionary<Type, object>();
        private readonly Dictionary<string, XTypeDefinition> fullNameCache = new Dictionary<string, XTypeDefinition>();
        //private readonly Dictionary<string, XTypeDefinition> scopeIdCache = new Dictionary<string, XTypeDefinition>();
        private readonly List<XTypeDefinition> types = new List<XTypeDefinition>();
        private List<XTypeDefinition> sortedTypes;
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
            if (fullNameCache.TryGetValue(fullName, out type))
                return true;

            for (var attempt = 0; attempt < 2; attempt++)
            {
                var noImports = (attempt == 0);
                sortedTypes = sortedTypes ?? types.OrderBy(x => x.Priority).ToList();
                foreach (var t in sortedTypes)
                {
                    if (t.TryGet(fullName, noImports, out type))
                    {
                        fullNameCache[fullName] = type;
                        return true;
                    }
                }
            }

            type = null;
            return false;
        }

        internal List<XTypeDefinition> Types { get { return types; } }

        /// <summary>
        /// Add the given type to my list.
        /// </summary>
        private void Add(XTypeDefinition type)
        {
            types.Add(type);
            sortedTypes = null;
        }

        /// <summary>
        /// Callback to call when an assembly was loaded.
        /// All types of the assembly are converted to XType's.
        /// </summary>
        public void OnAssemblyLoaded(AssemblyDefinition assembly)
        {
            foreach (var type in assembly.MainModule.Types)
            {
                Add(new DotNet.XBuilder.ILTypeDefinition(this, null, type));
            }
        }

        /// <summary>
        /// Callback to call when an java class was loaded.
        /// The class is converted to XType's.
        /// </summary>
        public void OnClassLoaded(ClassFile cf)
        {
            if (!cf.IsCreatedByLoader && !cf.IsNested)
            {
                Add(new XBuilder.JavaTypeDefinition(this, null, cf));
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
    }
}
