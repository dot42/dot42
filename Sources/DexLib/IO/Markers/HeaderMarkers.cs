namespace Dot42.DexLib.IO.Markers
{
    internal class HeaderMarkers
    {
        public UIntMarker CheckSumMarker { get; set; }
        public UIntMarker FileSizeMarker { get; set; }
        public SignatureMarker SignatureMarker { get; set; }
        public SizeOffsetMarker LinkMarker { get; set; }
        public UIntMarker MapMarker { get; set; }
        public SizeOffsetMarker StringsMarker { get; set; }
        public SizeOffsetMarker TypeReferencesMarker { get; set; }
        public SizeOffsetMarker PrototypesMarker { get; set; }
        public SizeOffsetMarker FieldReferencesMarker { get; set; }
        public SizeOffsetMarker MethodReferencesMarker { get; set; }
        public SizeOffsetMarker ClassDefinitionsMarker { get; set; }
        public SizeOffsetMarker DataMarker { get; set; }
    }
}