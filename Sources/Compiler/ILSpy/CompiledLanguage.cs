extern alias ilspy;
using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.ApkLib.Resources;
using Dot42.CompilerLib;
using Dot42.CompilerLib.CompilerCache;
using Dot42.CompilerLib.XModel;
using Dot42.LoaderLib.DotNet;
using Dot42.LoaderLib.Java;
using ICSharpCode.ILSpy;
using Mono.Cecil;
using TypeDefinition = ilspy::Mono.Cecil.TypeDefinition;

namespace Dot42.Compiler.ILSpy
{
    public abstract class CompiledLanguage : Language
    {
        private AssemblyCompiler compiler;

        public AssemblyCompiler AssemblyCompiler { get { return compiler; } }


        protected CompiledMethod GetCompiledMethod(ilspy::Mono.Cecil.MethodDefinition method)
        {
            var declaringType = method.DeclaringType;
            var assembly = declaringType.Module.Assembly;

            CompileIfRequired(assembly);

            var cmethod = AssemblyCompiler.GetCompiledMethod(GetXMethodDefinitionAfterCompile( method));

            return cmethod;
        }

        protected XMethodDefinition GetXMethodDefinition(ilspy::Mono.Cecil.MethodDefinition method)
        {
            var declaringType = method.DeclaringType;
            var assembly = declaringType.Module.Assembly;

            CompileIfRequired(assembly);

            return GetXMethodDefinitionAfterCompile(method);        }
        protected void CompileIfRequired(ilspy::Mono.Cecil.AssemblyDefinition assembly)
        {
            if ((compiler == null) || (compiler.Assemblies.All(a => a.FullName != assembly.FullName)))
            {
                compiler = null;

                // FIXME
                var refFolders = new List<string>() { @"c:\Program Files\dot42\Android\Frameworks\v4.3" };

                var module = new XModule();
                var classLoader = new AssemblyClassLoader(module.OnClassLoaded);
                var resolver = new AssemblyResolver(refFolders, classLoader, module.OnAssemblyLoaded);
                var parameter = new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = resolver};

                var assemblies = new[] { resolver.Load(assembly.MainModule.FullyQualifiedName, parameter) }.ToList();
                var references = assemblies.ToList();
                references.Clear();

                var c = new AssemblyCompiler(CompilationMode.All, assemblies, references, new Table("pkg.name"),
                    new NameConverter("pkg.name", ""), true, new AssemblyClassLoader(file => { }), definition => null,
                    new DexMethodBodyCompilerCache(), new HashSet<string>(), module, false);

                
                c.Compile();
                compiler = c;
            }
        }

        private XMethodDefinition GetXMethodDefinitionAfterCompile(ilspy::Mono.Cecil.MethodDefinition method)
        {
            XTypeDefinition tdef;
            var xFullName = GetXFullName(method.DeclaringType);

            if (!compiler.Module.TryGetType(xFullName, out tdef))
            {
                throw new Exception("type not found: " + xFullName);
            }

            int methodIdx = method.DeclaringType.Methods.IndexOf(method);
            return tdef.GetMethodByScopeId(methodIdx.ToString());
        }        protected static string GetXFullName(TypeDefinition type)
        {
            return GetScopePrefix(type) + type.Namespace + "." + type.Name;        }

        private static string GetScopePrefix(TypeDefinition type)
        {
            var scope = type.Scope.Name;
            if (scope.ToLowerInvariant().EndsWith(".dll"))
                scope = scope.Substring(0, scope.Length - 4);

            if (scope.ToLowerInvariant() == "dot42")
                return "";

            return scope.Replace(".", "_") + ((type.Namespace.Length == 0) ? "" : ".");        }
            

    }
}
