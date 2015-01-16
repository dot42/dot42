using System;
using System.Collections.Generic;
using Dot42.ImportJarLib.Doxygen;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Java;
using Mono.Cecil;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Contains modules being generated.
    /// </summary>
    public sealed class TargetFramework : IDocTypeNameResolver
    {
        private readonly Action<string> missingParamTypeReport;
        private readonly TypeNameMap typeNameMap;
        private readonly MethodMap methodMap = new MethodMap();
        private readonly HashSet<string> reportedMissingParamTypes = new HashSet<string>();
        private readonly bool importAsStubs;
        private readonly bool importPublicOnly;
        private readonly HashSet<string> excludedPackages;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TargetFramework(ITypeMapResolver resolver, AssemblyClassLoader assemblyClassLoader, DocModel xmlModel, Action<string> missingParamTypeReport, bool importAsStubs, bool importPublicOnly, IEnumerable<string> excludedPackages)
        {
            this.missingParamTypeReport = missingParamTypeReport;
            this.importAsStubs = importAsStubs;
            this.importPublicOnly = importPublicOnly;
            XmlModel = xmlModel;
            typeNameMap = new TypeNameMap(resolver, assemblyClassLoader);
            this.excludedPackages = new HashSet<string>(excludedPackages);
        }

        /// <summary>
        /// Loaded Xml model
        /// </summary>
        public DocModel XmlModel { get; private set; }

        /// <summary>
        /// Is the given package excluded?
        /// </summary>
        public bool IsExcludedPackage(string pkg)
        {
            return excludedPackages.Contains(pkg);
        }

        /// <summary>
        /// Try to get a XmlClass for the given root class file.
        /// </summary>
        public DocClass GetXmlClass(ClassFile classDef)
        {
            var model = XmlModel;
            if (model == null)
                return null;

            DocClass docClass;
            var name = classDef.ClassName.Replace('/', '.').Replace('$', '.');
            if (model.TryGetClassByName(name, out docClass))
                return docClass;
            return null;
        }

        /// <summary>
        /// Is the current module equal to mscorlib?
        /// </summary>
        public bool IsBuildingCorlib { get { return true; } }

        /// <summary>
        /// Mapping between java type names and NetTypeDefinitions.
        /// </summary>
        public TypeNameMap TypeNameMap { get { return typeNameMap; } }

        /// <summary>
        /// Mapping between java methods and NetMethodDefinitions.
        /// </summary>
        public MethodMap MethodMap { get { return methodMap; } }

        /// <summary>
        /// Should types and members be considered stubs only?
        /// </summary>
        public bool ImportAsStubs
        {
            get { return importAsStubs; }
        }

        /// <summary>
        /// Should only publicly visible types/members be imported?
        /// </summary>
        public bool ImportPublicOnly
        {
            get { return importPublicOnly; }
        }

        /// <summary>
        /// Resolve java type name into .NET name.
        /// </summary>
        string IDocTypeNameResolver.ResolveTypeName(string className)
        {
            var def = typeNameMap.TryGetByJavaClassName(className);
            return (def != null) ? def.FullName : null;
        }


        /// <summary>
        /// Add the given assembly to the list of import assemblies.
        /// </summary>
        public void ImportAssembly(AssemblyDefinition assembly, Action<ClassFile> classLoaded)
        {
            typeNameMap.ImportAssembly(assembly, classLoaded, this);
        }

        /// <summary>
        /// All assemblies have been imported.
        /// </summary>
        public void ImportAssembliesCompleted()
        {
            typeNameMap.ImportAssembliesCompleted(this);
        }

        /// <summary>
        /// Notify the build of parameters missing in the given type.
        /// </summary>
        public void ReportMissingParameterName(string className)
        {
            if ((XmlModel == null) || (missingParamTypeReport == null))
                return;

            var index = className.IndexOf('$');
            if (index > 0) className = className.Substring(0, index);
            index = className.LastIndexOf('/');
            var package = (index > 0) ? className.Substring(0, index) : className;
            if (reportedMissingParamTypes.Add(package))
            {
                missingParamTypeReport(package);
            }            
        }
    }
}
