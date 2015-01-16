using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Dot42.ImportJarLib.Doxygen;

namespace Dot42.ImportJarLib.Model
{
    public sealed class NetFieldDefinition : INetMemberDefinition, INetMemberVisibility
    {
        private readonly List<NetCustomAttribute> customAttributes = new List<NetCustomAttribute>();

        /// <summary>
        /// Name of the type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Human readable description.
        /// </summary>
        public DocDescription Description { get; set; }

        /// <summary>
        /// The editor browsable state of this member
        /// </summary>
        public EditorBrowsableState EditorBrowsableState { get; set; }

        /// <summary>
        /// Attributes of the field.
        /// </summary>
        public FieldAttributes Attributes { get; set; }

        /// <summary>
        /// Parent (in case of nested types)
        /// </summary>
        public NetTypeDefinition DeclaringType { get; set; }

        /// <summary>
        /// Type of the field
        /// </summary>
        public NetTypeReference FieldType { get; set; }

        /// <summary>
        /// Initialization value
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets all custom attributes
        /// </summary>
        public List<NetCustomAttribute> CustomAttributes { get { return customAttributes; } }

        /// <summary>
        /// Method name of the original java field (if any)
        /// </summary>
        public string OriginalJavaName { get; set; }

        /// <summary>
        /// Is this member public 
        /// </summary>
        public bool IsPublic
        {
            get { return IsVisibility(FieldAttributes.Public); }
            set { SetVisibility(FieldAttributes.Public, value); }
        }

        /// <summary>
        /// Is this member public and nested (only types)
        /// </summary>
        bool INetMemberVisibility.IsNestedPublic
        {
            get { return false; }
            set { IsPublic = value; }
        }

        /// <summary>
        /// Is this member private 
        /// </summary>
        public bool IsPrivate
        {
            get { return IsVisibility(FieldAttributes.Private); }
            set { SetVisibility(FieldAttributes.Private, value); }
        }

        /// <summary>
        /// Is this member protected
        /// </summary>
        public bool IsFamily
        {
            get { return IsVisibility(FieldAttributes.Family); }
            set { SetVisibility(FieldAttributes.Family, value); }
        }

        /// <summary>
        /// Is this member internal
        /// </summary>
        public bool IsAssembly
        {
            get { return IsVisibility(FieldAttributes.Assembly); }
            set { SetVisibility(FieldAttributes.Assembly, value); }
        }

        /// <summary>
        /// Is this member internal or protected
        /// </summary>
        public bool IsFamilyOrAssembly
        {
            get { return IsVisibility(FieldAttributes.FamORAssem); }
            set { SetVisibility(FieldAttributes.FamORAssem, value); }
        }

        /// <summary>
        /// Is this member internal and protected
        /// </summary>
        public bool IsFamilyAndAssembly
        {
            get { return IsVisibility(FieldAttributes.FamANDAssem); }
            set { SetVisibility(FieldAttributes.FamANDAssem, value); }
        }

        /// <summary>
        /// Are this member an the given type definition in the same scope?
        /// </summary>
        public bool HasSameScope(NetTypeDefinition type)
        {
            return (DeclaringType != null) && (DeclaringType.HasSameScope(type));
        }

        /// <summary>
        /// Are this member and the given other member in the same scope?
        /// </summary>
        public bool HasSameScope(INetMemberVisibility other)
        {
            return (DeclaringType != null) && other.HasSameScope(DeclaringType);
        }

        /// <summary>
        /// Is the given visibility value set?
        /// </summary>
        private bool IsVisibility(FieldAttributes value)
        {
            return (((Attributes) & FieldAttributes.FieldAccessMask) == value);
        }

        /// <summary>
        /// Set the given visibility value?
        /// </summary>
        private void SetVisibility(FieldAttributes mask, bool value)
        {
            if (!value)
                return;
            var remaining = (Attributes & ~FieldAttributes.FieldAccessMask);
            Attributes = remaining | (mask & FieldAttributes.FieldAccessMask);
        }

        /// <summary>
        /// Make sure that the visibility of types used in the signature of this member are high enough.
        /// </summary>
        public void EnsureVisibility()
        {
            FieldType.EnsureVisibility(this);
        }

        /// <summary>
        /// Make sure that the visibility of this member is not higher than allowed from types used in the signature of this member that are from another scope.
        /// </summary>
        public void LimitVisibility()
        {
            // Limit for field type
            this.LimitVisibility(FieldType);

            // Limit for sealed declaring type
            this.LimitIfDeclaringTypeSealed(DeclaringType);
        }
    }
}
