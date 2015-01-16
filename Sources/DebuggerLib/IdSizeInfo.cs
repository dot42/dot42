namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Size of various ID's.
    /// </summary>
    public sealed class IdSizeInfo
    {
        public readonly int FieldIdSize;
        public readonly int MethodIdSize;
        public readonly int ObjectIdSize;
        public readonly int ReferenceTypeIdSize;
        public readonly int FrameIdSize;

        public IdSizeInfo(int fieldIdSize, int methodIdSize, int objectIdSize, int referenceTypeIdSize, int frameIdSize)
        {
            FieldIdSize = fieldIdSize;
            MethodIdSize = methodIdSize;
            ObjectIdSize = objectIdSize;
            ReferenceTypeIdSize = referenceTypeIdSize;
            FrameIdSize = frameIdSize;
        }
    }
}
