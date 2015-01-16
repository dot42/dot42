using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Custom;

namespace Dot42.FrameworkBuilder.Custom
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(ICustomTypeBuilder))]
    internal sealed class SystemMulticastDelegateBuilder: CustomTypeBuilder 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public SystemMulticastDelegateBuilder()
            : base("System", "MulticastDelegate")
        {
        }

        /// <summary>
        /// Gets the attributes needed to create the type.
        /// </summary>
        protected override TypeAttributes Attributes
        {
            get { return TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Serializable; }
        }

        /// <summary>
        /// Implement members and setup references now that all types have been created
        /// </summary>
        public override void Implement(TargetFramework target)
        {
            base.Implement(target);
            TypeDefinition.BaseType = target.TypeNameMap.GetByType(typeof(Delegate));
        }
    }
}
