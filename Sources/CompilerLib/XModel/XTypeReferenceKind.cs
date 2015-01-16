namespace Dot42.CompilerLib.XModel
{
    public enum XTypeReferenceKind
    {
        Bool, 
        Byte, 
        SByte, 
        Char, 
        Short, 
        UShort, 
        Int, 
        UInt, 
        Long, 
        ULong, 
        Float, 
        Double, 
        Void,
        IntPtr,
        UIntPtr,
        TypedReference,

        TypeReference,
        TypeDefinition,
        ArrayType,
        GenericParameter,
        GenericInstanceType,
        ByReferenceType,
        PointerType,
        OptionalModifierType,
        RequiredModifierType,
    }
}
