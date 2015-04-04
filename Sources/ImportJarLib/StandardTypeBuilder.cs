using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dot42.ImportJarLib.Doxygen;
using Dot42.ImportJarLib.Mapped;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;
using Dot42.Utility;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    public class StandardTypeBuilder : TypeBuilder
    {
        private readonly ClassFile cf;
        private NetTypeDefinition typeDef;
        private DocClass docClass;

        private static readonly string[][] FixedNamespacePrefixRenames =
        {
            new[] {"android.os", "Android.OS"},
            new[] {"android.view", "Android.Views"},
        };

        /// <summary>
        /// Create a builder
        /// </summary>
        public static IEnumerable<TypeBuilder> Create(ClassFile cf, TargetFramework target)
        {
            if (!ShouldImplement(cf, target))
                yield break;
            TypeBuilder builder;
            if (MappedTypeBuilder.TryCreateTypeBuilder(cf, out builder))
            {
                yield return builder;
            }
            else
            {
                if (cf.IsInterface && cf.Fields.Any())
                {
                    yield return new StandardInterfaceConstantsTypeBuilder(cf);
                }
                yield return new StandardTypeBuilder(cf);
            }
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        protected StandardTypeBuilder(ClassFile cf)
        {
            this.cf = cf;
        }

        /// <summary>
        /// Create a type definition for the given class file and all inner classes.
        /// </summary>
        public override void CreateType(NetTypeDefinition declaringType, NetModule module, TargetFramework target)
        {
            if (declaringType != null)
                throw new ArgumentException("Declaring type should be null");
            docClass = target.GetXmlClass(cf);

            var fullName = GetFullName();
            var dotIndex = fullName.LastIndexOf('.');
            var ns = (dotIndex > 0) ? ConvertNamespace(fullName, dotIndex) : String.Empty;
            var name = (dotIndex > 0) ? NameConverter.UpperCamelCase(fullName.Substring(dotIndex + 1)) : fullName;

            name = CreateTypeName(null, cf, name, ns);

            typeDef = new NetTypeDefinition(cf, target, module.Scope);
            typeDef.Name = name;
            typeDef.Namespace = ns;
            typeDef.OriginalJavaClassName = cf.ClassName;
            typeDef.Attributes = GetAttributes(cf, cf.Fields.Any());
            typeDef.IgnoreGenericArguments = !AddGenericParameters;
            typeDef.Description = (docClass != null) ? docClass.Description : null;
            module.Types.Add(typeDef);

            // Prepare generics
            CreateGenericParameters(cf, typeDef);

            // Add mapping
            var finalFullName = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
            RegisterType(target, cf, typeDef);
            CreateNestedTypes(cf, typeDef, finalFullName, module, target);
        }

        private static string ConvertNamespace(string fullName, int dotIndex)
        {
            foreach (var fixedConv in FixedNamespacePrefixRenames)
            {
                var len = fixedConv[0].Length;
                if (fullName.StartsWith(fixedConv[0]))
                {
                    if (fullName.Length == len || fullName[len] == '.')
                    {
                        fullName = fixedConv[1] + fullName.Substring(len);
                        dotIndex += fixedConv[1].Length - len;
                        break;
                    }
                }
            }
            return NameConverter.UpperCamelCase(fullName.Substring(0, dotIndex));
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
            if (TypeDefinition.IsInterface)
            {
                // Add to namespace instead (alter name to avoid duplicates)
                nestedType.Name = typeDef.Name + "_" + namePrefix + nestedType.Name;
                nestedType.Namespace = typeDef.Namespace;
                nestedType.DeclaringType = null;
                fullNestedTypeName = nestedType.FullName;
                module.Types.Add(nestedType);
            }
            else
            {
                TypeDefinition.NestedTypes.Add(nestedType);
            }
        }

        /// <summary>
        /// Implement members and setup references now that all types have been created
        /// </summary>
        public override void Implement(TargetFramework target)
        {
            Implement(cf, typeDef, cf.IsEnum(), (this is IInterfaceConstantsTypeBuilder), target);
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
            FinalizeNames(typeDef, target, methodRenamer);
        }

        /// <summary>
        /// Gets the name of the given field
        /// </summary>
        public override string GetFieldName(FieldDefinition field)
        {
            return FixFieldName(base.GetFieldName(field), field, typeDef);
        }

        /// <summary>
        /// Update the attributes of the given method
        /// </summary>
        public override MethodAttributes GetMethodAttributes(MethodDefinition method, MethodAttributes attributes)
        {
            return GetMethodAttributes(typeDef, method, attributes);
        }

        /// <summary>
        /// Gets the full type name for the given java class.
        /// </summary>
        protected virtual string GetFullName()
        {
            return cf.ClrTypeName;            
        }

        /// <summary>
        /// Create type attributes
        /// </summary>
        protected virtual TypeAttributes GetAttributes(ClassFile cf, bool hasFields)
        {
            var result = cf.IsPublic ? TypeAttributes.Public : TypeAttributes.NotPublic;
            result |= cf.IsInterface ? TypeAttributes.Interface : TypeAttributes.Class;
            if (cf.IsAbstract) result |= TypeAttributes.Abstract;
            else if (cf.IsFinal) result |= TypeAttributes.Sealed;

            return result;
        }

        /// <summary>
        /// Gets the created CLR type
        /// </summary>
        protected NetTypeDefinition TypeDefinition { get { return typeDef; } }

        /// <summary>
        /// Resolve the given generic parameter into a type reference.
        /// </summary>
        protected override bool TryResolveTypeParameter(string name, TargetFramework target, out NetTypeReference type)
        {
            if (!AddGenericParameters)
            {
                if (cf.Signature.TypeParameters.Any(x => x.Name == name))
                {
                    type = target.TypeNameMap.Object;
                    return true;
                }
            }
            var result = typeDef.GenericParameters.FirstOrDefault(x => x.Name == name);
            if (result != null)
            {
                type = result;
                return true;
            }
            // This is a hack for non-behaving java classes
            type = target.TypeNameMap.Object;
            return false;
        }

        /// <summary>
        /// Full typename of the context without any generic types.
        /// </summary>
        protected override string FullTypeName { get { return typeDef.Name + "." + typeDef.Name; } }

        /// <summary>
        /// Gets the documentation of this type.
        /// Can be null.
        /// </summary>
        public override DocClass Documentation { get { return docClass; } }

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
        public override void FillLayoutXml(JarFile jf, XElement parent)
        {
            var baseName = GetLayoutBaseClassName(cf);
            if (baseName == null)
                return;
            string elementName;
            switch (baseName)
            {
                case SdkConstants.ViewClass:
                    elementName = "view";
                    break;
                case SdkConstants.ViewGroupClass:
                    elementName = "viewgroup";
                    break;
                default:
                    return;
            }
            var shortName = GetShortName(cf.ClassName);
            var element = new XElement(elementName,
                new XAttribute("name", shortName));
            if (cf.IsAbstract)
                element.Add(new XAttribute("abstract", "true"));
            ClassFile superClass ;
            if (cf.TryGetSuperClass(out superClass))
            {
                if ((superClass != null) && (GetLayoutBaseClassName(superClass) != null))
                    element.Add(new XAttribute("super", GetShortName(superClass.ClassName)));
            }
            parent.Add(element);
        }

        /// <summary>
        /// Create a short name for the given classname.
        /// </summary>
        private static string GetShortName(string className)
        {
            var nameParts = className.Split('/');
            return nameParts[nameParts.Length - 1];            
        }

        /// <summary>
        /// Gets the class name of the base class relevant for layout.xml.
        /// </summary>
        private static string GetLayoutBaseClassName(ClassFile classFile)
        {
            while (true)
            {
                var className = classFile.ClassName;
                switch (className)
                {
                    case SdkConstants.ViewClass:
                    case SdkConstants.ViewGroupClass:
                        return className;
                    case SdkConstants.ObjectClass:
                        return null;
                }

                // Go to the base class
                if (!classFile.TryGetSuperClass(out classFile))
                    return null;
            }
        }
    }
}
