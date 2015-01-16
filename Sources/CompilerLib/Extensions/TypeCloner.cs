using System;
using System.Collections.Generic;
using Dot42.Cecil;
using Mono.Cecil;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Cecil type cloner
    /// </summary>
    internal class TypeCloner : ITypeVisitor<TypeReference, IGenericContext>
    {
        private readonly bool objectForGenericParam;
        private readonly TypeSystem typeSystem;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="objectForGenericParam">If set, generic parameters will be replaced with System.Object</param>
        public TypeCloner(bool objectForGenericParam, TypeSystem typeSystem)
        {
            this.objectForGenericParam = objectForGenericParam;
            this.typeSystem = typeSystem;
        }

        /// <summary>
        /// Get an update type reference
        /// </summary>
        public TypeReference Get(TypeReference type, IGenericContext gcontext)
        {
            if (type == null) { return null; }
            return type.Accept(this, gcontext);
        }

        public TypeReference Visit(TypeDefinition type, IGenericContext gcontext)
        {
            return type;
        }

        public TypeReference Visit(TypeReference type, IGenericContext gcontext)
        {
            var name = type.Name;
            // Type is a reference to somewhere else
            if (type.IsDefinition)
            {
                throw new ArgumentException("Type definition cannot be outside merged assembly");
            }
            TypeReference result;
            var module = type.Module;
            if (type.IsNested)
            {
                var declType = Get(type.DeclaringType, gcontext);
                result = new TypeReference(type.Namespace, type.Name, module, type.Scope) {
                                                                                              DeclaringType = declType,
                                                                                              IsValueType =
                                                                                                  type.IsValueType
                                                                                          };
            }
            else
            {
                result = new TypeReference(type.Namespace, type.Name, module, type.Scope) {
                                                                                              IsValueType =
                                                                                                  type.IsValueType
                                                                                          };
            }
            if (type.HasGenericParameters)
            {
                for (int i = 0; i < type.GenericParameters.Count; i++)
                {
                    result.GenericParameters.Add(new GenericParameter(result));
                }
            }
            //result.module = icontext.module.Object;
            result.etype = type.etype;
            return result;
        }

        public TypeReference Visit(GenericParameter type, IGenericContext gcontext)
        {
            if (objectForGenericParam)
            {
                return typeSystem.Object;
            }
            IGenericParameterProvider owner;
            switch (type.Owner.GenericParameterType)
            {
                case GenericParameterType.Method:
                    if ((gcontext != null) && (gcontext.Method != null))
                    {
                        owner = ((MethodReference)gcontext.Method).GetElementMethod();
                    }
                    else
                    {
                        throw new ArgumentException("Method expected in generic context");
                        //owner = icontext.Get((MethodReference)type.Owner, gcontext);
                    }
                    break;
                case GenericParameterType.Type:
                    if ((gcontext != null) && (gcontext.Type != null))
                    {
                        owner = ((TypeReference)gcontext.Type).GetElementType();
                    }
                    else
                    {
                        owner = Get((TypeReference)type.Owner, gcontext);
                    }
                    break;
                default:
                    throw new ArgumentException("Unknown GenericParameterType " + type.Owner.GenericParameterType);
            }
            return owner.GenericParameters[type.Position];
        }

        public TypeReference Visit(GenericInstanceType current, IGenericContext gcontext)
        {
            var elementType = Get(current.GetElementType(), gcontext);
            var result = new GenericInstanceType(elementType);
            if (current.HasGenericArguments)
            {
                foreach (var ga in current.GenericArguments)
                {
                    result.GenericArguments.Add(Get(ga, gcontext/*elementType*/));
                }
            }
            return result;
        }

        public TypeReference Visit(ArrayType current, IGenericContext gcontext)
        {
            // Clone array
            var result = new ArrayType(Get(current.ElementType, gcontext));
            for (int i = 0; i < current.Rank; i++)
            {
                var dim = current.Dimensions[i];
                if (i + 1 > result.Dimensions.Count)
                {
                    // Add it
                    result.Dimensions.Add(new ArrayDimension(dim.LowerBound, dim.UpperBound));
                }
                else
                {
                    // Set it
                    result.Dimensions[i] = new ArrayDimension(dim.LowerBound, dim.UpperBound);
                }
            }
            if (current.HasGenericParameters) { CopyAndUpdate(result, current, gcontext); }
            return result;
        }

        public TypeReference Visit(ByReferenceType current, IGenericContext gcontext)
        {
            var result = new ByReferenceType(Get(current.ElementType, gcontext));
            if (current.HasGenericParameters) { CopyAndUpdate(result, current, gcontext); }
            return result;
        }

        public TypeReference Visit(FunctionPointerType current, IGenericContext gcontext)
        {
            // TODO Implement FunctionPointer
            //if (current.HasGenericParameters) { context.CopyAndUpdate(result, current); }
            throw new NotImplementedException();
        }

        public TypeReference Visit(OptionalModifierType current, IGenericContext gcontext)
        {
            var result = new OptionalModifierType(Get(current.ModifierType, gcontext), Get(current.ElementType, gcontext));
            if (current.HasGenericParameters) { CopyAndUpdate(result, current, gcontext); }
            return result;
        }

        public TypeReference Visit(RequiredModifierType current, IGenericContext gcontext)
        {
            var result = new RequiredModifierType(Get(current.ModifierType, gcontext), Get(current.ElementType, gcontext));
            if (current.HasGenericParameters) { CopyAndUpdate(result, current, gcontext); }
            return result;
        }

        public TypeReference Visit(PinnedType current, IGenericContext gcontext)
        {
            var result = new PinnedType(Get(current.ElementType, gcontext));
            if (current.HasGenericParameters) { CopyAndUpdate(result, current, gcontext); }
            return result;
        }

        public TypeReference Visit(PointerType current, IGenericContext gcontext)
        {
            var result = new PointerType(Get(current.ElementType, gcontext));
            if (current.HasGenericParameters) { CopyAndUpdate(result, current, gcontext); }
            return result;
        }

        public TypeReference Visit(SentinelType current, IGenericContext gcontext)
        {
            var result = new SentinelType(Get(current.ElementType, gcontext));
            if (current.HasGenericParameters) { CopyAndUpdate(result, current, gcontext); }
            return result;
        }

        /// <summary>
        /// Create a clone of the given source generic parameters into the given target.
        /// All types in it are updated using the given update function.
        /// </summary>
        internal void CopyAndUpdate(IGenericParameterProvider target, IGenericParameterProvider source, IGenericContext gcontext)
        {
            foreach (var ga in source.GenericParameters)
            {
                var clone = new GenericParameter(ga.Name, target);
                if (ga.HasConstraints)
                {
                    foreach (var constraint in ga.Constraints)
                    {
                        clone.Constraints.Add(Get(constraint, gcontext));
                    }
                }
                clone.Attributes = ga.Attributes;
                target.GenericParameters.Add(clone);
            }
        }

        /// <summary>
        /// Create a clone of the given source generic parameters into the given target.
        /// All types in it are updated using the given update function.
        /// </summary>
        internal void Update(IList<TypeReference> list, IGenericContext gcontext)
        {
            for (var i = 0; i < list.Count; i++)
            {
                list[i] = Get(list[i], gcontext);
            }
        }

    }
}
