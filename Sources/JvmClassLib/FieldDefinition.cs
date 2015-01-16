using System.Linq;
using Dot42.JvmClassLib.Attributes;

namespace Dot42.JvmClassLib
{
    public sealed class FieldDefinition : MemberDefinition, IModifiableAttributeProvider, IAccessModifiers
    {
        private readonly FieldAccessFlags accessFlags;
        private readonly string descriptor;
        private readonly TypeReference fieldType;

        public FieldDefinition(ClassFile cf, FieldAccessFlags accessFlags, string name, string descriptor, string signature)
            : base(cf, name)
        {
            this.accessFlags = accessFlags;
            this.descriptor = descriptor;
            fieldType = Descriptors.ParseFieldType(descriptor);
            Signature = (signature != null) ? Signatures.ParseFieldTypeSignature(signature) : fieldType;
        }

        /// <summary>
        /// Clone this definition for use in a given other class.
        /// </summary>
        public FieldDefinition CloneTo(ClassFile otherClass)
        {
            var clone = new FieldDefinition(otherClass, accessFlags, Name, descriptor, null);
            clone.Signature = Signature;
            return clone;
        }

        /// <summary>
        /// Gets the access flags
        /// </summary>
        public FieldAccessFlags AccessFlags { get { return accessFlags; } }

        /// <summary>
        /// Is this a public field?
        /// </summary>
        public bool IsPublic { get { return accessFlags.HasFlag(FieldAccessFlags.Public); } }

        /// <summary>
        /// Visible to class, package
        /// </summary>
        public bool IsPackagePrivate { get { return ((accessFlags & FieldAccessFlags.AccessMask) == 0); } }

        /// <summary>
        /// Is this a private field?
        /// </summary>
        public bool IsPrivate { get { return accessFlags.HasFlag(FieldAccessFlags.Private); } }

        /// <summary>
        /// Is this a protected field?
        /// </summary>
        public bool IsProtected { get { return accessFlags.HasFlag(FieldAccessFlags.Protected); } }

        /// <summary>
        /// Is this a static field?
        /// </summary>
        public bool IsStatic { get { return accessFlags.HasFlag(FieldAccessFlags.Static); } }

        /// <summary>
        /// Is this a final field?
        /// </summary>
        public bool IsFinal { get { return accessFlags.HasFlag(FieldAccessFlags.Final); } }

        /// <summary>
        /// Is this a volatile field?
        /// </summary>
        public bool IsVolatile { get { return accessFlags.HasFlag(FieldAccessFlags.Volatile); } }

        /// <summary>
        /// Is this a transient field?
        /// </summary>
        public bool IsTransient { get { return accessFlags.HasFlag(FieldAccessFlags.Transient); } }

        /// <summary>
        /// Is this field Synthetic?
        /// </summary>
        public bool IsSynthetic { get { return accessFlags.HasFlag(FieldAccessFlags.Synthetic); } }

        /// <summary>
        /// Is this an enum field?
        /// </summary>
        public bool IsEnum { get { return accessFlags.HasFlag(FieldAccessFlags.Enum); } }

        /// <summary>
        /// Type of this field.
        /// </summary>
        public TypeReference FieldType { get { return fieldType; } }

        /// <summary>
        /// Gets field type as descriptor
        /// </summary>
        public string Descriptor { get { return descriptor; } }

        /// <summary>
        /// Gets the constant value of this field (if any).
        /// Null otherwise.
        /// </summary>
        public object ConstantValue { get { return Attributes.OfType<ConstantValueAttribute>().Select(x => x.Value).SingleOrDefault(); } }

        /// <summary>
        /// Return type with generics
        /// </summary>
        public TypeReference Signature { get; private set; }

        /// <summary>
        /// Adds the given attribute to this provider.
        /// </summary>
        void IModifiableAttributeProvider.Add(Attribute attribute)
        {
            Add(attribute);
        }

        /// <summary>
        /// Signals that all attributes have been loaded.
        /// </summary>
        void IModifiableAttributeProvider.AttributesLoaded()
        {
            var signatureAttributes = Attributes.OfType<SignatureAttribute>();
            Signature = signatureAttributes.Select(x => Signatures.ParseFieldTypeSignature(x.Value)).SingleOrDefault() ?? fieldType;
        }
    }
}
