using System.Collections.Generic;
using System.Linq;
using Dot42.JvmClassLib;

namespace Dot42.CompilerLib.Ast.Extensions
{
    /// <summary>
    /// Java related extension methods
    /// </summary>
    partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Gets the first method the given method overrides.
        /// </summary>
        public static MethodDefinition GetBaseMethod(this MethodDefinition method)
        {
            var declaringType = method.DeclaringClass;
            ClassFile superClass;
            if (!declaringType.TryGetSuperClass(out superClass))
                return null;

            while (superClass != null)
            {
                var result = superClass.Methods.FirstOrDefault(x => x.AreSame(method));
                if (result != null)
                    return result;
                if (!superClass.TryGetSuperClass(out superClass))
                    break;
            }
            return null;
        }

        /// <summary>
        /// Gets all methods the given method overrides.
        /// </summary>
        public static IEnumerable<MethodDefinition> GetBaseMethods(this MethodDefinition method)
        {
            while (method != null)
            {
                var @base = method.GetBaseMethod();
                if (@base == null)
                    yield break;
                yield return @base;
                method = @base;
            }
        }

        /// <summary>
        /// Gets the first method the given method overrides from an implemented interface.
        /// </summary>
        public static MethodDefinition GetBaseInterfaceMethod(this MethodDefinition method)
        {
            var declaringType = method.DeclaringClass;
            while (declaringType != null)
            {
                if (declaringType.Interfaces != null)
                {
                    foreach (var ifaceRef in declaringType.Interfaces)
                    {
                        ClassFile iface;
                        if (!declaringType.Loader.TryLoadClass(ifaceRef.ClassName, out iface))
                            continue;

                        var result = iface.Methods.FirstOrDefault(x => x.AreSame(method));
                        if (result != null)
                            return result;
                    }
                }
                if (!declaringType.TryGetSuperClass(out declaringType))
                    break;
            }

            return null;
        }

        /// <summary>
        /// Gets all methods the given method overrides from an implemented interface.
        /// </summary>
        public static IEnumerable<MethodDefinition> GetBaseInterfaceMethods(this MethodDefinition method)
        {
            while (method != null)
            {
                var @base = method.GetBaseInterfaceMethod();
                if (@base == null)
                    yield break;
                yield return @base;
                method = @base;
            }
        }

        /// <summary>
        /// Are the given methods the same wrt name and parameters?
        /// </summary>
        private static bool AreSame(this MethodDefinition method, MethodDefinition other)
        {
            if (method.Name != other.Name)
                return false;
            return (method.Descriptor == other.Descriptor) ||
                   (Descriptors.StripMethodReturnType(method.Descriptor) == Descriptors.StripMethodReturnType(other.Descriptor));
        }
    }
}
