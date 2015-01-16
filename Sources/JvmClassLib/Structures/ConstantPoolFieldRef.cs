using System.Linq;

namespace Dot42.JvmClassLib.Structures
{
    public sealed class ConstantPoolFieldRef : ConstantPoolMemberRef
    {
        private FieldDefinition resolved;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolFieldRef(ConstantPool constantPool, int classIndex, int nameAndTypeIndex)
            : base(constantPool, classIndex, nameAndTypeIndex)
        {
        }

        /// <summary>
        /// Does this field have a long/double field type
        /// </summary>
        public bool IsWide
        {
            get
            {
                var descr = Descriptor;
                return (descr == "J") || (descr == "D");
            }
        }

        /// <summary>
        /// Resolve this reference.
        /// </summary>
        public FieldDefinition Resolve()
        {
            FieldDefinition field;
            if (TryResolve(out field))
                return field;
            throw new ResolveException(string.Format("Field {0} not found", this));
        }

        /// <summary>
        /// Resolve this reference.
        /// </summary>
        public bool TryResolve(out FieldDefinition field)
        {
            field = null;
            if (resolved == null)
            {
                ClassFile @class;
                if (TryResolveClass(out @class))
                {
                    while (@class != null)
                    {
                        resolved = @class.Fields.FirstOrDefault(IsMatchByNameAndDescriptor);
                        if (resolved != null)
                        {
                            break;
                        }
                        if (!@class.TryGetSuperClass(out @class))
                            break;
                    }
                }
            }
            field = resolved;
            return (field != null);
        }

        /// <summary>
        /// Match the given field on name and descriptor.
        /// </summary>
        private bool IsMatchByNameAndDescriptor(FieldDefinition field)
        {
            return (Name == field.Name) && (Descriptor == field.Descriptor);
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.Fieldref; }
        }
    }
}
