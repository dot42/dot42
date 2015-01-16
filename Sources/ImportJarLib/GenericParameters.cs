using System;
using System.Diagnostics;
using System.Linq;
using Dot42.ImportJarLib.Model;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Helpers used to resolve generic parameters
    /// </summary>
    internal static class GenericParameters
    {
        /// <summary>
        /// Resolve all generic parameters in the given type in the given context.
        /// </summary>
        internal static NetTypeReference Resolve(NetTypeReference type, INetGenericParameterProvider context, TypeNameMap typeNameMap)
        {
            if (type == null)
                return null;
            NetTypeReference result;
            if (!TryResolve(type, context, typeNameMap, out result))
                throw new ArgumentException(string.Format("Cannot resolve {0} in context of {1}", type, context));
            return result;
        }

        /// <summary>
        /// Resolve all generic parameters in the given type in the given context.
        /// </summary>
        internal static bool TryResolve(NetTypeReference type, INetGenericParameterProvider context, TypeNameMap typeNameMap, out NetTypeReference result)
        {
            result = null;
            if (type == null)
                return false;
            if (context == null)
                throw new ArgumentNullException("context");
            result = type.Accept(new Resolver(context, typeNameMap), 0);
            return (result != null);
        }

        /// <summary>
        /// Visitor that implements the actual resolve function.
        /// </summary>
        [DebuggerDisplay("context={@context}")]
        private sealed class Resolver : INetTypeVisitor<NetTypeReference, int>
        {
            private readonly INetGenericParameterProvider context;
            private readonly TypeNameMap typeNameMap;

            /// <summary>
            /// Default ctor
            /// </summary>
            public Resolver(INetGenericParameterProvider context, TypeNameMap typeNameMap)
            {
                this.context = context;
                this.typeNameMap = typeNameMap;
            }

            /// <summary>
            /// Resolve a type definition.
            /// </summary>
            public NetTypeReference Visit(NetTypeDefinition type, int data)
            {
                return type;
            }

            /// <summary>
            /// Resolve a generic parameter.
            /// </summary>
            public NetTypeReference Visit(NetGenericParameter item, int data)
            {
                if ((item.Owner.IsMethod) || (item.Owner == context))
                {
                    // We cannot fix it
                    return item;
                }

                var contextType = this.context.IsMethod ? ((NetMethodDefinition)this.context).DeclaringType : (NetTypeDefinition)this.context;

                // See if the owner is a direct basetype of the context.
                foreach (var baseType in contextType.GetBaseTypes(false).OfType<NetGenericInstanceType>())
                {
                    if (baseType.GetElementType() != item.Owner)
                        continue;

                    if (data > 20)
                    {
                        
                    }

                    // Generic parameter is owned by baseType.
                    // Replace generic parameter with generic argument at the right position
                    var result = baseType.GenericArgument(item.Position);
                    if (result == item)
                        return item;
                    return result.Accept(this, data + 1);
                }

                // See if the owner is a basetype of the context's base type.
                foreach (var baseType in contextType.GetBaseTypes(false))
                {
                    NetTypeReference resolved;
                    if (TryResolve(item, baseType.GetElementType(), typeNameMap, out resolved))
                    {
                        if (resolved == item)
                            return null/*resolved*/;
                        return resolved.Accept(this, data);
                    }
                }

                // See if the owner is a declaring type of the context's type.
                foreach (var declaringType in contextType.GetDeclaringTypes())
                {
                    if (declaringType == item.Owner)
                        return item;
                }

                return null;
            }

            /// <summary>
            /// Resolve a generic instance
            /// </summary>
            public NetTypeReference Visit(NetGenericInstanceType item, int data)
            {
                var declaringType = (item.DeclaringType != null) ? item.DeclaringType.Accept(this, data) : null;
                var result = new NetGenericInstanceType((NetTypeDefinition) item.ElementType.Accept(this, data), declaringType);
                foreach (var arg in item.GenericArguments)
                {
                    result.AddGenericArgument(arg.Accept(this, data), typeNameMap);
                }
                return result;
            }

            /// <summary>
            /// Resolve an array type.
            /// </summary>
            public NetTypeReference Visit(NetArrayType item, int data)
            {
                return new NetArrayType(item.ElementType.Accept(this, data));
            }

            /// <summary>
            /// Resolve an unboxed type.
            /// </summary>
            public NetTypeReference Visit(NetNullableType item, int data)
            {
                return new NetNullableType(item.ElementType.Accept(this, data));
            }
        }
    }
}
