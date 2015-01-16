using System.Reflection;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Custom;
using Dot42.ImportJarLib.Model;

namespace Dot42.FrameworkBuilder.Custom
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    internal abstract class BoxedBuilder : CustomTypeBuilder 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        protected BoxedBuilder(string @namespace, string name)
            : base(@namespace, name, true)
        {
        }

        /// <summary>
        /// Gets the attributes needed to create the type.
        /// </summary>
        protected override TypeAttributes Attributes
        {
            get { return TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Serializable | TypeAttributes.Sealed; }
        }

        /// <summary>
        /// Implement members and setup references now that all types have been created
        /// </summary>
        public override void Implement(TargetFramework target)
        {
            base.Implement(target);
            TypeDefinition.IsStruct = true;
            //var field = new NetFieldDefinition { FieldType = GetValueType(target), Name = "m_value", Attributes = FieldAttributes.Private };
            //TypeDefinition.Fields.Add(field);
        }

        /// <summary>
        /// Gets the type of the value field
        /// </summary>
        protected abstract NetTypeReference GetValueType(TargetFramework target);
    }
}
