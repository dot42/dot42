namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Unit of an application
    /// </summary>
    public sealed class XTypeSystem
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal XTypeSystem(XModule module)
        {
            Bool = new XPrimitiveType(module, XTypeReferenceKind.Bool, "Boolean");
            Byte = new XPrimitiveType(module, XTypeReferenceKind.Byte, "Byte");
            SByte = new XPrimitiveType(module, XTypeReferenceKind.SByte, "SByte");
            Char = new XPrimitiveType(module, XTypeReferenceKind.Char, "Char");
            Short = new XPrimitiveType(module, XTypeReferenceKind.Short, "Int16");
            UShort = new XPrimitiveType(module, XTypeReferenceKind.UShort, "UInt16");
            Int = new XPrimitiveType(module, XTypeReferenceKind.Int, "Int32");
            UInt = new XPrimitiveType(module, XTypeReferenceKind.UInt, "UInt32");
            Long = new XPrimitiveType(module, XTypeReferenceKind.Long, "Int64");
            ULong = new XPrimitiveType(module, XTypeReferenceKind.ULong, "UInt64");
            Float = new XPrimitiveType(module, XTypeReferenceKind.Float, "Single");
            Double = new XPrimitiveType(module, XTypeReferenceKind.Double, "Double");
            Void = new XPrimitiveType(module, XTypeReferenceKind.Void, "Void");
            IntPtr = new XPrimitiveType(module, XTypeReferenceKind.IntPtr, "IntPtr");
            UIntPtr = new XPrimitiveType(module, XTypeReferenceKind.UIntPtr, "UIntPtr");
            TypedReference = new XPrimitiveType(module, XTypeReferenceKind.TypedReference, "TypedReference");

            Exception = new XTypeReference.SimpleXTypeReference(module, "System", "Exception", null, false, null);
            Object = new XTypeReference.SimpleXTypeReference(module, "System", "Object", null, false, null);
            String = new XTypeReference.SimpleXTypeReference(module, "System", "String", null, false, null);
            Type = new XTypeReference.SimpleXTypeReference(module, "System", "Type", null, false, null);

            NoType = new XTypeReference.SimpleXTypeReference(module, "____no_type___", "___no_type`99999", null, false, null);
        }

        public XTypeReference Bool { get; private set; }
        public XTypeReference Byte { get; private set; }
        public XTypeReference SByte { get; private set; }
        public XTypeReference Char { get; private set; }
        public XTypeReference Short { get; private set; }
        public XTypeReference UShort { get; private set; }
        public XTypeReference Int { get; private set; }
        public XTypeReference UInt { get; private set; }
        public XTypeReference Long { get; private set; }
        public XTypeReference ULong { get; private set; }
        public XTypeReference Float { get; private set; }
        public XTypeReference Double { get; private set; }
        public XTypeReference Void { get; private set; }
        public XTypeReference IntPtr { get; private set; }
        public XTypeReference UIntPtr { get; private set; }
        public XTypeReference TypedReference { get; private set; }

        public XTypeReference Exception { get; private set; }
        public XTypeReference Object { get; private set; }
        public XTypeReference String { get; private set; }
        public XTypeReference Type { get; private set; }

        /// <summary>
        /// Special type that is used to indicate "type not found".
        /// </summary>
        public XTypeReference NoType { get; private set; }
    }
}
