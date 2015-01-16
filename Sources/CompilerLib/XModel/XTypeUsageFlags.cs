namespace Dot42.CompilerLib.XModel
{
    public enum XTypeUsageFlags
    {
        /// <summary>
        /// Type will be used as field type
        /// </summary>
        FieldType,

        /// <summary>
        /// Type will be used as method return type
        /// </summary>
        ReturnType,

        /// <summary>
        /// Type will be used as declaring type
        /// </summary>
        DeclaringType,

        /// <summary>
        /// Type will be used as type of a method parameter
        /// </summary>
        ParameterType,

        /// <summary>
        /// Type will be used as type of a catch block
        /// </summary>
        CatchType,

        /// <summary>
        /// Type will be used as type as result of typeof(X)
        /// </summary>
        TypeOf,

        /// <summary>
        /// Type will be used as type of an expression result
        /// </summary>
        ExpressionType,

        /// <summary>
        /// Type will be used in a cast/instanceof 
        /// </summary>
        Cast,

        /// <summary>
        /// Type will be used as implemented interface
        /// </summary>
        Interface,

        /// <summary>
        /// Type will be used as base type
        /// </summary>
        BaseType,

        /// <summary>
        /// Type will be used as annotation type
        /// </summary>
        AnnotationType,

        /// <summary>
        /// No specific reason
        /// </summary>
        Other
    }
}
