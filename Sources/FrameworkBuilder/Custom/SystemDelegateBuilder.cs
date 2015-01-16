using System.ComponentModel.Composition;
using System.Reflection;
using Dot42.ImportJarLib.Custom;

namespace Dot42.FrameworkBuilder.Custom
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(ICustomTypeBuilder))]
    internal sealed class SystemDelegateBuilder: CustomTypeBuilder 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public SystemDelegateBuilder()
            : base("System", "Delegate")
        {
        }

        /// <summary>
        /// Gets the attributes needed to create the type.
        /// </summary>
        protected override TypeAttributes Attributes
        {
            get { return TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Serializable; }
        }
    }
}
