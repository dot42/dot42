using System.Reflection;
using Dot42.FrameworkDefinitions;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    internal sealed class StandardInterfaceConstantsTypeBuilder : StandardTypeBuilder, IInterfaceConstantsTypeBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal StandardInterfaceConstantsTypeBuilder(ClassFile cf)
            : base(cf)
        {
        }

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

        protected override TypeAttributes GetAttributes(ClassFile cf, bool hasFields)
        {
            return TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Abstract;
        }

        protected internal override bool ShouldImplement(FieldDefinition field, TargetFramework target)
        {
            return true;
        }

        protected override bool ShouldImplement(MethodDefinition method, TargetFramework target)
        {
            return false;
        }
    }
}
