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

        public const string GenericTypeDefinitionMarker = "IGenericTypeDefinition";

        /// <summary>
        /// Name of IGenericInstanceClass class
        /// Interface for annotation that stores the field of a generic type that holds the type arguments.
        /// </summary>
        public const string TypeReflectionInfoAnnotation = "ITypeReflectionInfo";
        
        /// <summary>
        /// Name of field in the IGenericInstanceClass class that holds type arguments.
        /// </summary>
        public const string TypeReflectionInfoGenericArgumentsFields = "GenericArgumentsFields";

        /// <summary>
        /// Name of field in the IGenericInstanceClass class that holds type arguments.
        /// </summary>
        public const string TypeReflectionInfoGenericArgumentCountField = "GenericArgumentCount";

        public const string TypeReflectionInfoGenericDefinitionsField = "GenericDefinitions";
        public const string TypeReflectionInfoFieldsField = "Fields";

        public const string GenericDefinitionAnnotation = "IGenericDefinition";

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

        /// <summary>
        /// Annotation that preserves reflection information that would be lost in Java.
        /// </summary>
        public const string ReflectionInfoAnnotation = "IReflectionInfo";
        public const string ReflectionInfoAccessFlagsField = "AccessFlags";
        public const string ReflectionInfoParameterNamesField = "ParameterNames";

        /// <summary>
        /// If the number of generic parameters is larger than this value, the
        /// compiler will emit an array for the parameters. The choosen value is
        /// rather arbitrary. Maybe six or event eight would be better. It makes 
        /// sense to have some threshold.
        /// </summary>
        public const int GenericTypeParametersAsArrayThreshold = 4;
        /// <summary>
        /// If the number of generic parameters is larger than this value, the
        /// compiler will emit an array for the parameters. There does not seem
        /// to be any benefit to pack and unpack generic method parameters. The
        /// selected value disables array generation for generic method parameters.
        /// </summary>
        public const int GenericMethodParametersAsArrayThreshold = int.MaxValue - 1 ; // -1 to fool the once again too smart resharper emitting warnings.
    }
}
