extern alias ilspy;

using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.ApkLib.Resources;
using Dot42.CompilerLib;
using Dot42.CompilerLib.CompilerCache;
using Dot42.CompilerLib.XModel;
using Dot42.FrameworkDefinitions;
using Dot42.LoaderLib.DotNet;
using Dot42.LoaderLib.Java;
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

        public void CompileIfRequired(ilspy::Mono.Cecil.AssemblyDefinition assembly, bool stopBeforeGeneratingCode = false)
        {
            if (_compiler != null
             && _compiler.Assemblies.Any(a => a.FullName == assembly.FullName) 
             && (_isFullyCompiled || stopBeforeGeneratingCode))
                return;

            _compiler = null;
            
            string framework = Frameworks.Instance.GetNewestVersion().Folder;
            var refFolders = new List<string> { framework };

            var module = new XModule();
            var classLoader = new AssemblyClassLoader(module.OnClassLoaded);
            var resolver = new AssemblyResolver(refFolders, classLoader, module.OnAssemblyLoaded);
            var parameter = new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = resolver };

            var assemblies = new[] { resolver.Load(assembly.MainModule.FullyQualifiedName, parameter) }.ToList();
            var references = assemblies.ToList();
            references.Clear();

            var c = new AssemblyCompiler(CompilationMode.All, assemblies, references, new Table("pkg.name"),
                                         new NameConverter("pkg.name", ""), true, 
                                         new AssemblyClassLoader(file => { }), definition => null,
                                         new DexMethodBodyCompilerCache(), new HashSet<string>(), 
                                         module, false);

            c.IsCompileStopBeforeGeneratingCode = stopBeforeGeneratingCode;

            c.Compile();
            
            _compiler = c;
            _isFullyCompiled = !stopBeforeGeneratingCode;
        }

        static CachedCompiler()
        {
            Locations.SetTarget(null);
        }
    }
}
