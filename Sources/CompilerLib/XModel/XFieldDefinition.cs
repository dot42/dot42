namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Definition of a field
    /// </summary>
    public abstract class XFieldDefinition : XFieldReference, IXDefinition
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        protected XFieldDefinition(XTypeDefinition declaringType)
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
        /// Is this a static field?
        /// </summary>
        public abstract bool IsStatic { get; }

        /// <summary>
        /// Is this field used in code?
        /// </summary>
        public abstract bool IsReachable { get; }

        /// <summary>
        /// Is this a readonly field?
        /// </summary>
        public abstract bool IsReadOnly { get; }

        /// <summary>
        /// Does this field have a name with a special meaning for the runtime?
        /// </summary>
        public abstract bool IsRuntimeSpecialName { get; }

        /// <summary>
        /// Gets the initial value (if any) of this field.
        /// </summary>
        public abstract object InitialValue { get; }

        /// <summary>
        /// Try to get the value of the enum const if this field is an enum const field.
        /// </summary>
        public abstract bool TryGetEnumValue(out object value);

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public override bool TryResolve(out XFieldDefinition field)
        {
            string fieldName;
            string descriptor;
            string className;
            if (TryGetJavaImportNames(out fieldName, out descriptor, out className))
            {
                // Resolve to java field definition
                var declaringType = Java.XBuilder.AsTypeReference(Module, className, XTypeUsageFlags.DeclaringType);
                var fieldRef = Java.XBuilder.AsFieldReference(Module, fieldName, descriptor, declaringType, className);
                return fieldRef.TryResolve(out field);
            }
            field = this;
            return true;
        }

        /// <summary>
        /// Try to get the names from the DexImport attribute attached to this field.
        /// </summary>
        public abstract bool TryGetDexImportNames(out string fieldName, out string descriptor, out string className);

        /// <summary>
        /// Try to get the names from the JavaImport attribute attached to this field.
        /// </summary>
        public abstract bool TryGetJavaImportNames(out string fieldName, out string descriptor, out string className);

        /// <summary>
        /// Try to get the name of the ResourceId attribute attached to this field.
        /// </summary>
        public abstract bool TryGetResourceIdAttribute(out string resourceName);
    }
}
