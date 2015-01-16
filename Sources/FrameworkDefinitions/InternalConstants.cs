namespace Dot42.FrameworkDefinitions
{
    public static class InternalConstants
    {
        /// <summary>
        /// Namespace of all internal classes
        /// </summary>
        public const string Dot42InternalNamespace = "Dot42.Internal";

        /// <summary>
        /// Name of CompilerHelper class
        /// </summary>
        public const string CompilerHelperName = "CompilerHelper";

        /// <summary>
        /// Name of TypeHelper class
        /// </summary>
        public const string TypeHelperName = "TypeHelper";

        /// <summary>
        /// Name of IGenericInstanceClass class
        /// Interface for annotation that stores the field of a generic type that holds the type arguments.
        /// </summary>
        public const string GenericInstanceClassAnnotation = "IGenericInstanceClass";

        /// <summary>
        /// Name of field in the IGenericInstanceClass class that holds type arguments.
        /// </summary>
        public const string GenericInstanceClassArgumentsField = "Arguments";

        /// <summary>
        /// Name of IGenericMethodParameter class
        /// Interface for annotation that indicates a method parameter as the generic type information for the method.
        /// </summary>
        public const string GenericMethodParameterAnnotation = "IGenericMethodParameter";

        /// <summary>
        /// Name of IGenericTypeParameter class
        /// Interface for annotation that indicates a method parameter as the generic type information for the class.
        /// </summary>
        public const string GenericTypeParameterAnnotation = "IGenericTypeParameter";
    }
}
