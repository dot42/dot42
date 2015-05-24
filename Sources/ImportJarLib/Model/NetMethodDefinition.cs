using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Dot42.ImportJarLib.Doxygen;

namespace Dot42.ImportJarLib.Model
{
    [DebuggerDisplay("{Name} {DeclaringType} {Parameters.Count} {createReason}")]
    public sealed partial class NetMethodDefinition : INetGenericParameterProvider, INetMemberDefinition, INetMemberVisibility
    {
#if DEBUG
        private static int lastId;
        private readonly int id = lastId++;
#endif
        private readonly JvmClassLib.MethodDefinition javaMethod;
        private readonly TargetFramework target;
        private readonly List<NetParameterDefinition> parameters = new List<NetParameterDefinition>();
        private readonly List<NetGenericParameter> genericParameters = new List<NetGenericParameter>();
        private readonly List<NetCustomAttribute> customAttributes = new List<NetCustomAttribute>();
        private readonly OverrideCollection overrides;
        private readonly string createReason;
        private SignedByteMode signMode;
        private bool requiredNewSlot;
        private NetPropertyDefinition property;
        

        /// <summary>
        /// Default ctor
        /// </summary>
        public NetMethodDefinition(string name, JvmClassLib.MethodDefinition javaMethod, NetTypeDefinition declaringType, TargetFramework target, SignedByteMode signMode, string createReason)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");
            if (target == null)
                throw new ArgumentNullException("target");
            Name = name;
            DeclaringType = declaringType;
            this.javaMethod = javaMethod; // Can be null
            this.target = target;
            this.signMode = signMode;
            this.createReason = createReason;
            overrides = new OverrideCollection(this);
        }

        /// <summary>
        /// Name of the type
        /// </summary>
        public string Name { get; private set; }

        internal void SetName(string value)
        {
            Name = value;
        }

        /// <summary>
        /// Gets the descriptor of the java method for which this method is created.
        /// Returns null if there is no original java method.
        /// </summary>
        public string JavaDescriptor
        {
            get { return (javaMethod != null) ? javaMethod.Descriptor : null; }
        }

        /// <summary>
        /// Human readable description.
        /// </summary>
        public DocDescription Description { get; set; }

        /// <summary>
        /// Gets the target framework
        /// </summary>
        public TargetFramework Target { get { return target; } }

        /// <summary>
        /// Gets/sets the method to which this method belongs
        /// </summary>
        internal MethodRenamer.MethodGroup MethodGroup { get; set; }

        /// <summary>
        /// The editor browsable state of this member
        /// </summary>
        public EditorBrowsableState EditorBrowsableState { get; set; }

        /// <summary>
        /// JAVA access flags
        /// </summary>
        public int AccessFlags { get; set; }

        /// <summary>
        /// Is this an abstract method?
        /// </summary>
        public bool IsAbstract
        {
            get
            {
                if (DeclaringType.IsEnum)
                {
                    return false;
                }

                return Attributes.HasFlag(MethodAttributes.Abstract) && !IsOverride;
            }
         
            set
            {
                Attributes = Attributes.Set(value, MethodAttributes.Abstract);
            }
        }

        /// <summary>
        /// Is this an virtual method?
        /// </summary>
        public bool IsVirtual
        {
            get
            {
                if (DeclaringType.IsEnum)
                {
                    return false;
                }

                return Attributes.HasFlag(MethodAttributes.Virtual);
            }
            set
            {
                Attributes = Attributes.Set(value, MethodAttributes.Virtual);
            }
        }

        /// <summary>
        /// Is this an static method?
        /// </summary>
        public bool IsStatic
        {
            get { return Attributes.HasFlag(MethodAttributes.Static); }
            set { Attributes = Attributes.Set(value, MethodAttributes.Static); }
        }

        /// <summary>
        /// Is this an final method?
        /// </summary>
        public bool IsFinal
        {
            get { return Attributes.HasFlag(MethodAttributes.Final); }
            set { Attributes = Attributes.Set(value, MethodAttributes.Final); }
        }

        /// <summary>
        /// Does this method override other methods?
        /// </summary>
        public bool IsOverride
        {
            get { return overrides.Any(); }
        }

        /// <summary>
        /// Does this method need the override keyword
        /// </summary>
        public bool NeedsOverrideKeyword
        {
            get
            {
                var first = overrides.FirstOrDefault(x => !x.DeclaringType.IsInterface);
                return (!requiredNewSlot) && ((first != null) && !first.IsFinal);
            }
        }

        /// <summary>
        /// Is this a NewSlot method?
        /// </summary>
        public bool IsNewSlot
        {
            get
            {
                var first = overrides.FirstOrDefault(x => !x.DeclaringType.IsInterface);
                return requiredNewSlot || ((first != null) && first.IsFinal);
            }
        }

        /// <summary>
        /// Mark this method as requiring a "new" keyword.
        /// </summary>
        public void RequireNewSlot()
        {
            requiredNewSlot = true;
        }

        /// <summary>
        /// Gets all methods this method overrides.
        /// </summary>
        public IEnumerable<NetMethodDefinition> Overrides
        {
            get { return overrides; }
        }

        /// <summary>
        /// Attributes of the method.
        /// </summary>
        public MethodAttributes Attributes { get; set; }

        /// <summary>
        /// Parent
        /// </summary>
        public NetTypeDefinition DeclaringType { get; set; }

        /// <summary>
        /// Return type of the method
        /// </summary>
        public NetTypeReference ReturnType { get; set; }

        /// <summary>
        /// If set, this method is an explicit implementation of the given interface method.
        /// </summary>
        public NetMethodDefinition InterfaceMethod { get; private set; }

        /// <summary>
        /// If set, this method is an explicit implementation of the given interface method's type.
        /// </summary>
        public NetTypeReference InterfaceType { get; private set; }

        /// <summary>
        /// Mark this method as an explicit interface implementation.
        /// </summary>
        public void SetExplicitImplementation(NetMethodDefinition iMethod, NetTypeReference iType)
        {
            if (iMethod == null)
                throw new ArgumentNullException("iMethod");
            if (iType == null)
                throw new ArgumentNullException("iType");
            InterfaceMethod = iMethod;
            InterfaceType = iType;
        }

        /// <summary>
        /// All parameters
        /// </summary>
        public List<NetParameterDefinition> Parameters { get { return parameters; } }

        /// <summary>
        /// All generic parameters
        /// </summary>
        public List<NetGenericParameter> GenericParameters { get { return genericParameters; } }

        /// <summary>
        /// Is this a method?
        /// If not it's a type.
        /// </summary>
        bool INetGenericParameterProvider.IsMethod
        {
            get { return true; }
        }

        /// <summary>
        /// Is this a type?
        /// If not it's a method.
        /// </summary>
        bool INetGenericParameterProvider.IsType
        {
            get { return false; }
        }

        /// <summary>
        /// Is this an interface type?
        /// </summary>
        bool INetGenericParameterProvider.IsInterface
        {
            get { return false; }
        }

        /// <summary>
        /// Is this a constructor?
        /// </summary>
        public bool IsConstructor
        {
            get { return (Name == ".ctor") || (Name == ".cctor"); }
        }

        /// <summary>
        /// Is this method a deconstructor?
        /// </summary>
        public bool IsDeconstructor { get; set; }

        /// <summary>
        /// Gets all custom attributes
        /// </summary>
        public List<NetCustomAttribute> CustomAttributes { get { return customAttributes; } }

        /// <summary>
        /// Is this method an overload of another method with sign convertion?
        /// </summary>
        public bool IsSignConverted
        {
            get { return signMode == SignedByteMode.Convert || signMode == SignedByteMode.ConvertWithoutPartner; }
            set { signMode = value ? SignedByteMode.Convert : SignedByteMode.None; } }

        /// <summary>
        /// when IsSignConverted, was an unsigned partner generated?
        /// </summary>
        public bool HasUnsignedPartner { get { return signMode == SignedByteMode.HasUnsignedPartner || signMode == SignedByteMode.HasUnsignedPartnerOnlyInReturnType; } }

        public SignedByteMode SignConvertMode { get { return signMode; } }

        /// <summary>
        /// Method name of the original java method (if any)
        /// </summary>
        public string OriginalJavaName { get; set; }

        /// <summary>
        /// Is this member public 
        /// </summary>
        public bool IsPublic
        {
            get { return IsVisibility(MethodAttributes.Public); }
            set { SetVisibility(MethodAttributes.Public, value); }
        }

        /// <summary>
        /// Is this member public and nested (only types)
        /// </summary>
        bool INetMemberVisibility.IsNestedPublic
        {
            get { return false; }
            set { IsPublic = value; }
        }

        /// <summary>
        /// Is this member private 
        /// </summary>
        public bool IsPrivate
        {
            get { return IsVisibility(MethodAttributes.Private); }
            set { SetVisibility(MethodAttributes.Private, value); }
        }

        /// <summary>
        /// Is this member protected
        /// </summary>
        public bool IsFamily
        {
            get { return IsVisibility(MethodAttributes.Family); }
            set { SetVisibility(MethodAttributes.Family, value); }
        }

        /// <summary>
        /// Is this member internal
        /// </summary>
        public bool IsAssembly
        {
            get { return IsVisibility(MethodAttributes.Assembly); }
            set { SetVisibility(MethodAttributes.Assembly, value); }
        }

        /// <summary>
        /// Is this member internal or protected
        /// </summary>
        public bool IsFamilyOrAssembly
        {
            get { return IsVisibility(MethodAttributes.FamORAssem); }
            set { SetVisibility(MethodAttributes.FamORAssem, value); }
        }

        /// <summary>
        /// Is this member internal and protected
        /// </summary>
        public bool IsFamilyAndAssembly
        {
            get { return IsVisibility(MethodAttributes.FamANDAssem); }
            set { SetVisibility(MethodAttributes.FamANDAssem, value); }
        }

        /// <summary>
        /// Are this member an the given type definition in the same scope?
        /// </summary>
        public bool HasSameScope(NetTypeDefinition type)
        {
            return (DeclaringType != null) && (DeclaringType.HasSameScope(type));
        }

        /// <summary>
        /// Are this member and the given other member in the same scope?
        /// </summary>
        public bool HasSameScope(INetMemberVisibility other)
        {
            return (DeclaringType != null) && other.HasSameScope(DeclaringType);
        }

        /// <summary>
        /// Gets a string explaining why this method was created
        /// </summary>
        internal string CreateReason
        {
            get { return createReason; }
        }

        /// <summary>
        /// Is the given visibility value set?
        /// </summary>
        private bool IsVisibility(MethodAttributes value)
        {
            return (((Attributes) & MethodAttributes.MemberAccessMask) == value);
        }

        /// <summary>
        /// Set the given visibility value?
        /// </summary>
        private void SetVisibility(MethodAttributes mask, bool value)
        {
            if (!value)
                return;
            var remaining = (Attributes & ~MethodAttributes.MemberAccessMask);
            Attributes = remaining | (mask & MethodAttributes.MemberAccessMask);
        }

        /// <summary>
        /// Make sure that the visibility of types used in the signature of this member are high enough.
        /// Also make sure the visibility of the member is high enough to fullfill interface overrides
        /// </summary>
        public void EnsureVisibility()
        {
            ReturnType.EnsureVisibility(this);
            foreach (var p in Parameters)
            {
                p.ParameterType.EnsureVisibility(this);
            }
        }

        /// <summary>
        /// Make sure that the visibility of this member is not higher than allowed from types used in the signature of this member that are from another scope.
        /// </summary>
        public void LimitVisibility()
        {
            // Limit for signature types
            this.LimitVisibility(ReturnType);
            foreach (var p in Parameters)
            {
                this.LimitVisibility(p.ParameterType);
            }

            // Limit for sealed declaring type
            var baseMethod = Overrides.FirstOrDefault(x => !x.DeclaringType.IsInterface);
            if (baseMethod == null)
            {
                this.LimitIfDeclaringTypeSealed(DeclaringType);
            }

            // Limit when overriding "protected internal" from another scope
            if ((baseMethod != null) && IsFamilyOrAssembly && !baseMethod.HasSameScope(this))
            {
                IsFamily = true;
            }
        }
        
        /// <summary>
        /// Is this method getter or setter of a property?
        /// </summary>
        public NetPropertyDefinition Property
        {
            get { return property; }
            set { property = value; }
        }
    }
}
