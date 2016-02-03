using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.Ast.Extensions
{
    /// <summary>
    /// Mono.Cecil related extension methods
    /// </summary>
    public static partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Gets the method the given method overrides.
        /// </summary>
        public static MethodDefinition GetBaseMethod(this MethodDefinition method)
        {
            var declaringType = method.DeclaringType;
            var baseTypeRef = declaringType.BaseType;
            var resolver = new GenericsResolver(method.DeclaringType);

            while (baseTypeRef != null)
            {
                var baseType = baseTypeRef.Resolve();
                if (baseType == null)
                    return null;

                var result = baseType.Methods.FirstOrDefault(x => x.AreSame(method, resolver.Resolve));
                if (result != null)
                    return result;

                baseTypeRef = baseType.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Gets all methods the given method overrides.
        /// </summary>
        public static IEnumerable<MethodDefinition> GetBaseMethods(this MethodDefinition method)
        {
            while (method != null)
            {
                var @base = method.GetBaseMethod();
                if (@base == null)
                    yield break;
                yield return @base;
                method = @base;
            }
        }

        /// <summary>
        /// Gets the first method the given method overrides from an implemented interface.
        /// </summary>
        public static MethodDefinition GetBaseInterfaceMethod(this MethodDefinition method)
        {
            var declaringType = method.DeclaringType;
            var resolver = new GenericsResolver(method.DeclaringType);

            while (declaringType != null)
            {
                foreach (var ifaceRef in declaringType.Interfaces.Select(x => x.Interface))
                {
                    var iface = ifaceRef.Resolve();
                    if (iface == null)
                        continue;

                    var result = iface.Methods.FirstOrDefault(x => x.AreSame(method, resolver.Resolve));
                    if (result != null)
                        return result;
                }
                declaringType = (declaringType.BaseType != null) ? declaringType.BaseType.Resolve() : null;
            }

            return null;
        }

        /// <summary>
        /// Gets all methods the given method overrides from an implemented interface.
        /// </summary>
        public static IEnumerable<MethodDefinition> GetBaseInterfaceMethods(this MethodDefinition method)
        {
            while (method != null)
            {
                var @base = method.GetBaseInterfaceMethod();
                if (@base == null)
                    yield break;
                yield return @base;
                method = @base;
            }
        }

        /// <summary>
        /// Gets the first implementation of the given interface method in the given type.
        /// </summary>
        public static MethodDefinition GetImplementation(this MethodDefinition interfaceMethod, TypeDefinition type)
        {
            if (!interfaceMethod.DeclaringType.IsInterface)
                throw new ArgumentException("interfaceMethod");
            var resolver = new GenericsResolver(type);

            while (type != null)
            {
                // Look for explicit implementation
                var result = type.Methods.FirstOrDefault(x => x.IsExplicitImplementationOf(interfaceMethod, resolver));
                if (result != null)
                    return result;

                // Look for implicit implementation
                result = type.Methods.FirstOrDefault(x => x.AreSame(interfaceMethod, resolver.Resolve));
                if (result != null)
                    return result;

                type = (type.BaseType != null) ? type.BaseType.Resolve() : null;
            }

            return null;
        }

        /// <summary>
        /// Find the default ctor of this type (if any)
        /// </summary>
        public static MethodDefinition FindDefaultCtor(this TypeDefinition type)
        {
            if (!type.HasMethods) 
                return null;
            // We've got to ensure that the type can be created through reflection /
            // dependency injection. We are interested in any public constructor,
            // and private/protected default constructors.
            return type.Methods.Where(ctor => ctor.IsConstructor 
                                          && ctor.Name == ".ctor" 
                                          && (ctor.IsPublic || !ctor.HasParameters))
                               .OrderByDescending(ctor => ctor.IsPublic)
                               .ThenBy(ctor => ctor.HasParameters)
                               .FirstOrDefault();
        }

        /// <summary>
        /// Find the class ctor of this type (if any).
        /// Returns null if not found.
        /// </summary>
        public static MethodDefinition GetClassCtor(this TypeDefinition type)
        {
            return type.HasMethods
                       ? type.Methods.FirstOrDefault(ctor => (ctor.IsConstructor) && (ctor.Name == ".cctor"))
                       : null;
        }

        /// <summary>
        /// Gets the (exclusive) end offset of this instruction.
        /// </summary>
        public static int GetEndOffset(this Instruction inst)
        {
            return inst.Offset + inst.GetSize();
        }

        public static string OffsetToString(int offset)
        {
            return string.Format("IL_{0:x4}", offset);
        }

        public static HashSet<MethodDefinition> GetAccessorMethods(this TypeDefinition type)
        {
            HashSet<MethodDefinition> accessorMethods = new HashSet<MethodDefinition>();
            foreach (var property in type.Properties)
            {
                accessorMethods.Add(property.GetMethod);
                accessorMethods.Add(property.SetMethod);
                if (property.HasOtherMethods)
                {
                    foreach (var m in property.OtherMethods)
                        accessorMethods.Add(m);
                }
            }
            foreach (EventDefinition ev in type.Events)
            {
                accessorMethods.Add(ev.AddMethod);
                accessorMethods.Add(ev.RemoveMethod);
                accessorMethods.Add(ev.InvokeMethod);
                if (ev.HasOtherMethods)
                {
                    foreach (var m in ev.OtherMethods)
                        accessorMethods.Add(m);
                }
            }
            return accessorMethods;
        }

        public static TypeDefinition ResolveWithinSameModule(this TypeReference type)
        {
            if (type != null && type.GetElementType().Module == type.Module)
                return type.Resolve();
            else
                return null;
        }

        public static FieldDefinition ResolveWithinSameModule(this FieldReference field)
        {
            if (field != null && field.DeclaringType.GetElementType().Module == field.Module)
                return field.Resolve();
            else
                return null;
        }

        public static MethodDefinition ResolveWithinSameModule(this MethodReference method)
        {
            if (method != null && method.DeclaringType.GetElementType().Module == method.Module)
                return method.Resolve();
            else
                return null;
        }

        public static TypeDefinition ResolveOrThrow(this TypeReference typeReference)
        {
            var resolved = typeReference.Resolve();
            if (resolved == null)
                throw new ArgumentException("Cannot resolve " + typeReference);
            return resolved;
        }

        /*public static TypeReference GetEnumUnderlyingType(this TypeDefinition type)
        {
            if (!type.IsEnum)
                throw new ArgumentException("Type must be an enum", "type");

            var fields = type.Fields;

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (!field.IsStatic)
                    return field.FieldType;
            }

            throw new NotSupportedException();
        }*/

        public static bool IsAnonymousType(this TypeReference type)
        {
            if (type == null)
                return false;
            if (string.IsNullOrEmpty(type.Namespace) && type.HasGeneratedName() &&
                (type.Name.Contains("AnonType") || type.Name.Contains("AnonymousType")))
            {
                TypeDefinition td = type.Resolve();
                return td != null && td.IsCompilerGenerated();
            }
            return false;
        }

        public static bool ContainsAnonymousType(this TypeReference type)
        {
            GenericInstanceType git = type as GenericInstanceType;
            if (git != null)
            {
                if (IsAnonymousType(git))
                    return true;
                for (int i = 0; i < git.GenericArguments.Count; i++)
                {
                    if (git.GenericArguments[i].ContainsAnonymousType())
                        return true;
                }
                return false;
            }
            TypeSpecification typeSpec = type as TypeSpecification;
            if (typeSpec != null)
                return typeSpec.ElementType.ContainsAnonymousType();
            else
                return false;
        }

        public static string GetDefaultMemberName(this TypeDefinition type)
        {
            CustomAttribute attr;
            return type.GetDefaultMemberName(out attr);
        }

        public static string GetDefaultMemberName(this TypeDefinition type, out CustomAttribute defaultMemberAttribute)
        {
            if (type.HasCustomAttributes)
                foreach (CustomAttribute ca in type.CustomAttributes)
                    if (ca.Constructor.DeclaringType.Name == "DefaultMemberAttribute" &&
                        ca.Constructor.DeclaringType.Namespace == "System.Reflection"
                        &&
                        ca.Constructor.FullName ==
                        @"System.Void System.Reflection.DefaultMemberAttribute::.ctor(System.String)")
                    {
                        defaultMemberAttribute = ca;
                        return ca.ConstructorArguments[0].Value as string;
                    }
            defaultMemberAttribute = null;
            return null;
        }

        public static bool IsIndexer(this PropertyDefinition property)
        {
            CustomAttribute attr;
            return property.IsIndexer(out attr);
        }

        public static bool IsIndexer(this PropertyDefinition property, out CustomAttribute defaultMemberAttribute)
        {
            defaultMemberAttribute = null;
            if (property.HasParameters)
            {
                var accessor = property.GetMethod ?? property.SetMethod;
                PropertyDefinition basePropDef = property;
                if (accessor.HasOverrides)
                {
                    // if the property is explicitly implementing an interface, look up the property in the interface:
                    MethodDefinition baseAccessor = accessor.Overrides.First().Resolve();
                    if (baseAccessor != null)
                    {
                        foreach (PropertyDefinition baseProp in baseAccessor.DeclaringType.Properties)
                        {
                            if (baseProp.GetMethod == baseAccessor || baseProp.SetMethod == baseAccessor)
                            {
                                basePropDef = baseProp;
                                break;
                            }
                        }
                    }
                    else
                        return false;
                }
                CustomAttribute attr;
                var defaultMemberName = basePropDef.DeclaringType.GetDefaultMemberName(out attr);
                if (defaultMemberName == basePropDef.Name)
                {
                    defaultMemberAttribute = attr;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Does the given type extend from System.Attribute?
        /// </summary>
        public static bool IsAttribute(this TypeDefinition type)
        {
            while (true)
            {
                var baseType = type.BaseType;
                if (baseType == null)
                    return false;
                type = baseType.Resolve();
                if (type.FullName == typeof(Attribute).FullName)
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Does the given type extend from Java.Lang.Annotation.IAnnotation?
        /// </summary>
        public static bool IsAnnotation(this TypeDefinition type)
        {
            const string fullName = "Java.Lang.Annotation.IAnnotation";
            foreach (var intf in type.Interfaces)
            {
                if (intf.Interface.FullName == fullName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets all interfaces implemented by the given type and all its parents.
        /// </summary>
        internal static IList<TypeReference> GetImplementedInterfaces(this TypeDefinition type)
        {
            if (type.CachedImplementedInterfaces != null)
                return type.CachedImplementedInterfaces;

            var ret = new List<TypeReference>();
            var currentType = type;
            while (currentType != null)
            {
                foreach (var intf in currentType.Interfaces)
                {
                    ret.Add(intf.Interface);

                    var intfDef = intf.Interface.GetElementType().Resolve();

                    if (intfDef != null)
                    {
                        if (intfDef != intf.Interface)
                        {
                            ret.Add(intfDef);
                        }

                        ret.AddRange(GetImplementedInterfaces(intfDef));
                    }
                }

                currentType = (currentType.BaseType != null) ? currentType.BaseType.Resolve() : null;
            }
            // no need for locking. in case somebody was faster, we just overwrite him.
            return type.CachedImplementedInterfaces = ret.Distinct().ToArray();
        }

        /// <summary>
        /// Does the given type implement the given interface?
        /// </summary>
        public static bool Implements(this TypeDefinition typeDef, TypeReference @interface)
        {
            return typeDef.GetImplementedInterfaces().Any(x => x.AreSame(@interface, null));
        }

        /// <summary>
        /// Is the given method an explicit interface implementation?
        /// </summary>
        public static bool IsExplicitImplementation(this MethodDefinition method)
        {
            return method.IsPrivate && method.HasOverrides;
        }

        /// <summary>
        /// Is the given method an implicit interface implementation?
        /// </summary>
        internal static bool IsImplicitImplementation(this MethodDefinition method)
        {
            if (method.IsPrivate)
                return false;
            var declaringType = method.DeclaringType;
            var resolver = new GenericsResolver(method.DeclaringType);
            return declaringType.GetImplementedInterfaces().OfType<TypeDefinition>().SelectMany(x => x.Methods).Any(x => x.AreSame(method, resolver.Resolve));
        }

        /// <summary>
        /// Does the given type or any of its parents have an explicit implementation of the given interface method?
        /// </summary>
        internal static bool HasExplicitImplementationOf(this TypeDefinition type, MethodDefinition iMethod)
        {
            while (type != null)
            {
                if (type.Methods.Any(x => x.IsExplicitImplementation() && x.IsImplementationOf(iMethod)))
                    return true;
                var @base = type.BaseType;
                type = (@base != null) ? @base.Resolve() : null;
            }
            return false;
        }

        /// <summary>
        /// Is the given method an explicit implementation of the given interface method.
        /// </summary>
        /// <param name="method">The method to investigate</param>
        /// <param name="iMethod">The interface method.</param>
        public static bool IsExplicitImplementationOf(this MethodDefinition method, MethodDefinition iMethod, GenericsResolver resolver = null)
        {
            resolver = resolver ?? new GenericsResolver(method.DeclaringType);
            if (method.IsExplicitImplementation())
            {
                return method.Overrides.Any(x => x.GetElementMethod().AreSameIncludingDeclaringType(iMethod, resolver.Resolve));
            }
            return false;
        }

        /// <summary>
        /// Is the given method an implementation (implicit or explicit) of the given interface method.
        /// </summary>
        /// <param name="method">The method to investigate</param>
        /// <param name="iMethod">The interface method.</param>
        public static bool IsImplementationOf(this MethodDefinition method, MethodDefinition iMethod)
        {
            // Try explicit first
            var resolver = new GenericsResolver(method.DeclaringType);
            if (method.IsExplicitImplementation())
            {
                return method.Overrides.Any(x => x.GetElementMethod().AreSameIncludingDeclaringType(iMethod, resolver.Resolve));
            }
            // Private methods cannot be an implicit implementation
            if (method.IsPrivate)
                return false;

            // Try implicit
            if (method.AreSame(iMethod, resolver.Resolve))
            {
                // If the declaring class also has an explicit implementation of the method we have no match.
                // Otherwise we have a match.
                return !method.DeclaringType.HasExplicitImplementationOf(iMethod);
            }
            return false;
        }

        /// <summary>
        /// Is the given method a dex import or java import method with at least 1 generic parameter uses in parameters or return type.
        /// </summary>
        public static bool IsJavaMethodWithGenericParams(this MethodDefinition method)
        {
            if (!(method.HasDexImportAttribute() || method.HasJavaImportAttribute()))
                return false;
            return method.Parameters.Any(x => x.ParameterType.ContainsGenericParameter);
        }

        /// <summary>
        /// Is the given type an array of a generic parameter?
        /// </summary>
        public static bool IsGenericParameterArray(this TypeReference type)
        {
            if (!type.IsArray)
                return false;
            return ((ArrayType)type).ElementType.IsGenericParameter;
        }

        /// <summary>
        /// Is the given type an array of a primitive elements?
        /// </summary>
        public static bool IsPrimitiveArray(this TypeReference type)
        {
            if (!type.IsArray)
                return false;
            return ((ArrayType)type).ElementType.IsPrimitive;
        }

        /// <summary>
        /// Is the given type a type definition or a normal type reference?
        /// </summary>
        public static bool IsDefinitionOrReference(this TypeReference type)
        {
            if (type.IsDefinition)
                return true;
            return (type.GetType() == typeof (TypeReference));
        }

        /// <summary>
        /// Is the given type a static class?
        /// </summary>
        public static bool IsStatic(this TypeDefinition type)
        {
            return (type.IsAbstract && type.IsSealed);
        }

        /// <summary>
        /// Is the given method a class constructor?
        /// </summary>
        public static bool IsClassCtor(this MethodDefinition method)
        {
            return method.IsConstructor && (method.Name == ".cctor");
        }
    }
}
