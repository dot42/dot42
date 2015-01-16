using System;
using System.Linq;
using System.Reflection;
using Dot42.FrameworkDefinitions;
using Dot42.ImportJarLib.Doxygen;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Helper used to build field definitions from ClassFile's data
    /// </summary>
    internal class FieldBuilder
    {
        private readonly FieldDefinition javaField;
        private readonly TypeBuilder declaringTypeBuilder;
        private NetFieldDefinition field;
        private DocField docField;

        /// <summary>
        /// Default ctor
        /// </summary>
        public FieldBuilder(FieldDefinition javaField, TypeBuilder declaringTypeBuilder)
        {
            if (javaField == null)
                throw new ArgumentNullException("javaField");
            if (declaringTypeBuilder == null)
                throw new ArgumentNullException("declaringTypeBuilder");
            this.javaField = javaField;
            this.declaringTypeBuilder = declaringTypeBuilder;
        }

        /// <summary>
        /// Create the field in the given type
        /// </summary>
        public void Create(NetTypeDefinition declaringType, TargetFramework target)
        {
            // Do not add private fields
            if ((javaField.AccessFlags & FieldAccessFlags.Private) != 0)
                return;

            // Find documentation
            var docClass = declaringTypeBuilder.Documentation;
            docField = (docClass != null) ? docClass.Fields.FirstOrDefault(x => x.Name == javaField.Name) : null;

            //var fieldType = declaringType.IsEnum ? target.TypeNameMap.GetByType(typeof(int)) : javaField.Signature.Resolve(target, declaringTypeBuilder);
            NetTypeReference fieldType;
            if (!javaField.Signature.TryResolve(target, declaringTypeBuilder, false, out fieldType))
                return;
            
            //var fieldTypeIsValueType = declaringType.IsEnum ? true : javaField.FieldType.
            var name = declaringTypeBuilder.GetFieldName(javaField);
            field = new NetFieldDefinition();
            field.Name = name;
            field.OriginalJavaName = javaField.Name;
            field.FieldType = fieldType;
            field.Attributes = GetAttributes(javaField, false);
            field.Description = (docField != null) ? docField.Description : null;
            declaringType.Fields.Add(field);
        }

        /// <summary>
        /// Finalize all references now that all types have been created
        /// </summary>
        public void Complete(ClassFile declaringClass, TargetFramework target)
        {
            if (field == null)
                return;

            // Setup field value
            var cValue = javaField.ConstantValue;
            // Fixup value
            if (cValue != null)
            {
                if (field.FieldType.IsSingle() && (float.IsPositiveInfinity((float)cValue) || float.IsNegativeInfinity((float)cValue)))
                    cValue = null;
                if (field.FieldType.IsDouble() && (double.IsPositiveInfinity((double)cValue) || double.IsNegativeInfinity((double)cValue)))
                    cValue = null;
            }

            if (cValue != null)
            {
                field.DefaultValue = cValue;
            }
            else if (field.DeclaringType.IsEnum)
            {
                /*field.DefaultValue = field.DeclaringType.Fields.ToList().IndexOf(field);
                field.Attributes |= FieldAttributes.Literal | FieldAttributes.Static;
                field.Attributes &= ~FieldAttributes.InitOnly;*/
                field.Attributes |= FieldAttributes.InitOnly | FieldAttributes.Static;
                field.Attributes &= ~FieldAttributes.Literal;
            }
            else 
            {
                field.Attributes &= ~FieldAttributes.Literal;
            }

            // Add DexImport attribute
            var ca = new NetCustomAttribute(null, javaField.Name, javaField.Descriptor);
            ca.Properties.Add(AttributeConstants.DexImportAttributeAccessFlagsName, (int)javaField.AccessFlags);
            field.CustomAttributes.Add(ca);
        }

        /// <summary>
        /// Called in the finalize phase of the type builder.
        /// </summary>
        public void Finalize(TargetFramework target, FinalizeStates state)
        {
            if (field == null)
                return;
            if (state == FinalizeStates.FixTypes)
            {
                field.EnsureVisibility();
                field.LimitVisibility();
            }
        }

        /// <summary>
        /// Create type attributes
        /// </summary>
        private static FieldAttributes GetAttributes(FieldDefinition javaField, bool isValueType)
        {
            var result = (FieldAttributes)0;            
            var isStatic = javaField.IsStatic;

            if (javaField.IsPublic) result |= FieldAttributes.Public;
            else if (javaField.IsPrivate) result |= FieldAttributes.Private;
            else if (javaField.IsProtected) result |= FieldAttributes.FamORAssem;
            else if (javaField.IsPackagePrivate) result |= FieldAttributes.Assembly;

            if (isStatic) result |= FieldAttributes.Static;
            if (javaField.IsFinal)
            {
                if (isValueType && isStatic)
                    result |= FieldAttributes.Literal;
                else
                    result |= FieldAttributes.InitOnly;
            }
            //if ((accessFlags & FieldAccessFlags.Transient) != 0) result |= MemberAttributes..NotSerialized;

            return result;
        }
    }
}
