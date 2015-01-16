namespace Dot42.ImportJarLib.Model
{
    public interface INetTypeVisitor<T, TData>
    {
        T Visit(NetTypeDefinition type, TData data);
        T Visit(NetGenericParameter item, TData data);
        T Visit(NetGenericInstanceType item, TData data);
        T Visit(NetArrayType item, TData data);
        T Visit(NetNullableType item, TData data);
    }
}
