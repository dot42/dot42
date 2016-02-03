using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;

namespace Dot42.CompilerLib.XModel.Synthetic
{
    /// <summary>
    /// Field definition that is created by this compiler.
    /// </summary>
    public class XSyntheticFieldDefinition : XFieldDefinition
    {
        private readonly XSyntheticFieldFlags flags;
        private readonly string name;
        private readonly XTypeReference fieldType;
        private readonly object initialValue;
        private DexLib.FieldDefinition dexField;

        /// <summary>
        /// Default ctor
        /// </summary>
        private XSyntheticFieldDefinition(XTypeDefinition declaringType, XSyntheticFieldFlags flags, string name, XTypeReference fieldType, object initialValue)
            : base(declaringType)
        {
            this.flags = flags;
            this.name = name;
            this.fieldType = fieldType;
            this.initialValue = initialValue;
        }

        
        /// <summary>
        /// Create a synthetic field and add it to the given declaring type.
        /// </summary>
        public static XSyntheticFieldDefinition Create(XTypeDefinition declaringType, XSyntheticFieldFlags flags, string name, XTypeReference fieldType, object initialValue = null)
        {
            var field = new XSyntheticFieldDefinition(declaringType, flags, name, fieldType, initialValue);
            declaringType.Add(field);
            return field;
        }

        /// <summary>
        /// Name of the reference
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Type of field
        /// </summary>
        public override XTypeReference FieldType
        {
            get { return fieldType; }
        }

        /// <summary>
        /// Is this a static method?
        /// </summary>
        public override bool IsStatic
        {
            get { return flags.HasFlag(XSyntheticFieldFlags.Static); }
        }

        /// <summary>
        /// Is this a readonly field?
        /// </summary>
        public override bool IsReadOnly
        {
            get { return flags.HasFlag(XSyntheticFieldFlags.ReadOnly); }
        }

        /// <summary>
        /// Does this field have a name with a special meaning for the runtime?
        /// </summary>
        public override bool IsRuntimeSpecialName
        {
            get { return false; }
        }

        /// <summary>
        /// Is this a private field?
        /// </summary>
        public bool IsPrivate
        {
            get { return flags.HasFlag(XSyntheticFieldFlags.Private); }
        }

        /// <summary>
        /// Is this a protected field?
        /// </summary>
        public bool IsProtected
        {
            get { return flags.HasFlag(XSyntheticFieldFlags.Protected); }
        }

        /// <summary>
        /// Is this a public field?
        /// </summary>
        public bool IsPublic
        {
            get { return flags.HasFlag(XSyntheticFieldFlags.Public); }
        }

        /// <summary>
        /// Is this field used in code?
        /// </summary>
        public override bool IsReachable
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the initial value (if any) of this field.
        /// </summary>
        public override object InitialValue
        {
            get { return initialValue; }
        }

        /// <summary>
        /// Try to get the value of the enum const if this field is an enum const field.
        /// </summary>
        public override bool TryGetEnumValue(out object value)
        {
            value = null;
            return false;
        }

        /// <summary>
        /// Try to get the names from the DexImport attribute attached to this field.
        /// </summary>
        public override bool TryGetDexImportNames(out string fieldName, out string descriptor, out string className)
        {
            fieldName = null;
            descriptor = null;
            className = null;
            return false;
        }

        /// <summary>
        /// Try to get the names from the JavaImport attribute attached to this field.
        /// </summary>
        public override bool TryGetJavaImportNames(out string fieldName, out string descriptor, out string className)
        {
            fieldName = null;
            descriptor = null;
            className = null;
            return false;
        }

        /// <summary>
        /// Try to get the name of the ResourceId attribute attached to this field.
        /// </summary>
        public override bool TryGetResourceIdAttribute(out string resourceName)
        {
            resourceName = null;
            return false;
        }

        /// <summary>
        /// Create a dex field definition from this field.
        /// </summary>
        public DexLib.FieldDefinition GetDexField(DexLib.ClassDefinition owner, DexTargetPackage targetPackage)
        {
            if (dexField == null)
            {
                var fdef = new DexLib.FieldDefinition(owner, name, fieldType.GetReference(targetPackage));

                if      (IsStatic)    fdef.IsStatic = true;
                if      (IsReadOnly)  fdef.IsFinal = true;

                if      (IsPrivate)   fdef.IsPrivate = true;
                else if (IsProtected) fdef.IsProtected = true;
                else                  fdef.IsPublic = true;
                
                dexField = fdef;
                targetPackage.NameConverter.Record(this, dexField);
            }
            return dexField;
        }
    }
}
