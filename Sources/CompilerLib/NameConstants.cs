namespace Dot42.CompilerLib
{
    /// <summary>
    /// Name constants
    /// </summary>
    public static class NameConstants
    {
        public static class Enum
        {
            internal const string InfoFieldName = "info$";
            internal const string ValueFieldName = "value__";
            internal const string DefaultFieldName = "default$";

            internal const string UnboxMethodName = "unbox";
            internal const string ValuesMethodName = "values";
            internal const string ValueOfMethodName = "valueOf";
        }

        public static class Struct
        {
            internal const string CopyFromMethodName = "$CopyFrom";
            internal const string CloneMethodName = "$Clone";
            internal const string DefaultFieldName = "default$";
        }

        public static class Atomic
        {
            internal const string FieldUpdaterPostfix = "$atomicUpdater";
        }
    }
}
