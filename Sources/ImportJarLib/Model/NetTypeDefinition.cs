using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Dot42.ImportJarLib.Doxygen;
using Dot42.JvmClassLib;

namespace Dot42.ImportJarLib.Model
{
    [DebuggerDisplay("TDef: {@FullName}")]
    public sealed class NetTypeDefinition : NetTypeReference, INetMemberDefinition, INetGenericParameterProvider, INetMemberVisibility
    {
        private readonly ClassFile classFile;
        private readonly TargetFramework target;
        private readonly string scope;
        private readonly InterfacesCollection interfaces;
        private readonly List<NetGenericParameter> genericParameters = new List<NetGenericParameter>();
        private readonly List<NetCustomAttribute> customAttributes = new List<NetCustomAttribute>();
        private readonly NetMemberDefinitionCollection<NetFieldDefinition> fields;
        private readonly NetMemberDefinitionCollection<NetPropertyDefinition> properties;
        private readonly NetMemberDefinitionCollection<NetMethodDefinition> methods;
        private readonly NetMemberDefinitionCollection<NetTypeDefinition> nestedTypes;
        private readonly List<IFlushable> flushActions = new List<IFlushable>();
        private bool isStruct;
        private bool isEnum;
        private NetTypeReference baseType;

        /// <summary>
        /// Default ctor
        /// </summary>
        public NetTypeDefinition(ClassFile classFile, TargetFramework target, string scope)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            this.classFile = classFile;
            this.target = target;
            this.scope = scope;
            interfaces = new InterfacesCollection(this);
            fields = new NetMemberDefinitionCollection<NetFieldDefinition>(this);
            properties = new NetMemberDefinitionCollection<NetPropertyDefinition>(this);
            methods = new NetMemberDefinitionCollection<NetMethodDefinition>(this);
            nestedTypes = new NetMemberDefinitionCollection<NetTypeDefinition>(this);
        }

        /// <summary>
        /// Namespace of the type (if any)
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Parent (in case of nested types)
        /// </summary>
        public NetTypeDefinition DeclaringType { get; set; }

        /// <summary>
        /// Gets the entire C# name.
        /// </summary>
        public override string FullName
        {
            get
            {
                var fullName = new StringBuilder();
                if (DeclaringType != null)
                {
                    fullName.Append(DeclaringType.FullName);
                    fullName.Append('.');
                }
                var ns = Namespace;
                if (!string.IsNullOrEmpty(ns))
                {
                    fullName.Append(ns);
                    fullName.Append('.');
                }
                fullName.Append(Name);
                var result = fullName.ToString();
                return result;
            }
        }

        /// <summary>
        /// Gets the original java class file (can be empty)
        /// </summary>
        public ClassFile ClassFile { get { return classFile; } }

        /// <summary>
        /// The editor browsable state of this member
        /// </summary>
        public EditorBrowsableState EditorBrowsableState { get; set; }

        /// <summary>
        /// Is this an interface?
        /// </summary>
        public bool IsInterface
        {
            get { return (Attributes & TypeAttributes.Interface) != 0; }
            set { Attributes = Attributes.Set(value, TypeAttributes.Interface); }
        }

        /// <summary>
        /// Is this a struct?
        /// </summary>
        public bool IsStruct
        {
            get { return isStruct; }
            set
            {
                isStruct = value;
                if (value)
                {
                    IsInterface = false;
                    IsEnum = false;
                }
            }
        }

        /// <summary>
        /// Is this an enum?
        /// </summary>
        public bool IsEnum
        {
            get { return isEnum; }
            set
            {
                isEnum = value;
                if (value)
                {
                    IsInterface = false;
                    isStruct = false;
                }
            }
        }

        /// <summary>
        /// Is this a static class?
        /// </summary>
        public bool IsStatic 
        {
            get { return Attributes.HasFlag(TypeAttributes.Abstract) && Attributes.HasFlag(TypeAttributes.Sealed); }
        }

        /// <summary>
        /// Attributes of the type.
        /// </summary>
        public TypeAttributes Attributes { get; set; }

        /// <summary>
        /// Is this type sealed?
        /// </summary>
        public bool IsSealed
        {
            get { return Attributes.HasFlag(TypeAttributes.Sealed); }
        }

        /// <summary>
        /// Is this type not-public (internal)
        /// </summary>
        public bool IsNotPublic
        {
            get { return IsVisibility(TypeAttributes.NotPublic); }
            set { SetVisibility(TypeAttributes.NotPublic, value); }
        }

        /// <summary>
        /// Is this type public 
        /// </summary>
        public bool IsPublic
        {
            get { return IsVisibility(TypeAttributes.Public); }
            set { SetVisibility(TypeAttributes.Public, value); }
        }

        /// <summary>
        /// Is this member private 
        /// </summary>
        bool INetMemberVisibility.IsPrivate
        {
            get { return IsNestedPrivate; }
            set
            {
                if (DeclaringType != null)
                    IsNestedPrivate = value;
                else
                    IsNotPublic = value;
            }
        }

        /// <summary>
        /// Is this member protected
        /// </summary>
        bool INetMemberVisibility.IsFamily
        {
            get { return IsNestedFamily; }
            set
            {
                if (DeclaringType != null)
                    IsNestedFamily = value;
                else
                    IsNotPublic = value;
            }
        }

        /// <summary>
        /// Is this member internal
        /// </summary>
        bool INetMemberVisibility.IsAssembly
        {
            get { return IsNotPublic || IsNestedAssembly; }
            set
            {
                if (DeclaringType != null)
                    IsNestedAssembly = value;
                else
                    IsNotPublic = value;
            }
        }

        /// <summary>
        /// Is this member internal or protected
        /// </summary>
        bool INetMemberVisibility.IsFamilyOrAssembly
        {
            get { return IsNestedFammilyOrAssembly; }
            set
            {
                if (DeclaringType != null)
                    IsNestedFammilyOrAssembly = value;
                else
                    IsNotPublic = value;
            }
        }

        /// <summary>
        /// Is this member internal and protected
        /// </summary>
        bool INetMemberVisibility.IsFamilyAndAssembly
        {
            get { return IsNestedFammilyAndAssembly; }
            set
            {
                if (DeclaringType != null)
                    IsNestedFammilyAndAssembly = value;
                else
                    IsNotPublic = value;
            }
        }

        /// <summary>
        /// Is this type nested public 
        /// </summary>
        public bool IsNestedPublic
        {
            get { return IsVisibility(TypeAttributes.NestedPublic); }
            set { SetVisibility(TypeAttributes.NestedPublic, value); }
        }

        /// <summary>
        /// Is this type nested private 
        /// </summary>
        public bool IsNestedPrivate
        {
            get { return IsVisibility(TypeAttributes.NestedPrivate); }
            set { SetVisibility(TypeAttributes.NestedPrivate, value); }
        }

        /// <summary>
        /// Is this type nested protected
        /// </summary>
        public bool IsNestedFamily
        {
            get { return IsVisibility(TypeAttributes.NestedFamily); }
            set { SetVisibility(TypeAttributes.NestedFamily, value); }
        }

        /// <summary>
        /// Is this type nested internal
        /// </summary>
        public bool IsNestedAssembly
        {
            get { return IsVisibility(TypeAttributes.NestedAssembly); }
            set { SetVisibility(TypeAttributes.NestedAssembly, value); }
        }

        /// <summary>
        /// Is this type nested internal or protected
        /// </summary>
        public bool IsNestedFammilyOrAssembly
        {
            get { return IsVisibility(TypeAttributes.NestedFamORAssem); }
            set { SetVisibility(TypeAttributes.NestedFamORAssem, value); }
        }

        /// <summary>
        /// Is this type nested internal and protected
        /// </summary>
        public bool IsNestedFammilyAndAssembly
        {
            get { return IsVisibility(TypeAttributes.NestedFamANDAssem); }
            set { SetVisibility(TypeAttributes.NestedFamANDAssem, value); }
        }

        /// <summary>
        /// Are this member an the given type definition in the same scope?
        /// </summary>
        public bool HasSameScope(NetTypeDefinition other)
        {
            return (other != null) && (scope == other.scope);
        }

        /// <summary>
        /// Are this member and the given other member in the same scope?
        /// </summary>
        bool INetMemberVisibility.HasSameScope(INetMemberVisibility other)
        {
            return other.HasSameScope(this);
        }

        /// <summary>
        /// Is the given visibility value set?
        /// </summary>
        private bool IsVisibility(TypeAttributes value)
        {
            return (((Attributes) & TypeAttributes.VisibilityMask) == value);
        }

        /// <summary>
        /// Set the given visibility value?
        /// </summary>
        private void SetVisibility(TypeAttributes mask, bool value)
        {
            if (!value)
                return;
            var remaining = (Attributes & ~TypeAttributes.VisibilityMask);
            Attributes = remaining | (mask & TypeAttributes.VisibilityMask);
        }

        /// <summary>
        /// Base type (null if Object)
        /// </summary>
        public NetTypeReference BaseType
        {
            get { return baseType; }
            set
            {
                if ((value != null) && (value.FullName == "System.Enum"))
                {
                    IsEnum = true;
                    baseType = null;
                }
                else
                {
                    baseType = value;
                }
            }
        }

        /// <summary>
        /// Underlying type definition.
        /// </summary>
        public override NetTypeDefinition GetElementType()
        {
            return this;
        }

        /// <summary>
        /// Gets all types references in this type.
        /// This includes the element type and any generic arguments.
        /// </summary>
        public override IEnumerable<NetTypeDefinition> GetReferencedTypes()
        {
            yield return this;
        }

        /// <summary>
        /// All implemented interfaces
        /// </summary>
        public ICustomCollection<NetTypeReference> Interfaces { get { return interfaces; }}

        /// <summary>
        /// All generic parameters
        /// </summary>
        public List<NetGenericParameter> GenericParameters { get { return genericParameters; } }

        /// <summary>
        /// All fields
        /// </summary>
        public ICollection<NetFieldDefinition> Fields { get { return fields; } }

        /// <summary>
        /// All properties
        /// </summary>
        public ICollection<NetPropertyDefinition> Properties { get { return properties; } }

        /// <summary>
        /// All methods
        /// </summary>
        public ICollection<NetMethodDefinition> Methods { get { return methods; } }

        /// <summary>
        /// All nested types
        /// </summary>
        public ICollection<NetTypeDefinition> NestedTypes { get { return nestedTypes; } }

        /// <summary>
        /// Is this a method?
        /// If not it's a type.
        /// </summary>
        bool INetGenericParameterProvider.IsMethod
        {
            get { return false; }
        }

        /// <summary>
        /// Is this a type?
        /// If not it's a method.
        /// </summary>
        bool INetGenericParameterProvider.IsType
        {
            get { return true; }
        }

        /// <summary>
        /// Is this an interface type?
        /// </summary>
        bool INetGenericParameterProvider.IsInterface
        {
            get { return IsInterface; }
        }

        /// <summary>
        /// Gets all custom attributes
        /// </summary>
        public List<NetCustomAttribute> CustomAttributes { get { return customAttributes; } }

        /// <summary>
        /// If set, force this type to be non-generic.
        /// </summary>
        public bool IgnoreGenericArguments { get; set; }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override T Accept<T, TData>(INetTypeVisitor<T, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Human readable description.
        /// </summary>
        public DocDescription Description { get; set; }

        /// <summary>
        /// Is set, this type def is created from an imported type mapping.
        /// </summary>
        public bool ImportedFromMapping { get; set; }

        /// <summary>
        /// Class name of the original java class (if any)
        /// </summary>
        public string OriginalJavaClassName { get; set; }

        /// <summary>
        /// Set when the original base class is not publicly visible.
        /// </summary>
        public bool HasInternalBaseClass { get; set; }

        /// <summary>
        /// Gets the scope of this type
        /// </summary>
        public string Scope { get { return scope; } }

        /// <summary>
        /// Make sure that the visibility of the basetype of this type is high enough.
        /// </summary>
        public void EnsureVisibility()
        {
            if (baseType == null)
                return;

            // Make sure all base types and types used in generic arguments has a high enough visibility.
            foreach (var refType in baseType.GetReferencedTypes())
            {
                if (refType.EnsureVisibility(this))
                {
                    refType.EnsureVisibility();
                }
            }
        }

        /// <summary>
        /// Add the given item to my flush list.
        /// </summary>
        internal void AddFlushAction(IFlushable item)
        {
            if (!flushActions.Contains(item))
                flushActions.Add(item);
        }

        /// <summary>
        /// This type has been modified, flush where needed.
        /// </summary>
        internal void OnModified()
        {
            var i = 0;
            while (i < flushActions.Count)
            {
                var item = flushActions[i++];
                item.Flush();
            }
            //flushActions.Clear();
        }

        /// <summary>
        /// Collection of implemented interfaces
        /// </summary>
        private sealed class InterfacesCollection : CustomCollection<NetTypeReference>
        {
            private readonly NetTypeDefinition owner;

            /// <summary>
            /// Default ctor
            /// </summary>
            public InterfacesCollection(NetTypeDefinition owner)
            {
                if (owner == null)
                    throw new ArgumentNullException("owner");
                this.owner = owner;
            }

            /// <summary>
            /// Item is about to be added
            /// </summary>
            protected override void OnAdding(NetTypeReference item)
            {
                owner.OnModified();
            }

            /// <summary>
            /// Item is about to be removed.
            /// </summary>
            protected override void OnRemoving(NetTypeReference item)
            {
                owner.OnModified();
            }
        }
    }
}
