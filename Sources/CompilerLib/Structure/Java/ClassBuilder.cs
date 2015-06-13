using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Java;
using Dot42.DexLib;
using Dot42.JvmClassLib;
using Dot42.Mapping;

namespace Dot42.CompilerLib.Structure.Java
{
    /// <summary>
    /// Build ClassDefinition structures for existing java classes.
    /// </summary>
    [DebuggerDisplay("{@Type}")]
    internal abstract class ClassBuilder : IClassBuilder
    {
        private readonly AssemblyCompiler compiler;
        private readonly ClassFile typeDef;
        private ClassDefinition classDef;
        private List<ClassBuilder> nestedBuilders;
        private List<FieldBuilder> fieldBuilders;
        private List<MethodBuilder> methodBuilders;
        private XTypeDefinition xType;

        /// <summary>
        /// Create a type builder for the given type.
        /// </summary>
        internal static ClassBuilder Create(AssemblyCompiler compiler, ClassFile typeDef)
        {
            return new StandardClassBuilder(compiler, typeDef);
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        protected ClassBuilder(AssemblyCompiler compiler, ClassFile typeDef)
        {
            this.compiler = compiler;
            this.typeDef = typeDef;
        }

        /// <summary>
        /// Sorting low comes first
        /// </summary>
        int IClassBuilder.SortPriority { get { return -1000; /* create java types before .Net types */ } }

        /// <summary>
        /// Gets fullname of the underlying type.
        /// </summary>
        string IClassBuilder.FullName
        {
            get { return Type.ClassName; }
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
            var dtp = (DexTargetPackage) targetPackage;
            Create(dtp.DexFile, dtp.NameConverter);
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        public void Create(Dex target, NameConverter nsConverter)
        {
            Create(target, nsConverter, null, null, null);
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected void Create(Dex target, NameConverter nsConverter, ClassDefinition parent, ClassFile parentClass, XTypeDefinition parentXType)
        {
            xType = new XBuilder.JavaTypeDefinition(compiler.Module, parentXType, typeDef);
            CreateClassDefinition(target, nsConverter, parent, parentClass);
            CreateNestedClasses(target, nsConverter, parent);
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected virtual void CreateClassDefinition(Dex target, NameConverter nsConverter, ClassDefinition parent, ClassFile parentClass)
        {
            // Create classdef
            classDef = new ClassDefinition();
            classDef.MapFileId = compiler.GetNextMapFileId();
            classDef.Namespace = nsConverter.GetConvertedNamespace(typeDef);
            var name = NameConverter.GetConvertedName(typeDef);
            classDef.Name = (parent != null) ? parent.Name + "$" + name : name;

            // Set access flags
            //if (typeDef.IsPublic) classDef.IsPublic = true;
            //else classDef.IsPrivate = true;
            classDef.IsPublic = true;
            if (typeDef.IsFinal) classDef.IsFinal = true;
            if (typeDef.IsInterface)
            {
                classDef.IsInterface = true;
                classDef.IsAbstract = true;
            }
            else if (typeDef.IsAbstract)
            {
                classDef.IsAbstract = true;
            }
            if (typeDef.Interfaces.Any(x => x.ClassName == "java/lang/annotation/Annotation"))
            {
                classDef.IsAnnotation = true;
            }

            classDef.IsEnum = typeDef.IsEnum;

            if (parent != null)
            {
                // Add to parent if this is a nested type
                classDef.Owner = parent;
                parent.AddInnerClass(classDef);
            }
            else
            {
                // Add to dex if it is a root class
                target.AddClass(classDef);
            }
        }

        /// <summary>
        /// Create the nested classes for this type.
        /// </summary>
        protected virtual void CreateNestedClasses(Dex target, NameConverter nsConverter, ClassDefinition parent)
        {
            nestedBuilders = typeDef.InnerClasses.Where(x => x.InnerClassFile.IsReachable && (x.InnerClassFile.DeclaringClass == typeDef)).Select(x => Create(compiler, x.InnerClassFile)).ToList();
            nestedBuilders.ForEach(x => x.Create(target, nsConverter, classDef, typeDef, xType));
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        void IClassBuilder.Implement(ITargetPackage targetPackage)
        {
            Implement((DexTargetPackage) targetPackage);
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        public void Implement(DexTargetPackage targetPackage)
        {
            var target = targetPackage.DexFile;
            var nameConverter = targetPackage.NameConverter;
            ImplementSuperClass(target, nameConverter);
            ImplementInterfaces(target, nameConverter);
            ImplementInnerClasses(targetPackage);
            CreateMembers(targetPackage);
        }

        /// <summary>
        /// Implement make minor fixes after the implementation phase.
        /// </summary>
        void IClassBuilder.FixUp(ITargetPackage targetPackage)
        {
            var dtp = (DexTargetPackage) targetPackage;
            FixUp(dtp.DexFile, dtp.NameConverter);
        }

        /// <summary>
        /// Implement make minor fixes after the implementation phase.
        /// </summary>
        public void FixUp(Dex target, NameConverter nsConverter)
        {
            FixUpInnerClasses(target, nsConverter);
            FixUpMethods(target, nsConverter);
        }

        /// <summary>
        /// Record the mapping from .NET to Dex
        /// </summary>
        public void RecordMapping(MapFile mapFile)
        {
            var dexName = (classDef != null) ? classDef.Fullname : null;
            var mapFileId = (classDef != null) ? classDef.MapFileId : 0;
            var entry = new TypeEntry(typeDef.ClassName, "<java>", dexName, mapFileId);
            if (fieldBuilders != null) fieldBuilders.ForEach(x => x.RecordMapping(entry));
            if (methodBuilders != null) methodBuilders.ForEach(x => x.RecordMapping(entry));
            mapFile.Add(entry);
        }

        /// <summary>
        /// Set the super class of the class definition.
        /// </summary>
        protected void ImplementSuperClass(Dex target, NameConverter nsConverter)
        {
            // Set base type
            var baseType = typeDef.SuperClass;
            if (baseType != null)
            {
                classDef.SuperClass = new ClassReference(baseType.ClassName);
            }
            else if (typeDef.IsInterface)
            {
                classDef.SuperClass = new ClassReference("java/lang/Object");
            }
            else
            {
                throw new ArgumentException(string.Format("Type {0} has no base type", typeDef.ClassName));
            }
        }

        /// <summary>
        /// Add references to all implemented interfaces.
        /// </summary>
        protected virtual void ImplementInterfaces(Dex target, NameConverter nsConverter)
        {
            // Implement interfaces
            foreach(var intf in typeDef.Interfaces.Select(x => new ClassReference(x.ClassName)))
                classDef.Interfaces.Add(intf);
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
        protected virtual void FixUpInnerClasses(Dex target, NameConverter nsConverter)
        {
            // FixUp nested type
            nestedBuilders.ForEach(x => x.FixUp(target, nsConverter));
        }

        /// <summary>
        /// FixUp all methods.
        /// </summary>
        protected virtual void FixUpMethods(Dex target, NameConverter nsConverter)
        {
            // FixUp methods
            methodBuilders.ForEach(x => x.FixUp(target, nsConverter));
        }

        /// <summary>
        /// Implemented all fields and methods.
        /// </summary>
        protected virtual void CreateMembers(DexTargetPackage targetPackage)
        {
            // Build fields
            fieldBuilders = typeDef.Fields.Where(ShouldImplementField).Select(x => FieldBuilder.Create(compiler, x)).ToList();
            fieldBuilders.ForEach(x => x.Create(classDef, targetPackage));

            // Build methods
            methodBuilders = typeDef.Methods.Where(ShouldImplementMethod).Select(x => MethodBuilder.Create(compiler, x)).ToList();
            methodBuilders.ForEach(x => x.Create(classDef, targetPackage));

            // Implement members
            fieldBuilders.ForEach(x => x.Implement(classDef, targetPackage));
        }

        /// <summary>
        /// Create all annotations for this class and it's members
        /// </summary>
        void IClassBuilder.CreateAnnotations(ITargetPackage targetPackage)
        {
            CreateAnnotations((DexTargetPackage) targetPackage);
        }

        /// <summary>
        /// Create all annotations for this class and it's members
        /// </summary>
        public virtual void CreateAnnotations(DexTargetPackage targetPackage)
        {
            // Build nested class annotation
            nestedBuilders.ForEach(x => x.CreateAnnotations(targetPackage));

            // Build field annotations
            fieldBuilders.ForEach(x => x.CreateAnnotations(targetPackage));

            // Build method annotations
            methodBuilders.ForEach(x => x.CreateAnnotations(targetPackage));

            // Add annotations from java
            AnnotationBuilder.BuildAnnotations(typeDef, classDef, targetPackage, compiler.Module);
        }

        /// <summary>
        /// Should the given field be implemented?
        /// </summary>
        protected virtual bool ShouldImplementField(JvmClassLib.FieldDefinition field)
        {
            return field.IsReachable;
        }

        /// <summary>
        /// Should the given method be implemented?
        /// </summary>
        protected virtual bool ShouldImplementMethod(JvmClassLib.MethodDefinition method)
        {
            return method.IsReachable;
        }

        /// <summary>
        /// Generate code for all methods.
        /// </summary>
        void IClassBuilder.GenerateCode(ITargetPackage targetPackage, bool stopAtFirstError)
        {
            GenerateCode((DexTargetPackage) targetPackage);
        }

        /// <summary>
        /// Generate code for all methods.
        /// </summary>
        public void GenerateCode(DexTargetPackage targetPackage)
        {
            if (nestedBuilders != null) nestedBuilders.ForEach(x => x.GenerateCode(targetPackage));
            if (methodBuilders != null) methodBuilders.ForEach(x => x.GenerateCode(classDef, targetPackage));
        }

        /// <summary>
        /// Gets the generated class definition.
        /// </summary>
        protected ClassDefinition Class { get { return classDef; } }

        /// <summary>
        /// Gets the source type definition
        /// </summary>
        internal ClassFile Type { get { return typeDef; } }

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

        public override string ToString()
        {
            return classDef.Fullname;
        }
    }
}
