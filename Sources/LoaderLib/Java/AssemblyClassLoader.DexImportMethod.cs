using System.Linq;
using Dot42.FrameworkDefinitions;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Extensions;
using Mono.Cecil;
using ILMethodDefinition = Mono.Cecil.MethodDefinition;
using JavaMethodDefinition = Dot42.JvmClassLib.MethodDefinition;

namespace Dot42.LoaderLib.Java
{
    /// <summary>
    /// Load java classes from JavaClass attributes included in assemblies.
    /// </summary>
    partial class AssemblyClassLoader
    {
        /// <summary>
        /// Data of a DexImportAttribute on a method
        /// </summary>
        public sealed class DexImportMethod
        {
            private readonly ILMethodDefinition method;
            private JavaMethodDefinition resolved;
            private readonly CustomAttribute attr;
            private readonly bool ignoreFromJava;

            /// <summary>
            /// Default ctor
            /// </summary>
            public DexImportMethod(ILMethodDefinition method, CustomAttribute attr)
            {
                this.method = method;
                this.attr = attr;
                ignoreFromJava = attr.Properties.Where(x => x.Name == AttributeConstants.DexImportAttributeIgnoreFromJavaName).Select(x => (bool)x.Argument.Value).FirstOrDefault();
            }

            /// <summary>
            /// Gets the method;
            /// </summary>
            public ILMethodDefinition Method
            {
                get { return method; }
            }

            /// <summary>
            /// Does this import has the ignore from java flag set?
            /// </summary>
            public bool IgnoreFromJava { get { return ignoreFromJava; } }

            /// <summary>
            /// Returns the dex/java import attribute
            /// </summary>
            public CustomAttribute ImportAttribute
            {
                get { return attr; }
            }

            /// <summary>
            /// Resolve the method into a java method definition.
            /// </summary>
            public JavaMethodDefinition Resolve(ClassFile declaringClass)
            {
                if (resolved == null)
                {
                    // Load the class
                    resolved = BuildMethod(declaringClass);
                }
                return resolved;
            }

            /// <summary>
            /// Build a java method from my data.
            /// </summary>
            private JavaMethodDefinition BuildMethod(ClassFile declaringClass)
            {
                var attr = method.GetDexOrJavaImportAttribute(true);
                var nCtorArgs = attr.ConstructorArguments.Count;
                var name = (string)attr.ConstructorArguments[(nCtorArgs == 2) ? 0 : 1].Value;
                var descriptor = (string)attr.ConstructorArguments[(nCtorArgs == 2) ? 1 : 2].Value;
                var accessFlags = attr.Properties.Where(x => x.Name == AttributeConstants.DexImportAttributeAccessFlagsName).Select(x => (int) x.Argument.Value).FirstOrDefault();
                var signature = attr.Properties.Where(x => x.Name == AttributeConstants.DexImportAttributeSignature).Select(x => (string)x.Argument.Value).FirstOrDefault();
                return new JavaMethodDefinition(declaringClass, (MethodAccessFlags)accessFlags, name, descriptor, signature);
            }
        }
    }
}
