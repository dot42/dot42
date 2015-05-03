using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;

namespace Dot42.CompilerLib.XModel.Synthetic
{
    /// <summary>
    /// Type definition that is created by this compiler.
    /// </summary>
    public class XSyntheticTypeDefinition : XTypeDefinition
    {
        private readonly XSyntheticTypeFlags flags;
        private readonly string @namespace;
        private readonly string name;
        private readonly XTypeReference baseType;
        private DexLib.ClassDefinition dexType;
        private readonly List<XFieldDefinition> fields;
        private readonly List<XMethodDefinition> methods;
        private readonly List<XTypeDefinition> nestedTypes;
        private readonly List<XTypeReference> interfaces;
        private string _scopeId;

        /// <summary>
        /// Default ctor
        /// </summary>
        private XSyntheticTypeDefinition(XModule module, XTypeDefinition declaringType, XSyntheticTypeFlags flags, string @namespace, string name, XTypeReference baseType, string _scopeId)
            : base(module, declaringType, flags.HasFlag(XSyntheticTypeFlags.ValueType), null)
        {
            this.flags = flags;
            this.@namespace = @namespace;
            this.name = name;
            this.baseType = baseType;
            fields = new List<XFieldDefinition>();
            methods = new List<XMethodDefinition>();
            nestedTypes = new List<XTypeDefinition>();
            interfaces = new List<XTypeReference>();
            this._scopeId = _scopeId;
       }

        
        /// <summary>
        /// Create a synthetic field and add it to the given declaring type.
        /// </summary>
        public static XSyntheticTypeDefinition Create(XModule module, XTypeDefinition declaringType, XSyntheticTypeFlags flags, string @namespace, string name, XTypeReference baseType, string fullScopeId)
        {
            var type = new XSyntheticTypeDefinition(module, declaringType, flags, @namespace, name, baseType, fullScopeId);
            if(declaringType != null)
                declaringType.Add(type);
            return type;
        }

        /// <summary>
        /// Name of the reference
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the namespace of this type.
        /// Returns the namespace of the declaring type for nested types.
        /// </summary>
        public override string Namespace
        {
            get { return @namespace; }
        }

        /// <summary>
        /// Gets the underlying type object.
        /// </summary>
        public override object OriginalTypeDefinition
        {
            get { return null; }
        }

        /// <summary>
        /// Sort order priority.
        /// Low values come first
        /// </summary>
        public override int Priority
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the type this type extends (null if System.Object)
        /// </summary>
        public override XTypeReference BaseType
        {
            get { return baseType; }
        }

        /// <summary>
        /// Is this type a type that needs it's generic types implemented at runtime?
        /// </summary>
        public override bool IsGenericClass
        {
            get { return false; }
        }

        /// <summary>
        /// Gets all fields defined in this type.
        /// </summary>
        public override ReadOnlyCollection<XFieldDefinition> Fields
        {
            get { return fields.AsReadOnly(); }
        }

        /// <summary>
        /// Gets a field by it's underlying field object.
        /// </summary>
        public override XFieldDefinition GetByOriginalField(object originalField)
        {
            return null;
        }

        /// <summary>
        /// Gets all methods defined in this type.
        /// </summary>
        public override ReadOnlyCollection<XMethodDefinition> Methods
        {
            get { return methods.AsReadOnly(); }
        }

        /// <summary>
        /// Gets a field by it's underlying method object.
        /// </summary>
        public override XMethodDefinition GetByOriginalMethod(object originalMethod)
        {
            return null;
        }

        /// <summary>
        /// Add the given generated method to this type.
        /// </summary>
        internal override void Add(XSyntheticMethodDefinition method)
        {
            methods.Add(method);
            Reset();
        }

        /// <summary>
        /// Add the given generated field to this type.
        /// </summary>
        internal override void Add(XSyntheticFieldDefinition field)
        {
            fields.Add(field);
            Reset();
        }

        /// <summary>
        /// Add the given generated nestedt type to this type.
        /// </summary>
        internal override void Add(XSyntheticTypeDefinition nestedType)
        {
            nestedTypes.Add(nestedType);
            Reset();
        }

        /// <summary>
        /// Gets all types defined in this type.
        /// </summary>
        public override ReadOnlyCollection<XTypeDefinition> NestedTypes
        {
            get { return nestedTypes.AsReadOnly(); }
        }

        /// <summary>
        /// Gets all interfaces this type implements.
        /// </summary>
        public override ReadOnlyCollection<XTypeReference> Interfaces
        {
            get { return interfaces.AsReadOnly(); }
        }

        /// <summary>
        /// Is this an interface
        /// </summary>
        public override bool IsInterface
        {
            get { return flags.HasFlag(XSyntheticTypeFlags.Interface); }
        }

        /// <summary>
        /// Is this an enum type?
        /// </summary>
        public override bool IsEnum
        {
            get { return false; }
        }

        public override bool IsStruct
        {
            get { return false; }
        }

        public override bool IsImmutableStruct { get { return false; } }

        /// <summary>
        /// Gets the type of the enum value field.
        /// </summary>
        public override XTypeReference GetEnumUnderlyingType()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Is this class abstract?
        /// </summary>
        public override bool IsAbstract
        {
            get { return flags.HasFlag(XSyntheticTypeFlags.Abstract); }
        }

        /// <summary>
        /// Is this class sealed/final (cannot be extended)?
        /// </summary>
        public override bool IsSealed
        {
            get { return flags.HasFlag(XSyntheticTypeFlags.Sealed); }
        }

        /// <summary>
        /// Is this type reachable?
        /// </summary>
        public override bool IsReachable { get { return true; } }

        /// <summary>
        /// our unique id, constant accross builds.
        /// </summary>
        public override string ScopeId { get { return _scopeId; } }

        /// <summary>
        /// Is there a DexImport attribute on this type?
        /// </summary>
        public override bool HasDexImportAttribute()
        {
            return false;
        }

        /// <summary>
        /// Is there a CustomView attribute on this type?
        /// </summary>
        public override bool HasCustomViewAttribute()
        {
            return false;
        }

        /// <summary>
        /// Try to get the classname from the DexImport attribute attached to this method.
        /// </summary>
        public override bool TryGetDexImportNames(out string className)
        {
            className = null;
            return false;
        }

        /// <summary>
        /// Try to get the classname from the JavaImport attribute attached to this method.
        /// </summary>
        public override bool TryGetJavaImportNames(out string className)
        {
            className = null;
            return false;
        }

        /// <summary>
        /// Try to get the enum field that defines the given constant.
        /// </summary>
        public override bool TryGetEnumConstField(object value, out XFieldDefinition field)
        {
            field = null;
            return false;
        }

        /// <summary>
        /// Is this a private type?
        /// </summary>
        public bool IsPrivate
        {
            get { return flags.HasFlag(XSyntheticFieldFlags.Private); }
        }

        /// <summary>
        /// Is this a protected type?
        /// </summary>
        public bool IsProtected
        {
            get { return flags.HasFlag(XSyntheticFieldFlags.Protected); }
        }

        /// <summary>
        /// Get relation to dex class.
        /// </summary>
        public DexLib.ClassDefinition DexClass
        {
            get { return dexType; }
        }

        /// <summary>
        /// Set relation to dex class.
        /// </summary>
        internal void SetDexClass(DexLib.ClassDefinition @class, DexTargetPackage targetPackage)
        {
            if (@class == null)
                throw new ArgumentNullException("@class");
            if (dexType != null)
                throw new InvalidOperationException("Cannot set dex class twice");
            dexType = @class;
        }
    }
}
