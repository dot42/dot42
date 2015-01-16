using System.Reflection;
using Dot42.FrameworkDefinitions;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    internal class NestedInterfaceConstantsTypeBuilder : NestedTypeBuilder, IInterfaceConstantsTypeBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public NestedInterfaceConstantsTypeBuilder(TypeBuilder parent, string parentFullName, ClassFile cf, InnerClass inner)
            : base(parent, parentFullName, cf, inner)
        {
        }

        /// <summary>
        /// Improve the type name for the given class.
        /// </summary>
        protected override string CreateTypeName(NetTypeDefinition declaringType, ClassFile classFile, string name, string @namespace)
        {
            return base.CreateTypeName(declaringType, classFile, name, @namespace) + "Constants";
        }

        protected override Model.NetCustomAttribute CreateDexImportAttribute(ClassFile cf)
        {
            var ca = base.CreateDexImportAttribute(cf);
            ca.Properties.Add(AttributeConstants.DexImportAttributeIgnoreFromJavaName, true);
            ca.Properties.Add(AttributeConstants.DexImportAttributePriority, 1);
            return ca;
        }

        protected override void RegisterType(TargetFramework target, ClassFile classFile, Model.NetTypeDefinition typeDef)
        {
            // Do nothing
        }

        protected override void CreateNestedTypes(ClassFile cf, Model.NetTypeDefinition declaringType, string parentFullName, Model.NetModule module, TargetFramework target)
        {
            // Do nothing
        }

        /// <summary>
        /// Should the given field be implemented?
        /// </summary>
        protected internal override bool ShouldImplement(FieldDefinition field, TargetFramework target)
        {
            return true;
        }

        /// <summary>
        /// Should the given method be implemented?
        /// </summary>
        protected override bool ShouldImplement(MethodDefinition method, TargetFramework target)
        {
            return false;
        }

        /// <summary>
        /// Create type attributes
        /// </summary>
        protected override TypeAttributes GetAttributes(ClassFile cf)
        {
            return TypeAttributes.NestedPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Abstract;
        }
    }
}
