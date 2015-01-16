using System;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Java;
using Dot42.DexLib;
using TypeReference = Dot42.DexLib.TypeReference;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Dex related extension methods
    /// </summary>
    public static partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Gets a class reference for the given type reference.
        /// </summary>
        internal static TypeReference GetReference(this JvmClassLib.TypeReference type, XTypeUsageFlags usageFlags, DexTargetPackage targetPackage, XModule module)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var xType = XBuilder.AsTypeReference(module, type, usageFlags);
            return xType.GetReference(targetPackage);
        }

        /// <summary>
        /// Gets a class reference for the given type reference.
        /// </summary>
        internal static ClassReference GetClassReference(this JvmClassLib.TypeReference type, XTypeUsageFlags usageFlags, DexTargetPackage targetPackage, XModule module)
        {
            var classRef = type.GetReference(usageFlags, targetPackage, module) as ClassReference;
            if (classRef == null)
                throw new ArgumentException(string.Format("type {0} is not a class reference", type.ClassName));
            return classRef;
        }
    }
}
