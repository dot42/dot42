using System.Collections.Generic;

namespace Dot42.DexLib
{
    public class FieldDefinition : FieldReference, IMemberDefinition
    {
        public FieldDefinition()
        {
            Annotations = new List<Annotation>();
        }

        public FieldDefinition(ClassDefinition owner, string name, TypeReference fieldType) : this()
        {
            Owner = owner;
            Name = name;
            Type = fieldType;
        }

        // for prefetching
        internal FieldDefinition(FieldReference fref) : this()
        {
            Owner = fref.Owner as ClassDefinition;
            Type = fref.Type;
            Name = fref.Name;
        }

        #region " AccessFlags "

        public bool IsPublic
        {
            get { return (AccessFlags & AccessFlags.Public) != 0; }
            set { AccessFlags |= AccessFlags.Public; }
        }

        public bool IsPrivate
        {
            get { return (AccessFlags & AccessFlags.Private) != 0; }
            set { AccessFlags |= AccessFlags.Private; }
        }

        public bool IsProtected
        {
            get { return (AccessFlags & AccessFlags.Protected) != 0; }
            set { AccessFlags |= AccessFlags.Protected; }
        }

        public bool IsStatic
        {
            get { return (AccessFlags & AccessFlags.Static) != 0; }
            set { AccessFlags |= AccessFlags.Static; }
        }

        public bool IsFinal
        {
            get { return (AccessFlags & AccessFlags.Final) != 0; }
            set { AccessFlags |= AccessFlags.Final; }
        }

        public bool IsVolatile
        {
            get { return (AccessFlags & AccessFlags.Volatile) != 0; }
            set { AccessFlags |= AccessFlags.Volatile; }
        }

        public bool IsTransient
        {
            get { return (AccessFlags & AccessFlags.Transient) != 0; }
            set { AccessFlags |= AccessFlags.Transient; }
        }

        public bool IsSynthetic
        {
            get { return (AccessFlags & AccessFlags.Synthetic) != 0; }
            set { AccessFlags |= AccessFlags.Synthetic; }
        }

        public bool IsEnum
        {
            get { return (AccessFlags & AccessFlags.Enum) != 0; }
            set { AccessFlags |= AccessFlags.Enum; }
        }

        #endregion

        #region " IEquatable "

        public override bool Equals(IMemberReference other)
        {
            return (other is FieldDefinition)
                   && Equals(other as FieldDefinition);
        }

        public bool Equals(FieldDefinition other)
        {
            // Should be enough (ownership)
            return base.Equals(other);
        }

        #endregion

        public object Value { get; set; }

        #region IMemberDefinition Members

        public AccessFlags AccessFlags { get; set; }

        public new ClassDefinition Owner
        {
            get { return base.Owner as ClassDefinition; }
            set { base.Owner = value; }
        }

        public List<Annotation> Annotations { get; set; }

        #endregion
    }
}