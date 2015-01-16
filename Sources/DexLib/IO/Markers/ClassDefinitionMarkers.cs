namespace Dot42.DexLib.IO.Markers
{
    internal class ClassDefinitionMarkers
    {
        public UIntMarker InterfacesMarker { get; set; }
        public UIntMarker AnnotationsMarker { get; set; }
        public UIntMarker ClassDataMarker { get; set; }
        public UIntMarker StaticValuesMarker { get; set; }
    }
}