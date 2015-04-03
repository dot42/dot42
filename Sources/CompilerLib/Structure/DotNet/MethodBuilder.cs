using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.DexLib;
using Dot42.DexLib.Extensions;
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
        private XMethodDefinition xMethod;

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
            if (method.IsPrivate)
            {
                if (method.DeclaringType.HasNestedTypes)
                    dmethod.IsProtected = true;
                else
                    dmethod.IsPrivate = true;
            }
            else if (method.IsFamily) dmethod.IsProtected = true;
            else dmethod.IsPublic = true;

            if (method.DeclaringType.IsInterface)
            {
                dmethod.IsAbstract = true;
                //dmethod.
            }
            else
            {
                if (method.IsConstructor) dmethod.IsConstructor = true;
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
        /// Implement make minor fixes after the implementation phase.
        /// </summary>
        public void FixUp(DexTargetPackage targetPackage)
        {
            var iMethod = method.GetBaseInterfaceMethod();
            //var iMethod = method.Overrides.Select(x => x.Resolve()).Where(x => x != null).FirstOrDefault(x => x.DeclaringType.IsInterface);
            if (iMethod != null)
            {
                dmethod.Prototype.ReturnType = iMethod.ReturnType.GetReference(targetPackage, compiler.Module);
            }

            var baseMethod = method.GetBaseMethod();
            if (baseMethod != null)
            {
                dmethod.Prototype.ReturnType = baseMethod.ReturnType.GetReference(targetPackage, compiler.Module);                
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
            if (method.HasBody)
            {
                ExpandSequencePoints(method.Body);
                var source = new MethodSource(xMethod, method);
                DexMethodBodyCompiler.TranslateToRL(compiler, targetPackage, source, dmethod, out compiledMethod);
            }
        }

        /// <summary>
        /// Create all annotations for this method
        /// </summary>
        internal virtual void CreateAnnotations(DexTargetPackage targetPackage)
        {
            // Build method annotations
            AnnotationBuilder.Create(compiler, method, dmethod, targetPackage);
            
            if (!dmethod.IsSynthetic && !dmethod.Owner.IsSynthetic)
            {
                // only add generics annotation for getters or setters.
                if (method.IsGetter)
                    dmethod.AddGenericDefinitionAnnotationIfGeneric(xMethod.ReturnType, compiler, targetPackage);
                else if (method.IsSetter)
                    for (int i = 0; i < xMethod.Parameters.Count; ++i)
                    {
                        var dp = dmethod.Prototype.Parameters[i];
                        dp.AddGenericDefinitionAnnotationIfGeneric(xMethod.Parameters[i].ParameterType, compiler,
                            targetPackage);
                    }
            }
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
