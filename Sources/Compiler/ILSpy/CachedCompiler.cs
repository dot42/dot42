extern alias ilspy;

using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.ApkLib.Resources;
using Dot42.CompilerLib;
using Dot42.CompilerLib.Target.CompilerCache;
using Dot42.CompilerLib.XModel;
using Dot42.FrameworkDefinitions;
using Dot42.LoaderLib.DotNet;
using Dot42.LoaderLib.Java;
using Dot42.Mapping;
using Dot42.Utility;
using Mono.Cecil;
using NameConverter = Dot42.CompilerLib.NameConverter;

namespace Dot42.Compiler.ILSpy
{
    public class CachedCompiler
    {
        private AssemblyCompiler _compiler;
        private bool _isFullyCompiled = false;
        public AssemblyCompiler AssemblyCompiler { get { return _compiler; } }
        private ilspy::Mono.Cecil.AssemblyDefinition _previousAssembly;
        private bool _generateSetNextInstructionCode;

        public List<string> CompilationErrors { get; private set; }
        public MapFileLookup MapFile { get; private set; }

        public bool GenerateSetNextInstructionCode
        {
            get { return _generateSetNextInstructionCode; }
            set { _generateSetNextInstructionCode = value; _compiler = null; }
        }

        public void CompileIfRequired(ilspy::Mono.Cecil.AssemblyDefinition assembly, bool stopBeforeGeneratingCode = false)
        {
            if (_compiler != null && _previousAssembly == assembly
            && (_isFullyCompiled || stopBeforeGeneratingCode))
                return;

            CompilationErrors = null;
            _compiler = null;

#if DEBUG
            var framework = Frameworks.Instance.GetBySdkVersion(15);
#else
            var framework = Frameworks.Instance.GetNewestVersion();
#endif
            string frameworkFolder = framework.Folder;
            var refFolders = new List<string> { frameworkFolder };

            var module = new XModule();
            var classLoader = new AssemblyClassLoader(module.OnClassLoaded);
            var resolver = new AssemblyResolver(refFolders, classLoader, module.OnAssemblyLoaded);
            var parameter = new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = resolver,ReadSymbols = true};

            var assemblies = new[] { resolver.Load(assembly.MainModule.FullyQualifiedName, parameter) }.ToList();
            List<AssemblyDefinition> references = new List<AssemblyDefinition>();
            
            if(assembly.MainModule.Name != "dot42.dll")
                references = new[] { resolver.Load(AssemblyConstants.SdkAssemblyName, parameter) }.ToList();
            
            foreach (var a in assemblies)
                references.Remove(a);

            var c = new AssemblyCompiler(CompilationMode.All, assemblies, references, new Table("pkg.name"),
                                         new NameConverter("pkg.name", ""), true, 
                                         new AssemblyClassLoader(file => { }), definition => null,
                                         new DexMethodBodyCompilerCache(), new HashSet<string>(), 
                                         module, _generateSetNextInstructionCode);

            c.StopCompilationBeforeGeneratingCode = stopBeforeGeneratingCode;
            c.StopAtFirstError = false;
            
            try
            {
                c.Compile();
            }
            catch (AggregateException ex)
            {
                CompilationErrors = ex.Flatten().InnerExceptions.Select(e => e.Message.Replace(": ", "\n//      ").Replace("; ", "\n//      &  ")).ToList();
            }

            if (c.MapFile != null)
            {
                c.MapFile.Optimize();
                MapFile = new MapFileLookup(c.MapFile);
            }

            _compiler = c;
            _previousAssembly = assembly;
            _isFullyCompiled = !stopBeforeGeneratingCode;
        }

        static CachedCompiler()
        {
            Locations.SetTarget(null);
        }
    }
}
