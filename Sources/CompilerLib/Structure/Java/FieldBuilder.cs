using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Java;
using Dot42.DexLib;
using Dot42.Mapping;
using FieldDefinition = Dot42.JvmClassLib.FieldDefinition;

namespace Dot42.CompilerLib.Structure.Java
{
    internal class FieldBuilder
    {
        private readonly AssemblyCompiler compiler;
        private readonly FieldDefinition field;
        private Dot42.DexLib.FieldDefinition dfield;
        private XFieldDefinition xField;

        /// <summary>
        /// Default ctor
        /// </summary>
        public static FieldBuilder Create(AssemblyCompiler compiler, FieldDefinition field)
        {
            return new FieldBuilder(compiler, field);
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
        public void Create(ClassDefinition declaringClass, DexTargetPackage targetPackage)
        {
            // Find xField
            xField = XBuilder.AsFieldDefinition(compiler.Module, field);

            // Create field definition
            dfield = new Dot42.DexLib.FieldDefinition();
            dfield.Name = NameConverter.GetConvertedName(field);
            AddFieldToDeclaringClass(declaringClass, dfield, targetPackage);
            targetPackage.NameConverter.Record(xField, dfield);

            // Set access flags
            SetAccessFlags(dfield, field);
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
            if (field.IsPrivate) dfield.IsPrivate = true;
            if (field.IsProtected) dfield.IsProtected = true;
            if (field.IsPublic) dfield.IsPublic = true;

            if (field.IsFinal) dfield.IsFinal = true;
            if (field.IsStatic) dfield.IsStatic = true;
            if (field.IsTransient) dfield.IsTransient = true;
            if (field.IsVolatile) dfield.IsVolatile = true;
            if (field.IsSynthetic) dfield.IsSynthetic = true;
            if (field.IsEnum) dfield.IsEnum = true;
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        public void Implement(ClassDefinition declaringClass, DexTargetPackage targetPackage)
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
            if (dfield == null)
                return;

            // Add annotations from java
            AnnotationBuilder.BuildAnnotations(field, dfield, targetPackage, compiler.Module);
        }

        /// <summary>
        /// Record the mapping from .NET to Dex
        /// </summary>
        public void RecordMapping(TypeEntry typeEntry)
        {
            var entry = new FieldEntry(field.Name, field.FieldType.ClassName, dfield.Name, dfield.Type.ToString());
            typeEntry.Fields.Add(entry);
        }

        /// <summary>
        /// Set the field type of the given dex field.
        /// </summary>
        protected virtual void SetFieldType(DexLib.FieldDefinition dfield, FieldDefinition field, DexTargetPackage targetPackage)
        {
            dfield.Type = field.FieldType.GetReference(XTypeUsageFlags.FieldType, targetPackage, compiler.Module);            
        }

        /// <summary>
        /// Set the value of the given dex field.
        /// </summary>
        protected virtual void SetFieldValue(DexLib.FieldDefinition dfield, FieldDefinition field)
        {
            dfield.Value = field.ConstantValue;
        }
    }
}
