using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.JvmClassLib;
using Mono.Cecil;

namespace Dot42.LoaderLib.Java
{
    /// <summary>
    /// Load java classes from JavaClass attributes included in assemblies.
    /// </summary>
    public sealed partial class AssemblyClassLoader : IClassLoader
    {
        private readonly object dataLock = new object();
        private readonly List<AssemblyClasses> loadedAssemblies = new List<AssemblyClasses>();
        private readonly Action<ClassFile> classLoaded;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AssemblyClassLoader(Action<ClassFile> classLoaded)
        {
            this.classLoaded = classLoaded;
        }

        /// <summary>
        /// Load the classes in the given assembly.
        /// </summary>
        public void LoadAssembly(AssemblyDefinition assembly, Action<AssemblyClasses> initialize = null)
        {
            lock (dataLock)
            {
                var existing = loadedAssemblies.FirstOrDefault(x => x.Assembly == assembly);
                if (existing != null)
                {
                    if (initialize != null)
                    {
                        initialize(existing);
                    }
                    return;
                }

                var classes = new AssemblyClasses(assembly);
                loadedAssemblies.Add(classes);
                if (initialize != null)
                {
                    initialize(classes);
                }
            }
        }

        /// <summary>
        /// True if not assemblies have been loaded.
        /// </summary>
        public bool IsEmpty { get { return (loadedAssemblies.Count == 0); } }

        /// <summary>
        /// Load a class by the given name.
        /// </summary>
        public bool TryLoadClass(string className, out ClassFile result)
        {
#if DEBUG
            //Debugger.Launch();
#endif
            lock (dataLock)
            {
                foreach (var assemblyClasses in loadedAssemblies)
                {
                    JavaClass jClass;
                    if (assemblyClasses.TryGetJavaClass(className, out jClass))
                    if (jClass != null)
                    {
                        result = jClass.Resolve(this, classLoaded);
                        return (result != null);
                    }

                    DexImport dexImport;
                    if (assemblyClasses.TryGetDexImport(className, out dexImport))
                    {
                        result = dexImport.Resolve(this, classLoaded, null);
                        return (result != null);
                    }
                }
                result = null;
                return false;
            }
        }

        public ClassSource TryGetClassSource(string className)
        {
            lock (dataLock)
            {
                foreach (var assemblyClasses in loadedAssemblies)
                {
                    JavaClass jClass;
                    if (!assemblyClasses.TryGetJavaClass(className, out jClass) || jClass == null)
                        continue;
                    return jClass.ClassSource;
                }
                return null;
            }
        }

        /// <summary>
        /// Try to get dex import data for the given java class name?
        /// </summary>
        /// <returns>False if not found.</returns>
        public bool TryGetDexImport(string className, out DexImport dexImport)
        {
            lock (dataLock)
            {
                foreach (var assemblyClasses in loadedAssemblies)
                {
                    if (assemblyClasses.TryGetDexImport(className, out dexImport))
                        return true;
                }
                dexImport = null;
                return false;
            }
        }

        /// <summary>
        /// Try to get dex import data for the given type definition?
        /// </summary>
        /// <returns>False if not found.</returns>
        public bool TryGetDexImport(TypeDefinition type, out DexImport dexImport)
        {
            lock (dataLock)
            {
                foreach (var assemblyClasses in loadedAssemblies)
                {
                    if (assemblyClasses.TryGetDexImport(type, out dexImport))
                        return true;
                }
                dexImport = null;
                return false;
            }
        }

        /// <summary>
        /// Gets all class names found in the given assembly.
        /// </summary>
        public IEnumerable<string> GetClassNames(AssemblyDefinition assembly)
        {
            lock (dataLock)
            {
                return loadedAssemblies.Where(x => x.Assembly == assembly).SelectMany(x => x.ClassNames).Distinct().ToList();
            }
        }

        /// <summary>
        /// Gets all package names found in this loader.
        /// </summary>
        public IEnumerable<string> Packages
        {
            get
            {
                lock (dataLock)
                {
                    return loadedAssemblies.SelectMany(x => x.Packages).Distinct().ToList();
                }
            }
        }

        public AssemblyDefinition GetAssembly(ClassFile classFile)
        {
            AssemblyClasses cl;
            lock(dataLock)
                cl = loadedAssemblies.SingleOrDefault(c => c.ClassNames.Contains(classFile.ClassName));
            
            if (cl == null)
                return null;

            return cl.Assembly;
        }
    }
}
