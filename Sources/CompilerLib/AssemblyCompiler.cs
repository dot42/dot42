using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.ApkLib.Resources;
using Dot42.CompilerLib.Reachable;
using Dot42.CompilerLib.Structure.DotNet;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.XModel;
using Dot42.FrameworkDefinitions;
using Dot42.LoaderLib.Java;
using Dot42.Mapping;
using Mono.Cecil;

namespace Dot42.CompilerLib
{
    public class AssemblyCompiler
    {
        private readonly XModule module;
        private readonly AssemblyClassLoader assemblyClassLoader;
        private readonly HashSet<string> rootClassNames;
        private readonly CompilationMode mode;
        private readonly List<AssemblyDefinition> assemblies;
        private readonly List<AssemblyDefinition> references;
        private readonly Table resources;
        private readonly bool generateDebugInfo;
        private readonly Dictionary<XTypeDefinition, DelegateType> delegateTypes = new Dictionary<XTypeDefinition, DelegateType>();
        private readonly Dictionary<TypeDefinition, AttributeAnnotationInterface> attributeAnnotationTypes = new Dictionary<TypeDefinition, AttributeAnnotationInterface>();
        private readonly MapFile mapFile = new MapFile();
        private int lastMapFileId;
        private readonly Dictionary<string, XTypeReference> internalTypeReferences = new Dictionary<string, XTypeReference>();
        private string freeAppsKey;
        private bool? addPropertyAnnotations;
        private readonly ITargetPackage targetPackage;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AssemblyCompiler(CompilationMode mode, List<AssemblyDefinition> assemblies, List<AssemblyDefinition> references, Table resources, NameConverter nameConverter, bool generateDebugInfo, AssemblyClassLoader assemblyClassLoader,
            HashSet<string> rootClassNames, XModule module)
        {
            this.mode = mode;
            this.assemblies = assemblies;
            this.references = references;
            this.resources = resources;
            this.generateDebugInfo = generateDebugInfo;
            this.assemblyClassLoader = assemblyClassLoader;
            this.rootClassNames = rootClassNames;
            this.module = module;
            targetPackage = new Target.Dex.DexTargetPackage(nameConverter, this);
        }

        public CompilationMode CompilationMode { get { return mode; } }
        public XModule Module { get { return module; } }
        public List<AssemblyDefinition> Assemblies { get { return assemblies; } }
        public Table ResourceTable { get { return resources; } }
        public MapFile MapFile { get { return mapFile; } }

        /// <summary>
        /// Gets the classloader used by this compiler.
        /// </summary>
        public AssemblyClassLoader ClassLoader
        {
            get { return assemblyClassLoader; }
        }

        /// <summary>
        /// Compile all types and members
        /// </summary>
        public void Compile()
        {
#if DEBUG
            //System.Diagnostics.Debugger.Launch();
#endif

            // Detect Free Apps Key 
            var allAttributes = assemblies.SelectMany(x => x.CustomAttributes);
            var attr = allAttributes.FirstOrDefault(x => x.AttributeType.Name == AttributeConstants.FreeAppsKeyAttributeName && x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace);
            freeAppsKey = (attr != null) ? (string)attr.ConstructorArguments[0].Value : null;

            // Detect all types to include in the compilation
            var reachableContext = new ReachableContext(this, assemblies.Select(x => x.Name), rootClassNames);
            const bool includeAllJavaCode = false;
            foreach (var assembly in assemblies)
            {
                reachableContext.MarkRoots(assembly, includeAllJavaCode);
            }
            foreach (var assembly in references.Where(IsLibraryProject))
            {
                reachableContext.MarkRoots(assembly, includeAllJavaCode);                
            }
            reachableContext.Complete();

            // Convert IL to java compatible constructs.
            ILConversion.ILToJava.Convert(reachableContext);

            // Convert all types
            var classBuilders = reachableContext.ClassBuilders.OrderBy(x => x.SortPriority).ThenBy(x => x.FullName).ToList();
            classBuilders.ForEach(x => x.Create(targetPackage));
            classBuilders.ForEach(x => x.Implement(targetPackage));
            classBuilders.ForEach(x => x.FixUp(targetPackage));
            classBuilders.ForEach(x => x.GenerateCode(targetPackage));
            classBuilders.ForEach(x => x.CreateAnnotations(targetPackage));

            // Compile all methods
            targetPackage.CompileToTarget(generateDebugInfo, mapFile);

            // Add structure annotations
            targetPackage.AfterCompileMethods();

            // Verify
            targetPackage.VerifyBeforeSave(freeAppsKey);

            // Optimize map file
            classBuilders.ForEach(x => x.RecordMapping(mapFile));
            mapFile.Optimize();
        }

        /// <summary>
        /// Save classes.dex
        /// </summary>
        public void Save(string outputFolder, string freeAppsKeyPath)
        {
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);
            targetPackage.Save(outputFolder);

            var mapPath = Path.Combine(outputFolder, "classes.d42map");
            mapFile.Save(mapPath);

            // Save free apps key
            if (!string.IsNullOrEmpty(freeAppsKeyPath))
            {
                freeAppsKeyPath = Path.GetFullPath(freeAppsKeyPath);
                var folder = Path.GetDirectoryName(freeAppsKeyPath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                File.WriteAllText(freeAppsKeyPath, freeAppsKey ?? string.Empty);
            }
        }

        /// <summary>
        /// Record the given delegate type.
        /// </summary>
        internal void Record(DelegateType delegateType)
        {
            delegateTypes.Add(delegateType.Type, delegateType);
        }

        /// <summary>
        /// Gets the recorded delegate type for the given .NET delegate type.
        /// </summary>
        internal DelegateType GetDelegateType(XTypeDefinition type)
        {
            DelegateType result;
            if (delegateTypes.TryGetValue(type, out result))
                return result;
            throw new ArgumentException(string.Format("No delegate type found for {0}", type.FullName));
        }

        /// <summary>
        /// Record the attribute annotation information for the given attribute type.
        /// </summary>
        internal void Record(TypeDefinition attributeType, AttributeAnnotationInterface attributeAnnotationInterface)
        {
            attributeAnnotationTypes.Add(attributeType, attributeAnnotationInterface);
        }

        /// <summary>
        /// Gets the recorded annotation information for the given attribute type.
        /// </summary>
        internal AttributeAnnotationInterface GetAttributeAnnotationType(TypeDefinition attributeType)
        {
            AttributeAnnotationInterface result;
            if (attributeAnnotationTypes.TryGetValue(attributeType, out result))
                return result;
            throw new ArgumentException(string.Format("No annotation information found for attribute type {0}", attributeType.FullName));
        }

        /// <summary>
        /// Gets the next value of a map file id.
        /// </summary>
        internal int GetNextMapFileId()
        {
            return ++lastMapFileId;
        }

        /// <summary>
        /// Gets the type definition for a Dot42.Internal.x type where x is the given type name.
        /// </summary>
        internal XTypeReference GetDot42InternalType(string typeName)
        {
            return GetDot42InternalType(InternalConstants.Dot42InternalNamespace, typeName);
        }

        /// <summary>
        /// Gets the type definition for a Dot42 type.
        /// </summary>
        internal XTypeReference GetDot42InternalType(string typeNamespace, string typeName)
        {
            XTypeReference result;
            var key = typeNamespace + "." + typeName;
            if (!internalTypeReferences.TryGetValue(key, out result))
            {
                //result = new XTypeReference(Module, typeNamespace, typeName, assembly.MainModule, new AssemblyNameReference { Name = AssemblyConstants.SdkAssemblyName });
                result = new XTypeReference.SimpleXTypeReference(Module, typeNamespace, typeName, null, false, null);
                internalTypeReferences[key] = result;
            }
            return result;
        }

        /// <summary>
        /// Should property annotations be added?
        /// </summary>
        internal bool AddPropertyAnnotations()
        {
            if (!addPropertyAnnotations.HasValue)
            {
                var properyInfoRef = GetDot42InternalType("System.Reflection", "PropertyInfo");
                XTypeDefinition typeDef;
                addPropertyAnnotations = properyInfoRef.TryResolve(out typeDef) && typeDef.IsReachable;
            }
            return addPropertyAnnotations.Value;
        }

        /// <summary>
        /// Does the given assembly contain one of more LibraryProjectReference attributes?
        /// </summary>
        private static bool IsLibraryProject(AssemblyDefinition assembly)
        {
            return
                assembly.CustomAttributes.Any(
                    x => (x.AttributeType.Name == AttributeConstants.LibraryProjectReferenceAttributeName) &&
                         (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace));
        }
    }
}
