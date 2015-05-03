using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;

namespace Dot42.CompilerLib.XModel.Synthetic
{
    /// <summary>
    /// Method definition that is created by this compiler.
    /// </summary>
    public class XSyntheticMethodDefinition : XMethodDefinition
    {
        private readonly XSyntheticMethodFlags flags;
        private readonly string name;
        private readonly string scopeId;
        private readonly XTypeReference returnType;
        private readonly List<XParameter> parameters;
        private readonly List<AstVariable> astParameters;
        private DexLib.MethodDefinition dexMethod;

        /// <summary>
        /// Default ctor
        /// </summary>
        private XSyntheticMethodDefinition(XTypeDefinition declaringType, XSyntheticMethodFlags flags, string name, string scopeId, XTypeReference returnType, params XParameter[] parameters)
            : base(declaringType)
        {
            this.flags = flags;
            this.name = name;
            this.scopeId = scopeId;
            this.returnType = returnType;
            this.parameters = parameters.ToList();
            astParameters = parameters.Select(x => new AstParameterVariable(x)).Cast<AstVariable>().ToList();
        }

        
        /// <summary>
        /// Create a synthetic method and add it to the given declaring type.
        /// </summary>
        public static XSyntheticMethodDefinition Create(XTypeDefinition declaringType, XSyntheticMethodFlags flags, string name, string scopeId, XTypeReference returnType, params XParameter[] parameters)
        {
            var method = new XSyntheticMethodDefinition(declaringType, flags, name, scopeId, returnType, parameters);
            declaringType.Add(method);
            return method;
        }

        /// <summary>
        /// Gets/sets the method body
        /// </summary>
        public AstBlock Body { get; set; }

        /// <summary>
        /// Compile this method to dex code.
        /// </summary>
        public void Compile(AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            if (Body == null)
                throw new ArgumentException("Body expected");
            if (dexMethod == null)
                throw new ArgumentException("dexMethod expected");
            CompiledMethod compiledMethod;
            var source = new MethodSource(this, Body);
            DexMethodBodyCompiler.TranslateToRL(compiler, targetPackage, source, dexMethod, false, out compiledMethod);
            compiledMethod.DexMethod = dexMethod;
        }

        /// <summary>
        /// Name of the reference
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Is this an instance method ref?
        /// </summary>
        public override bool HasThis
        {
            get { return !IsStatic; }
        }

        /// <summary>
        /// Gets all generic parameters
        /// </summary>
        public override ReadOnlyCollection<XGenericParameter> GenericParameters
        {
            get { return new List<XGenericParameter>().AsReadOnly(); }
        }

        /// <summary>
        /// Return type of the method
        /// </summary>
        public override XTypeReference ReturnType
        {
            get { return returnType; }
        }

        /// <summary>
        /// Parameters of the method
        /// </summary>
        public override ReadOnlyCollection<XParameter> Parameters
        {
            get { return parameters.AsReadOnly(); }
        }

        /// <summary>
        /// Parameters of the method as Ast variables.
        /// </summary>
        public ReadOnlyCollection<AstVariable> AstParameters
        {
            get { return astParameters.AsReadOnly(); }
        }

        /// <summary>
        /// Does this method have the given name (or does the original method have this name).
        /// </summary>
        public override bool EqualsName(string name)
        {
            return (name == this.name);
        }

        /// <summary>
        /// Is this method abstract?
        /// </summary>
        public override bool IsAbstract
        {
            get { return flags.HasFlag(XSyntheticMethodFlags.Abstract); }
        }

        /// <summary>
        /// Is this a virtual method?
        /// </summary>
        public override bool IsVirtual
        {
            get { return flags.HasFlag(XSyntheticMethodFlags.Virtual); }
        }

        /// <summary>
        /// Is this a static method?
        /// </summary>
        public override bool IsStatic
        {
            get { return flags.HasFlag(XSyntheticMethodFlags.Static); }
        }

        /// <summary>
        /// Is this an instance or class constructor?
        /// </summary>
        public override bool IsConstructor
        {
            get { return flags.HasFlag(XSyntheticMethodFlags.Constructor); }
        }

        /// <summary>
        /// Is this a property get method?
        /// </summary>
        public override bool IsGetter
        {
            get { return flags.HasFlag(XSyntheticMethodFlags.Getter); }
        }

        /// <summary>
        /// Is this a property set method?
        /// </summary>
        public override bool IsSetter
        {
            get { return flags.HasFlag(XSyntheticMethodFlags.Setter); }
        }

        /// <summary>
        /// Is this a private method?
        /// </summary>
        public bool IsPrivate
        {
            get { return flags.HasFlag(XSyntheticMethodFlags.Private); }
        }

        /// <summary>
        /// Is this a protected method?
        /// </summary>
        public bool IsProtected
        {
            get { return flags.HasFlag(XSyntheticMethodFlags.Protected); }
        }

        /// <summary>
        /// Is this an android extension method?
        /// </summary>
        public override bool IsAndroidExtension
        {
            get { return false; }
        }

        /// <summary>
        /// Should this method be called with an invoke_direct in dex?
        /// </summary>
        public override bool IsDirect
        {
            get { return (!IsStatic) && (IsPrivate || IsConstructor); }
        }

        public override string ScopeId { get { return scopeId ?? dexMethod.Name + dexMethod.Prototype.ToSignature(); } }

        /// <summary>
        /// Should this method be called with invoke_interface?
        /// </summary>
        public override bool UseInvokeInterface
        {
            get { return false; }
        }

        /// <summary>
        /// Does this method need a parameter to pass the generic instance array for the generic types of the method itself?
        /// </summary>
        public override bool NeedsGenericInstanceMethodParameter
        {
            get { return false; }
        }

        /// <summary>
        /// Does this method have a DexNative attribute?
        /// </summary>
        public override bool HasDexNativeAttribute()
        {
            return false;
        }

        /// <summary>
        /// Does this method have a DexImport attribute?
        /// </summary>
        public override bool HasDexImportAttribute()
        {
            return false;
        }

        /// <summary>
        /// Try to get the names from the DexImport attribute attached to this method.
        /// </summary>
        public override bool TryGetDexImportNames(out string methodName, out string descriptor, out string className)
        {
            methodName = null;
            descriptor = null;
            className = null;
            return false;
        }

        /// <summary>
        /// Does this method have a JavaImport attribute?
        /// </summary>
        public override bool HasJavaImportAttribute()
        {
            return false;
        }

        /// <summary>
        /// Try to get the names from the JavaImport attribute attached to this method.
        /// </summary>
        public override bool TryGetJavaImportNames(out string methodName, out string descriptor, out string className)
        {
            methodName = null;
            descriptor = null;
            className = null;
            return false;
        }

        /// <summary>
        /// Create a dex method definition from this method.
        /// </summary>
        public DexLib.MethodDefinition GetDexMethod(DexLib.ClassDefinition owner, DexTargetPackage targetPackage)
        {
            if (dexMethod == null)
            {
                var prototype = new DexLib.Prototype(ReturnType.GetReference(targetPackage),
                                              parameters.Select(x => new DexLib.Parameter(x.ParameterType.GetReference(targetPackage), x.Name)).ToArray());
                var mdef = new DexLib.MethodDefinition(owner, name, prototype);
                if (IsAbstract) mdef.IsAbstract = true;
                if (IsVirtual) mdef.IsVirtual = true;
                if (IsStatic) mdef.IsStatic = true;
                if (IsPrivate) mdef.IsPrivate = true;
                else if (IsProtected) mdef.IsProtected = true;
                else mdef.IsPublic = true;
                if (IsConstructor)
                {
                    mdef.Name = IsStatic ? "<clinit>" : "<init>";
                    mdef.IsConstructor = true;
                }
                dexMethod = mdef;
                targetPackage.NameConverter.Record(this, dexMethod);
            }
            return dexMethod;
        }

        /// <summary>
        /// AstVariable type for my parameters
        /// </summary>
        private class AstParameterVariable : AstVariable, IParameter
        {
            private readonly XParameter parameter;

            /// <summary>
            /// Default ctor
            /// </summary>
            public AstParameterVariable(XParameter parameter)
            {
                this.parameter = parameter;
                Name = parameter.Name;
                Type = parameter.ParameterType;
                IsGenerated = false;
            }

            public override bool IsPinned
            {
                get { return false; }
            }

            public override bool IsParameter
            {
                get { return true; }
            }

            public override bool IsThis
            {
                get { return false; }
            }

            public override object OriginalVariable
            {
                get { return null; }
            }

            public override object OriginalParameter
            {
                get { return parameter; }
            }

            public override string OriginalName
            {
                get { return parameter.Name; }
            }

            protected override TTypeRef GetType<TTypeRef>(ITypeResolver<TTypeRef> typeResolver)
            {
                return typeResolver.GetTypeReference(parameter.ParameterType);
            }

            string IParameter.Name { get { return parameter.Name; } }

            bool IParameter.Equals(IVariable variable)
            {
                return (variable == this);
            }
        }
    }
}
