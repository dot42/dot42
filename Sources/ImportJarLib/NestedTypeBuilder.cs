using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dot42.ImportJarLib.Doxygen;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;
using Dot42.Utility;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    internal class NestedTypeBuilder : TypeBuilder
    {
        private readonly TypeBuilder parent;
        private readonly string parentFullName;
        private readonly ClassFile cf;
        private readonly InnerClass inner;
        private NetTypeDefinition typeDef;
        private DocClass docClass;

        internal static IEnumerable<NestedTypeBuilder> Create(TypeBuilder parent, string parentFullName, ClassFile cf, InnerClass inner)
        {
            if (cf.IsInterface && cf.Fields.Any())
            {
                yield return new NestedInterfaceConstantsTypeBuilder(parent, parentFullName, cf, inner);
            }
            yield return new NestedTypeBuilder(parent, parentFullName, cf, inner);
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        protected NestedTypeBuilder(TypeBuilder parent, string parentFullName, ClassFile cf, InnerClass inner)
        {
            this.parent = parent;
            this.parentFullName = parentFullName;
            this.cf = cf;
            this.inner = inner;
        }

        /// <summary>
        /// Implement members and setup references now that all types have been created
        /// </summary>
        public override void Implement(TargetFramework target)
        {
            var isEnum = (inner.IsEnum) || cf.IsEnum();
            Implement(cf, typeDef, isEnum, (this is IInterfaceConstantsTypeBuilder), target);
            base.Implement(target);
        }

        /// <summary>
        /// Implement interface members
        /// </summary>
        public override void Finalize(TargetFramework target, FinalizeStates state)
        {
            Finalize(typeDef, target, state);
            base.Finalize(target, state);
        }

        /// <summary>
        /// Update names where needed
        /// </summary>
        public override void FinalizeNames(TargetFramework target, MethodRenamer methodRenamer)
        {
            // Make sure there is no name class with other members
            var prefix = "Java";
            var postfix = 0;
            while ((typeDef.DeclaringType != null) && (typeDef.DeclaringType.Methods.Any(x => x.Name == typeDef.Name)))
            {
                typeDef.Name = prefix + typeDef.Name;
                if (postfix > 0) typeDef.Name += postfix;
                prefix = "";
                postfix++;
            }

            FinalizeNames(typeDef, target, methodRenamer);
        }

        /// <summary>
        /// Create a type defrinition for the given class file and all inner classes.
        /// </summary>
        public override void CreateType(NetTypeDefinition declaringType, NetModule module, TargetFramework target)
        {
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");
            docClass = target.GetXmlClass(cf);

            var name = NameConverter.UpperCamelCase(inner.IsAnonymous ? cf.Name : inner.Name);
            name = CreateTypeName(declaringType, cf, name, null);

            var finalFullName = parentFullName + "/" + name;

            var attributes = GetAttributes(cf);
            typeDef = new NetTypeDefinition(cf, target, module.Scope) { Name = name, Attributes = attributes };
            typeDef.OriginalJavaClassName = cf.ClassName;
            typeDef.Description = (docClass != null) ? docClass.Description : null;
            parent.AddNestedType(typeDef, "", module, ref finalFullName);

            // Prepare generics
            CreateGenericParameters(cf, typeDef);

            // Add mapping
            RegisterType(target, cf, typeDef);
            CreateNestedTypes(cf, typeDef, finalFullName, module, target);
        }

        /// <summary>
        /// Register the given type in the type map.
        /// </summary>
        protected virtual void RegisterType(TargetFramework target, ClassFile classFile, NetTypeDefinition typeDef)
        {
            target.TypeNameMap.Add(classFile.ClassName, typeDef);
        }

        /// <summary>
        /// Make sure that base types are visible.
        /// </summary>
        public override void FinalizeVisibility(TargetFramework target)
        {
            typeDef.EnsureVisibility();
        }

        /// <summary>
        /// Adds the given nested type to my type declaration.
        /// </summary>
        protected internal override void AddNestedType(NetTypeDefinition nestedType, string namePrefix, NetModule module, ref string fullNestedTypeName)
        {
            if (typeDef.IsInterface)
            {
                // Add to my parent
                parent.AddNestedType(nestedType, cf.Name + "_" + namePrefix, module, ref fullNestedTypeName);
            }
            else
            {
                typeDef.NestedTypes.Add(nestedType);
            }
        }

        /// <summary>
        /// Update the attributes of the given method
        /// </summary>
        public override MethodAttributes GetMethodAttributes(MethodDefinition method, MethodAttributes methodAttributes)
        {
            return GetMethodAttributes(typeDef, method, methodAttributes);
        }

        /// <summary>
        /// Full typename of the context without any generic types.
        /// </summary>
        protected override string FullTypeName
        {
            get { return ((IBuilderGenericContext)parent).FullTypeName + "." + typeDef.Name; }
        }

        /// <summary>
        /// Resolve the given generic parameter into a type reference.
        /// </summary>
        protected override bool TryResolveTypeParameter(string name, TargetFramework target, out NetTypeReference type)
        {
            var parentContext = (IBuilderGenericContext) parent;
            type = typeDef.GenericParameters.FirstOrDefault(x => x.Name == name);
            if (type != null)
                return true;
            return parentContext.TryResolveTypeParameter(name, target, out type);
        }

        /// <summary>
        /// Gets the documentation of this type.
        /// Can be null.
        /// </summary>
        public override DocClass Documentation { get { return docClass; } }

        /// <summary>
        /// Create type attributes
        /// </summary>
        protected virtual TypeAttributes GetAttributes(ClassFile cf)
        {
            var result = (TypeAttributes)0;

            if (cf.IsPublic) result |= TypeAttributes.NestedPublic;
            else if (cf.IsProtected) result |= TypeAttributes.NestedFamORAssem;
            else if (cf.IsPrivate) result |= TypeAttributes.NestedPrivate;
            else if (cf.IsPackagePrivate) result |= TypeAttributes.NestedAssembly;

            result |= cf.IsInterface ? TypeAttributes.Interface : TypeAttributes.Class;
            if (cf.IsAbstract) result |= TypeAttributes.Abstract;
            else if (cf.IsFinal) result |= TypeAttributes.Sealed;

            return result;
        }

        /// <summary>
        /// Should the given field be implemented?
        /// </summary>
        protected internal override bool ShouldImplement(FieldDefinition field, TargetFramework target)
        {
            if (cf.IsInterface)
                return false;
            return base.ShouldImplement(field, target);
        }

        /// <summary>
        /// Add this type to the layout.xml file if needed.
        /// </summary>
        public override void FillLayoutXml(JarFile jf, XElement xElement)
        {
            // Do nothing
        }

        /// <summary>
        /// Gets the name of the given field
        /// </summary>
        public override string GetFieldName(FieldDefinition field)
        {
            return FixFieldName(base.GetFieldName(field), field, typeDef);
        }
    }
}
