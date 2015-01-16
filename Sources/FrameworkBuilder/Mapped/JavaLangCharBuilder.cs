using System;
using System.CodeDom;
using System.ComponentModel.Composition;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Mapped;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Mapped
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangCharBuilder : JavaBoxedBuilder 
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangCharBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangCharBuilder(ClassFile cf)
            : base(cf, "System.Char", "java/lang/Character")
        {
        }

        /// <summary>
        /// Gets the name of the given method
        /// </summary>
        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            
            if (method.Name == "IsLowerCase")
                renamer.Rename(method, "IsLower");
            
            if (method.Name == "IsUpperCase")
                renamer.Rename(method, "IsUpper");
        }
    }
}
