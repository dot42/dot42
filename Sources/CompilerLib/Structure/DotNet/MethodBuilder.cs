using System.Globalization;
using System.Linq;
using System.Text;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.CompilerCache;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.DexLib;
using Dot42.Mapping;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodDefinition = Mono.Cecil.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    internal class MethodBuilder
    {
        private readonly AssemblyCompiler compiler;
        private readonly MethodDefinition method;
        private DexLib.MethodDefinition dmethod;
        private CompiledMethod compiledMethod;
        private XBuilder.ILMethodDefinition xMethod;
        private CacheEntry cachedBody;

        /// <summary>
        /// Default ctor
        /// </summary>
        public static MethodBuilder Create(AssemblyCompiler compiler, MethodDefinition method)
        {
            string dllName;
            if (method.IsAndroidExtension())
            {
                return new DexImportMethodBuilder(compiler, method);
            }
            if (method.TryGetDllImportName(out dllName))
            {
                return new DllImportMethodBuilder(compiler, method, dllName);
            }
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
            xMethod = (XBuilder.ILMethodDefinition)XBuilder.AsMethodDefinition(compiler.Module, method);

            // Create method definition
            dmethod = new DexLib.MethodDefinition
            {
                Name = GetMethodName(method, targetPackage),
                MapFileId = compiler.GetNextMapFileId()
            };
            
            AddMethodToDeclaringClass(declaringClass, dmethod, targetPackage);

            targetPackage.Record(xMethod, dmethod);

            // Set access flags
            SetAccessFlags(dmethod, method);

            // Create prototype
            dmethod.Prototype = PrototypeBuilder.BuildPrototype(compiler, targetPackage, declaringClass, xMethod);
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
            // subclass accesses have already been fixed on an actual use basis.
            if (method.IsPrivate)
                dmethod.IsPrivate = true;
            else if (method.IsFamily || method.IsFamilyOrAssembly) 
                dmethod.IsProtected = true;
            else
                dmethod.IsPublic = true;

            if (method.DeclaringType.IsInterface)
            {
                dmethod.IsAbstract = true;
                //dmethod.
            }
            else
            {
                if (method.IsConstructor)
                {
                    dmethod.IsConstructor = true;
                    dmethod.IsStatic = method.IsStatic;
                    if (method.IsStatic)
                    {
                        // reset access modifiers for static constructors.
                        dmethod.IsPrivate = false;
                        dmethod.IsProtected = false;
                    }
                }
                else if (method.IsAbstract) dmethod.IsAbstract = true;
                else if (method.IsVirtual) dmethod.IsVirtual = true;
                else if (method.IsStatic) dmethod.IsStatic = true;
                else dmethod.IsFinal = true;
                //if (method.IsInitOnly) dmethod.IsFinal = true;
            }

            if (method.IsCompilerGenerated())
                dmethod.IsSynthetic = true;
        }
        
        /// <summary>
        /// Make minor fixes after the implementation phase.
        /// </summary>
        public void FixUp(DexTargetPackage targetPackage)
        {
            var iMethod = method.GetBaseInterfaceMethod();
            Mono.Cecil.TypeReference inheritedReturnType = null;
            //var iMethod = method.Overrides.Select(x => x.Resolve()).Where(x => x != null).FirstOrDefault(x => x.DeclaringType.IsInterface);
            if (iMethod != null)
            {
                inheritedReturnType = iMethod.ReturnType;
            }

            var baseMethod = method.GetBaseMethod();
            if (baseMethod != null)
            {
                inheritedReturnType = baseMethod.ReturnType;
            }

            
            if (inheritedReturnType != null)
            {
                var inheritedTargetReturnType = inheritedReturnType.GetReference(targetPackage, compiler.Module);
                if (inheritedTargetReturnType.Descriptor != dmethod.Prototype.ReturnType.Descriptor)
                {
                    dmethod.Prototype.Unfreeze();
                    dmethod.Prototype.ReturnType = inheritedTargetReturnType;
                    dmethod.Prototype.Freeze();
                    //// update the original method's return type as well, 
                    //// to make sure the code generation later knows what it is handling.
                    //// TODO: this seems to be a hack. shouldn't this have been handled 
                    ////       during the IL-conversion phase?
                    xMethod.SetInheritedReturnType(inheritedReturnType);
                }
            }
        }

        /// <summary>
        /// Generate method code
        /// </summary>
        public virtual void GenerateCode(ClassDefinition declaringClass, DexTargetPackage targetPackage)
        {
            if (dmethod == null)
                return;

            // Create body (if any)
            if (!method.HasBody) 
                return;

            cachedBody = compiler.MethodBodyCompilerCache.GetFromCache(dmethod, xMethod, compiler, targetPackage);

            if (cachedBody != null)
            {
                dmethod.Body = cachedBody.Body;
                // important to fix the owners source file as early as possible, 
                // so it can't be changed later. Else we would have to recreate
                // all cached method bodies debug infos.
                dmethod.Owner.SetSourceFile(cachedBody.ClassSourceFile);
                return;
            }

            var source = new MethodSource(xMethod, method);
            ExpandSequencePoints(method.Body);

            bool generateSetNextInstructionCode = compiler.GenerateSetNextInstructionCode 
                                                  && method.DeclaringType.IsInDebugBuildAssembly();

            DexMethodBodyCompiler.TranslateToRL(compiler, targetPackage, source, dmethod, generateSetNextInstructionCode, out compiledMethod);
        }

        /// <summary>
        /// Create all annotations for this method
        /// </summary>
        internal virtual void CreateAnnotations(DexTargetPackage targetPackage)
        {
            // Build method annotations
            AnnotationBuilder.Create(compiler, method, dmethod, targetPackage);
            
            // only add generics annotation for getters or setters or constructors
            if (method.IsGetter)
            {
                // Note that the return type might has been 
                // changed above, to compensate for interface 
                // inheritance and generic specialization.
                // We need to use the original declaration.
                // TODO: why not get rid of "OriginalReturnType"
                //       and use the IL's return type??
                var returnType = xMethod.OriginalReturnType;
                var xType = XBuilder.AsTypeReference(compiler.Module, returnType);
                dmethod.AddGenericDefinitionAnnotationIfGeneric(xType, compiler, targetPackage);
            }
            else if (method.IsSetter)
            {
                for (int i = 0; i < xMethod.Parameters.Count; ++i)
                {
                    var dp = dmethod.Prototype.Parameters[i];
                    dp.AddGenericDefinitionAnnotationIfGeneric(xMethod.Parameters[i].ParameterType, compiler,
                        targetPackage);
                }
            }
            //else if (method.IsConstructor)
            //{
            //    for (int i = 0; i < xMethod.Parameters.Count; ++i)
            //    {
            //        var dp = dmethod.Prototype.Parameters[i];
            //        dp.AddGenericDefinitionAnnotationIfGeneric(xMethod.Parameters[i].ParameterType, compiler,
            //            targetPackage);
            //    }
            //}
        }

        /// <summary>
        /// Is this method a property accessor?
        /// </summary>
        internal bool IsPropertyAccessor(out PropertyDefinition @property, out bool isSetter)
        {
            var attr = method.SemanticsAttributes;
            @property = null;
            isSetter = attr.HasFlag(MethodSemanticsAttributes.Setter);
            if (!(attr.HasFlag(MethodSemanticsAttributes.Getter) || attr.HasFlag(MethodSemanticsAttributes.Setter)))
                return false;
            @property = method.DeclaringType.Properties.FirstOrDefault(x => (x.GetMethod == method) || (x.SetMethod == method));
            if (@property == null)
                return false;
            return true;
        }

        /// <summary>
        /// Make sure that all instructions have a sequence point when available.
        /// </summary>
        private static void ExpandSequencePoints(MethodBody body)
        {
            SequencePoint lastSeqPoint = null;
            foreach (var ins in body.Instructions)
            {
                if (ins.SequencePoint != null)
                {
                    lastSeqPoint = ins.SequencePoint;
                }
                else
                {
                    ins.SequencePoint = lastSeqPoint;
                }
            }
        }

        /// <summary>
        /// Record the mapping from .NET to Dex
        /// </summary>
        public void RecordMapping(TypeEntry typeEntry, MapFile mapFile)
        {
            var entry = RecordMapping(typeEntry, xMethod, method, dmethod, compiledMethod);

            if (cachedBody != null)
            {
                // take the mapping and debug info from the cached body.
                foreach (var v in cachedBody.MethodEntry.Variables)
                    entry.Variables.Add(v);
                foreach (var p in cachedBody.MethodEntry.Parameters)
                    entry.Parameters.Add(p);

                if (cachedBody.SourceCodePositions.Count > 0)
                {
                    var doc = mapFile.GetOrCreateDocument(cachedBody.SourceCodePositions[0].Document.Path, true);
                    foreach (var pos in cachedBody.SourceCodePositions.Select(p=>p.Position))
                    {
                        var newPos = new DocumentPosition(pos.Start.Line,pos.Start.Column, pos.End.Line, pos.End.Column, typeEntry.Id, dmethod.MapFileId, pos.MethodOffset)
                        {
                            AlwaysKeep = true
                        };
                        doc.Positions.Add(newPos);
                    }
                        
                }
            }
        }

        public static MethodEntry RecordMapping(TypeEntry typeEntry, XMethodDefinition xMethod, MethodDefinition method, DexLib.MethodDefinition dmethod, CompiledMethod compiledMethod)
        {
            var scopeId = xMethod.ScopeId;

            StringBuilder netSignature = new StringBuilder();
            method.MethodSignatureFullName(netSignature);

            var entry = new MethodEntry(method.OriginalName, netSignature.ToString(), dmethod.Name, dmethod.Prototype.ToSignature(), dmethod.MapFileId,
                                        scopeId);

            typeEntry.Methods.Add(entry);

            if (compiledMethod != null)
            {
                compiledMethod.RecordMapping(entry);
            }

            return entry;
        }

        /// <summary>
        /// Perform name conversion
        /// </summary>
        protected virtual string GetMethodName(MethodDefinition method, DexTargetPackage targetPackage)
        {
            // Handle special names
            switch (method.Name)
            {
                case ".ctor":
                    return "<init>";
                case ".cctor":
                    return "<clinit>";
            }

            // Handle regular names
            // Test for overrides of dex/java imported methods
            var javaBaseMethod = method.GetDexImportBaseMethod() ?? method.GetJavaImportBaseMethod();
            if (javaBaseMethod != null)
            {
                var javaMethodRef = javaBaseMethod.GetReference(targetPackage, compiler.Module);
                return javaMethodRef.Name;
            }

            // Test for overrides of interface methods
            var interfaceJavaBaseMethod = method.GetDexImportBaseInterfaceMethod() ?? method.GetJavaImportBaseInterfaceMethod();
            if (interfaceJavaBaseMethod != null)
            {
                var javaMethodRef = interfaceJavaBaseMethod.GetReference(targetPackage, compiler.Module);
                return javaMethodRef.Name;                
            }

            // If a dex name is specified, use that
            var dexName = method.GetDexNameAttribute();
            if (dexName != null)
            {
                return (string) dexName.ConstructorArguments[0].Value;
            }

            return NameConverter.GetConvertedName(xMethod);
        }

        /// <summary>
        /// Gets a reference to the compiler.
        /// </summary>
        protected AssemblyCompiler Compiler { get { return compiler; } }

        /// <summary>
        /// Gets the dex method.
        /// </summary>
        public DexLib.MethodDefinition DexMethod
        {
            get { return dmethod; }
        }
    }
}
