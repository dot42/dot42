namespace Dot42.DexLib
{
    public interface IMemberDefinition : IMemberReference, IAnnotationProvider
    {
        AccessFlags AccessFlags { get; set; }
        ClassDefinition Owner { get; set; }
    }
}