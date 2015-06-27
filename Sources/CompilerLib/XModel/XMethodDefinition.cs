using System.Linq;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Definition of a method
    /// </summary>
    public abstract class XMethodDefinition : XMethodReference, IXDefinition
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        protected XMethodDefinition(XTypeDefinition declaringType)
            : base(declaringType)
        {
        }

        /// <summary>
        /// Gets the type that contains this member
        /// </summary>
        public new XTypeDefinition DeclaringType
        {
            get { return (XTypeDefinition)base.DeclaringType; }
        }

        /// <summary>
        /// Does this method have the given name (or does the original method have this name).
        /// </summary>
        public abstract bool EqualsName(string name);

        /// <summary>
        /// Is this method abstract?
        /// </summary>
        public abstract bool IsAbstract { get; }

        /// <summary>
        /// Is this a virtual method?
        /// </summary>
        public abstract bool IsVirtual { get; }

        /// <summary>
        /// Is this a static method?
        /// </summary>
        public abstract bool IsStatic { get; }

        /// <summary>
        /// Is this an instance or class constructor?
        /// </summary>
        public abstract bool IsConstructor { get; }

        /// <summary>
        /// Is this a property get method?
        /// </summary>
        public abstract bool IsGetter { get; }

        /// <summary>
        /// Is this a property set method?
        /// </summary>
        public abstract bool IsSetter { get; }

        /// <summary>
        /// Is this an android extension method?
        /// </summary>
        public abstract bool IsAndroidExtension { get; }

        /// <summary>
        /// Should this method be called with an invoke_direct in dex?
        /// </summary>
        public abstract bool IsDirect { get; }

        /// <summary>
        /// Returns a scope id, that is guaranteed to be
        /// <para> - unique for all methods declared in this type</para><para>
        ///        - constant accross builds, if the underlying 
        ///          definition has not changed.</para>
        /// </summary>
        public abstract string ScopeId { get; }

        /// <summary>
        /// Gets the "base" method of the given method or null if there is no such method.
        /// </summary>
        public virtual XMethodDefinition GetBaseMethod(bool ignoreVirtual)
        {
            if (!IsVirtual && !ignoreVirtual)
                return this;

            var baseType = DeclaringType.BaseType;
            while (baseType != null)
            {
                XTypeDefinition baseTypeDef;
                if (!baseType.TryResolve(out baseTypeDef))
                    break;
                var @base = baseTypeDef.Methods.FirstOrDefault(x => x.IsSameExceptDeclaringType(this));
                if (@base != null)
                    return @base;

                baseType = baseTypeDef.BaseType;
            }

            return this;
        }

        /// <summary>
        /// Should this method be called with invoke_interface?
        /// </summary>
        public abstract bool UseInvokeInterface { get; }

        /// <summary>
        /// Should invoke_super be used to call this method from the given method?
        /// </summary>
        public bool UseInvokeSuper(XMethodDefinition currentlyCompiledMethod)
        {
            if (IsConstructor)
                return false;
            if (this == currentlyCompiledMethod)
                return false;

            if (!DeclaringType.IsBaseOf(currentlyCompiledMethod.DeclaringType))
                return false;

            // Check if this method is a direct base method of the currently compiled method
            var iterator = currentlyCompiledMethod;
            while (true)
            {
                var baseMethod = iterator.GetBaseMethod(true);
                if ((baseMethod == null) || (baseMethod == iterator))
                    break;
                if ((baseMethod.IsSame(this)) && (baseMethod != currentlyCompiledMethod))
                    return true;
                // Keep the order or baseMethod.IsSame(..)
                iterator = baseMethod;
            }

            // Check if this method is a direct base method of any of the methods in the declaring type of the currently compiled method.
            var sameInCurrentType = currentlyCompiledMethod.DeclaringType.Methods.FirstOrDefault(x => x.IsSameExceptDeclaringType(this));
            if (sameInCurrentType != null)
            {
                iterator = sameInCurrentType;
                while (true)
                {
                    var baseMethod = iterator.GetBaseMethod(true);
                    if ((baseMethod == null) || (baseMethod == iterator))
                        break;
                    if ((baseMethod.IsSame(this)) && (baseMethod != currentlyCompiledMethod))
                        return true;
                    // Keep the order or baseMethod.IsSame(..)
                    iterator = baseMethod;
                }                
            }

            return false;
        }

        /// <summary>
        /// Does this method need a parameter to pass the generic instance (array) for the generic types of the declaring type?
        /// </summary>
        public bool NeedsGenericInstanceTypeParameter
        {
            get
            {
                if (DeclaringType.IsGenericClass)
                    return ((Name == ".ctor") || ((IsStatic) && (Name != ".cctor")));
                return false;
            }
        }

        /// <summary>
        /// Does this method need a parameter to pass the generic instance array for the generic types of the method itself?
        /// </summary>
        public abstract bool NeedsGenericInstanceMethodParameter { get; }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public override bool TryResolve(out XMethodDefinition method)
        {
            string methodName;
            string descriptor;
            string className;
            if (TryGetJavaImportNames(out methodName, out descriptor, out className))
            {
                // Resolve to java method definition
                var declaringType = Java.XBuilder.AsTypeReference(Module, className, XTypeUsageFlags.DeclaringType);
                var methodRef = Java.XBuilder.AsMethodReference(Module, methodName, descriptor, declaringType, className, HasThis);
                return methodRef.TryResolve(out method);
            }
            method = this;
            return true;
        }

        /// <summary>
        /// Does this method have a DexNative attribute?
        /// </summary>
        public abstract bool HasDexNativeAttribute();

        /// <summary>
        /// Does this method have a DexImport attribute?
        /// </summary>
        public abstract bool HasDexImportAttribute();

        /// <summary>
        /// Try to get the names from the DexImport attribute attached to this method.
        /// </summary>
        public abstract bool TryGetDexImportNames(out string methodName, out string descriptor, out string className);

        /// <summary>
        /// Does this method have a JavaImport attribute?
        /// </summary>
        public abstract bool HasJavaImportAttribute();

        /// <summary>
        /// Try to get the names from the JavaImport attribute attached to this method.
        /// </summary>
        public abstract bool TryGetJavaImportNames(out string methodName, out string descriptor, out string className);
    }
}
