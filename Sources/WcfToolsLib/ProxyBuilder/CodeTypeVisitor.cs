using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dot42.Cecil;
using Mono.Cecil;

namespace Dot42.WcfTools.ProxyBuilder
{
    class CodeTypeVisitor : ITypeVisitor<CodeTypeReference, object>
    {
        public static readonly CodeTypeVisitor Instance = new CodeTypeVisitor();
        public CodeTypeReference Visit(TypeDefinition type, object data)
        {
            return new CodeTypeReference(type.FullName);
        }

        public CodeTypeReference Visit(TypeReference type, object data)
        {
            return new CodeTypeReference(type.FullName);
        }

        public CodeTypeReference Visit(GenericParameter type, object data)
        {
            throw new System.NotImplementedException();
        }

        public CodeTypeReference Visit(GenericInstanceType type, object data)
        {
            throw new System.NotImplementedException();
        }

        public CodeTypeReference Visit(ArrayType type, object data)
        {
            return new CodeTypeReference(type.ElementType.Accept(this, data), type.Rank);
        }

        public CodeTypeReference Visit(ByReferenceType type, object data)
        {
            throw new System.NotImplementedException();
        }

        public CodeTypeReference Visit(FunctionPointerType type, object data)
        {
            throw new System.NotImplementedException();
        }

        public CodeTypeReference Visit(OptionalModifierType type, object data)
        {
            throw new System.NotImplementedException();
        }

        public CodeTypeReference Visit(RequiredModifierType type, object data)
        {
            throw new System.NotImplementedException();
        }

        public CodeTypeReference Visit(PinnedType type, object data)
        {
            throw new System.NotImplementedException();
        }

        public CodeTypeReference Visit(PointerType type, object data)
        {
            throw new System.NotImplementedException();
        }

        public CodeTypeReference Visit(SentinelType type, object data)
        {
            throw new System.NotImplementedException();
        }
    }
}
