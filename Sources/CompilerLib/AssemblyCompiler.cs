using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dot42.ApkLib.Resources;
using Dot42.CompilerLib.CompilerCache;
using Dot42.CompilerLib.Reachable;
using Dot42.CompilerLib.Structure.DotNet;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.XModel;
using Dot42.FrameworkDefinitions;
using Dot42.LoaderLib.Java;
using Dot42.Mapping;
using Dot42.Utility;
using Mono.Cecil;

namespace Dot42.CompilerLib
{
    public class AssemblyCompiler
    {
        private readonly XModule module;
        private readonly bool generateSetNextInstructionCode;
        private readonly AssemblyClassLoader assemblyClassLoader;
        private readonly Func<AssemblyDefinition, string> assemblyToFilename;
        private readonly HashSet<string> rootClassNames;
        private readonly CompilationMode mode;
        private readonly List<AssemblyDefinition> assemblies;
        private readonly List<AssemblyDefinition> references;
        private readonly Table resources;
        private readonly bool generateDebugInfo;
        private readonly Dictionary<XTypeDefinition, DelegateType> delegateTypes = new Dictionary<XTypeDefinition, DelegateType>();
        private readonly Dictionary<TypeDefinition, AttributeAnnotationInterface> attributeAnnotationTypes = new Dictionary<TypeDefinition, AttributeAnnotationInterface>();
        private readonly MapFile mapFile = new MapFile();
        private bool optimizedMapFile = false;
        private int lastMapFileId;
        private readonly Dictionary<string, XTypeReference> internalTypeReferences = new Dictionary<string, XTypeReference>();
        private string freeAppsKey;
        private bool? addPropertyAnnotations, addFrameworkPropertyAnnotations;
        private bool? addAssemblyTypesAnnotation;
        private readonly ITargetPackage targetPackage;
        private readonly DexMethodBodyCompilerCache methodBodyCompilerCache;

        /// <summary>
        /// If true, the compiler will stop compilation just before
        /// generating code. Useful for debugging the compiler.
        /// </summary>
        public bool StopCompilationBeforeGeneratingCode { get; set; }

        /// <summary>
        /// If false, compilation errors will be accumulated and thrown as
        /// a AggregateException
        /// </summary>
        public bool StopAtFirstError { get; set; }

        /// <summary>
        /// TODO: the list of parameters has gotten way to long.
        /// </summary>
        public AssemblyCompiler(CompilationMode mode, List<AssemblyDefinition> assemblies, 
                                List<AssemblyDefinition> references, Table resources, NameConverter nameConverter, 
                                bool generateDebugInfo, AssemblyClassLoader assemblyClassLoader, 
                                Func<AssemblyDefinition, string> assemblyToFilename, DexMethodBodyCompilerCache ccache,
                                HashSet<string> rootClassNames, XModule module, bool generateSetNextInstructionCode)
        {
            this.mode = mode;
            this.assemblies = assemblies;
            this.references = references;
            this.resources = resources;
            this.generateDebugInfo = generateDebugInfo;
            this.assemblyClassLoader = assemblyClassLoader;
            this.assemblyToFilename = assemblyToFilename;
            this.rootClassNames = rootClassNames;
            this.module = module;
            this.generateSetNextInstructionCode = generateDebugInfo && generateSetNextInstructionCode;
            targetPackage = new Target.Dex.DexTargetPackage(nameConverter, this);
            methodBodyCompilerCache = ccache;
            StopAtFirstError = true;
        }

        public CompilationMode CompilationMode { get { return mode; } }
        public XModule Module { get { return module; } }
        public List<AssemblyDefinition> Assemblies { get { return assemblies; } }
        public Table ResourceTable { get { return resources; } }
        public bool GenerateSetNextInstructionCode { get { return generateSetNextInstructionCode; } }
        internal DexMethodBodyCompilerCache MethodBodyCompilerCache { get { return methodBodyCompilerCache; } }

        public MapFile MapFile
        {
            get
            {
                if (!optimizedMapFile && mapFile != null)
                {
                    lock (mapFile)
                    {
                        mapFile.Optimize();
                        optimizedMapFile = true;
                    } 
                }
                return mapFile;
            }
        }

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

            using (Profile("for marking roots"))
            {
                assemblies.Concat(references.Where(IsLibraryProject))
                          .ToList()
                          .AsParallel()
                          .ForAll(assembly => reachableContext.MarkRoots(assembly, includeAllJavaCode));

                reachableContext.MarkPatternIncludes(assemblies.Concat(references).ToList());
            }

            using (Profile("for finding reachables"))
                reachableContext.Complete();

            // Convert IL to java compatible constructs.
            using (Profile("for IL conversion"))
                ILConversion.ILToJava.Convert(reachableContext);

            // Convert all types
            var classBuilders = reachableContext.ClassBuilders.OrderBy(x => x.SortPriority)
                                                              .ThenBy(x => x.FullName)
                                                              .ToList();
            using (Profile("for creating/implementing/fixup"))
            {
                classBuilders.ForEachWithExceptionMessage(x => x.Create(targetPackage));

                classBuilders.ForEachWithExceptionMessage(x => x.Implement(targetPackage));
                classBuilders.ForEachWithExceptionMessage(x => x.FixUp(targetPackage));

                // update sort priority which might have changed after XType creation.
                classBuilders = classBuilders.OrderBy(x => x.SortPriority)
                                             .ThenBy(x => x.FullName)
                                             .ToList();
            }

            if (StopCompilationBeforeGeneratingCode)
                return;

            using (Profile("for generating code"))
            {
                if (StopAtFirstError)
                    classBuilders.ForEachWithExceptionMessage(x => x.GenerateCode(targetPackage));
                else
                {
                    List<Exception> exs = new List<Exception>();
                    foreach (var classBuilder in classBuilders)
                    {
                        try
                        {
                            classBuilder.GenerateCode(targetPackage);
                        }
                        catch (Exception ex)
                        {
                            exs.Add(new Exception("Error while handling " + classBuilder.FullName + ": " + ex.Message, ex));
                        }
                    }

                    if(exs.Count > 0)
                        throw new AggregateException(exs);
                }
            }

            classBuilders.ForEachWithExceptionMessage(x => x.CreateAnnotations(targetPackage));

            if (AddAssemblyTypesAnnotations())
                AssemblyTypesBuilder.CreateAssemblyTypesAnnotations(this, (Target.Dex.DexTargetPackage)targetPackage, reachableContext.ReachableTypes);

            if (AddFrameworkPropertyAnnotations())
                DexImportClassBuilder.FinalizeFrameworkPropertyAnnotations(this, (Target.Dex.DexTargetPackage)targetPackage);

            // Compile all methods
            using (Profile("for compiling to target"))
                targetPackage.CompileToTarget(generateDebugInfo, mapFile);

            // Add structure annotations
            targetPackage.AfterCompileMethods();

            // Verify
            targetPackage.VerifyBeforeSave(freeAppsKey);


            // Create MapFile, but don't optimize jet.
            optimizedMapFile = false;
            RecordScopeMapping(reachableContext);
            classBuilders.ForEachWithExceptionMessage(x => x.RecordMapping(mapFile));
        }

        /// <summary>
        /// Save classes.dex
        /// </summary>
        public void Save(string outputFolder, string freeAppsKeyPath)
        {
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            var task1 = Task.Factory.StartNew(() => targetPackage.Save(outputFolder));

            var mapPath = Path.Combine(outputFolder, "classes.d42map");
            var task2 = Task.Factory.StartNew(() => { MapFile.Save(mapPath); });

            Task.WaitAll(task1, task2);

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
            return Interlocked.Increment(ref lastMapFileId);
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
        /// Should property annotations be added?
        /// </summary>
        internal bool AddFrameworkPropertyAnnotations()
        {
            if (!addFrameworkPropertyAnnotations.HasValue)
            {
                if (!AddPropertyAnnotations())
                    addFrameworkPropertyAnnotations = false;
                else
                {
                    var fpRef = GetDot42InternalType("Dot42", "IncludeFrameworkProperties");
                    XTypeDefinition typeDef;
                    addFrameworkPropertyAnnotations = fpRef.TryResolve(out typeDef) && typeDef.IsReachable;
                }
            }
            return addFrameworkPropertyAnnotations.Value;
        }

        /// <summary>
        /// Should property annotations be added?
        /// </summary>
        internal bool AddAssemblyTypesAnnotations()
        {
            if (!addAssemblyTypesAnnotation.HasValue)
            {
                var assemblyTypes = GetDot42InternalType("AssemblyTypes");
                XTypeDefinition typeDef;
                addAssemblyTypesAnnotation = assemblyTypes.TryResolve(out typeDef) && typeDef.IsReachable;
            }
            return addAssemblyTypesAnnotation.Value;
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

        private void RecordScopeMapping(ReachableContext reachableContext)
        {
            foreach (var scope in reachableContext.ReachableTypes.GroupBy(g => g.Scope, g => g.Module.Assembly))
            {
                var assm = scope.Distinct().ToList();
                if (assm.Count > 1)
                {
                    DLog.Warning(DContext.CompilerCodeGenerator, "More than one assembly for scope {0}", scope.Key.Name);
                    // let's not risk a wrong mapping.
                    continue;
                }

                var filename = assemblyToFilename(assm.First());

                if (filename != null)
                {
                    var scopeEntry = new ScopeEntry(scope.Key.Name, filename, File.GetLastWriteTimeUtc(filename),
                                                    Hash.HashFileMD5(filename));
                    mapFile.Add(scopeEntry);
                }
            }
        }

        internal CompiledMethod GetCompiledMethod(XMethodDefinition method)
        {
            return ((Target.Dex.DexTargetPackage) targetPackage).GetMethod(method);
        }


        public static IDisposable Profile(string msg, bool isSummary=false)
        {
            return Profiler.Profile(x => Console.WriteLine("{2}{0} ms {1}", x.TotalMilliseconds.ToString("#,000", CultureInfo.InvariantCulture).PadLeft(6), msg, isSummary?"------\n":""));
        }

    }
}
