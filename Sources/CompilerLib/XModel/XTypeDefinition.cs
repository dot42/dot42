using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dot42.CompilerLib.XModel.Synthetic;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Definition of a type
    /// </summary>
    public abstract class XTypeDefinition : XTypeReference.XTypeReferenceWithNamespace, IXDefinition
    {
        private readonly List<Action> flushActions = new List<Action>();
 
        /// <summary>
        /// Default ctor
        /// </summary>
        protected XTypeDefinition(XModule module, XTypeDefinition declaringType, bool isValueType, IEnumerable<string> genericParameterNames)
            : base(module, declaringType, isValueType, genericParameterNames)
        {
        }

        /// <summary>
        /// Gets the underlying type object.
        /// </summary>
        public abstract object OriginalTypeDefinition { get; }

        /// <summary>
        /// Gets the type that contains this member
        /// </summary>
        public new XTypeDefinition DeclaringType
        {
            get { return (XTypeDefinition)base.DeclaringType; }
        }

        /// <summary>
        /// Sort order priority.
        /// Low values come first
        /// </summary>
        public abstract int Priority { get; }

        /// <summary>
        /// Gets the type this type extends (null if System.Object)
        /// </summary>
        public abstract XTypeReference BaseType { get; }

        /// <summary>
        /// Is this a type defition?
        /// </summary>
        public override bool IsDefinition { get { return true; } }

        /// <summary>
        /// Is this type a type that needs it's generic types implemented at runtime?
        /// </summary>
        public abstract bool IsGenericClass { get; }

        /// <summary>
        /// Gets all fields defined in this type.
        /// </summary>
        public abstract ReadOnlyCollection<XFieldDefinition> Fields { get; }

        /// <summary>
        /// Gets a field by it's underlying field object.
        /// </summary>
        public abstract XFieldDefinition GetByOriginalField(object originalField);

        /// <summary>
        /// Gets all methods defined in this type.
        /// </summary>
        public abstract ReadOnlyCollection<XMethodDefinition> Methods { get; }

        /// <summary>
        /// Gets a field by it's underlying method object.
        /// </summary>
        public abstract XMethodDefinition GetByOriginalMethod(object originalMethod);

        /// <summary>
        /// Add the given generated method to this type.
        /// </summary>
        internal abstract void Add(XSyntheticMethodDefinition method);

        /// <summary>
        /// Add the given generated field to this type.
        /// </summary>
        internal abstract void Add(XSyntheticFieldDefinition field);

        /// <summary>
        /// Add the given generated nestedt type to this type.
        /// </summary>
        internal abstract void Add(XSyntheticTypeDefinition nestedType);

        /// <summary>
        /// Gets all types defined in this type.
        /// </summary>
        public abstract ReadOnlyCollection<XTypeDefinition> NestedTypes { get; }

        /// <summary>
        /// Gets all interfaces this type implements.
        /// </summary>
        public abstract ReadOnlyCollection<XTypeReference> Interfaces { get; }

        /// <summary>
        /// Is this an interface
        /// </summary>
        public abstract bool IsInterface { get; }

        /// <summary>
        /// Is this an enum type?
        /// </summary>
        public abstract bool IsEnum { get; }

        /// <summary>
        /// Is this type a struct (non-primitive, non-enum, non-nullableT value type)?
        /// </summary>
        public abstract bool IsStruct { get; }

        /// <summary>
        /// Is this type a struct, and is it to be treated as immutable?
        /// TODO: think about storing this information somewhere else.
        /// </summary>
        public abstract bool IsImmutableStruct { get; }

        /// <summary>
        /// Gets the type of the enum value field.
        /// </summary>
        public abstract XTypeReference GetEnumUnderlyingType();

        /// <summary>
        /// Is this class abstract?
        /// </summary>
        public abstract bool IsAbstract { get; }

        /// <summary>
        /// Is this class sealed/final (cannot be extended)?
        /// </summary>
        public abstract bool IsSealed { get; }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public override bool TryResolve(out XTypeDefinition type)
        {
            type = this;
            return true;
        }

        /// <summary>
        /// Try to get a type definition (me or one of my nested typed) by the given full name.
        /// </summary>
        public virtual bool TryGet(string fullName, bool noImports, out XTypeDefinition type)
        {
            if (FullName == fullName)
            {
                type = this;
                return true;
            }
            foreach (var nested in NestedTypes)
            {
                if (nested.TryGet(fullName, noImports, out type))
                    return true;
            }
            type = null;
            return false;
        }

        /// <summary>
        /// Try to get a method with given reference in this type.
        /// The declaring type of the method reference is assumed to refer to this type.
        /// </summary>
        public bool TryGet(XMethodReference methodRef, out XMethodDefinition method)
        {
            // Look in my own methods
            method = Methods.FirstOrDefault(x => x.IsSameExceptDeclaringType(methodRef));
            if (method != null)
                return true;

            // Look in base type
            XTypeDefinition baseTypeDef;
            if ((BaseType != null) && BaseType.TryResolve(out baseTypeDef))
            {
                return baseTypeDef.TryGet(methodRef, out method);
            }
            return false;
        }

        /// <summary>
        /// Try to get a field with given reference in this type.
        /// The declaring type of the method reference is assumed to refer to this type.
        /// </summary>
        public bool TryGet(XFieldReference fieldRef, out XFieldDefinition field)
        {
            // Look in my own fields
            field = Fields.FirstOrDefault(x => x.IsSameExceptDeclaringType(fieldRef));
            if (field != null)
                return true;

            // Look in base type
            XTypeDefinition baseTypeDef;
            if ((BaseType != null) && BaseType.TryResolve(out baseTypeDef))
            {
                return baseTypeDef.TryGet(fieldRef, out field);
            }
            return false;
        }

        /// <summary>
        /// Gets the type without array/generic modifiers
        /// </summary>
        public override XTypeReference ElementType
        {
            get { return this; }
        }

        /// <summary>
        /// Is this type reachable?
        /// </summary>
        public abstract bool IsReachable { get; }

        /// <summary>
        /// Gets the deepest <see cref="ElementType"/>.
        /// </summary>
        public override XTypeReference GetElementType()
        {
            return this;
        }

        /// <summary>
        /// Is there a DexImport attribute on this type?
        /// </summary>
        public abstract bool HasDexImportAttribute();

        /// <summary>
        /// Is there a CustomView attribute on this type?
        /// </summary>
        public abstract bool HasCustomViewAttribute();

        /// <summary>
        /// Try to get the classname from the DexImport attribute attached to this method.
        /// </summary>
        public abstract bool TryGetDexImportNames(out string className);

        /// <summary>
        /// Try to get the classname from the JavaImport attribute attached to this method.
        /// </summary>
        public abstract bool TryGetJavaImportNames(out string className);

        /// <summary>
        /// Try to get the enum field that defines the given constant.
        /// </summary>
        public abstract bool TryGetEnumConstField(object value, out XFieldDefinition field);

        /// <summary>
        /// Add an action that is called on a reset.
        /// </summary>
        internal void AddFlushAction(Action action)
        {
            flushActions.Add(action);
        }

        /// <summary>
        /// Flush all cached members
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
            while (flushActions.Count > 0)
            {
                var first = flushActions[0];
                first();
                flushActions.Remove(first);
            }
        }
    }
}
