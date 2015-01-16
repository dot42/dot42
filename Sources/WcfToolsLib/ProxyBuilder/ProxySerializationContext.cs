using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;
using Mono.Cecil;

namespace Dot42.WcfTools.ProxyBuilder
{
    internal class ProxySerializationContext
    {
        internal const string SerializationHelperTypeName = "SerializationHelper";

        private readonly Dictionary<TypeDefinition, TypeSerializerBuilder> typeSerializerBuilders = new Dictionary<TypeDefinition, TypeSerializerBuilder>();
        private readonly Dictionary<TypeDefinition, TypeDeserializerBuilder> typeDeserializerBuilders = new Dictionary<TypeDefinition, TypeDeserializerBuilder>();

        /// <summary>
        /// Gets a type serializer builder for the given type.
        /// The serializer is created when needed.
        /// </summary>
        public TypeSerializerBuilder GetSerializer(TypeDefinition type)
        {
            TypeSerializerBuilder builder;
            if (typeSerializerBuilders.TryGetValue(type, out builder))
                return builder;

            builder = new TypeSerializerBuilder(type);
            typeSerializerBuilders.Add(type, builder);
            return builder;
        }

        /// <summary>
        /// Gets a type deserializer builder for the given type.
        /// The deserializer is created when needed.
        /// </summary>
        public TypeDeserializerBuilder GetDeserializer(TypeDefinition type)
        {
            TypeDeserializerBuilder builder;
            if (typeDeserializerBuilders.TryGetValue(type, out builder))
                return builder;

            builder = new TypeDeserializerBuilder(type);
            typeDeserializerBuilders.Add(type, builder);
            return builder;
        }

        /// <summary>
        /// Create all internal structures
        /// </summary>
        public void Create(ProxySerializationContext context)
        {
            var start = 0;
            var serializeBuilders = typeSerializerBuilders.Values.Skip(start).ToArray();
            while (serializeBuilders.Any())
            {
                serializeBuilders.ForEach(x => x.Create(context));
                start = start + serializeBuilders.Length;
                serializeBuilders = typeSerializerBuilders.Values.Skip(start).ToArray();
            }

            start = 0;
            var deserializeBuilders = typeDeserializerBuilders.Values.Skip(start).ToArray();
            while (deserializeBuilders.Any())
            {
                deserializeBuilders.ForEach(x => x.Create(context));
                start = start + deserializeBuilders.Length;
                deserializeBuilders = typeDeserializerBuilders.Values.Skip(start).ToArray();
            }
        }

        /// <summary>
        /// Create the entire (de)serialization code as C# code.
        /// </summary>
        public void Generate(CodeGenerator generator)
        {
            // Prepare type
            var typeDecl = new CodeTypeDeclaration(SerializationHelperTypeName);
            typeDecl.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            typeDecl.BaseTypes.Add("Dot42.Internal.SerializationHelperBase");

            // Generate method code
            typeSerializerBuilders.Values.ForEach(x => x.Generate(typeDecl, generator));
            typeDeserializerBuilders.Values.ForEach(x => x.Generate(typeDecl, generator));

            // Add type
            generator.Add(typeDecl);
        }
    }
}
