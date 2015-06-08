using System;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Java;
using Dot42.DexLib;
using Dot42.Mapping;
using MethodDefinition = Dot42.JvmClassLib.MethodDefinition;

namespace Dot42.CompilerLib.Structure.Java
{
    internal class MethodBuilder
    {
        private readonly AssemblyCompiler compiler;
        private readonly MethodDefinition method;
        private DexLib.MethodDefinition dmethod;
        private CompiledMethod compiledMethod;
        private XMethodDefinition xMethod;

        /// <summary>
        /// Default ctor
        /// </summary>
        public static MethodBuilder Create(AssemblyCompiler compiler, MethodDefinition method)
        {
            return new MethodBuilder(compiler, method);
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        protected MethodBuilder(AssemblyCompiler compiler, MethodDefinition method)
        {
            this.compiler = compiler;
            this.method = method;
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        public void Create(ClassDefinition declaringClass, DexTargetPackage targetPackage)
        {
            // Find xMethod
            xMethod = XBuilder.AsMethodDefinition(compiler.Module, method);

            // Create method definition
            dmethod = new DexLib.MethodDefinition();
            dmethod.Name = GetMethodName(method, targetPackage);
            dmethod.MapFileId = compiler.GetNextMapFileId();
            AddMethodToDeclaringClass(declaringClass, dmethod, targetPackage);

            targetPackage.Record(xMethod, dmethod);

            // Set access flags
            SetAccessFlags(dmethod, method);

            // Create prototype
            dmethod.Prototype = PrototypeBuilder.BuildPrototype(compiler, targetPackage, declaringClass, method);
        }

        /// <summary>
        /// Add the given method to its declaring class.
        /// </summary>
        protected virtual void AddMethodToDeclaringClass(ClassDefinition declaringClass, DexLib.MethodDefinition dmethod, DexTargetPackage targetPackage)
        {
            dmethod.Owner = declaringClass;
            declaringClass.Methods.Add(dmethod);            
        }

        /// <summary>
        /// Add the given method to its declaring class.
        /// </summary>
        protected virtual void SetAccessFlags(DexLib.MethodDefinition dmethod, MethodDefinition method)
        {
            if (method.IsPrivate) dmethod.IsPrivate = true;
            if (method.IsProtected) dmethod.IsProtected = true;
            if (method.IsPublic) dmethod.IsPublic = true;

            if (method.DeclaringClass.IsInterface)
            {
                dmethod.IsAbstract = true;
            }
            else
            {
                if (method.IsConstructor) dmethod.IsConstructor = true;
                if (method.IsAbstract) dmethod.IsAbstract = true;
                if (method.IsStatic) dmethod.IsStatic = true;
                if (!method.IsStatic && !method.IsFinal && !method.IsConstructor && !method.IsPrivate) dmethod.IsVirtual = true;
                if (method.IsFinal || method.IsPrivate) dmethod.IsFinal = true;
            }
        }
        
        /// <summary>
        /// Implement make minor fixes after the implementation phase.
        /// </summary>
        public void FixUp(Dex target, NameConverter nsConverter)
        {
        }

        /// <summary>
        /// Generate method code
        /// </summary>
        public void GenerateCode(ClassDefinition declaringClass, DexTargetPackage targetPackage)
        {
            if (dmethod == null)
                return;

            // Create body (if any)
            if (!method.HasCode) 
                return;

            if (compiler.DxClassfileMethodBodyCompiler != null)
            {
                var dxBody = compiler.DxClassfileMethodBodyCompiler.GetMethodBody(dmethod, xMethod);
                if (dxBody == null) 
                    throw new Exception("unable to get method body through 'dx': " + dmethod);

                dmethod.Body = dxBody;
                dmethod.Owner.SetSourceFile(dxBody.Owner.Owner.SourceFile);
                return;
            }

            var cachedBody = compiler.MethodBodyCompilerCache.GetFromCache(dmethod, xMethod, compiler, targetPackage);

            if (cachedBody != null)
            {
                dmethod.Body = cachedBody.Body;
                // important to fix the owners source file as early as possible, 
                // so it can't be changed later. Else we would have to recreate
                // all cached method bodies debug infos.
                dmethod.Owner.SetSourceFile(cachedBody.ClassSourceFile);
                return;
            }

            //ExpandSequencePoints(method.Body);
            var source = new MethodSource(xMethod, method);
            DexMethodBodyCompiler.TranslateToRL(compiler, targetPackage, source, dmethod, false, out compiledMethod);
        }

        /// <summary>
        /// Create all annotations for this method
        /// </summary>
        internal virtual void CreateAnnotations(DexTargetPackage targetPackage)
        {
            if (dmethod == null)
                return;

            // Add annotations from java
            AnnotationBuilder.BuildAnnotations(method, dmethod, targetPackage, compiler.Module);
        }

        /// <summary>
        /// Record the mapping from .NET to Dex
        /// </summary>
        public void RecordMapping(TypeEntry typeEntry)
        {
            var entry = new MethodEntry(method.Name, "", dmethod.Name, dmethod.Prototype.ToSignature(), dmethod.MapFileId);
            typeEntry.Methods.Add(entry);

            if (compiledMethod != null)
            {
                compiledMethod.RecordMapping(entry);
            }
        }

        /// <summary>
        /// Perform name conversion
        /// </summary>
        protected virtual string GetMethodName(MethodDefinition method, DexTargetPackage targetPackage)
        {
            // Handle special names
            switch (method.Name)
            {
                case "<init>":
                case "<clinit>":
                    return method.Name;
            }

            // Handle regular names
            return method.Name;
        }

        /// <summary>
        /// Gets a reference to the compiler.
        /// </summary>
        protected AssemblyCompiler Compiler { get { return compiler; } }
    }
}
