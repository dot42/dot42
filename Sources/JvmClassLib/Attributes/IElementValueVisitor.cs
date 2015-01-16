namespace Dot42.JvmClassLib.Attributes
{
    public interface IElementValueVisitor<TReturn, TData>
    {
        TReturn Visit(AnnotationElementValue value, TData data);
        TReturn Visit(ArrayElementValue value, TData data);
        TReturn Visit(ClassElementValue value, TData data);
        TReturn Visit(ConstElementValue value, TData data);
        TReturn Visit(EnumConstElementValue value, TData data);
    }
}
