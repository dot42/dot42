﻿extern alias ilspy;












{
    public abstract class CompiledLanguage : Language
    {
        private static CachedCompiler compiler = new CachedCompiler();

        public AssemblyCompiler AssemblyCompiler { get { return compiler.AssemblyCompiler; } }
        public MapFileLookup MapFile { get { return compiler.MapFile; }  }


        {
            get { return compiler.GenerateSetNextInstructionCode; }
            set { compiler.GenerateSetNextInstructionCode = value; }
        }


        {
            compiler.CompileIfRequired(assembly.AssemblyDefinition);


            {
                output.WriteLine("// ERRORS: \n// " + string.Join("\n// ", compiler.CompilationErrors));

            foreach(var type in compiler.AssemblyCompiler.Assemblies
                                                         .Select(a=>a.MainModule)
                                                         .SelectMany(m=>m.GetTypes())
                                                         .Where(t=>t.IsReachable))
            { 
                output.WriteLine(type.FullName);
            }



        {
            var declaringType = method.DeclaringType;
            var assembly = declaringType.Module.Assembly;

            compiler.CompileIfRequired(assembly);

            var cmethod = AssemblyCompiler.GetCompiledMethod(xMethod);

            return cmethod;
        }

        protected XMethodDefinition GetXMethodDefinition(MethodDefinition method)
        {
            var declaringType = method.DeclaringType;
            var assembly = declaringType.Module.Assembly;

            compiler.CompileIfRequired(assembly, true);

            return GetXMethodDefinitionAfterCompilerSetup(method);

        protected XTypeDefinition GetXTypeDefinition(TypeDefinition type)
        {
            var assembly = type.Module.Assembly;

            compiler.CompileIfRequired(assembly, true);

            var xFullName = GetXFullName(type);

            XTypeDefinition tdef;
            if (!AssemblyCompiler.Module.TryGetType(xFullName, out tdef))
            {
                throw new Exception("type not found: " + xFullName);
            }

        }

        private XMethodDefinition GetXMethodDefinitionAfterCompilerSetup(MethodDefinition method)
        {
            XTypeDefinition tdef;
            var xFullName = GetXFullName(method.DeclaringType);

            if (!AssemblyCompiler.Module.TryGetType(xFullName, out tdef))
            {
                throw new Exception("type not found: " + xFullName);
            }

            int methodIdx = method.DeclaringType.Methods.IndexOf(method);
            return tdef.GetMethodByScopeId(methodIdx.ToString());
        }
        {
            return GetScopePrefix(type) + GetNamespace(type) + "." + type.Name;


        {
            if (!type.IsNested)
                return type.Namespace;

            return GetNamespace(type.DeclaringType) + "." + type.DeclaringType.Name;
        }



        {
            var scope = type.Scope.Name;
            if (scope.ToLowerInvariant().EndsWith(".dll"))
                scope = scope.Substring(0, scope.Length - 4);

            if (scope.ToLowerInvariant() == "dot42")
                return "";

            return scope.Replace(".", "_") + ((type.Namespace.Length == 0 && !type.IsNested) ? "" : ".");
            

    }
}