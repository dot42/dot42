using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Reference to a type
    /// </summary>
    public abstract class XTypeReference : XReference, IXGenericParameterProvider
    {
        private readonly bool isValueType;
        private readonly ReadOnlyCollection<XGenericParameter> genericParameters;
        private string fullNameCache;
        private XTypeDefinition resolvedType;
        private XTypeReference withoutModifiers;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected XTypeReference(XModule module, bool isValueType, IEnumerable<string> genericParameterNames)
            : base(module/*, declaringType*/)
        {
            this.isValueType = isValueType;
            genericParameters = (genericParameterNames ?? Enumerable.Empty<string>()).Select((x, i) => new XGenericParameter.SimpleXGenericParameter(this, i)).Cast<XGenericParameter>().ToList().AsReadOnly();
        }

        /// <summary>
        /// Is this a struct?
        /// </summary>
        public bool IsValueType { get { return isValueType; } }

        /// <summary>
        /// Is this a generic instance?
        /// </summary>
        public virtual bool IsGenericInstance
        {
            get { return false; }
        }

        /// <summary>
        /// Is this a generic parameter?
        /// </summary>
        public virtual bool IsGenericParameter
        {
            get { return false; }
        }

        /// <summary>
        /// Is this an array type?
        /// </summary>
        public virtual bool IsArray
        {
            get { return false; }
        }

        /// <summary>
        /// Is this a primitive type?
        /// </summary>
        public virtual bool IsPrimitive
        {
            get { return false; }
        }

        /// <summary>
        /// Is this an byref type?
        /// </summary>
        public virtual bool IsByReference
        {
            get { return false; }
        }

        /// <summary>
        /// Is this a type defition?
        /// </summary>
        public virtual bool IsDefinition
        {
            get { return false; }
        }

        /// <summary>
        /// What kind of reference is this?
        /// </summary>
        public virtual XTypeReferenceKind Kind { get { return XTypeReferenceKind.TypeReference; } }

        /// <summary>
        /// Gets all generic parameters
        /// </summary>
        public ReadOnlyCollection<XGenericParameter> GenericParameters { get { return genericParameters; } }

        /// <summary>
        /// Is this provider the same as the given other provider?
        /// </summary>
        bool IXGenericParameterProvider.IsSame(IXGenericParameterProvider other)
        {
            var otherType = other as XTypeReference;
            return (otherType != null) && (FullName == otherType.FullName); // Do not use IsSame because of endless recursion
        }

        /// <summary>
        /// Gets the type without array/generic modifiers
        /// </summary>
        public virtual XTypeReference ElementType { get { return this; } }

        /// <summary>
        /// Gets the deepest <see cref="ElementType"/>.
        /// </summary>
        public virtual XTypeReference GetElementType()
        {
            return this;
        }

        /// <summary>
        /// get the ElementType stripped of Required- or OptionalModifierTypes
        /// (is this correct to do here?)
        /// </summary>
        /// <returns></returns>
        public virtual XTypeReference GetWithoutModifiers()
        {
            if (withoutModifiers != null) 
                return withoutModifiers;

            XTypeReference ret = this;
            while (ret is XRequiredModifierType || ret is XOptionalModifierType)
            {
                ret = ret.ElementType;
            }
            withoutModifiers = ret;
            return ret;
        }

        /// <summary>
        /// Name of the member (including all)
        /// </summary>
        public sealed override string FullName
        {
            get { return fullNameCache ?? (fullNameCache = GetFullName(false)); }
        }

        /// <summary>
        /// Create a fullname of this type reference.
        /// </summary>
        public abstract string GetFullName(bool noGenerics);

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public virtual bool TryResolve(out XTypeDefinition type)
        {
            if (resolvedType != null)
            {
                type = resolvedType;
                return true;
            }
            if (!Module.TryGetType(FullName, out type))
                return false;
            // Cache for later
            resolvedType = type;
            type.AddFlushAction(() => resolvedType = null);
            return true;
        }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// Throw an exception is the resolution failed.
        /// </summary>
        public XTypeDefinition Resolve()
        {
            XTypeDefinition typeDef;
            if (TryResolve(out typeDef))
                return typeDef;
            throw new XResolutionException(this);
        }

        /// <summary>
        /// Does this type reference point to the same type as the given other reference?
        /// </summary>
        public virtual bool IsSame(XTypeReference other, bool ignoreSign = false)
        {
            return ToCompareReference().IsSameX(other.ToCompareReference(), ignoreSign);
        }

        /// <summary>
        /// Does this type reference point to the same type as the given other reference?
        /// </summary>
        private bool IsSameX(XTypeReference other, bool ignoreSign)
        {
            var myKind = Kind;
            var otherKind = other.Kind;
            switch (myKind)
            {
                case XTypeReferenceKind.TypeReference:
                case XTypeReferenceKind.TypeDefinition:
                    return ((otherKind == XTypeReferenceKind.TypeReference) ||
                            (otherKind == XTypeReferenceKind.TypeDefinition)) && (FullName == other.FullName);
                case XTypeReferenceKind.GenericParameter:
                    return (otherKind == XTypeReferenceKind.GenericParameter) &&
                           ((XGenericParameter)this).IsSame((XGenericParameter)other);
                case XTypeReferenceKind.GenericInstanceType:
                    return (otherKind == XTypeReferenceKind.GenericInstanceType) &&
                           ((XGenericInstanceType)this).IsSame((XGenericInstanceType)other);
                case XTypeReferenceKind.ArrayType:
                case XTypeReferenceKind.ByReferenceType:
                case XTypeReferenceKind.PointerType:
                case XTypeReferenceKind.OptionalModifierType:
                case XTypeReferenceKind.RequiredModifierType:
                    return (otherKind == myKind) && ElementType.IsSame(other.ElementType, ignoreSign);
                case XTypeReferenceKind.Bool:
                case XTypeReferenceKind.Byte:
                case XTypeReferenceKind.SByte:
                case XTypeReferenceKind.Char:
                case XTypeReferenceKind.Short:
                case XTypeReferenceKind.UShort:
                case XTypeReferenceKind.Int:
                case XTypeReferenceKind.UInt:
                case XTypeReferenceKind.Long:
                case XTypeReferenceKind.ULong:
                case XTypeReferenceKind.Float:
                case XTypeReferenceKind.Double:
                case XTypeReferenceKind.Void:
                case XTypeReferenceKind.IntPtr:
                case XTypeReferenceKind.UIntPtr:
                    if (ignoreSign)
                    {
                        otherKind = NormalizeSigned(otherKind);
                        myKind = NormalizeSigned(myKind);
                    }
                    return (otherKind == myKind);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Change unsigned types to signed types as they are used on Android.
        /// </summary>
        private static XTypeReferenceKind NormalizeSigned(XTypeReferenceKind kind)
        {
            switch (kind)
            {
                case XTypeReferenceKind.Byte:
                    return XTypeReferenceKind.SByte;
                case XTypeReferenceKind.UShort:
                    return XTypeReferenceKind.Short;
                case XTypeReferenceKind.UInt:
                    return XTypeReferenceKind.Int;
                case XTypeReferenceKind.ULong:
                    return XTypeReferenceKind.Long;
                case XTypeReferenceKind.UIntPtr:
                    return XTypeReferenceKind.IntPtr;
                default:
                    return kind;
            }
        }

        /// <summary>
        /// Gets the reference to compare in <see cref="IsSame"/>.
        /// </summary>
        protected virtual XTypeReference ToCompareReference()
        {
            return this;
        }

        /// <summary>
        /// Create a method reference for the given method using this this as declaring type.
        /// Usually the method will be returned, unless this type is a generic instance type.
        /// </summary>
        public virtual XMethodReference CreateReference(XMethodDefinition method)
        {
            return method;
        }

        /// <summary>
        /// Flush all cached members
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
            fullNameCache = null;
        }

        /// <summary>
        /// Extension of XTypeReference that has a namespace.
        /// </summary>
        public abstract class XTypeReferenceWithNamespace : XTypeReference, IXMemberReference 
        {
            private readonly XTypeReference declaringType;

            /// <summary>
            /// Default ctor
            /// </summary>
            protected XTypeReferenceWithNamespace(XModule module, XTypeReference declaringType, bool isValueType, IEnumerable<string> genericParameterNames) :
                base(module, isValueType, genericParameterNames)
            {
                this.declaringType = declaringType;
            }


            /// <summary>
            /// Gets the namespace of this type.
            /// Returns the namespace of the declaring type for nested types.
            /// </summary>
            public abstract string Namespace { get; }

            /// <summary>
            /// Gets the type that contains this member
            /// </summary>
            public XTypeReference DeclaringType { get { return declaringType; } }

            /// <summary>
            /// Is this a nested type?
            /// </summary>
            public bool IsNested { get { return (DeclaringType != null); } }

            /// <summary>
            /// Create a fullname of this type reference.
            /// </summary>
            public override string GetFullName(bool noGenerics)
            {
                var generics = "";
                if (!noGenerics)
                {
                    var genericInstance = this as IXGenericInstance;
                    if (genericInstance != null)
                    {
                        generics = "<" + string.Join(", ", genericInstance.GenericArguments.Select(x => x.Name)) + ">";
                    }
                }
                /*else if (GenericParameters.Any())
                {
                    generics = "<" + string.Join(", ", GenericParameters.Select(x => x.Name)) + ">";
                }*/
                if (IsNested)
                {
                    return DeclaringType.GetFullName(noGenerics) + "." + Name + generics;
                }
                return (string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name) + generics;
            }
        }

        /// <summary>
        /// Simple implementation
        /// </summary>
        public sealed class SimpleXTypeReference : XTypeReferenceWithNamespace 
        {
            private readonly string ns;
            private readonly string name;

            /// <summary>
            /// Default ctor
            /// </summary>
            public SimpleXTypeReference(XModule module, string @namespace, string name, XTypeReference declaringType, bool isValueType,
                          IEnumerable<string> genericParameterNames)
                : base(module, declaringType, isValueType, genericParameterNames)
            {
                ns = @namespace;
                this.name = name;
            }

            /// <summary>
            /// Name of the member
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
                get { return ns; }
            }
        }
    }
}
