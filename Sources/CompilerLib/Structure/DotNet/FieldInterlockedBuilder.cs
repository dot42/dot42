using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Java;
using Dot42.CompilerLib.XModel.Synthetic;
using Dot42.DexLib;
using Dot42.JvmClassLib;
using Dot42.Mapping;
using FieldDefinition = Mono.Cecil.FieldDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// builds AutomicUpdater fields for fields that are used in System.Threading.Interlocked calls.
    /// </summary>
    internal class FieldInterlockedBuilder : FieldBuilder
    {
        private readonly FieldBuilder _baseFieldBuilder;

        public FieldInterlockedBuilder(AssemblyCompiler compiler, FieldDefinition field, FieldBuilder fieldBuilder) : base(compiler, field)
        {
            _baseFieldBuilder = fieldBuilder;
        }

     

        public override void Create(ClassDefinition declaringClass, XTypeDefinition declaringType, DexTargetPackage targetPackage)
        {
            if (_baseFieldBuilder.dfield == null)
                return;

            // can't create udater for static fields.
            if (field.IsStatic)
                return;

            var updaterType = GetAtomicFieldUpdaterType(field.FieldType);
            if (updaterType == null)
                return;

            var fullUpdateTypeName = "Java.Util.Concurrent.Atomic." + updaterType;

            // create matching xField. Note: this seems to be a hack. what to do?

            var objType = new ObjectTypeReference(fullUpdateTypeName, new TypeArgument[0]);
            var javaTypeReference = new XBuilder.JavaTypeReference(Compiler.Module, objType, objType.ClassName);

            var basexField = _baseFieldBuilder.xField;
            var basedField = _baseFieldBuilder.dfield;
            var fieldName = basedField.Name + NameConstants.Atomic.FieldUpdaterPostfix;

            var xflags = XSyntheticFieldFlags.Static | XSyntheticFieldFlags.ReadOnly;

            if (basedField.IsProtected)
                xflags |= XSyntheticFieldFlags.Protected;
            if (basedField.IsPrivate)
                xflags |= XSyntheticFieldFlags.Private;


            var xAtomicField = XSyntheticFieldDefinition.Create(basexField.DeclaringType, xflags, fieldName, javaTypeReference);
            xField = xAtomicField;

            // create dfield.
            
            dfield = new DexLib.FieldDefinition
            {
                Name = fieldName,
                IsStatic = true,
                IsFinal = true,
                IsSynthetic = true,
                // same access as the original field.
                IsPublic = basedField.IsPublic,
                IsPrivate = basedField.IsPrivate,
                IsProtected = basedField.IsProtected,
            };
            
            AddFieldToDeclaringClass(declaringClass, dfield, targetPackage);

            targetPackage.NameConverter.Record(xField, dfield);
        }

        public override void Implement(ClassDefinition declaringClass, DexTargetPackage targetPackage)
        {
            if (xField == null)
                return;

            dfield.Type = xField.FieldType.GetReference(targetPackage);
        }

        public override void RecordMapping(TypeEntry typeEntry)
        {
            //var entry = new FieldEntry(xField.Name, xField.FieldType.FullName, dfield.Name, dfield.Type.ToString());
            //typeEntry.Fields.Add(entry);
        }

        internal override void CreateAnnotations(DexTargetPackage targetPackage)
        {
        }

        private string GetAtomicFieldUpdaterType(Mono.Cecil.TypeReference fieldType)
        {
            if (fieldType.IsInt64())
                return "AtomicLongFieldUpdater`1";
            if (fieldType.IsInt32())
                return "AtomicIntegerFieldUpdater`1";
            if (fieldType.IsPrimitive)
                return null;
            return "AtomicReferenceFieldUpdater`2";
        }
    }
}
