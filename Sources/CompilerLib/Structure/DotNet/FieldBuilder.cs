using System;
using System.Collections.Generic;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.CompilerLib.XModel.Synthetic;
using Dot42.DexLib;
using Dot42.Mapping;
using Dot42.Utility;
using Mono.Cecil;
using FieldDefinition = Mono.Cecil.FieldDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    internal class FieldBuilder
    {
        private readonly AssemblyCompiler compiler;
        protected readonly FieldDefinition field;
        protected internal Dot42.DexLib.FieldDefinition dfield;
        protected internal XFieldDefinition xField;
        /// <summary>
        /// Default ctor
        /// </summary>
        public static IEnumerable<FieldBuilder> Create(AssemblyCompiler compiler, FieldDefinition field)
        {
            if (field.IsAndroidExtension())
                return new[] {new DexImportFieldBuilder(compiler, field)};
            if (field.DeclaringType.IsEnum)
            {
                if (!field.IsStatic)
                    throw new ArgumentException("value field should not be implemented this way");
                return new[] {new EnumFieldBuilder(compiler, field)};
            }

            var fieldBuilder = new FieldBuilder(compiler, field);
            if (!field.IsUsedInInterlocked)
                return new[] { fieldBuilder };

            return new[] { fieldBuilder, new FieldInterlockedBuilder(compiler, field, fieldBuilder) };
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        protected FieldBuilder(AssemblyCompiler compiler, FieldDefinition field)
        {
            this.compiler = compiler;
            this.field = field;
        }

        /// <summary>
        /// Gets containing compiler
        /// </summary>
        public AssemblyCompiler Compiler
        {
            get { return compiler; }
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        public virtual void Create(ClassDefinition declaringClass, XTypeDefinition declaringType, DexTargetPackage targetPackage)
        {
            // Find xfield
            xField = XBuilder.AsFieldDefinition(compiler.Module, field);

            // Create field definition
            dfield = new Dot42.DexLib.FieldDefinition();
            dfield.Name = NameConverter.GetConvertedName(field);
            AddFieldToDeclaringClass(declaringClass, dfield, targetPackage);
            targetPackage.NameConverter.Record(xField, dfield);

            // Set access flags
            SetAccessFlags(dfield, field);

            // give warning if static in generic class.
            if (field.IsStatic && declaringType.IsGenericClass)
            {
                DLog.Warning(DContext.CompilerILConverter, "Static field {0} in generic class {1}: All generic instances will share the same static field, contrary on how CLR operates. A workaround is to use ConcurrentDictionaries to access the values dependent on the type.", field.Name, declaringType.FullName);
            }
        }

        /// <summary>
        /// Add the given method to its declaring class.
        /// </summary>
        protected virtual void AddFieldToDeclaringClass(ClassDefinition declaringClass, DexLib.FieldDefinition dmethod, DexTargetPackage targetPackage)
        {
            dmethod.Owner = declaringClass;
            declaringClass.Fields.Add(dmethod);
        }

        /// <summary>
        /// Set the access flags of the created field.
        /// </summary>
        protected virtual void SetAccessFlags(DexLib.FieldDefinition dfield, FieldDefinition field)
        {
            // subclass accesses have already been fixed on an actual use basis.
            if (field.IsPrivate)
                dfield.IsPrivate = true;
            else if (field.IsFamily)
                dfield.IsProtected = true;
            else
                dfield.IsPublic = true;

            if (field.IsInitOnly) dfield.IsFinal = true;
            if (field.IsStatic) dfield.IsStatic = true;

            if (field.IsCompilerGenerated())
                dfield.IsSynthetic = true;

            dfield.IsVolatile = field.IsUsedInInterlocked || IsVolatile(field); ;
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        public virtual void Implement(ClassDefinition declaringClass, DexTargetPackage targetPackage)
        {
            if (dfield == null)
                return;

            SetFieldType(dfield, field, targetPackage);
            SetFieldValue(dfield, field);
        }
        
        /// <summary>
        /// Create all annotations for this field
        /// </summary>
        internal virtual void CreateAnnotations(DexTargetPackage targetPackage)
        {
            // Build field annotations
            AnnotationBuilder.Create(compiler, field, dfield, targetPackage);

            dfield.AddGenericDefinitionAnnotationIfGeneric(xField.FieldType, compiler, targetPackage);
        }

        /// <summary>
        /// Record the mapping from .NET to Dex
        /// </summary>
        public virtual void RecordMapping(TypeEntry typeEntry)
        {
            var entry = new FieldEntry(field.Name, field.FieldType.FullName, dfield.Name, dfield.Type.ToString());
            typeEntry.Fields.Add(entry);
        }

        /// <summary>
        /// Set the field type of the given dex field.
        /// </summary>
        protected virtual void SetFieldType(DexLib.FieldDefinition dfield, FieldDefinition field, DexTargetPackage targetPackage)
        {
            dfield.Type = field.FieldType.GetReference(targetPackage, compiler.Module);            
        }

        /// <summary>
        /// Set the value of the given dex field.
        /// </summary>
        protected virtual void SetFieldValue(DexLib.FieldDefinition dfield, FieldDefinition field)
        {
            var constant = field.Constant;
            if (constant != null)
            {
                var fieldType = field.FieldType;
                if (fieldType.IsByte())
                {
                    constant = XConvert.ToByte(constant);
                }
                else if (fieldType.IsUInt16())
                {
                    constant = XConvert.ToShort(constant);
                }
                else if (fieldType.IsUInt32())
                {
                    constant = XConvert.ToInt(constant);
                }
                else if (fieldType.IsUInt64())
                {
                    constant = XConvert.ToLong(constant);
                }
            }
            dfield.Value = constant;
        }

        private static bool IsVolatile(FieldDefinition field)
        {
            bool isVolatile = false;
            TypeSpecification modtype = field.FieldType as TypeSpecification;
            while (modtype != null)
            {
                if (modtype.IsRequiredModifier)
                {
                    var req = (RequiredModifierType)modtype;
                    if (req.ModifierType.Name == "IsVolatile")
                    {
                        isVolatile = true;
                        break;
                    }
                }
                modtype = modtype.ElementType as TypeSpecification;
            }
            return isVolatile;
        }
    }
}
