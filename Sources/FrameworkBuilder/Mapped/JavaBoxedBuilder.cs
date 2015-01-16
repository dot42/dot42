using System.Reflection;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Mapped
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    internal abstract class JavaBoxedBuilder : JavaBaseTypeBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        protected JavaBoxedBuilder(ClassFile cf, string clrTypeName, string className)
            : base(cf, clrTypeName, className)
        {
        }

        /// <summary>
        /// Implement members and setup references now that all types have been created
        /// </summary>
        public override void Implement(TargetFramework target)
        {
            TypeDefinition.IsStruct = true;
            base.Implement(target);
        }

        /// <summary>
        /// Fix base type relations.
        /// </summary>
        protected override void FixBaseType(NetTypeDefinition typeDef, TargetFramework target)
        {
            if ((typeDef.BaseType != null) && (typeDef.BaseType.FullName == "Java.Lang.Number"))
                typeDef.BaseType = null;
            base.FixBaseType(typeDef, target);
        }

        /// <summary>
        /// Gets the type of the value field
        /// </summary>
        protected virtual NetTypeReference GetValueType(TargetFramework target)
        {
            return target.TypeNameMap.GetByJavaClassName(ClassName);
        }

        /// <summary>
        /// Update the attributes of the given method
        /// </summary>
        public override MethodAttributes GetMethodAttributes(MethodDefinition method, MethodAttributes methodAttributes)
        {
            return (base.GetMethodAttributes(method, methodAttributes) | MethodAttributes.Final);
        }
    }
}
