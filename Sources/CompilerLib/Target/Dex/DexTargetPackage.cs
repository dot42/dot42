using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.Mapping;

namespace Dot42.CompilerLib.Target.Dex
{
    /// <summary>
    /// Dex target package
    /// </summary>
    public class DexTargetPackage : ITargetPackage, ITypeResolver<TypeReference>
    {
        private readonly DexLib.Dex dex;
        private readonly NameConverter nameConverter;
        private readonly AssemblyCompiler compiler;
        private ClassDefinition generatedCodeClass;
        private readonly List<CompiledMethod> compiledMethods = new List<CompiledMethod>();
        private readonly Dictionary<XMethodDefinition, CompiledMethod> xMethodMap = new Dictionary<XMethodDefinition, CompiledMethod>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public DexTargetPackage(NameConverter nameConverter, AssemblyCompiler compiler)
        {
            this.nameConverter = nameConverter;
            this.compiler = compiler;
            dex = new DexLib.Dex();
        }

        /// <summary>
        /// Gets the target dex file.
        /// </summary>
        public DexLib.Dex DexFile
        {
            get { return dex; }
        }

        /// <summary>
        /// Gets the name converter.
        /// </summary>
        public NameConverter NameConverter
        {
            get { return nameConverter; }
        }

        /// <summary>
        /// Gets a class definition used to store generated methods in.
        /// Create if needed.
        /// </summary>
        internal ClassDefinition GetOrCreateGeneratedCodeClass()
        {
            if (generatedCodeClass == null)
            {
                generatedCodeClass = new ClassDefinition();
                generatedCodeClass.Name = "__generated";
                generatedCodeClass.Namespace = nameConverter.PackageName;
                generatedCodeClass.AccessFlags = AccessFlags.Public | AccessFlags.Synthetic;
                generatedCodeClass.SuperClass = new ClassReference("java/lang/Object");
                dex.AddClass(generatedCodeClass);
            }
            return generatedCodeClass;
        }

        /// <summary>
        /// Record the given method mapping
        /// </summary>
        internal void Record(CompiledMethod compiledMethod)
        {
            compiledMethods.Add(compiledMethod);
        }

        /// <summary>
        /// Record the given method mapping
        /// </summary>
        internal CompiledMethod Record(MethodSource source, RL.MethodBody rlBody, InvocationFrame frame)
        {
            var compiledMethod = GetOrCreateCompileMethod(source);
            compiledMethod.RLBody = rlBody;
            compiledMethod.InvocationFrame = frame;
            return compiledMethod;
        }

        /// <summary>
        /// Get a compiled method info for the given method.
        /// Create if needed.
        /// </summary>
        internal CompiledMethod GetOrCreateCompileMethod(XMethodDefinition method)
        {
            CompiledMethod result;
            if (!xMethodMap.TryGetValue(method, out result))
            {
                result = new CompiledMethod(method);
                compiledMethods.Add(result);
                xMethodMap.Add(method, result);
            }
            return result;
        }

        /// <summary>
        /// Get a compiled method info for the given method.
        /// Create if needed.
        /// </summary>
        private CompiledMethod GetOrCreateCompileMethod(MethodSource method)
        {
            return GetOrCreateCompileMethod(method.Method);
        }

        /// <summary>
        /// Compile all methods to the target platform.
        /// </summary>
        void ITargetPackage.CompileToTarget(bool generateDebugInfo, MapFile mapFile)
        {
            compiledMethods.ForEach(x => x.CompileToTarget(this, generateDebugInfo, mapFile));

            if (mapFile != null && generatedCodeClass != null)
            {
                // Only generate the type name; the methods are generated under their respective type. 
                var typeEntry = new TypeEntry("(generated)", "(none)", generatedCodeClass.Fullname, -1);
                mapFile.Add(typeEntry);
            }
        }

        /// <summary>
        /// Called after all methods have been compiled.
        /// </summary>
        void ITargetPackage.AfterCompileMethods()
        {
            AddStructureAnnotations();
        }

        /// <summary>
        /// Add all structure annotations.
        /// </summary>
        private void AddStructureAnnotations()
        {
            foreach (var @class in dex.Classes)
            {
                AddStructureAnnotations(@class);
            }
        }

        /// <summary>
        /// Verify the entire package before it is saved.
        /// </summary>
        void ITargetPackage.VerifyBeforeSave(string freeAppsKey)
        {
            // Verify
            DexVerifier.Verifier.Verify(dex, OnError);
        }

        /// <summary>
        /// Save the package to disk.
        /// </summary>
        void ITargetPackage.Save(string outputFolder)
        {
            var path = Path.Combine(outputFolder, "classes.dex");
            dex.Write(path);
        }

        /// <summary>
        /// Record the given method mapping
        /// </summary>
        internal void Record(XMethodDefinition xMethod, MethodDefinition dMethod)
        {
            NameConverter.Record(xMethod, dMethod);
            GetOrCreateCompileMethod(xMethod).DexMethod = dMethod;
        }

        /// <summary>
        /// Get a compiled method info for the given method.
        /// </summary>
        internal CompiledMethod GetMethod(XMethodDefinition method)
        {
            CompiledMethod result;
            return xMethodMap.TryGetValue(method, out result) ? result : null;
        }

        /// <summary>
        /// Is it illegal to use the given class with the current free-apps + community license?
        /// </summary>
        private static int IsIllegalClassWithFreeAppsKey(ClassDefinition @class)
        {
            var ns = @class.Namespace ?? string.Empty;
            if (ns.StartsWith("com.google.ads.") || (ns == "com.google.ads"))
                return ns.Length;
            return int.MinValue;
        }

        /// <summary>
        /// Add annotation for inner classes.
        /// </summary>
        private static void AddStructureAnnotations(ClassDefinition @class)
        {
            if (@class.InnerClasses.Count == 0)
                return;

            foreach (var innerClass in @class.InnerClasses)
            {
                innerClass.AddEnclosingClassAnnotation(@class);
                innerClass.AddInnerClassAnnotation(NameConverter.GetSimpleName(innerClass.Name), innerClass.AccessFlags | AccessFlags.Static);
                AddStructureAnnotations(innerClass);
            }
            @class.AddMemberClassesAnnotation(@class.InnerClasses.Cast<ClassReference>().ToArray());
        }

        /// <summary>
        /// Throw exception on error.
        /// </summary>
        private void OnError(string msg)
        {
            throw new CompilerException(msg);
        }

        /// <summary>
        /// Gets a dex type reference from the given IL type reference.
        /// </summary>
        TypeReference ITypeResolver<TypeReference>.GetTypeReference(XTypeReference type)
        {
            return type.GetReference(this);
        }
    }
}
