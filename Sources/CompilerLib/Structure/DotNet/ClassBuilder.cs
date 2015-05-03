using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.DexLib;
using Dot42.DexLib.Extensions;
using Dot42.FrameworkDefinitions;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Extensions;
using Dot42.LoaderLib.Java;
using Dot42.Mapping;
using Dot42.Utility;
using Mono.Cecil;
using FieldDefinition = Dot42.DexLib.FieldDefinition;
using MethodDefinition = Mono.Cecil.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing .NET types.
    /// </summary>
    [DebuggerDisplay("{@Type}")]
    internal abstract class ClassBuilder : IClassBuilder
    {
        private readonly ReachableContext context;
        private readonly AssemblyCompiler compiler;
        private readonly TypeDefinition typeDef;
        private ClassDefinition classDef;
        private List<ClassBuilder> nestedBuilders;
        private List<FieldBuilder> fieldBuilders;
        private List<MethodBuilder> methodBuilders;
        private XTypeDefinition xType;

        /// <summary>
        /// Create a type builder for the given type.
        /// </summary>
        internal static IClassBuilder[] Create(ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
        {
            if (typeDef.FullName == "<Module>")
                return new IClassBuilder[] { new SkipClassBuilder() };
            if (typeDef.IsDelegate())
                return new IClassBuilder[] {new DelegateClassBuilder(context, compiler, typeDef) };
            if (typeDef.IsAttribute()) 
                return new IClassBuilder[]  {new AttributeClassBuilder(context, compiler, typeDef) };
            if (typeDef.IsAnnotation())
                return new IClassBuilder[] {new AnnotationClassBuilder(context, compiler, typeDef) };
            if (typeDef.HasDexImportAttribute())
                return new IClassBuilder[] {new DexImportClassBuilder(context, compiler, typeDef) };
            if (typeDef.HasJavaImportAttribute())
                return new IClassBuilder[] {CreateJavaImportBuilder(context, compiler, typeDef)};
            if (typeDef.BaseType != null && typeDef.BaseType.FullName == "Android.App.Application")
                return new IClassBuilder[] { new AndroidAppApplicationDerivedBuilder(context,compiler,typeDef) };
            if (typeDef.IsEnum)
            {
                if (typeDef.UsedInNullableT)
                {
                    var nullableBaseClassBuilder = new NullableEnumBaseClassBuilder(context, compiler, typeDef);
                    IClassBuilder builder = new EnumClassBuilder(context, compiler, typeDef, nullableBaseClassBuilder);
                    return new[] { builder, nullableBaseClassBuilder };
                }
                return new IClassBuilder[] { new EnumClassBuilder(context, compiler, typeDef, null) };
            }
            else
            {
                if (!typeDef.UsedInNullableT)
                    return new[] { new StandardClassBuilder(context, compiler, typeDef) };

                var builder = new StandardClassBuilder(context, compiler, typeDef);
                var nullableBuilder = new NullableMarkerClassBuilder(context, compiler, typeDef, builder);

                return new IClassBuilder[] { builder, nullableBuilder };
            }
        }

        /// <summary>
        /// Create a type builder for the given type with JavaImport attribute.
        /// </summary>
        internal static IClassBuilder CreateJavaImportBuilder(ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
        {
            var javaImportAttr = typeDef.GetJavaImportAttribute(true);
            var className = (string)javaImportAttr.ConstructorArguments[0].Value;
            ClassFile classFile;
            if (!compiler.ClassLoader.TryLoadClass(className, out classFile))
                throw new ClassNotFoundException(className);
            context.RecordReachableType(classFile);
            return new SkipClassBuilder();
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        protected ClassBuilder(ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
        {
            this.context = context;
            this.compiler = compiler;
            this.typeDef = typeDef;
        }

        /// <summary>
        /// Sorting low comes first
        /// </summary>
        int IClassBuilder.SortPriority { get { return SortPriority; } }

        /// <summary>
        /// Sorting low comes first
        /// 
        /// Note that this does not work globally for nested types.
        /// </summary>
        protected abstract int SortPriority { get; }

        /// <summary>
        /// Gets fullname of the underlying type.
        /// </summary>
        string IClassBuilder.FullName
        {
            get { return Type.FullName; }
        }

        /// <summary>
        /// Gets the (abstracted) type for which a class is build.
        /// </summary>
        public XTypeDefinition XType
        {
            get
            {
                if (xType == null)
                    throw new InvalidOperationException("xType not yet set");
                return xType;
            }
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        void IClassBuilder.Create(ITargetPackage targetPackage)
        {
            var dtp = (DexTargetPackage)targetPackage;
            Create(dtp, null, null, null);
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected void Create(DexTargetPackage targetPackage, ClassDefinition parent, TypeDefinition parentType, XTypeDefinition parentXType)
        {
            xType = CreateXType(parentXType);
            CreateClassDefinition(targetPackage, parent, parentType, parentXType);
            CreateNestedClasses(targetPackage, parent);
        }

        /// <summary>
        /// Create the XType for this builder.
        /// </summary>
        protected virtual XTypeDefinition CreateXType(XTypeDefinition parentXType)
        {
            return XBuilder.AsTypeReference(compiler.Module, typeDef).Resolve();            
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected virtual void CreateClassDefinition(DexTargetPackage targetPackage, ClassDefinition parent, TypeDefinition parentType, XTypeDefinition parentXType)
        {
            // Create classdef
            var nsConverter = targetPackage.NameConverter;
            classDef = new ClassDefinition();
            classDef.MapFileId = compiler.GetNextMapFileId();
            classDef.Namespace = nsConverter.GetConvertedNamespace(XType);
            var name = CreateClassName(XType);
            if ((parentType != null) && parentType.HasDexImportAttribute())
            {
                var fullName = nsConverter.GetConvertedFullName(parentXType) + "_" + name;
                var index = fullName.LastIndexOf('.');
                classDef.Name = (index < 0) ? fullName : fullName.Substring(index + 1);
            }
            else
            {
                classDef.Name = (parent != null) ? parent.Name + "$" + name : name;
            }

            // Set access flags
            //if (typeDef.IsPublic) classDef.IsPublic = true;
            //else classDef.IsPrivate = true;
            classDef.IsPublic = true;
            if (typeDef.IsSealed) classDef.IsFinal = true;
            if (typeDef.IsInterface)
            {
                classDef.IsInterface = true;
                classDef.IsAbstract = true;
            }
            else if (typeDef.IsAbstract)
            {
                classDef.IsAbstract = true;
            }

            if ((parent != null) && (!parentType.HasDexImportAttribute()))
            {
                // Add to parent if this is a nested type
                classDef.Owner = parent;
                parent.AddInnerClass(classDef);
            }
            else
            {
                // Add to dex if it is a root class
                // TODO: here we could simplify the names, e.g. remove the scope, as long as no
                //       clashing does occur.
                targetPackage.DexFile.AddClass(classDef);
            }
        }

        /// <summary>
        /// Create the name of the class.
        /// </summary>
        protected virtual string CreateClassName(XTypeDefinition xType)
        {
            return NameConverter.GetConvertedName(XType);
        }

        /// <summary>
        /// Create the nested classes for this type.
        /// </summary>
        protected void CreateNestedClasses(DexTargetPackage targetPackage, ClassDefinition parent)
        {
            nestedBuilders = CreateNestedClassBuilders(context, targetPackage, parent)
                                                     .OrderBy(x => x.SortPriority)
                                                     .ToList();
            nestedBuilders.ForEach(x => x.Create(targetPackage, classDef, typeDef, XType));
        }

        /// <summary>
        /// Create the nested classes for this type.
        /// </summary>
        protected virtual IEnumerable<ClassBuilder> CreateNestedClassBuilders(ReachableContext context, DexTargetPackage targetPackage, ClassDefinition parent)
        {
            return typeDef.NestedTypes.Where(x => x.IsReachable).SelectMany(x => Create(context, compiler, x)).Cast<ClassBuilder>();
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        void IClassBuilder.Implement(ITargetPackage targetPackage)
        {
            Implement((DexTargetPackage)targetPackage);
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        public void Implement(DexTargetPackage targetPackage)
        {
            ImplementSuperClass(targetPackage);
            ImplementInterfaces(targetPackage);
            ImplementCloneable(targetPackage);
            ImplementInnerClasses(targetPackage);
            CreateMembers(targetPackage);
        }

        /// <summary>
        /// Implement make minor fixes after the implementation phase.
        /// </summary>
        void IClassBuilder.FixUp(ITargetPackage targetPackage)
        {
            FixUp((DexTargetPackage)targetPackage);
        }

        /// <summary>
        /// Implement make minor fixes after the implementation phase.
        /// </summary>
        public void FixUp(DexTargetPackage targetPackage)
        {
            FixUpInnerClasses(targetPackage);
            FixUpMethods(targetPackage);
        }

        /// <summary>
        /// Record the mapping from .NET to Dex
        /// </summary>
        public void RecordMapping(MapFile mapFile)
        {
            var entry = CreateMappingEntry();

            mapFile.Add(entry);

            if (fieldBuilders != null) fieldBuilders.ForEach(x => x.RecordMapping(entry));
            if (methodBuilders != null) methodBuilders.ForEach(x => x.RecordMapping(entry));

            // Create mapping of nested classes
            if (nestedBuilders != null) nestedBuilders.ForEach(x => x.RecordMapping(mapFile));
        }

        protected virtual TypeEntry CreateMappingEntry()
        {
            if (classDef == null)
            {
                DLog.Warning(DContext.CompilerCodeGenerator, "dexName not available for type {0}.", typeDef.FullName);
            }
            // Create mapping
            var dexName = (classDef != null) ? classDef.Fullname : null;
            var mapFileId = (classDef != null) ? classDef.MapFileId : 0;
            // first part of XType.ScopeId contains the module name, cut it out.
            var scopeId = XType.ScopeId.Substring(XType.ScopeId.IndexOf(':') + 1);
            var entry = new TypeEntry(typeDef.FullName, typeDef.Scope.Name, dexName, mapFileId, scopeId);
            return entry;
        }

        /// <summary>
        /// Set the super class of the class definition.
        /// </summary>
        protected virtual void ImplementSuperClass(DexTargetPackage targetPackage)
        {
            // Set base type
            var baseType = typeDef.BaseType;
            if (baseType != null)
            {
                classDef.SuperClass = (ClassReference)baseType.GetReference(targetPackage, compiler.Module);
            }
            else if (typeDef.IsInterface)
            {
                classDef.SuperClass = new ClassReference("java/lang/Object");
            }
            else
            {
                throw new ArgumentException(string.Format("Type {0} has no base type", typeDef.FullName));
            }
        }

        /// <summary>
        /// Add references to all implemented interfaces.
        /// </summary>
        protected virtual void ImplementInterfaces(DexTargetPackage targetPackage)
        {
            // Implement interfaces
            classDef.Interfaces.AddRange(typeDef.Interfaces.Select(x => x.Interface.GetClassReference(targetPackage, compiler.Module)).Distinct());
        }

        /// <summary>
        /// Implement java.lang.Cloneable
        /// </summary>
        protected virtual void ImplementCloneable(DexTargetPackage targetPackage)
        {
            // Do not implement in some cases
            if ((classDef == null) || typeDef.IsInterface || typeDef.IsStatic())
                return;

            // Do not implement, when there is a .NET base type (it is already implemented there)
            var baseType = (typeDef.BaseType != null) ? typeDef.BaseType.GetElementType().Resolve() : null;
            if ((baseType != null) && (baseType.GetDexOrJavaImportAttribute() == null))
                return;

            // If explicitly implemented, do not implement again
            if (classDef.Interfaces.Any(x => x.Fullname == FrameworkReferences.Cloneable.Fullname))
                return;

            // Add Cloneable implementation
            classDef.Interfaces.Add(FrameworkReferences.Cloneable);
        }

        /// <summary>
        /// Implemented all nested types.
        /// </summary>
        protected virtual void ImplementInnerClasses(DexTargetPackage targetPackage)
        {
            // Implemented nested type
            nestedBuilders.ForEach(x => x.Implement(targetPackage));
        }

        /// <summary>
        /// FixUp all nested types.
        /// </summary>
        protected virtual void FixUpInnerClasses(DexTargetPackage targetPackage)
        {
            // FixUp nested type
            nestedBuilders.ForEach(x => x.FixUp(targetPackage));
        }

        /// <summary>
        /// FixUp all methods.
        /// </summary>
        protected virtual void FixUpMethods(DexTargetPackage targetPackage)
        {
            // FixUp methods
            if (methodBuilders != null) methodBuilders.ForEach(x => x.FixUp(targetPackage));
        }

        /// <summary>
        /// Implemented all fields and methods.
        /// </summary>
        protected virtual void CreateMembers(DexTargetPackage targetPackage)
        {
            // Build fields
            fieldBuilders = typeDef.Fields.Where(ShouldImplementField).SelectMany(x => FieldBuilder.Create(compiler, x)).ToList();
            fieldBuilders.ForEach(x => x.Create(classDef, XType, targetPackage));

            // Build GenericInstance field (if generic)
            if (typeDef.HasGenericParameters && !typeDef.HasDexImportAttribute())
            {
                //Class.IsGenericClass = true;
                if (!typeDef.IsStatic())
                {
                    CreateGenericInstanceField(targetPackage);
                }
            }

            // Build methods
            methodBuilders = typeDef.Methods.Where(ShouldImplementMethod).Select(x => MethodBuilder.Create(compiler, x)).ToList();
            methodBuilders.ForEach(x => x.Create(classDef, targetPackage));

            // Implement members
            fieldBuilders.ForEach(x => x.Implement(classDef, targetPackage));
        }

        /// <summary>
        /// Create and add GenericInstance field.
        /// </summary>
        protected virtual void CreateGenericInstanceField(DexTargetPackage targetPackage)
        {
            var field = new FieldDefinition {
                Name = CreateUniqueFieldName("$g"),
                Type = FrameworkReferences.ClassArray,
                AccessFlags = AccessFlags.Protected | AccessFlags.Synthetic,
                Owner = Class
            };
            Class.Fields.Add(field);
            Class.GenericInstanceField = field;
        }

        /// <summary>
        /// Create all annotations for this class and it's members
        /// </summary>
        void IClassBuilder.CreateAnnotations(ITargetPackage targetPackage)
        {
            CreateAnnotations((DexTargetPackage)targetPackage);
        }

        /// <summary>
        /// Create all annotations for this class and it's members
        /// </summary>
        public virtual void CreateAnnotations(DexTargetPackage targetPackage)
        {
            // Build class annotations
            if (Class != null)
            {
                // Custom attributes
                AnnotationBuilder.Create(compiler, Type, Class, targetPackage);

                // Properties
                if ((methodBuilders != null) && compiler.AddPropertyAnnotations())
                {
                    AddPropertiesAnnotation(targetPackage);
                }

                AddDefaultAnnotations(targetPackage);
            }

            // Build nested class annotation
            nestedBuilders.ForEach(x => x.CreateAnnotations(targetPackage));

            // Build field annotations
            if (fieldBuilders != null) fieldBuilders.ForEach(x => x.CreateAnnotations(targetPackage));

            // Build method annotations
            if (methodBuilders != null) methodBuilders.ForEach(x => x.CreateAnnotations(targetPackage));
        }

        private void AddPropertiesAnnotation(DexTargetPackage targetPackage)
        {
            // Find property accessors
            var propertyMap = new Dictionary<PropertyDefinition, MethodBuilder[]>();
            foreach (var methodBuilder in methodBuilders)
            {
                PropertyDefinition propertyDef;
                bool isSetter;
                if (!methodBuilder.IsPropertyAccessor(out propertyDef, out isSetter))
                    continue;
                MethodBuilder[] accessors;
                if (!propertyMap.TryGetValue(propertyDef, out accessors))
                {
                    accessors = new MethodBuilder[2];
                    propertyMap[propertyDef] = accessors;
                }
                accessors[isSetter ? 1 : 0] = methodBuilder;
            }

            // Build annotations
            if (propertyMap.Count > 0)
            {
                var propertyClass = compiler.GetDot42InternalType("IProperty").GetClassReference(targetPackage);
                var propertiesClass = compiler.GetDot42InternalType("IProperties").GetClassReference(targetPackage);
                var propertyAnnotations = new List<Annotation>();

                foreach (var pair in propertyMap)
                {
                    var provider = new PropertyAnnotationProvider {Annotations = new List<Annotation>()};
                    AnnotationBuilder.Create(compiler, pair.Key, provider, targetPackage, true);

                    string propName = pair.Key.Name;

                    var ann = new Annotation(propertyClass, AnnotationVisibility.Runtime,
                        new AnnotationArgument("Name", propName));
                    if (pair.Value[0] != null)
                    {
                        var getter = pair.Value[0].DexMethod;

                        if (getter.Prototype.Parameters.Count > 0)
                        {
                            DLog.Info(DContext.CompilerCodeGenerator,
                                "not generating property for getter with arguments " + getter);
                            continue;
                        }

                        var getterName = getter.Name;
                        if (getterName != "get_" + propName)
                            ann.Arguments.Add(new AnnotationArgument("Get", getterName));
                    }

                    if (pair.Value[1] != null)
                    {
                        var setter = pair.Value[1].DexMethod;
                        if (setter.Prototype.Parameters.Count != 1)
                        {
                            DLog.Info(DContext.CompilerCodeGenerator,
                                "not generating property for setter with wrong argument count " + setter);
                            continue;
                        }


                        var setterName = setter.Name;
                        if (setterName != "set_" + propName)
                            ann.Arguments.Add(new AnnotationArgument("Set", setterName));
                    }

                    //propType = pair.Key.PropertyType;
                    // Mono.Cecil.TypeReference propType = null;

                    var attributes = provider.Annotations.FirstOrDefault();
                    if (attributes != null && attributes.Arguments[0].Value != null)
                    {
                        ann.Arguments.Add(new AnnotationArgument("Attributes", attributes.Arguments[0].Value));
                    }
                    propertyAnnotations.Add(ann);
                }

                var propAnn = new Annotation(propertiesClass, AnnotationVisibility.Runtime,
                    new AnnotationArgument("Properties", propertyAnnotations.ToArray()));
                Class.Annotations.Add(propAnn);
            }
        }

        private void AddDefaultAnnotations(DexTargetPackage targetPackage)
        {
            // Add annotation defaults
            if ((Type.Namespace == InternalConstants.Dot42InternalNamespace) && (Type.Name == "IProperty"))
            {
                var propertyClass = compiler.GetDot42InternalType("IProperty").GetClassReference(targetPackage);
                var defValue = new Annotation(propertyClass, AnnotationVisibility.Runtime,
                    new AnnotationArgument("Get", ""),
                    new AnnotationArgument("Set", ""),
                    new AnnotationArgument("Attributes", new Annotation[0]));
                var defAnnotation = new Annotation(new ClassReference("dalvik.annotation.AnnotationDefault"),
                    AnnotationVisibility.System, new AnnotationArgument("value", defValue));
                Class.Annotations.Add(defAnnotation);
            }
            // Add annotation defaults
            if ((Type.Namespace == InternalConstants.Dot42InternalNamespace) &&
                (Type.Name == InternalConstants.GenericDefinitionAnnotation))
            {
                var annotationClass = compiler.GetDot42InternalType(InternalConstants.GenericDefinitionAnnotation)
                                              .GetClassReference(targetPackage);
                var objectClass = compiler.Module.TypeSystem.Object.GetClassReference(targetPackage);

                var defValue = new Annotation(annotationClass, AnnotationVisibility.Runtime,
                    new AnnotationArgument("GenericArguments", new object[0]),
                    new AnnotationArgument("GenericInstanceType", objectClass),
                    new AnnotationArgument("GenericTypeDefinition", objectClass),
                    new AnnotationArgument("GenericParameter", -1));

                var defAnnotation = new Annotation(new ClassReference("dalvik.annotation.AnnotationDefault"),
                    AnnotationVisibility.System, new AnnotationArgument("value", defValue));
                Class.Annotations.Add(defAnnotation);
            }

            // Add annotation defaults
            if ((Type.Namespace == InternalConstants.Dot42InternalNamespace) 
                && (Type.Name == InternalConstants.TypeReflectionInfoAnnotation))
            {
                var annotationClass = compiler.GetDot42InternalType(InternalConstants.TypeReflectionInfoAnnotation)
                                              .GetClassReference(targetPackage);

                var defValue = new Annotation(annotationClass, AnnotationVisibility.Runtime,
                    new AnnotationArgument("GenericArgumentField", ""),
                    new AnnotationArgument("GenericArgumentCount", 0),
                    new AnnotationArgument("GenericDefinitions", new Annotation[0]),
                    new AnnotationArgument("Fields", new Annotation[0]));

                var defAnnotation = new Annotation(new ClassReference("dalvik.annotation.AnnotationDefault"),
                    AnnotationVisibility.System, new AnnotationArgument("value", defValue));
                Class.Annotations.Add(defAnnotation);
            }
        }

        /// <summary>
        /// Should the given field be implemented?
        /// </summary>
        protected virtual bool ShouldImplementField(Mono.Cecil.FieldDefinition field)
        {
            return field.IsReachable && !field.HasDexImportAttribute();
        }

        /// <summary>
        /// Should the given method be implemented?
        /// </summary>
        protected virtual bool ShouldImplementMethod(MethodDefinition method)
        {
            return method.IsReachable && !method.HasDexImportAttribute() && !method.HasDexNativeAttribute();
        }

        /// <summary>
        /// Generate code for all methods.
        /// </summary>
        void IClassBuilder.GenerateCode(ITargetPackage targetPackage)
        {
            GenerateCode((DexTargetPackage)targetPackage);
        }

        /// <summary>
        /// Generate code for all methods.
        /// </summary>
        public virtual void GenerateCode(DexTargetPackage targetPackage)
        {
            if (nestedBuilders != null) nestedBuilders.ForEach(x => x.GenerateCode(targetPackage));
            if (methodBuilders != null) methodBuilders.ForEach(x => x.GenerateCode(classDef, targetPackage));
        }

        /// <summary>
        /// Gets the generated class definition.
        /// </summary>
        internal ClassDefinition Class { get { return classDef; } }

        /// <summary>
        /// Gets the source type definition
        /// </summary>
        internal TypeDefinition Type { get { return typeDef; } }

        /// <summary>
        /// Gets the containing compiler
        /// </summary>
        protected AssemblyCompiler Compiler { get { return compiler; } }

        /// <summary>
        /// Create a field name that is based on the given name, and is unique in this class.
        /// </summary>
        protected string CreateUniqueFieldName(string name)
        {
            var baseName = name;
            var index = 0;
            while (true)
            {
                if (Class.Fields.All(x => x.Name != name))
                    return name;
                name = baseName + (index++);
            }
        }

        private class PropertyAnnotationProvider : IAnnotationProvider
        {
            public List<Annotation> Annotations { get; set; }
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
